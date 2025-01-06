using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SmoothVideoPlayer.Services.AddWord;
using SmoothVideoPlayer.Services.Translator;
using SmoothVideoPlayer.ViewModels;

namespace SmoothVideoPlayer.Services.GlobalHotkeys
{
    public class GlobalHotkeyService : IGlobalHotkeyService
    {
        [DllImport("user32.dll")]
        static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll")]
        static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        const int WH_KEYBOARD_LL = 13;
        const int WM_KEYDOWN = 0x0100;
        const int WM_SYSKEYDOWN = 0x0104;
        const int VK_CAPSLOCK = 0x14;

        readonly MainViewModel mainViewModel;
        readonly ITranslatorOverlayService translatorOverlayService;
        readonly IAddWordOverlayService addWordOverlayService;

        bool hotkeysEnabled = true;
        IntPtr hookId;
        LowLevelKeyboardProc keyboardProc;

        public GlobalHotkeyService(
            MainViewModel mainViewModel,
            ITranslatorOverlayService translatorOverlayService,
            IAddWordOverlayService addWordOverlayService)
        {
            this.mainViewModel = mainViewModel;
            this.translatorOverlayService = translatorOverlayService;
            this.addWordOverlayService = addWordOverlayService;
        }

        public void Initialize()
        {
            keyboardProc = HookCallback;
            hookId = SetWindowsHookEx(WH_KEYBOARD_LL, keyboardProc, IntPtr.Zero, 0);
        }

        public void Dispose()
        {
            if (hookId != IntPtr.Zero) UnhookWindowsHookEx(hookId);
        }

        IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int msg = wParam.ToInt32();
                if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN)
                {
                    int vkCode = Marshal.ReadInt32(lParam);

                    // Completely consume CapsLock so it never toggles system CapsLock
                    if (vkCode == VK_CAPSLOCK)
                    {
                        translatorOverlayService.ToggleOverlay();
                        hotkeysEnabled = !(translatorOverlayService.IsOverlayOpen || addWordOverlayService.IsOverlayOpen);
                        // Return any nonzero to consume the event, preventing Windows from toggling CapsLock
                        return (IntPtr)1;
                    }

                    if (!hotkeysEnabled || translatorOverlayService.IsOverlayOpen || addWordOverlayService.IsOverlayOpen)
                    {
                        return CallNextHookEx(hookId, nCode, wParam, lParam);
                    }

                    if (vkCode == (int)Keys.Space || vkCode == (int)Keys.W)
                    {
                        mainViewModel.TogglePlayPause();
                    }
                    else if (vkCode == (int)Keys.A)
                    {
                        if (mainViewModel.JumpToPreviousSubtitleCommand.CanExecute(null))
                        {
                            mainViewModel.JumpToPreviousSubtitleCommand.Execute(null);
                        }
                    }
                    else if (vkCode == (int)Keys.D)
                    {
                        if (mainViewModel.JumpToNextSubtitleCommand.CanExecute(null))
                        {
                            mainViewModel.JumpToNextSubtitleCommand.Execute(null);
                        }
                    }
                }
            }
            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }
    }
}
