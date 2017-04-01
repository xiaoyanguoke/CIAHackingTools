/*
 * Filename:		MarbleTester.cpp
 *
 * Classification:	SECRET//NOFORN
 * Classified By:	
 *
 * Tool Name:		MarbleTester
 * Requirement #:	2015-XXXX
 * Customer:		EDG
 *
 * Author:			???
 * Date Created:	04/07/2015
 * Version 1.0:		04/07/2015 (???)
 *
 * Intended to test the Marble Framework
 *
 *
 */

#pragma warning(disable : 4309)

#include <Windows.h>
#include <stdio.h>
#include <stdlib.h>
#include "Marble.h"

#include "ASCII.h" 
#include "Unicode.h"
#include "UTF8.h"

#include "MD5.h"

int wmain(int argc, wchar_t* argv[])
{
	LPBYTE lpbRes = NULL;
	DWORD dwResLen = 0;

	//----------------------------------------------------
	//Call Ascii File
	AsciiStrings(lpbRes, dwResLen);
	if (lpbRes == NULL || dwResLen == 0)
	{
		OutputDebugString(L"Failed to unscramble ascii strings");
		if (lpbRes)
			free(lpbRes);
		goto failed;
	}
	

	//Validate MD5
	BYTE byCorrectAsciiMD5[] = { 0x5A, 0x9b, 0x60, 0x02, 0xc8, 0xbc, 0xef, 0xde, 0x57, 0x5d, 0x88, 0x2e, 0xd2, 0x60, 0xd0, 0x22 };
	BYTE byObtainedMD5[16] = { 0 };
	ComputeMD5(lpbRes, dwResLen, byObtainedMD5);
	if (memcmp(byCorrectAsciiMD5, byObtainedMD5, 16) != 0)
	{
		OutputDebugString(L"Failed to match ascii strings");
		free(lpbRes);
		goto failed;
	}
	
	//Cleanup variables for reuse
	free(lpbRes);
	lpbRes = NULL;
	dwResLen = 0;
	 
	//----------------------------------------------------
	//Call Unicode File
	UnicodeStrings(lpbRes, dwResLen);
	if (lpbRes == NULL || dwResLen == 0)
	{
		OutputDebugString(L"Failed to unscramble Unicode strings");
		if (lpbRes)
			free(lpbRes);
		goto failed;
	}

	//Validate MD5
	BYTE byCorrectUnicodeMD5[] = { 0xea, 0x28, 0xeb, 0xad, 0x1b, 0x7e, 0x4e, 0xa2, 0xdd, 0x26, 0x86, 0x44, 0x77, 0xa2, 0x3f, 0xfa };
	SecureZeroMemory(byObtainedMD5, sizeof(byObtainedMD5));
	ComputeMD5(lpbRes, dwResLen, byObtainedMD5);
	if (memcmp(byCorrectUnicodeMD5, byObtainedMD5, 16) != 0)
	{
		OutputDebugString(L"Failed to match Unicode strings");
		free(lpbRes);
		goto failed;
	}

	//Cleanup variables for reuse
	free(lpbRes);
	lpbRes = NULL;
	dwResLen = 0;

	//----------------------------------------------------
	//Call UTF-8 Source File
	UTF8Strings(lpbRes, dwResLen);
	if (lpbRes == NULL || dwResLen == 0)
	{
		OutputDebugString(L"Failed to unscramble UTF-8 strings");
		if (lpbRes)
			free(lpbRes);
		goto failed;
	}

	//Validate MD5
	BYTE byCorrectUTF8MD5[] = { 0xea, 0x28, 0xeb, 0xad, 0x1b, 0x7e, 0x4e, 0xa2, 0xdd, 0x26, 0x86, 0x44, 0x77, 0xa2, 0x3f, 0xfa };
	SecureZeroMemory(byObtainedMD5, sizeof(byObtainedMD5));
	ComputeMD5(lpbRes, dwResLen, byObtainedMD5);
	if (memcmp(byCorrectUTF8MD5, byObtainedMD5, 16) != 0)
	{
		OutputDebugString(L"Failed to match UTF-8 strings");
		free(lpbRes);
		goto failed;
	}

	//Cleanup variables for reuse
	free(lpbRes);
	lpbRes = NULL;
	dwResLen = 0;

	OutputDebugString(L"Success!!");
	printf("Success!!\n\n");
	return 0;

failed:
	return -1;
}
