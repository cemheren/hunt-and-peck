using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using HuntAndPeck.Extensions;
using HuntAndPeck.NativeMethods;
using HuntAndPeck.Services;
using HuntAndPeck.Services.Interfaces;
using Application = System.Windows.Application;

namespace HuntAndPeck.ViewModels
{
    internal class ShellViewModel
    {
        private readonly Action<OverlayViewModel> _showOverlay;
        private readonly Action<DebugOverlayViewModel> _showDebugOverlay;
        private readonly Action<OptionsViewModel> _showOptions;
        private readonly IHintLabelService _hintLabelService;
        private readonly IHintProviderService _hintProviderService;
        private readonly IDebugHintProviderService _debugHintProviderService;

        public KeyListenerService keyListener { get; }

        private bool insertMode;

        private List<OverlayViewModel> currentOverlays = new List<OverlayViewModel>();

        private HotKey f_key = new HotKey
        {
            Keys = Keys.F
        };

        private HotKey i_key = new HotKey
        {
            Keys = Keys.I
        };

        private HotKey esc_key = new HotKey
        {
            Keys = Keys.Escape
        };

        private HotKey j_key = new HotKey
        {
            Keys = Keys.J
        };

        private HotKey k_key = new HotKey
        {
            Keys = Keys.K
        };

        private HotKey quit_key = new HotKey
        {
            Keys = Keys.Q,
            Modifier = KeyModifier.Control
        };

        private HotKey right_win = new HotKey
        {
            Keys = Keys.R,
            Modifier = KeyModifier.Alt
        };
        
        public ShellViewModel(
            Action<OverlayViewModel> showOverlay,
            Action<DebugOverlayViewModel> showDebugOverlay,
            Action<OptionsViewModel> showOptions,
            IHintLabelService hintLabelService,
            IHintProviderService hintProviderService,
            IDebugHintProviderService debugHintProviderService,
            KeyListenerService keyListener)
        {
            _showOverlay = showOverlay;
            _showDebugOverlay = showDebugOverlay;
            _showOptions = showOptions;
            _hintLabelService = hintLabelService;
            _hintProviderService = hintProviderService;
            _debugHintProviderService = debugHintProviderService;

            this.keyListener = keyListener;
            keyListener.RegisterKey(right_win, MouseRightClick);

            keyListener.RegisterKey(f_key, f_keyActivated);
            keyListener.RegisterKey(j_key, j_keyActivated);
            keyListener.RegisterKey(k_key, k_keyActivated);

            keyListener.RegisterKey(i_key, i_keyActivated);

            keyListener.RegisterKey(esc_key, esc_KeyActivated);

            keyListener.RegisterKey(quit_key, Exit);

            ShowOptionsCommand = new DelegateCommand(ShowOptions);
            ExitCommand = new DelegateCommand(Exit);
        }

        public DelegateCommand ShowOptionsCommand { get; }
        public DelegateCommand ExitCommand { get; }

        private void f_keyActivated()
        {
            keyListener.RegisterKey(esc_key, esc_KeyActivated);
            var foregroundWindow = User32.GetForegroundWindow();

            if (foregroundWindow != IntPtr.Zero)
            {
                var bound = foregroundWindow.GetWindowBounds();

                foreach(var chunk in _hintProviderService.EnumHints(foregroundWindow).Chunk(800))
                {
                    var vm = new OverlayViewModel(chunk, bound, foregroundWindow, _hintLabelService, _hintProviderService);
                    currentOverlays.Add(vm);
                   _showOverlay(vm);
                }
            }
        }

        private void esc_KeyActivated()
        {
            keyListener.UnregisterKey(esc_key);
            keyListener.RegisterKey(f_key, f_keyActivated);
            keyListener.RegisterKey(i_key, i_keyActivated);
            keyListener.RegisterKey(j_key, j_keyActivated);
            keyListener.RegisterKey(k_key, k_keyActivated);

            if (this.currentOverlays.Any())
            {
                foreach (var overlay in this.currentOverlays)
                {
                    overlay.CloseOverlay();
                }

                this.currentOverlays.Clear();
                return;
            }

            if (this.insertMode)
            {
                this.insertMode = false;
                keyListener.RegisterKey(f_key, f_keyActivated);
                keyListener.RegisterKey(i_key, i_keyActivated);
                keyListener.RegisterKey(j_key, j_keyActivated);
                keyListener.RegisterKey(k_key, k_keyActivated);

                return;
            }
        }

        private void i_keyActivated()
        {
            this.insertMode = true;

            this.keyListener.UnregisterKey(f_key);
            this.keyListener.UnregisterKey(i_key);
            this.keyListener.UnregisterKey(j_key);
            this.keyListener.UnregisterKey(k_key);

            keyListener.RegisterKey(esc_key, esc_KeyActivated);
        }

        private void j_keyActivated()
        {
            MouseInput.ScrollWheel(-6);
        }

        private void k_keyActivated()
        {
            MouseInput.ScrollWheel(6);
        }

        private void MouseRightClick()
        {
            MouseInput.RightClick();
        }

        public void Exit()
        {
            Application.Current.Shutdown();
        }

        public void ShowOptions()
        {
            var vm = new OptionsViewModel();
            _showOptions(vm);
        }
    }
}
