#include <windows.h>
#include <commctrl.h>
#include <richedit.h>
#include <string>

LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam);

HINSTANCE hInst;
HWND hRichEdit;

// TODO: add support for backColor and textColor as params
void ColorWord(const std::wstring& word, COLORREF color) {
	FINDTEXTEXW ft;
	ft.chrg.cpMin = 0;
	ft.chrg.cpMax = -1;
	ft.lpstrText = const_cast<wchar_t*>(word.c_str());
	int pos = 0;
	while ((pos = (int)SendMessageW(hRichEdit, EM_FINDTEXTEX, FR_DOWN | FR_MATCHCASE | FR_WHOLEWORD, (LPARAM)&ft)) != -1) {
		SendMessageW(hRichEdit, EM_SETSEL, pos, pos + (int)word.length());
		CHARFORMAT2W cf;
		ZeroMemory(&cf, sizeof(cf));
		cf.cbSize = sizeof(cf);
		//cf.dwMask = CFM_BACKCOLOR;
		//cf.crBackColor = color;
		cf.dwMask = CFM_COLOR;
		cf.crTextColor = color;
		SendMessageW(hRichEdit, EM_SETCHARFORMAT, SCF_SELECTION, (LPARAM)&cf);
		ft.chrg.cpMin = pos + (int)word.length();
	}
}

void ResetToDefaultColorText() {
	SendMessageW(hRichEdit, EM_SETSEL, -1, -1);
	CHARFORMAT2W cf;
	ZeroMemory(&cf, sizeof(cf));
	cf.cbSize = sizeof(cf);
	//cf.dwMask = CFM_BACKCOLOR;
	//cf.crBackColor = RGB(1, 1, 1);
	cf.dwMask = CFM_COLOR;
	cf.crTextColor = RGB(0, 0, 0);
	SendMessageW(hRichEdit, EM_SETCHARFORMAT, SCF_SELECTION, (LPARAM)&cf);
}

void ResetAndColorText() {
	// Save current selection
	CHARRANGE cr;
	SendMessageW(hRichEdit, EM_GETSEL, (WPARAM)&cr.cpMin, (LPARAM)&cr.cpMax);

	// Hide selection to prevent flicker
	SendMessageW(hRichEdit, EM_HIDESELECTION, TRUE, 0);

	// Reset all text to default color (black)
	SendMessageW(hRichEdit, EM_SETSEL, 0, -1);
	CHARFORMAT2W cf;
	ZeroMemory(&cf, sizeof(cf));
	cf.cbSize = sizeof(cf);
	//cf.dwMask = CFM_BACKCOLOR;
	//cf.crBackColor = RGB(1, 1, 1);
	cf.dwMask = CFM_COLOR;
	cf.crTextColor = RGB(0, 0, 0);
	SendMessageW(hRichEdit, EM_SETCHARFORMAT, SCF_SELECTION, (LPARAM)&cf);

	// Apply keyword coloring
	ColorWord(L"[USERS]", RGB(255, 165, 0)); // Orange
	ColorWord(L"[DISABLED]", RGB(255, 165, 0));
	ColorWord(L"[BUGS]", RGB(255, 0, 0)); // Red
	ColorWord(L"[EXCLUDE]", RGB(255, 0, 0));
	ColorWord(L"[TASKS]", RGB(128, 0, 128)); // Purple
	ColorWord(L"[TODOS]", RGB(128, 0, 128));

	ResetToDefaultColorText();

	// Restore selection
	SendMessageW(hRichEdit, EM_SETSEL, cr.cpMin, cr.cpMax);
	SendMessageW(hRichEdit, EM_HIDESELECTION, FALSE, 0);
}

int WINAPI wWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, PWSTR lpCmdLine, int nCmdShow) {
	hInst = hInstance;
	LoadLibrary(TEXT("Msftedit.dll"));

	WNDCLASSW wc = { 0 };
	wc.style = CS_HREDRAW | CS_VREDRAW;
	wc.lpfnWndProc = WndProc;
	wc.hInstance = hInstance;
	wc.hbrBackground = (HBRUSH)GetStockObject(WHITE_BRUSH);
	wc.lpszClassName = L"WinClass";
	RegisterClassW(&wc);

	HWND hwnd = CreateWindowExW(0, L"WinClass", L"RichEdit Example", WS_OVERLAPPEDWINDOW, CW_USEDEFAULT, CW_USEDEFAULT, 600, 400, NULL, NULL, hInstance, NULL);
	ShowWindow(hwnd, nCmdShow);
	UpdateWindow(hwnd);

	MSG msg;
	while (GetMessageW(&msg, NULL, 0, 0)) {
		TranslateMessage(&msg);
		DispatchMessageW(&msg);
	}
	return (int)msg.wParam;
}

LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam) {
	switch (msg) {
		case WM_CREATE: {
			auto flags = WS_CHILD | WS_VISIBLE | WS_VSCROLL | ES_MULTILINE | ES_AUTOVSCROLL;
			hRichEdit = CreateWindowExW(0, MSFTEDIT_CLASS, NULL, flags, 0, 0, 0, 0, hwnd, NULL, hInst, NULL);
			SendMessageW(hRichEdit, EM_SETEVENTMASK, 0, ENM_CHANGE | ENM_UPDATE);
			SendMessageW(hRichEdit, EM_SETTEXTMODE, TM_RICHTEXT, 0);
			//SendMessageW(hRichEdit, EM_SETUNDOLIMIT, 100, 0); // Enable undo // TODO: do this later maybe
			SetWindowTextW(hRichEdit, L"[USERS] [DISABLED] [BUGS] [EXCLUDE] [TASKS] [TODOS]"); // TODO: this is temp text that needs to be removed
			ResetAndColorText(); // Initial coloring
			break;
		}
		case WM_SIZE: {
			MoveWindow(hRichEdit, 0, 0, LOWORD(lparam), HIWORD(lparam), TRUE);
			break;
		}
		case WM_COMMAND: {
			const int controlId = LOWORD(wparam);
			const int notifyCode = HIWORD(wparam);
			HWND from = (HWND)lparam;
			if (from == hRichEdit && notifyCode == EN_CHANGE) {
				ResetAndColorText();
			} 
			else if (from == hRichEdit) {
				wchar_t buffer[256];
				wsprintfW(buffer, L"RichEdit WM_COMMAND, notifyCode: 0x%X\n", notifyCode);
				OutputDebugStringW(buffer);
			}
			break;
		}
		case WM_DESTROY: {
			PostQuitMessage(0);
			break;
		}
		default: {
			return DefWindowProcW(hwnd, msg, wparam, lparam);
		}
	}
	return 0;
}