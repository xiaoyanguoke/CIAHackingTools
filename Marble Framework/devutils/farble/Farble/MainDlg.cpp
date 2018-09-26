#define _CRT_SECURE_NO_WARNINGS
#define _CRT_NON_CONFORMING_SWPRINTFS
#pragma warning(disable : 4800) //Some bullshit about performance warnings when casting to boolean

#include "stdafx.h"
#include <time.h>
#include "resource.h"
#include "MainDlg.h"

BOOL CMainDlg::PreTranslateMessage(MSG* pMsg)
{
	switch(pMsg->message)
	{
	case WM_KEYDOWN:
		if (pMsg->hwnd == m_inputBox.m_hWnd)
		{
			if (::GetKeyState(VK_CONTROL)<0)
			{
				if (pMsg->wParam == 0x41)
				{
					m_inputBox.SetSel(0,-1);
					return TRUE;
				}
				if (pMsg->wParam == 0x43)
				{
					m_inputBox.Copy();
					return TRUE;
				}
			}
		}
	}
	return CWindow::IsDialogMessage(pMsg);
}

BOOL CMainDlg::OnIdle()
{
	//UIUpdateChildWindows();
//		wchar_t bitch1[] = {L'\x7FD9',L'\x7FB0',L'\x7FC4',L'\x7FA7',L'\x7FCF',L'\x7FFE',L'\x7FFE'};
//		for( int i = 6; i > 0; i-- ) bitch1[i] = bitch1[i-1] ^ bitch1[i]; bitch1[0] = bitch1[0] ^ 0x7FBB;
	//	BYTE bitch2[] = {'\x7B69','\x7B00','\x7B74','\x7B17','\x7B7F','\x7B4D','\x7B4D'}; for( int i = 6; i > 0; i-- ) bitch2[i] = bitch2[i-1] ^ bitch2[i]; bitch2[0] = bitch2[0] ^ 0x7B0B;

	int a = 0;

	return FALSE;
}

LRESULT CMainDlg::OnInitDialog(UINT, WPARAM, LPARAM, BOOL&)
{
	// center the dialog on the screen
	CenterWindow();

	// set icons
	HICON hIcon = (HICON)::LoadImage(_Module.GetResourceInstance(), MAKEINTRESOURCE(IDR_MAINFRAME), 
		IMAGE_ICON, ::GetSystemMetrics(SM_CXICON), ::GetSystemMetrics(SM_CYICON), LR_DEFAULTCOLOR);
	SetIcon(hIcon, TRUE);
	HICON hIconSmall = (HICON)::LoadImage(_Module.GetResourceInstance(), MAKEINTRESOURCE(IDR_MAINFRAME), 
		IMAGE_ICON, ::GetSystemMetrics(SM_CXSMICON), ::GetSystemMetrics(SM_CYSMICON), LR_DEFAULTCOLOR);
	SetIcon(hIconSmall, FALSE);

	// register object for message filtering and idle updates
	CMessageLoop* pLoop = _Module.GetMessageLoop();
	ATLASSERT(pLoop != NULL);
	pLoop->AddMessageFilter(this);
	pLoop->AddIdleHandler(this);

	UIAddChildWindowContainer(m_hWnd);

	//UIEnable(IDOK, false);
	//UIEnable(IDC_BUTTONTEXT, false);
	//UIEnable(IDC_BUTTONSOURCE, false);

	m_RXOR.Attach(GetDlgItem(IDC_RADIO_RXOR));
	m_XOR.Attach(GetDlgItem(IDC_RADIO_XOR));
	m_RXOR.SetCheck(TRUE);
	m_XOR.SetCheck(FALSE);

	m_Encrypt.Attach(GetDlgItem(IDC_RADIO_Encrypt));
	m_Decrypt.Attach(GetDlgItem(IDC_RADIO_Decrypt));
	m_Encrypt.SetCheck(TRUE);
	m_Decrypt.SetCheck(FALSE);
	//Decrypt is not yet implemented:
	m_Encrypt.EnableWindow( FALSE );
	m_Decrypt.EnableWindow(FALSE);

	m_ViewSource.Attach(GetDlgItem(IDC_RADIO_Source));
	m_ViewDescramble.Attach(GetDlgItem(IDC_RADIO_Descramble));
	m_ViewSource.SetCheck(TRUE);
	m_ViewDescramble.SetCheck(FALSE);
	m_ViewSource.EnableWindow( FALSE );
	m_ViewDescramble.EnableWindow(FALSE);

	m_ButtonEncrypt.Attach(GetDlgItem(IDOK));
	m_ButtonEncrypt.EnableWindow(TRUE);

	m_CopySource.Attach(GetDlgItem(IDC_BUTTON_Source));
	m_CopySource.EnableWindow(FALSE);
	m_CopyDescramble.Attach(GetDlgItem(IDC_BUTTON_Descramble));
	m_CopyDescramble.EnableWindow(FALSE);

	m_Manual.Attach( GetDlgItem(IDC_RADIO_Manual) );
	m_Randomize.Attach( GetDlgItem(IDC_RADIO_Randomize) );
	m_Manual.SetCheck(FALSE);
	m_Randomize.SetCheck(TRUE);
	m_Manual.EnableWindow( TRUE );
	m_Randomize.EnableWindow( TRUE );

	m_xorBox.Attach( GetDlgItem(IDC_XOR_VAL) );
	m_xorBox.EnableWindow( FALSE );

	m_Line.Attach( GetDlgItem(IDC_RADIO_Line) );
	m_Macro.Attach( GetDlgItem(IDC_RADIO_Macro) );
	m_Function.Attach( GetDlgItem(IDC_RADIO_Function) );
	m_Line.SetCheck(TRUE);
	m_Macro.SetCheck(FALSE);
	m_Function.SetCheck(FALSE);
	m_Line.EnableWindow(TRUE);
	m_Macro.EnableWindow(TRUE);
	m_Function.EnableWindow(TRUE);

	m_inputBox.Attach( GetDlgItem(IDC_Input) );

	srand( (unsigned int)time(NULL) );

	return TRUE;
}

