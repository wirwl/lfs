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

namespace AvalonWizard
{
    /// <summary>
    /// Defines the way to render Aero glow in header.
    /// </summary>
    public enum AeroGlowStyle
    {
        /// <summary>
        /// Do not render Aero glow.
        /// </summary>
        None = -1,

        /// <summary>
        /// Render Aero glow as blurred text.
        /// </summary>
        BlurredText = 0,

        /// <summary>
        /// Render Aero glow as blurred rectangle.
        /// </summary>
        BlurredRectangle = 1,

        /// <summary>
        /// Render Aero glow as text with outer glow. This option is not supported on .Net 4 or later.
        /// </summary>
        [Obsolete("Not supported on .Net 4 or later.")]
        GlowingText = 2,
    }
}
