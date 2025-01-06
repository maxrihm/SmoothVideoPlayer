using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using SmoothVideoPlayer.Services.AddWord;
using SmoothVideoPlayer.Services.Translator;
using SmoothVideoPlayer.Services.OverlayManager;
using SmoothVideoPlayer.ViewModels;
using SmoothVideoPlayer.Services.TimeJumpFeature;

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

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vKey);

        delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

        const int WH_KEYBOARD_LL = 13;
        const int WM_KEYDOWN = 0x0100;
        const int WM_SYSKEYDOWN = 0x0104;
        const int VK_CAPSLOCK = 0x14;

        readonly MainViewModel mainViewModel;
        readonly ITranslatorOverlayService translatorOverlayService;
        readonly IAddWordOverlayService addWordOverlayService;
        readonly IOverlayManager overlayManager;
        readonly ITimeJumpService timeJumpService;
        IntPtr hookId;
        LowLevelKeyboardProc keyboardProc;

        public GlobalHotkeyService(
            MainViewModel mainViewModel,
            ITranslatorOverlayService translatorOverlayService,
            IAddWordOverlayService addWordOverlayService,
            IOverlayManager overlayManager)
        {
            this.mainViewModel = mainViewModel;
            this.translatorOverlayService = translatorOverlayService;
            this.addWordOverlayService = addWordOverlayService;
            this.overlayManager = overlayManager;
            timeJumpService = new TimeJumpService(mainViewModel.MediaService);
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

        bool IsKeyDown(int vk)
        {
            return (GetAsyncKeyState(vk) & 0x8000) != 0;
        }

        IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                int msg = wParam.ToInt32();
                if (msg == WM_KEYDOWN || msg == WM_SYSKEYDOWN)
                {
                    int vkCode = Marshal.ReadInt32(lParam);
                    if (vkCode == VK_CAPSLOCK)
                    {
                        translatorOverlayService.ToggleOverlay();
                        return (IntPtr)1;
                    }
                    if (!overlayManager.AreHotkeysEnabled) return CallNextHookEx(hookId, nCode, wParam, lParam);
                    if (vkCode == (int)Keys.Space || vkCode == (int)Keys.W)
                    {
                        mainViewModel.TogglePlayPause();
                    }
                    else if (vkCode == (int)Keys.A)
                    {
                        if (IsKeyDown(0x10)) timeJumpService.JumpSeconds(-10);
                        else if (IsKeyDown(0x11)) timeJumpService.JumpSeconds(-30);
                        else
                        {
                            if (mainViewModel.JumpToPreviousSubtitleCommand.CanExecute(null))
                            {
                                mainViewModel.JumpToPreviousSubtitleCommand.Execute(null);
                            }
                        }
                    }
                    else if (vkCode == (int)Keys.D)
                    {
                        if (IsKeyDown(0x10)) timeJumpService.JumpSeconds(10);
                        else if (IsKeyDown(0x11)) timeJumpService.JumpSeconds(30);
                        else
                        {
                            if (mainViewModel.JumpToNextSubtitleCommand.CanExecute(null))
                            {
                                mainViewModel.JumpToNextSubtitleCommand.Execute(null);
                            }
                        }
                    }
                }
            }
            return CallNextHookEx(hookId, nCode, wParam, lParam);
        }
    }
}