LRESULT CMainDlg::OnOK(WORD, WORD wID, HWND, BOOL&)
{
	CloseDialog(wID);
	return 0;
}

LRESULT CMainDlg::OnCancel(WORD, WORD wID, HWND, BOOL&)
{
	CloseDialog(wID);
	return 0;
}

void CMainDlg::CloseDialog(int nVal)
{
	DestroyWindow();
	::PostQuitMessage(nVal);
}

LRESULT CMainDlg::OnBnClickedRadioSource(WORD, WORD, HWND, BOOL&)
{
	if( code.getSize() > 0 )
		SetDlgItemText(IDC_output, code.getArray());
	else
		SetDlgItemText(IDC_output, L"");
	return 0;
}
LRESULT CMainDlg::OnBnClickedRadioDescramble(WORD, WORD, HWND, BOOL&)
{
	if( descramble.getSize() > 0 )
		SetDlgItemText(IDC_output, descramble.getArray());
	else
		SetDlgItemText(IDC_output, L"");
	return 0;
}
LRESULT CMainDlg::OnBnClickedEncrypt(WORD, WORD, HWND, BOOL&)
{
	SetDlgItemText(IDOK, _T("Encrypt"));
	return 0;
}

LRESULT CMainDlg::OnBnClickedDecrypt(WORD, WORD, HWND, BOOL&)
{
	SetDlgItemText(IDOK, _T("Decrypt"));
	return 0;
}

LRESULT CMainDlg::OnBnClickedClear(WORD, WORD, HWND, BOOL&)
{
	SetDlgItemText( IDC_Input, _T("") );
	return 0;
}

LRESULT CMainDlg::OnBnClickedRandomize(WORD, WORD, HWND, BOOL&)
{
	TCHAR randChars[3];
	_stprintf(randChars, _T("%.2x"), rand() % 256);
	SetDlgItemText(IDC_XOR_VAL, randChars);
	return 0;
}

LRESULT CMainDlg::OnBnClickedRadioManual(WORD, WORD, HWND, BOOL&)
{
	m_xorBox.EnableWindow( TRUE );
	m_ButtonEncrypt.EnableWindow(FALSE);
	return 0;
}

LRESULT CMainDlg::OnBnClickedRadioRandomize(WORD, WORD, HWND, BOOL&)
{
	m_xorBox.SetWindowTextW(L"");
	m_XORByte = 0;
	m_xorBox.EnableWindow( FALSE );
	m_ButtonEncrypt.EnableWindow(TRUE);
	return 0;
}

