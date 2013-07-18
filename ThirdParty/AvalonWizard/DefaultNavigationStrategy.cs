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

using System.Linq;

namespace AvalonWizard
{
    /// <summary>
    /// The default implementation of <see cref="INavigationStrategy"/>.
    /// </summary>
    /// <remarks>
    /// This implementation uses the <see cref="WizardPage.NextPage"/> property of the <see cref="Wizard.CurrentPage"/> 
    /// as the next page. If the value of the property is <c>null</c> it uses the next page 
    /// in the <see cref="Wizard.Pages"/> collection.
    /// </remarks>
    public class DefaultNavigationStrategy : INavigationStrategy
    {
        #region Implementation of INavigationStrategy

        /// <summary>
        /// Gets the next page.
        /// </summary>
        /// <param name="wizard">Wizard instance.</param>
        /// <returns>The next page.</returns>
        /// <remarks>
        /// This implementation uses the <see cref="WizardPage.NextPage"/> property of the <see cref="Wizard.CurrentPage"/> 
        /// as the next page. If the value of the property is <c>null</c> it uses the next page 
        /// in the <see cref="Wizard.Pages"/> collection.
        /// </remarks>
        public WizardPage GetNextPage(Wizard wizard)
        {
            var currentPage = wizard.CurrentPage;
            if (currentPage == null)
            {
                return wizard.Pages.Any() ? wizard.Pages.First() : null;
            }

            if (currentPage.NextPage != null)
            {
                return currentPage.NextPage;
            }

            return wizard.Pages.ElementAtOrDefault(wizard.Pages.IndexOf(currentPage) + 1);
        }

        #endregion
    }
}