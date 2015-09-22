using System.IO;
using System.Runtime.InteropServices;
using FalloutSnip.Domain.Data;
using FalloutSnip.Properties;
using FalloutSnip.UI.Docking;
using FalloutSnip.UI.Services;
using WeifenLuo.WinFormsUI.Docking;

namespace FalloutSnip.UI.Forms
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Linq;
    using System.Media;
    using System.Windows.Forms;

    using Domain.Data.Structure;
    using FalloutSnip.Domain.Model;
    using FalloutSnip.Framework;

    using Settings = FalloutSnip.Properties.Settings;

    /// <summary>
    /// This file contains the incremental search related functionality for the main form.
    /// </summary>
    internal partial class MainView
    {
        private const int WM_SETREDRAW = 0x0b;


        #region MessageFilter

        public class MainViewMessageFilter : IMessageFilter
        {
            public const int WM_CHAR = 0x102;

            public const int WM_KEYDOWN = 0x100;

            public const int WM_KEYUP = 0x101;

            private const ushort KEY_PRESSED = 0x8000;

            private readonly MainView owner;

            public MainViewMessageFilter(MainView owner)
            {
                this.owner = owner;
            }

            public bool PreFilterMessage(ref Message m)
            {
                try
                {
                    return owner.PreFilterMessage(ref m);
                }
                catch
                {
                }

                return true;
            }

            [DllImport("user32.dll")]
            public static extern ushort GetAsyncKeyState(VirtualKeyStates nVirtKey);

            [DllImport("user32.dll")]
            public static extern ushort GetKeyState(VirtualKeyStates nVirtKey);

            public static bool IsAltDown()
            {
                return 1 == GetKeyState(VirtualKeyStates.VK_MENU);
            }

            public static bool IsControlDown()
            {
                return 1 == GetKeyState(VirtualKeyStates.VK_CONTROL);
            }

            public static bool IsShiftDown()
            {
                return 1 == GetKeyState(VirtualKeyStates.VK_SHIFT);
            }

            internal enum VirtualKeyStates
            {
                VK_LBUTTON = 0x01,
                VK_RBUTTON = 0x02,
                VK_CANCEL = 0x03,
                VK_MBUTTON = 0x04,
                VK_LSHIFT = 0xA0,
                VK_RSHIFT = 0xA1,
                VK_LCONTROL = 0xA2,
                VK_RCONTROL = 0xA3,
                VK_LMENU = 0xA4,
                VK_RMENU = 0xA5,
                VK_LEFT = 0x25,
                VK_UP = 0x26,
                VK_RIGHT = 0x27,
                VK_DOWN = 0x28,
                VK_SHIFT = 0x10,
                VK_CONTROL = 0x11,
                VK_MENU = 0x12,
            }
        }

        #endregion

    }
}