LRESULT CMainDlg::OnBnClickedButtonSource(WORD, WORD, HWND, BOOL&)
{
	if(OpenClipboard())
	{
		EmptyClipboard();
		HGLOBAL hClipboardData;
		hClipboardData = GlobalAlloc(/*GMEM_DDESHARE*/GMEM_MOVEABLE, code.getSize() * sizeof(TCHAR));
		TCHAR *pchData;
		pchData = (TCHAR *)GlobalLock(hClipboardData);
		lstrcpy(pchData, code.getArray());
		GlobalUnlock(hClipboardData);
		SetClipboardData(CF_UNICODETEXT, hClipboardData);
		CloseClipboard();
		GlobalFree(hClipboardData);
	}
	return 0;
}

LRESULT CMainDlg::OnBnClickedButtonDescramble(WORD, WORD, HWND, BOOL&)
{
	if(OpenClipboard())
	{
		EmptyClipboard();
		HGLOBAL hClipboardData;
		hClipboardData = GlobalAlloc(/*GMEM_DDESHARE*/GMEM_MOVEABLE, descramble.getSize() * sizeof(TCHAR));
		TCHAR *pchData;
		pchData = (TCHAR *)GlobalLock(hClipboardData);
		lstrcpy(pchData, descramble.getArray());
		GlobalUnlock(hClipboardData);
		SetClipboardData(CF_UNICODETEXT, hClipboardData);
		CloseClipboard();
		GlobalFree(hClipboardData);
	}
	return 0;
}

LRESULT CMainDlg::OnEnChangeXor(WORD, WORD, HWND, BOOL&)
{
	TCHAR szXORString[11], szInputString[11];
	BOOL bLower = FALSE;
	int i, j;

	// Set cursor to highlight last character
//	::SendMessage(GetDlgItem(IDC_XOR_VAL), EM_SETSEL, 1, -1);

	GetDlgItemText(IDC_XOR_VAL, szXORString, 10);
	int StaticLen = lstrlen(szXORString);
	GetDlgItemText(IDC_Input, szInputString, 10);
	int InputLen = lstrlen(szInputString);


	if(StaticLen >= 2 )//&& InputLen > 0)
		m_ButtonEncrypt.EnableWindow(TRUE);
	else
	{
		m_ButtonEncrypt.EnableWindow(FALSE);
		m_CopySource.EnableWindow(FALSE);
		m_CopyDescramble.EnableWindow(FALSE);
	}

// 	// Make a - f upper case
// 	for(i = 0; i < StaticLen; i++)
// 	{
// 		if(_istlower(szXORString[i]) && _totupper(szXORString[i]) >= TEXT('A') && _totupper(szXORString[i]) <= TEXT('F'))
// 		{
// 			szXORString[i] = _totupper(szXORString[i]);
// 			bLower = TRUE;
// 		}
// 	}
// 	if( bLower )
// 		SetDlgItemText(IDC_XOR_VAL, szXORString);

	// Ensure we have valid hex characters
	for(i = 0; i < StaticLen; i++)
	{
		// Illegal character
		if(!_istdigit(szXORString[i]) && (szXORString[i] < TEXT('A') || szXORString[i] > TEXT('F')))
		{
			for(j = i; j < StaticLen; j++)
				szXORString[j] = szXORString[j+1];
			szXORString[j] = TEXT('\0');
			SetDlgItemText(IDC_XOR_VAL, szXORString);
		}
	}

	// Throw away extra characters
	if(StaticLen > 4)
	{
		szXORString[4] = TEXT('\0');
		SetDlgItemText(IDC_XOR_VAL, szXORString);
	}
	_stscanf(szXORString, _T("%x"), &m_XORByte);
	::SendMessage(GetDlgItem(IDC_XOR_VAL), EM_SETSEL, 4, 4);

	return 0;
}


