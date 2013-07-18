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
    /// <summary>
    /// Contains commands for the <see cref="Wizard"/> control.
    /// </summary>
    public static class WizardCommands
    {
        private static readonly ICommand nextPageCommand = new RoutedUICommand();

        private static readonly ICommand previousPageCommand = new RoutedUICommand();

        private static readonly ICommand finishCommand = new RoutedUICommand();

        private static readonly ICommand cancelCommand = new RoutedUICommand();

        /// <summary>
        /// Makes the wizard go to the next page.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Is the command parameter is <c>null</c>, the wizard navigates to next page in <see cref="Wizard.Pages"/> collection
        /// or finishes the wizard if the current page is the last one.
        /// </para>
        /// <para>
        /// The command parameter may be one of the following types:
        /// </para>
        /// <list type="table">
        ///     <listheader>
        ///         <term>Type</term>
        ///         <description>Usage</description>
        ///     </listheader>
        ///     <item>
        ///         <term><see cref="WizardPage"/></term>
        ///         <description>Navigates to the specified page.</description>
        ///     </item>
        ///     <item>
        ///         <term><see cref="string"/></term>
        ///         <description>Navigates to the page with the specified name.</description>
        ///     </item>
        ///     <item>
        ///         <term>Any type convertible to <see cref="int"/></term>
        ///         <description>Navigates to the page with the specified index in the <see cref="Wizard.Pages"/> collection.</description>
        ///     </item>
        /// </list>
        /// <para>
        /// If the parameter is of type <see cref="string"/> then the wizard will search for the page with the corresponding name.
        /// If the page is not found, then the wizard will try to convert the string to <see cref="int"/> 
        /// and use it as the index in <see cref="Wizard.Pages"/> collection.
        /// </para>
        /// </remarks>
        public static ICommand NextPage
        {
            get
            {
                return nextPageCommand;
            }
        }

        /// <summary>
        /// Makes the wizard go to the previous page.
        /// </summary>
        public static ICommand PreviousPage
        {
            get
            {
                return previousPageCommand;
            }
        }

        /// <summary>
        /// Finishes the wizard..
        /// </summary>
        public static ICommand Finish
        {
            get
            {
                return finishCommand;
            }
        }

        /// <summary>
        /// Cancels the wizard.
        /// </summary>
        public static ICommand Cancel
        {
            get
            {
                return cancelCommand;
            }
        }
    }
}
