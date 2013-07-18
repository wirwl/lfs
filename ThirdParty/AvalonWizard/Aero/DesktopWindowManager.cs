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

namespace AvalonWizard.Aero
{
    /// <summary>
    /// Gets the information about Desktop Window Manager.
    /// </summary>
    public static class DesktopWindowManager
    {
        /// <summary>
        /// Gets the value indicating whether DWM composition is enabled.
        /// </summary>
        public static bool IsCompositionEnabled
        {
            get
            {
                return isSupported && WinApi.DwmIsCompositionEnabled();
            }
        }

        /// <summary>
        /// Gets the value indicating whether the Desktop Window Manager is available on the operating system.
        /// Returns <c>true</c> for Windows Vista and higher. 
        /// Returns <c>false</c> for Windows XP and Windows 2003 Server.
        /// </summary>
        public static bool IsSupported
        {
            get
            {
                return isSupported;
            }
        }

        internal static void ExtendFrameIntoClientArea(IntPtr hwnd, ref WinApi.Margins margins)
        {
            if (isSupported)
            {
                WinApi.DwmExtendFrameIntoClientArea(hwnd, ref margins);
            }
        }

        internal static void ResetGlass(IntPtr hwnd)
        {
            if (isSupported)
            {
                var margins = new WinApi.Margins(false);
                WinApi.DwmExtendFrameIntoClientArea(hwnd, ref margins);
            }
        }

        internal static void SetTitleAndIconVisibility(IntPtr hwnd, bool visible)
        {
            if (isSupported)
            {
                const WinApi.WTNCA mask = WinApi.WTNCA.NoDrawCaption | WinApi.WTNCA.NoDrawIcon | WinApi.WTNCA.NoSysMenu;
                var flags = visible ? default(WinApi.WTNCA) : mask;

                var options = new WinApi.WTA_OPTIONS { Flags = flags, Mask = mask };
                WinApi.SetWindowThemeAttribute(hwnd, WinApi.WINDOWTHEMEATTRIBUTETYPE.NonClient,
                    ref options, WinApi.WTA_OPTIONS.Size);
            }
        }

        private static readonly bool isSupported = Environment.OSVersion.Version.Major >= 6;
    }
}