LRESULT CMainDlg::OnBnClickedOk(WORD, WORD, HWND, BOOL&)
{
	TCHAR* szInputString = new TCHAR[MAX_PATH];
	bool encryptAsByte = ((CButton)GetDlgItem(IDC_CHECK_BYTE)).GetCheck();
	bool rxor = m_RXOR.GetCheck();
	farbleType fType;

	//	m_ButtonEncrypt.EnableWindow(FALSE);
	if( m_Line.GetCheck() )
		fType = FARBLE_Line;
	else if( m_Macro.GetCheck() )
		fType = FARBLE_Macro;
	else
		fType = FARBLE_Function;

	//Get variable-length input
	for(int i = 1; GetDlgItemText(IDC_Input, szInputString, MAX_PATH * i) != 0 && lstrlen(szInputString) == MAX_PATH * i - 1;
		delete [] szInputString, szInputString = new TCHAR[MAX_PATH*++i] ) ;
		unsigned int stringLen = _tcslen( szInputString );
	if( stringLen == 0)
	{
		MessageBox(_T("No input to encrypt!"), _T("ERROR"), MB_ICONERROR);
		delete [] szInputString;
		return 0;
	}

	//Initialize code and descramble:
	code.erase(/*100*/stringLen);
	initDescramble(fType, rxor);

	// Append commented-out original data:
	code.addChar('/');
	code.addChar('/');
	for( unsigned int i = 0; i < stringLen; )
	{
		code.addChar( szInputString[i] );
		if( szInputString[i++] == '\n' )
		{
			code.addChar('/');
			code.addChar('/');
			while( isspace( szInputString[i] ) ) i++;
		}
	}
	code.addChar('\r');
	code.addChar('\n');
	code.addChar('\r');
	code.addChar('\n');

	//Iterates over ENTIRE user input; Depends on new-line to decipher multiple strings
	for( unsigned int i = 0; i < stringLen; )
	{
		while( isspace( szInputString[i] ) ) i++;
		CharArrayList<TCHAR> temp;
		
		//We only care about what's in quotes
		while( szInputString[i] != _T('\"') && i < stringLen )
		{
			code.addChar(szInputString[i]);
			temp.addChar(szInputString[i++]);
		}
		if( i >= stringLen )
			break;

		bool unicode = isUnicode( /*code*/temp, false );//encryptAsByte );
		TCHAR* szVarName = determineVarName( temp.getArray() );
		if( encryptAsByte )
		{
			//Test if 'L' was appended to unicode string
			if( code.getArray()[code.getSize()-1] == 'L' )
				code.remove(code.getSize()-1);
			code.addChar( '{' );
		}
		else
			code.addChar(szInputString[i]);
		i++;

		unsigned int totalChars = 0;
		TCHAR szCByte = m_Manual.GetCheck() ? (unicode ? m_XORByte : /*(char)*/m_XORByte & 0x00FF) : (unicode ? rand() % 0x10000 : rand() % 0x100);
		bool finished = false;
		for( TCHAR xorByte = szCByte, currChar = szInputString[i]; /*currChar != '\"'*/!finished; totalChars++, currChar = szInputString[++i])
		{
			if( currChar == '\\' )
				currChar = convertEscapeSequence(szInputString[++i]);
			else if( currChar == '\"' )
			{
				finished = true;
				if( !encryptAsByte )
					break;
				currChar = '\0';
			}
			currChar ^= xorByte;
			if( rxor )
				xorByte = currChar;

			TCHAR temp[16];
			if( unicode )//&& !encryptAsByte )
				_stprintf_s(temp, 7, _T("\\x%04X"), currChar);
			// 			else if( unicode && encryptAsByte )
			// 			{
			// 				_stprintf_s(temp, 16, _T("\\x%02X\',\'\\x%02X"), (BYTE)(currChar << 8), (BYTE)currChar);
			// 			}
			else
				_stprintf_s(temp, 7, _T("\\x%02X"), currChar);

			if( encryptAsByte )
			{
				code.addChar('L');
				code.addChar('\'');
			}
			for( unsigned int j = 0; j < _tcslen(temp); j++ )
				code.addChar(temp[j]);
			if( encryptAsByte )
			{
				code.addChar('\'');
				code.addChar(',');
			}
		}
		if( encryptAsByte )
		{
			code.remove(code.getSize()-1);
			code.addChar('}');
		}
		//Add remainder of line, including close quotes
		for(TCHAR tempChar = szInputString[i]; tempChar != _T(';') && tempChar != _T('\r') && i < stringLen;
			code.addChar(tempChar), tempChar = szInputString[++i] );
			addDescramble( fType, szVarName, totalChars, rxor, szCByte, encryptAsByte );
		while( !isspace(szInputString[i]) && i < stringLen ) i++;
		while( isspace(szInputString[i]) && i < stringLen ) i++;
		delete [] szVarName;
	}
	code.addChar('\0');
	if( fType == FARBLE_Function )
		descramble.addChar(_T('}'));
	descramble.addChar(_T('\r'));
	descramble.addChar(_T('\n'));
	descramble.addChar('\0');

	SetDlgItemText(IDC_output, code.getArray());
	m_ViewSource.SetCheck(TRUE);
	m_ViewDescramble.SetCheck(FALSE);
	m_CopySource.EnableWindow( TRUE );
	m_CopyDescramble.EnableWindow( TRUE );
	m_ViewSource.EnableWindow( TRUE );
	m_ViewDescramble.EnableWindow( TRUE );

	delete[] szInputString;
	return 0;
}

