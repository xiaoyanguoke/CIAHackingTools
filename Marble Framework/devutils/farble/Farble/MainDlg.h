// MainDlg.h : interface of the CMainDlg class
//
/////////////////////////////////////////////////////////////////////////////

#pragma once
#include "CharArrayList.cpp"

class CMainDlg : public CDialogImpl<CMainDlg>, public CUpdateUI<CMainDlg>,
	public CMessageFilter, public CIdleHandler
{
public:
	enum { IDD = IDD_MAINDLG };
	enum farbleType
	{
		FARBLE_Line, FARBLE_Macro, FARBLE_Function
	};

	virtual BOOL PreTranslateMessage(MSG* pMsg);
	virtual BOOL OnIdle();

	BEGIN_UPDATE_UI_MAP(CMainDlg)
	END_UPDATE_UI_MAP()

	BEGIN_MSG_MAP(CMainDlg)
		MESSAGE_HANDLER(WM_INITDIALOG, OnInitDialog)
		COMMAND_ID_HANDLER(IDCANCEL, OnCancel)
		COMMAND_HANDLER(IDC_XOR_VAL, EN_CHANGE, OnEnChangeXor)
		COMMAND_HANDLER(IDOK, BN_CLICKED, OnBnClickedOk)
		COMMAND_HANDLER(IDC_RADIO_Encrypt, BN_CLICKED, OnBnClickedEncrypt)
		COMMAND_HANDLER(IDC_RADIO_Decrypt, BN_CLICKED, OnBnClickedDecrypt)
		COMMAND_HANDLER(IDC_BUTTON_Source, BN_CLICKED, OnBnClickedButtonSource)
		COMMAND_HANDLER(IDC_BUTTON_Descramble, BN_CLICKED, OnBnClickedButtonDescramble)
		COMMAND_HANDLER(IDC_RADIO_Source, BN_CLICKED, OnBnClickedRadioSource)
		COMMAND_HANDLER(IDC_RADIO_Descramble, BN_CLICKED, OnBnClickedRadioDescramble)
		COMMAND_HANDLER(IDC_BUTTON_CLEAR, BN_CLICKED, OnBnClickedClear)
		COMMAND_HANDLER(IDC_BUTTON_RANDOMIZE, BN_CLICKED, OnBnClickedRandomize)
		COMMAND_HANDLER(IDC_RADIO_Manual, BN_CLICKED, OnBnClickedRadioManual)
		COMMAND_HANDLER(IDC_RADIO_Randomize, BN_CLICKED, OnBnClickedRadioRandomize)
	END_MSG_MAP()

	LRESULT OnInitDialog(UINT, WPARAM, LPARAM, BOOL&);
	LRESULT OnOK(WORD, WORD wID, HWND, BOOL&);
	LRESULT OnCancel(WORD, WORD wID, HWND, BOOL&);

	void CloseDialog(int nVal);
	LRESULT OnEnChangeXor(WORD, WORD, HWND , BOOL&);
	LRESULT OnBnClickedOk(WORD, WORD, HWND, BOOL&);
	LRESULT OnBnClickedEncrypt(WORD, WORD, HWND, BOOL&);
	LRESULT OnBnClickedDecrypt(WORD, WORD, HWND, BOOL&);
	LRESULT OnBnClickedButtonSource(WORD, WORD, HWND, BOOL&);
	LRESULT OnBnClickedButtonDescramble(WORD, WORD, HWND, BOOL&);
	LRESULT OnBnClickedRadioSource(WORD, WORD, HWND, BOOL&);
	LRESULT OnBnClickedRadioDescramble(WORD, WORD, HWND, BOOL&);
	LRESULT OnBnClickedClear(WORD, WORD, HWND, BOOL&);
	LRESULT OnBnClickedRandomize(WORD, WORD, HWND, BOOL& );
	LRESULT OnBnClickedRadioManual(WORD, WORD, HWND, BOOL&);
	LRESULT OnBnClickedRadioRandomize(WORD, WORD, HWND, BOOL&);

private:
	CButton m_Encrypt, m_Decrypt, m_RXOR, m_XOR, m_ViewSource, m_ViewDescramble, m_ButtonEncrypt, m_CopySource, m_CopyDescramble,
		m_Manual, m_Randomize, m_Line, m_Macro, m_Function;
	CharArrayList<TCHAR> code, descramble;
	wchar_t m_XORByte;
	CEdit m_xorBox, m_inputBox;

	TCHAR* szInputString;
	bool isUnicode(CharArrayList<TCHAR>&, bool);
	TCHAR* determineVarName(const TCHAR*);
	void addDescramble( farbleType, TCHAR*, unsigned int, bool, TCHAR, bool );
	TCHAR convertEscapeSequence(TCHAR);
	void initDescramble(farbleType, bool);
};