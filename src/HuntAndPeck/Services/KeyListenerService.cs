using HuntAndPeck.NativeMethods;
using System;
using System.Windows.Forms;
using HuntAndPeck.Services.Interfaces;
using System.Collections.Generic;

namespace HuntAndPeck.Services
{
    internal class KeyListenerService : Form, IDisposable
    {
        /// <summary>
        /// Global counter for assigning ids to identiy the hot key registration
        /// </summary>
        private int _hotkeyIdCounter = 0;

        private Dictionary<uint, Dictionary<uint, (int registrationId, Action action)>> hotkeyActions = new Dictionary<uint, Dictionary<uint, (int registrationId, Action action)>>();

        public void RegisterKey(HotKey hotKey, Action hotkeyAction)
        {
            var hk = this.GetSavedHotKey(hotKey.Keys, hotKey.Modifier);

            if (hk != null)
            {
                return;
            }

            if (hotKey.RegistrationId > 0)
            {
                User32.UnregisterHotKey(Handle, hotKey.RegistrationId);
            }

            hotKey.RegistrationId = _hotkeyIdCounter++;
            User32.RegisterHotKey(Handle, hotKey.RegistrationId, (uint)hotKey.Modifier, (uint)hotKey.Keys);

            Dictionary<uint, (int registrationId, Action action)> modifierPart;
            if (this.hotkeyActions.TryGetValue((uint)hotKey.Keys, out modifierPart))
            {
                modifierPart.Add((uint)hotKey.Modifier, (hotKey.RegistrationId, hotkeyAction));
            }
            else
            {
                modifierPart = new Dictionary<uint, (int registrationId, Action action)>();
                modifierPart.Add((uint)hotKey.Modifier, (hotKey.RegistrationId, hotkeyAction));
                this.hotkeyActions.Add((uint)hotKey.Keys, modifierPart);
            }
        }

        public void UnregisterKey(HotKey hotKey)
        {
            var hk = this.GetSavedHotKey(hotKey.Keys, hotKey.Modifier);

            if (hk != null)
            {
                this.hotkeyActions[(uint)hotKey.Keys].Remove((uint)hotKey.Modifier);
                User32.UnregisterHotKey(Handle, hotKey.RegistrationId);
            }
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == Constants.WM_HOTKEY)
            {
                var e = new HotKeyEventArgs(m.LParam);
                var hk = this.GetSavedHotKey(e.Key, e.Modifiers);

                hk?.action();
            }

            base.WndProc(ref m);
        }

        private (int registrationId, Action action)? GetSavedHotKey(Keys key, KeyModifier modifiers)
        {
            Dictionary<uint, (int, Action)> modifierPart;
            if (this.hotkeyActions.TryGetValue((uint)key, out modifierPart))
            {
                (int registrationId, Action action) registrationIdActionPair;
                if (modifierPart.TryGetValue((uint)modifiers, out registrationIdActionPair))
                {
                    return registrationIdActionPair;
                }
            }

            return null;
        }

        protected override void SetVisibleCore(bool value)
        {
            // Ensures that the window will never be displayed
            base.SetVisibleCore(false);
        }
    }
}