bool CMainDlg::isUnicode(CharArrayList<TCHAR>& arr, bool replaceWithBYTE)
{
	TCHAR* string = arr.getArray(), *strPtr = NULL;
	bool retVal = false;
	if( (strPtr = _tcsstr( string, _T("TCHAR") )) != NULL || (strPtr = _tcsstr( string, _T("wchar_t") )) != NULL ||
		(strPtr = _tcsstr( string, _T("WCHAR") )) != NULL || (strPtr = _tcsstr( string, _T("LPWSTR") )) != NULL )
		retVal = true;
// 	else if( (strPtr = _tcsstr( string, _T("char") )) != NULL || (strPtr = _tcsstr( string, _T("CHAR") )) != NULL ||
// 		(strPtr = _tcsstr( string, _T("LPSTR") )) != NULL ) ;

	if( strPtr != NULL && replaceWithBYTE )
	{
		TCHAR replace[] = _T("BYTE");
		memcpy( strPtr, replace, sizeof(TCHAR)*_tcslen(replace) );
		strPtr+=_tcslen(replace);
		int i = strPtr - string;
		for(; !isspace( arr.getChar(i) ); arr.remove(i) ) ;
		for(; isspace( arr.getChar(i) ) && isspace( arr.getChar(i+1) ); arr.remove(i) ) ;
		//		for(int i = 0; !isspace( strPtr[i] ); strPtr[i++] = ' ') ;
	}
	return retVal;
}
TCHAR* CMainDlg::determineVarName(const TCHAR* string)
{
	//Damn I think I over-complicated this. Just look for the last word before the '='
	//Technically, there should be a bracket otherwise it's immutable, but we'll just add it
	//if the user forgets.

	TCHAR* retVal;
	const TCHAR* varName = _tcschr(string, '[');
	if( varName == NULL && (varName = _tcschr(string, '=')) == NULL )
	{
		retVal = new TCHAR[8];
		_tcscpy_s( retVal, 8, _T("unknown") );
		return retVal;
	}

	varName-=1;
	while( _istspace(*varName) ) varName--;
	const TCHAR* endChar = varName;
	while( !_istspace(*varName) ) varName--;
	varName++;

	DWORD varLen = endChar - varName + 1;
	retVal = new TCHAR[varLen + 1];
	_tcsncpy( retVal, varName, varLen );
	retVal[varLen] = '\0';
	return retVal;
}

