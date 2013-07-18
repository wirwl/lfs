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
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AvalonWizard
{
    /// <summary>
    /// The page of <see cref="AvalonWizard.Wizard"/> control.
    /// </summary>
    public class WizardPage : HeaderedContentControl
    {
        static WizardPage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WizardPage), new FrameworkPropertyMetadata(typeof(WizardPage)));

            IsTabStopProperty.OverrideMetadata(typeof(WizardPage), new FrameworkPropertyMetadata(false));
        }

        #region [Properties]

        /// <summary>
        /// Gets the <see cref = "AvalonWizard.Wizard" /> owing this page.
        /// </summary>
        [Browsable(false)]
        public Wizard Wizard
        {
            get;
            internal set;
        }

        /// <summary>
        /// <para>
        /// Gets or sets the next page that should be used when the user clicks the Next button 
        /// or when the <see cref = "AvalonWizard.Wizard.NextPage()" /> method is called. 
        /// </para>
        /// <para>
        /// This is used to override the default behavior of going to the next page 
        /// in the sequence defined within the <see cref = "AvalonWizard.Wizard.Pages" /> collection.
        /// </para>
        /// </summary>
        /// <value>The wizard page to go to.</value>
        [DefaultValue(null)]
        [Category("Behavior")]
        [Description("Specify a page other than the next page in the Pages collection as the next page.")]
        public virtual WizardPage NextPage
        {
            get; 
            set;
        }

        #endregion [Properties]

        #region [Dependency Properties]

        #region [IsFinishPage Property]

        /// <summary>
        /// Identifies the page as a the last page of the wizard.
        /// </summary>
        /// <remarks>
        /// You may set this property on several pages if your wizard requires non-linear navigation.
        /// </remarks>
        [DefaultValue(false)]
        public bool IsFinishPage
        {
            get { return (bool)GetValue(IsFinishPageProperty); }
            set { SetValue(IsFinishPageProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="IsFinishPage"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsFinishPageProperty =
            DependencyProperty.Register("IsFinishPage", typeof(bool), typeof(WizardPage), new UIPropertyMetadata(false));

        #endregion [IsFinishPage Property]

        #region [AllowCancel Property]

        /// <summary>
        /// Gets or sets the value indicating whether the Cancel button is enabled on this page.
        /// </summary>
        [DefaultValue(true)]
        public bool AllowCancel
        {
            get { return (bool)GetValue(AllowCancelProperty); }
            set { SetValue(AllowCancelProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="AllowCancel"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AllowCancelProperty = DependencyProperty.Register(
            "AllowCancel", typeof(bool), typeof(WizardPage), new UIPropertyMetadata(true, OnAllowCommandChanged));

        #endregion

        #region [AllowNext Property]

        /// <summary>
        /// Gets or sets the value indicating whether the Next button is enabled on this page.
        /// </summary>
        [DefaultValue(true)]
        public bool AllowNext
        {
            get { return (bool)GetValue(AllowNextProperty); }
            set { SetValue(AllowNextProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="AllowNext"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AllowNextProperty = DependencyProperty.Register(
            "AllowNext", typeof(bool), typeof(WizardPage), new UIPropertyMetadata(true, OnAllowCommandChanged));

        #endregion [AllowNext Property]

        #region [AllowBack Property]

        /// <summary>
        /// Gets or sets the value indicating whether the Back button is enabled on this page.
        /// </summary>
        [DefaultValue(true)]
        public bool AllowBack
        {
            get { return (bool)GetValue(AllowBackProperty); }
            set { SetValue(AllowBackProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="AllowBack"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty AllowBackProperty = DependencyProperty.Register(
            "AllowBack", typeof(bool), typeof(WizardPage), new UIPropertyMetadata(true, OnAllowCommandChanged));

        #endregion [AllowBack Property]

        #region [AllowFinish Property]

        /// <summary>
        /// Gets or sets the value indicating whether the Finish button is enabled on this page.
        /// </summary>
        public bool AllowFinish
        {
            get { return (bool)GetValue(AllowFinishProperty); }
            set { SetValue(AllowFinishProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="AllowFinish"/> property.
        /// </summary>
        public static readonly DependencyProperty AllowFinishProperty = DependencyProperty.Register(
            "AllowFinish", typeof(bool), typeof(WizardPage), new UIPropertyMetadata(true, OnAllowCommandChanged));

        #endregion

        #region [ShowCancel Property]

        /// <summary>
        /// Gets or sets the value indicating whether the Cancel button is visible on this page.
        /// </summary>
        [DefaultValue(true)]
        public bool ShowCancel
        {
            get { return (bool)GetValue(ShowCancelProperty); }
            set { SetValue(ShowCancelProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ShowCancel"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowCancelProperty =
            DependencyProperty.Register("ShowCancel", typeof(bool), typeof(WizardPage), new UIPropertyMetadata(true));

        #endregion

        #region [ShowNext Property]

        /// <summary>
        /// Gets or sets the value indicating whether the Next button is visible on this page.
        /// </summary>
        [DefaultValue(true)]
        public bool ShowNext
        {
            get { return (bool)GetValue(ShowNextProperty); }
            set { SetValue(ShowNextProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ShowNext"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowNextProperty =
            DependencyProperty.Register("ShowNext", typeof(bool), typeof(WizardPage), new UIPropertyMetadata(true));

        #endregion [ShowNext Property]

        #region [ShowBack Property]

        /// <summary>
        /// Gets or sets the value indicating whether the Back button is visible on this page.
        /// </summary>
        [DefaultValue(true)]
        public bool ShowBack
        {
            get { return (bool)GetValue(ShowBackProperty); }
            set { SetValue(ShowBackProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="ShowBack"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty ShowBackProperty =
            DependencyProperty.Register("ShowBack", typeof(bool), typeof(WizardPage), new UIPropertyMetadata(true));

        #endregion [ShowNext Property]

        #endregion [Dependency Properties]

        #region [Events]

        #region [Initialize Event]

        /// <summary>
        /// Identifies the <see cref="Initialize"/> routed event.
        /// </summary>
        public static readonly RoutedEvent InitializeEvent = EventManager.RegisterRoutedEvent(
            "Initialize", RoutingStrategy.Bubble, typeof(EventHandler<WizardPageInitEventArgs>), typeof(WizardPage));

        /// <summary>
        /// Raised just before showing the page.
        /// </summary>
        public event EventHandler<WizardPageInitEventArgs> Initialize
        {
            add { AddHandler(InitializeEvent, value); }
            remove { RemoveHandler(InitializeEvent, value); }
        }

        /// <summary>
        /// Invoked just before showing the page.
        /// </summary>
        protected virtual void OnInitialize(WizardPageInitEventArgs args)
        {
            args.RoutedEvent = InitializeEvent;
            RaiseEvent(args);
        }

        #endregion [Initialize Event]

        #region [Commit Event]

        /// <summary>
        /// Identifies the <see cref="Commit"/> routed event.
        /// </summary>
        public static readonly RoutedEvent CommitEvent = EventManager.RegisterRoutedEvent(
            "Commit", RoutingStrategy.Bubble, typeof(EventHandler<WizardPageConfirmEventArgs>), typeof(WizardPage));

        /// <summary>
        /// Raised before going to the next page.
        /// </summary>
        public event EventHandler<WizardPageConfirmEventArgs> Commit
        {
            add { AddHandler(CommitEvent, value); }
            remove { RemoveHandler(CommitEvent, value); }
        }

        /// <summary>
        /// Invoked before going to the next page.
        /// </summary>
        protected virtual void OnCommit(WizardPageConfirmEventArgs args)
        {
            args.RoutedEvent = CommitEvent;
            RaiseEvent(args);
        }

        #endregion [Commit Event]

        #region [Rollback Event]

        /// <summary>
        /// Identifies the <see cref="Rollback"/> routed event.
        /// </summary>
        public static readonly RoutedEvent RollbackEvent = EventManager.RegisterRoutedEvent(
            "Rollback", RoutingStrategy.Bubble, typeof(EventHandler<WizardPageConfirmEventArgs>), typeof(WizardPage));

        /// <summary>
        /// Raised before going to the previous page.
        /// </summary>
        public event EventHandler<WizardPageConfirmEventArgs> Rollback
        {
            add { AddHandler(RollbackEvent, value); }
            remove { RemoveHandler(RollbackEvent, value); }
        }

        /// <summary>
        /// Invoked before going to the previous page.
        /// </summary>
        protected virtual void OnRollback(WizardPageConfirmEventArgs args)
        {
            args.RoutedEvent = RollbackEvent;
            RaiseEvent(args);
        }

        #endregion [Rollback Event]

        #endregion [Events]

        #region [Internal Methods]

        internal void InitializePage(WizardPage previousPage)
        {
            var args = new WizardPageInitEventArgs(this, previousPage);
            OnInitialize(args);
        }

        internal bool CommitPage()
        {
            var args = new WizardPageConfirmEventArgs(this);
            OnCommit(args);
            return !args.Cancel;
        }

        internal bool RollbackPage()
        {
            var args = new WizardPageConfirmEventArgs(this);
            OnRollback(args);
            return !args.Cancel;
        }

        #endregion

        #region [Private Methods]

        private static void OnAllowCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CommandManager.InvalidateRequerySuggested();
        }

        #endregion
    }
}
