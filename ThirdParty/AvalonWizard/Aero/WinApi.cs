#region License
// Copyright © Pavel Fedarovich, 2010-2012
// 
// This file is part of AvalonWizard.
//  
// You may at your option receive a license to Avalon Wizard under 
// either the terms of the Apache License version 2.0 or 
// the GNU Lesser General Public License (LGPL) version 2.1 or any later version.
//  
// AvalonWizard is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
//  
// You may obtain a copy of the Apache License at [http://www.apache.org/licenses/LICENSE-2.0].
// You may obtain a copy of the LGPL at [http://www.gnu.org/licenses/].
#endregion

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace AvalonWizard.Aero
{
    [SuppressUnmanagedCodeSecurity]
    internal static class WinApi
    {

        #region [Functions]

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowRect(IntPtr hwnd, out Rect rect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetClientRect(IntPtr hwnd, out Rect rect);

        [DllImport("DwmApi.dll")]
        internal static extern int DwmExtendFrameIntoClientArea(
            IntPtr hwnd,
            ref Margins m);

        [DllImport("DwmApi.dll", PreserveSig = false)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool DwmIsCompositionEnabled();

        [DllImport("DwmApi.dll")]
        internal static extern int DwmEnableComposition(
            CompositionEnable compositionAction);

        [DllImport("uxtheme.dll", PreserveSig = false)]
        internal static extern void SetWindowThemeAttribute(IntPtr hwnd, WINDOWTHEMEATTRIBUTETYPE eAttribute,
            ref WTA_OPTIONS pvAttribute, int cbAttribute);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, SetWindowPosFlags uFlags);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        internal static extern int GetWindowLong(IntPtr hWnd, GWL nIndex);

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        internal static extern int SetWindowLong(IntPtr hWnd, GWL nIndex, int dwNewLong);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);

        [DllImport("user32.dll")]
        internal static extern int TrackPopupMenu(IntPtr hMenu, TPM uFlags, int x, int y,
           int nReserved, IntPtr hWnd, IntPtr prcRect);

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        #endregion [Functions]

        #region [Constants]

        internal const int WM_DWMCOMPOSITIONCHANGED = 0x031E;
        internal const int WM_DWMNCRENDERINGCHANGED = 0x031F;

        internal const int WS_EX_DLGMODALFRAME = 0x01;

        internal const int WM_SYSCOMMAND = 0x0112;

        #endregion [Constants]

        #region [Types]

        internal enum CompositionEnable
        {
            Disable = 0,
            Enable = 1
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct Margins
        {
            public int Left;
            public int Right;
            public int Top;
            public int Bottom;

            public Margins(bool fullWindow)
            {
                Left = Right = Top = Bottom = (fullWindow ? -1 : 0);
            }
        };

        /// <summary>
        /// A wrapper for a RECT struct
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct Rect
        {
            /// <summary>
            /// Position of left edge
            /// </summary>            
            public int Left { get; set; }

            /// <summary>
            /// Position of top edge
            /// </summary>            
            public int Top { get; set; }

            /// <summary>
            /// Position of right edge
            /// </summary>            
            public int Right { get; set; }

            /// <summary>
            /// Position of bottom edge
            /// </summary>            
            public int Bottom { get; set; }

            /// <summary>
            /// Creates a new Rect initialized with supplied values.
            /// </summary>
            /// <param name="left">Position of left edge</param>
            /// <param name="top">Position of top edge</param>
            /// <param name="right">Position of right edge</param>
            /// <param name="bottom">Position of bottom edge</param>
            public Rect(int left, int top, int right, int bottom)
                : this()
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            /// <summary>
            /// Determines if two Rects are equal.
            /// </summary>
            /// <param name="first">First Rect</param>
            /// <param name="second">Second Rect</param>
            /// <returns>True if first Rect is equal to second; false otherwise.</returns>
            public static bool operator ==(Rect first, Rect second)
            {
                return first.Left == second.Left
                    && first.Top == second.Top
                    && first.Right == second.Right
                    && first.Bottom == second.Bottom;
            }

            /// <summary>
            /// Determines if two Rects are not equal
            /// </summary>
            /// <param name="first">First Rect</param>
            /// <param name="second">Second Rect</param>
            /// <returns>True if first is not equal to second; false otherwise.</returns>
            public static bool operator !=(Rect first, Rect second)
            {
                return !(first == second);
            }

            /// <summary>
            /// Determines if the Rect is equal to another Rect.
            /// </summary>
            /// <param name="obj">Another Rect to compare</param>
            /// <returns>True if this Rect is equal to the one provided; false otherwise.</returns>
            public override bool Equals(object obj)
            {
                return (obj != null && obj is Rect) ? this == (Rect)obj : false;
            }

            /// <summary>
            /// Creates a hash code for the Rect
            /// </summary>
            /// <returns>Returns hash code for this Rect</returns>
            public override int GetHashCode()
            {
                int hash = Left.GetHashCode();
                hash = hash * 31 + Top.GetHashCode();
                hash = hash * 31 + Right.GetHashCode();
                hash = hash * 31 + Bottom.GetHashCode();
                return hash;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WTA_OPTIONS
        {
            public WTNCA Flags;

            public WTNCA Mask;

            public static int Size
            {
                get
                {
                    return Marshal.SizeOf(typeof(WTA_OPTIONS));
                }
            }
        }

        [Flags]
        internal enum WTNCA : int
        {
            NoDrawCaption = 0x01,
            NoDrawIcon = 0x02,
            NoSysMenu = 0x04,
            NoMirrorHelp = 0x08
        }

        internal enum WINDOWTHEMEATTRIBUTETYPE
        {
            NonClient = 0x01
        }

        [Flags]
        internal enum SetWindowPosFlags : uint
        {
            /// <summary>If the calling thread and the thread that owns the window are attached to different input queues, 
            /// the system posts the request to the thread that owns the window. This prevents the calling thread from 
            /// blocking its execution while other threads process the request.</summary>
            /// <remarks>SWP_ASYNCWINDOWPOS</remarks>
            SynchronousWindowPosition = 0x4000,
            /// <summary>Prevents generation of the WM_SYNCPAINT message.</summary>
            /// <remarks>SWP_DEFERERASE</remarks>
            DeferErase = 0x2000,
            /// <summary>Draws a frame (defined in the window's class description) around the window.</summary>
            /// <remarks>SWP_DRAWFRAME</remarks>
            DrawFrame = 0x0020,
            /// <summary>Applies new frame styles set using the SetWindowLong function. Sends a WM_NCCALCSIZE message to 
            /// the window, even if the window's size is not being changed. If this flag is not specified, WM_NCCALCSIZE 
            /// is sent only when the window's size is being changed.</summary>
            /// <remarks>SWP_FRAMECHANGED</remarks>
            FrameChanged = 0x0020,
            /// <summary>Hides the window.</summary>
            /// <remarks>SWP_HIDEWINDOW</remarks>
            HideWindow = 0x0080,
            /// <summary>Does not activate the window. If this flag is not set, the window is activated and moved to the 
            /// top of either the topmost or non-topmost group (depending on the setting of the hWndInsertAfter 
            /// parameter).</summary>
            /// <remarks>SWP_NOACTIVATE</remarks>
            DoNotActivate = 0x0010,
            /// <summary>Discards the entire contents of the client area. If this flag is not specified, the valid 
            /// contents of the client area are saved and copied back into the client area after the window is sized or 
            /// repositioned.</summary>
            /// <remarks>SWP_NOCOPYBITS</remarks>
            DoNotCopyBits = 0x0100,
            /// <summary>Retains the current position (ignores X and Y parameters).</summary>
            /// <remarks>SWP_NOMOVE</remarks>
            IgnoreMove = 0x0002,
            /// <summary>Does not change the owner window's position in the Z order.</summary>
            /// <remarks>SWP_NOOWNERZORDER</remarks>
            DoNotChangeOwnerZOrder = 0x0200,
            /// <summary>Does not redraw changes. If this flag is set, no repainting of any kind occurs. This applies to 
            /// the client area, the nonclient area (including the title bar and scroll bars), and any part of the parent 
            /// window uncovered as a result of the window being moved. When this flag is set, the application must 
            /// explicitly invalidate or redraw any parts of the window and parent window that need redrawing.</summary>
            /// <remarks>SWP_NOREDRAW</remarks>
            DoNotRedraw = 0x0008,
            /// <summary>Same as the SWP_NOOWNERZORDER flag.</summary>
            /// <remarks>SWP_NOREPOSITION</remarks>
            DoNotReposition = 0x0200,
            /// <summary>Prevents the window from receiving the WM_WINDOWPOSCHANGING message.</summary>
            /// <remarks>SWP_NOSENDCHANGING</remarks>
            DoNotSendChangingEvent = 0x0400,
            /// <summary>Retains the current size (ignores the cx and cy parameters).</summary>
            /// <remarks>SWP_NOSIZE</remarks>
            IgnoreResize = 0x0001,
            /// <summary>Retains the current Z order (ignores the hWndInsertAfter parameter).</summary>
            /// <remarks>SWP_NOZORDER</remarks>
            IgnoreZOrder = 0x0004,
            /// <summary>Displays the window.</summary>
            /// <remarks>SWP_SHOWWINDOW</remarks>
            ShowWindow = 0x0040,
        }

        internal enum GWL
        {
            WndProc = -4,
            HInstance = -6,
            HwndParent = -8,
            Style = -16,
            ExStyle = -20,
            UserDate = -21,
            Id = -12
        }

        [Flags]
        internal enum TPM
        {
            LeftHAlign = 0x0000,
            CenterHAlign = 0x0004,
            RightHAlign = 0x0008,
            TopVAlign = 0x0000,
            CenterVAlign = 0x0010,
            BottomVAlign = 0x0020,
            Horizontal = 0x0000,
            Vertical = 0x0040,
            ReturnCmd = 0x0100
        }

        #endregion [Types]
    }
}