void CMainDlg::addDescramble( farbleType fType, TCHAR* szVarName, unsigned int totalChars, bool RXOR, TCHAR val, bool encryptAsByte )
{
	TCHAR* system = NULL;
	switch( fType )
	{
	case FARBLE_Line:
		{
			if( RXOR )
			{
				TCHAR descString[] = _T("; for( int i = %u; i > 0; i-- ) %s[i] = %s[i-1] ^ %s[i]; %s[0] = %s[0] ^ 0x%X;\r\n");
				DWORD len = _tcslen( descString ) + 1 + _tcslen(szVarName) * 5 + 10 * 2;
				system = new TCHAR[len];
				_stprintf_s( system, len, descString, totalChars - 1, szVarName, szVarName, szVarName, szVarName, szVarName, val);
			}
			else
			{
				TCHAR descString[] = _T("; for( int i = 0; i < %u; i++ ) %s[i] ^= 0x%X;\r\n");
				DWORD len = _tcslen( descString ) + 1 + _tcslen(szVarName) + 10 * 2;
				system = new TCHAR[len];
				_stprintf_s( system, len, descString, totalChars, szVarName, val);
			}
			for( unsigned int i = 0; i < _tcslen(system); i++ )
				code.addChar( system[i] );
			break;
		}
	case FARBLE_Macro:
		{
			if( RXOR )
			{
				TCHAR descString[] = _T(";\r\nUNSCRAM_RXOR( %s, %d, 0x%X );\r\n");
				DWORD len = _tcslen( descString ) + 1 + _tcslen(szVarName) + 10 * 2;
				system = new TCHAR[len];
				_stprintf_s( system, len, descString, szVarName, totalChars - 1, val);
			}
			else
			{
				TCHAR descString[] = _T(";\r\nUNSCRAM_XOR( %s, %d, 0x%X );\r\n");
				DWORD len = _tcslen( descString ) + 1 + _tcslen(szVarName) + 10 * 2;
				system = new TCHAR[len];
				_stprintf_s( system, len, descString, szVarName, totalChars - 1, val);
			}
			for( unsigned int i = 0; i < _tcslen(system); i++ )
				code.addChar( system[i] );
			break;
		}
	case FARBLE_Function:
		{
			TCHAR* system;
			if( RXOR )
			{
				TCHAR descString[] = _T("\r\n\tfor( i = %u; i > 0; i-- )\r\n\t\t%s[i] = %s[i-1] ^ %s[i];\r\n\t%s[0] = %s[0] ^ 0x%X;\r\n");
				DWORD len = _tcslen( descString ) + 1 + _tcslen(szVarName) * 5 + 10 * 2;
				system = new TCHAR[len];
				_stprintf_s( system, len, descString, totalChars - 1, szVarName, szVarName, szVarName, szVarName, szVarName, val);
			}
			else
			{
				TCHAR descString[] = _T("\r\n\tfor( i = 0; i < %u; i++ )\r\n\t\t%s[i] ^= 0x%X;\r\n");
				DWORD len = _tcslen( descString ) + 1 + _tcslen(szVarName) + 10 * 2;
				system = new TCHAR[len];
				_stprintf_s( system, len, descString, totalChars, szVarName, val);
			}
			for( unsigned int i = 0; i < _tcslen(system); i++ )
				descramble.addChar( system[i] );
			code.addChar(';');
			code.addChar('\r');
			code.addChar('\n');
			break;
		}
	default: break;
	}
	if( system != NULL ) delete [] system;
}

//Will _NOT_ work for any other escapes
TCHAR CMainDlg::convertEscapeSequence(TCHAR val)
{
	switch( val )
	{
	case '\'': return '\'';
	case '\"': return '\"';
	case '\?': return '\?';
	case '\\': return '\\';
	case '0': return '\0';
	case 'a': return '\a';
	case 'b': return '\b';
	case 'f': return '\f';
	case 'n': return '\n';
	case 'r': return '\r';
	case 't': return '\t';
	case 'v': return '\v';
		//		case '0-8?'					arbitrary octal value
		//		case 'x'					arbitrary hex value
		//		case 'u'					arbitrary unicode value
		//		case 'U'					arbitrary unicode value
	default: MessageBox(_T("Error. You have an unsupported escape sequence in your string."),_T("FAIL"),0);return'\0';// exit(-1);
	}
}

void CMainDlg::initDescramble(farbleType fType, bool RXOR)
{
	descramble.erase(100);

	switch( fType )
	{
	case FARBLE_Line: break;
	case FARBLE_Macro:
		{
			if( RXOR )
			{
				TCHAR descString[] = _T("#define UNSCRAM_RXOR(s, l, v) for( int i = l; i > 0; i-- ) s[i] = s[i-1] ^ s[i]; s[0] = s[0] ^ v");
				for( unsigned int i = 0; i < _tcslen(descString); i++ )
					descramble.addChar( descString[i] );
			}
			else
			{
				TCHAR descString[] = _T("#define UNSCRAM_XOR(s, l, v) for( int i = 0; i < l; i++ ) s[i] ^= v");
				for( unsigned int i = 0; i < _tcslen(descString); i++ )
					descramble.addChar( descString[i] );
			}
			break;
		}
	case FARBLE_Function:
		{
			TCHAR descString[] = _T("static void UnscrambleCollectionStrings()\r\n{\r\n\tint i;\r\n\tstatic bool bUnscrambled = false;\r\n\r\n\tif(bUnscrambled)\r\n\t\treturn;\r\n\telse\r\n\t\tbUnscrambled = true;\r\n");
			for( unsigned int i = 0; i < _tcslen(descString); i++ )
				descramble.addChar( descString[i] );
			break;
		}
	default: break;
	}
}