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
    /// Wizard's navigation strategy allows to implement the custom way to select the next page.
    /// </summary>
    public interface INavigationStrategy
    {
        /// <summary>
        /// Gets the next page to go.
        /// </summary>
        /// <param name="wizard">Wizard instance.</param>
        /// <returns>The next page.</returns>
        WizardPage GetNextPage(Wizard wizard);
    }
}
