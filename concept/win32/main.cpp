#include <windows.h>
#include <commctrl.h>
#include <richedit.h>
#include <string>

LRESULT CALLBACK WndProc(HWND hwnd, UINT msg, WPARAM wparam, LPARAM lparam);

HINSTANCE hInst;
HWND hRichEdit;

void ColorWord(const std::wstring& word, COLORREF color) {
	FINDTEXTEXW ft;
	ft.chrg.cpMin = 0;
	ft.chrg.cpMax = -1;
	ft.lpstrText = const_cast<wchar_t*>(word.c_str());
	int pos = 0;
	while ((pos = (int)SendMessageW(hRichEdit, EM_FINDTEXTEX, FR_DOWN, (LPARAM)&ft)) != -1) {
		SendMessageW(hRichEdit, EM_SETSEL, pos, pos + (int)word.length());
		CHARFORMAT2W cf;
		ZeroMemory(&cf, sizeof(cf));
		cf.cbSize = sizeof(cf);
		cf.dwMask = CFM_COLOR;
		cf.crTextColor = color;
		SendMessageW(hRichEdit, EM_SETCHARFORMAT, SCF_SELECTION, (LPARAM)&cf);
		ft.chrg.cpMin = pos + (int)word.length();
	}
}

int WINAPI wWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, PWSTR lpCmdLine, int nCmdShow) {
	hInst = hInstance;
	LoadLibraryW(L"Msftedit.dll"); // For RICHEDIT50W

	WNDCLASSW wc = { 0 };
	wc.style = CS_HREDRAW | CS_VREDRAW;
	wc.lpfnWndProc = WndProc;
	wc.hInstance = hInstance;
	wc.hbrBackground = (HBRUSH)GetStockObject(WHITE_BRUSH); // Fixed cast
	wc.lpszClassName = L"WinClass";
	RegisterClassW(&wc);

	HWND hwnd = CreateWindowExW(0, L"WinClass", L"RichEdit Example", WS_OVERLAPPEDWINDOW,
								CW_USEDEFAULT, CW_USEDEFAULT, 600, 400, NULL, NULL, hInstance, NULL);
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
			hRichEdit = CreateWindowExW(0, MSFTEDIT_CLASS, NULL, WS_CHILD | WS_VISIBLE | WS_VSCROLL | ES_MULTILINE | ES_AUTOVSCROLL,
										0, 0, 0, 0, hwnd, NULL, hInst, NULL);
			SendMessageW(hRichEdit, EM_SETTEXTMODE, TM_RICHTEXT, 0);
			SetWindowTextW(hRichEdit, L"[USERS] [DISABLED] [BUGS] [EXCLUDE] [TASKS] [TODOS]");
			break;
		}
		case WM_SIZE: {
			MoveWindow(hRichEdit, 0, 0, LOWORD(lparam), HIWORD(lparam), TRUE);
			break;
		}
		case WM_PAINT: {
			static bool colored = false;
			if (!colored) {
				colored = true;
				ColorWord(L"[USERS]", RGB(255, 165, 0)); // Orange
				ColorWord(L"[DISABLED]", RGB(255, 165, 0));
				ColorWord(L"[BUGS]", RGB(255, 0, 0)); // Red
				ColorWord(L"[EXCLUDE]", RGB(255, 0, 0));
				ColorWord(L"[TASKS]", RGB(128, 0, 128)); // Purple
				ColorWord(L"[TODOS]", RGB(128, 0, 128));
			}
			break;
		}
		case WM_DESTROY: {
			PostQuitMessage(0);
			break;
		}
		default:
			return DefWindowProcW(hwnd, msg, wparam, lparam);
	}
	return 0;
}