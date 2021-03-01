using System;
using System.Windows;
using System.Diagnostics;
using System.Runtime.InteropServices;
using PlatformInvoke;
using System.Collections.Generic;

namespace LeftCraft2
{
	/// <summary>
	/// Haven't spent any time trying to make this be more general than it needs to be for me.
	/// If I end up needing more stuff I might spend that time down the line.
	/// </summary>
	public partial class MainWindow : Window
    {
        private static User32.LowLevelKeyboardProc _proc;
        private static IntPtr _hookID = IntPtr.Zero;
        private static readonly List<VirtualKeyShort> _newShiftKeys = new List<VirtualKeyShort>() 
		{ 
			VirtualKeyShort.NONCONVERT,
			VirtualKeyShort.CONVERT
		};

        public MainWindow()
		{
			InitializeComponent();

			_proc = HookCallback;
			_hookID = SetHook(_proc);
		}
		
		protected override void OnClosed(EventArgs e)
        {
			User32.UnhookWindowsHookEx(_hookID);
            base.OnClosed(e);
        }

        private static IntPtr SetHook(User32.LowLevelKeyboardProc proc)
		{
            using Process curProcess = Process.GetCurrentProcess();
            using ProcessModule curModule = curProcess.MainModule;
            return User32.SetWindowsHookEx((int)PlatformFlags.WH_KEYBOARD_LL, proc, Kernel32.GetModuleHandle(curModule.ModuleName), 0);
        }

		private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (nCode >= 0)
			{
				VirtualKeyShort newKey = (VirtualKeyShort)Marshal.ReadInt32(lParam);
				if(_newShiftKeys.Contains(newKey))
                {
                    var input = new INPUT();
                    input.type = (uint)InputType.INPUT_KEYBOARD;
                    input.U.ki = new KEYBDINPUT()
                    {
                        wScan = ScanCodeShort.LSHIFT,
                        wVk = VirtualKeyShort.LSHIFT
                    };

                    bool wasPressedDown = (wParam == (IntPtr)PlatformFlags.WM_KEYDOWN);
                    if (!wasPressedDown)
                    {
                        input.U.ki.dwFlags = KEYEVENTF.KEYUP;
                    }

                    var pInputs = new[] { input };
                    User32.SendInput((uint)pInputs.Length, pInputs, INPUT.Size);

					return IntPtr.Zero;
				}
			}

			return User32.CallNextHookEx(_hookID, nCode, wParam, lParam);
		}
	}
}
