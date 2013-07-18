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

using System.Windows.Input;

namespace AvalonWizard
{
    internal enum ExtendedMouseAction : byte
    {
        None = MouseAction.None,

        LeftClick = MouseAction.LeftClick,
        
        RightClick = MouseAction.RightClick,

        MiddleClick = MouseAction.MiddleClick,

        WheelClick = MouseAction.WheelClick,

        LeftDoubleClick = MouseAction.LeftDoubleClick,
        
        RightDoubleClick = MouseAction.RightDoubleClick,

        MiddleDoubleClick = MouseAction.MiddleDoubleClick,

        XButton1Click,

        XButton2Click
    }
}
