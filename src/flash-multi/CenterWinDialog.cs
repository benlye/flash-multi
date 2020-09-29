// -------------------------------------------------------------------------------
// <copyright file="CenterWinDialog.cs" company="Ben Lye">
// Copyright 2020 Ben Lye
//
// This file is part of Flash Multi.
//
// Flash Multi is free software: you can redistribute it and/or modify it under
// the terms of the GNU General Public License as published by the Free Software
// Foundation, either version 3 of the License, or(at your option) any later
// version.
//
// Flash Multi is distributed in the hope that it will be useful, but WITHOUT
// ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
// FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License along with
// Flash Multi. If not, see http://www.gnu.org/licenses/.
// </copyright>
// -------------------------------------------------------------------------------

namespace Flash_Multi
{
    using System;
    using System.Drawing;
    using System.Runtime.InteropServices;
    using System.Text;
    using System.Windows.Forms;

    /// <summary>
    /// Shows a messagebox centered on the parent window.
    /// </summary>
    internal class CenterWinDialog : IDisposable
    {
        private int mTries = 0;
        private Form mOwner;

        /// <summary>
        /// Initializes a new instance of the <see cref="CenterWinDialog"/> class.
        /// </summary>
        /// <param name="owner">The form to center the messagebox over.</param>
        public CenterWinDialog(Form owner)
        {
            this.mOwner = owner;
            owner.BeginInvoke(new MethodInvoker(this.FindDialog));
        }

        // P/Invoke declarations
        private delegate bool EnumThreadWndProc(IntPtr hWnd, IntPtr lp);

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            this.mTries = -1;
        }

        [DllImport("user32.dll")]
        private static extern bool EnumThreadWindows(int tid, EnumThreadWndProc callback, IntPtr lp);

        [DllImport("kernel32.dll")]
        private static extern int GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder buffer, int buflen);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT rc);

        [DllImport("user32.dll")]
        private static extern bool MoveWindow(IntPtr hWnd, int x, int y, int w, int h, bool repaint);

        private void FindDialog()
        {
            // Enumerate windows to find the message box
            if (this.mTries < 0)
            {
                return;
            }

            EnumThreadWndProc callback = new EnumThreadWndProc(this.CheckWindow);
            if (EnumThreadWindows(GetCurrentThreadId(), callback, IntPtr.Zero))
            {
                if (++this.mTries < 10)
                {
                    this.mOwner.BeginInvoke(new MethodInvoker(this.FindDialog));
                }
            }
        }

        private bool CheckWindow(IntPtr hWnd, IntPtr lp)
        {
            // Checks if <hWnd> is a dialog
            StringBuilder sb = new StringBuilder(260);
            GetClassName(hWnd, sb, sb.Capacity);
            if (sb.ToString() != "#32770")
            {
                return true;
            }

            // Got it
            Rectangle frmRect = new Rectangle(this.mOwner.Location, this.mOwner.Size);
            RECT dlgRect;
            GetWindowRect(hWnd, out dlgRect);
            MoveWindow(
                hWnd,
                frmRect.Left + ((frmRect.Width - dlgRect.Right + dlgRect.Left) / 2),
                frmRect.Top + ((frmRect.Height - dlgRect.Bottom + dlgRect.Top) / 2),
                dlgRect.Right - dlgRect.Left,
                dlgRect.Bottom - dlgRect.Top,
                true);
            return false;
        }

        private struct RECT
        {
            public int Left; public int Top; public int Right; public int Bottom;
        }
    }
}
