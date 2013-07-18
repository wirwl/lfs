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
namespace AvalonWizard
{
    /// <summary>
    /// Represents a wizard style.
    /// </summary>
    /// <remarks>
    /// When <see cref="WizardStyle.Auto"/> is used, the actual style is automatically selected based on the OS version.
    /// <list type="bullet">
    ///     <item>
    ///         <term><see cref="WizardStyle.Wizard97"/></term>
    ///         <description> is selected for Windows XP and Windows 2003 Server.</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="WizardStyle.Aero"/></term>
    ///         <description> is selected for Windows Vista or higher.</description>
    ///     </item>
    /// </list>
    /// </remarks>
    public enum WizardStyle
    {
        /// <summary>
        /// <para>The style is automatically selected based on the OS version.</para>
        /// <list type="bullet">
        ///     <item>
        ///         <term><see cref="WizardStyle.Wizard97"/></term>
        ///         <description> is selected for Windows XP and Windows 2003 Server.</description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="WizardStyle.Aero"/></term>
        ///         <description> is selected for Windows Vista or higher.</description>
        ///     </item>
        /// </list>
        /// </summary>
        Auto,
        
        /// <summary>
        /// Wizard 97 style.
        /// </summary>
        Wizard97,
        
        /// <summary>
        /// Aero Wizard style.
        /// </summary>
        Aero
    }
}
