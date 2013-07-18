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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using AvalonWizard.Extensions;

namespace AvalonWizard
{
    /// <summary>
    /// Wizard Control for Windows Presentation foundation.
    /// </summary>
    /// <remarks>
    /// The wizard supports two styles: the legacy Wizard 97 and the modern Aero Wizard.
    /// </remarks>
    [DefaultProperty("Pages")]
    [ContentProperty("Pages")]
    public class Wizard : Control, IAddChild
    {
        #region [Constructors]

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public Wizard()
        {
            pages = new WizardPageCollection(this);
            pages.CollectionChanged += HandlePagesCollectionChanged;

            Loaded += OnLoaded;

            UpdateEffectiveWizardStyle(WizardStyle.Auto);
        }

        /// <summary>
        /// Initializes static members of the <see cref="Wizard" /> class.
        /// </summary>
        static Wizard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Wizard), new FrameworkPropertyMetadata(typeof(Wizard)));

            IsTabStopProperty.OverrideMetadata(typeof(Wizard), new FrameworkPropertyMetadata(false));
            
            var backBinding = new CommandBinding(WizardCommands.PreviousPage,
                OnBackCommand,
                OnBackCommandCanExecute);
            CommandManager.RegisterClassCommandBinding(typeof(Wizard), backBinding);
            CommandManager.RegisterClassInputBinding(typeof(Wizard),
                new KeyBinding(WizardCommands.PreviousPage, Key.Left, ModifierKeys.Alt));
            CommandManager.RegisterClassInputBinding(typeof(Wizard),
                new InputBinding(WizardCommands.PreviousPage, new ExtendedMouseGesture(ExtendedMouseAction.XButton1Click)));
            
            var nextBinding = new CommandBinding(WizardCommands.NextPage,
                OnNextCommand,
                OnNextCommandCanExecute);
            CommandManager.RegisterClassCommandBinding(typeof(Wizard), nextBinding);
            CommandManager.RegisterClassInputBinding(typeof(Wizard),
                new InputBinding(WizardCommands.NextPage, new ExtendedMouseGesture(ExtendedMouseAction.XButton2Click)));

            var finishBinding = new CommandBinding(WizardCommands.Finish,
                OnFinishCommand,
                OnFinishCommandCanExecute);
            CommandManager.RegisterClassCommandBinding(typeof(Wizard), finishBinding);

            var cancelBinding = new CommandBinding(WizardCommands.Cancel,
                OnCancelCommand,
                OnCancelCommandCanExecute);
            CommandManager.RegisterClassCommandBinding(typeof(Wizard), cancelBinding);
        }

        #endregion [Constructors]

        #region [Public Methods]

        /// <summary>
        /// Steps to the next page or finishes the wizard if the current page is the last one.
        /// </summary>
        /// <seealso cref="NextPage(WizardPage)"/>
        public virtual void NextPage()
        {
            NextPage(null);
        }

        /// <summary>
        /// Steps to the specified page.
        /// </summary>
        /// <param name="nextPage">
        /// The page to go. 
        /// If the value is <c>null</c>, the wizard steps to the next page or finishes the wizard if the current page is the last one.
        /// </param>
        /// <seealso cref="NextPage()"/>
        public virtual void NextPage(WizardPage nextPage)
        {
            if (nextPage == null)
            {
                if (CurrentPage == Pages.Last() || CurrentPage.IsFinishPage)
                {
                    Finish();
                    return;
                }
            }
            else if (!Pages.Contains(nextPage))
            {
                throw new ArgumentException(
                    "When specifying a value for nextPage, it must already be in the Pages collection.", "nextPage");
            }

            if (CurrentPage == null || CurrentPage.CommitPage())
            {
                if (!DesignerProperties.GetIsInDesignMode(this))
                {
                    pageHistory.Push(CurrentPage);
                }
                CurrentPage = nextPage ?? strategy.GetNextPage(this);
            }
        }

        /// <summary>
        /// Steps to the page with the specified name.
        /// </summary>
        /// <param name="pageName">Page name to navigate to.</param>
        /// <exception cref="ArgumentException">
        /// <see cref="Wizard.Pages"/> collection does not contains the page with the specified name.
        /// </exception>
        public virtual void NextPageByName(String pageName)
        {
            var page = Pages.First(p => p.Name == pageName);
            if (page == null)
            {
                throw new ArgumentException("Page with the specified name is not found.", "pageName");
            }
            NextPage(page);
        }

        /// <summary>
        /// Steps to the page with the specified index in <see cref="Wizard.Pages"/> collection.
        /// </summary>
        /// <param name="pageIndex">Page index to navigate to.</param>
        /// <exception cref="IndexOutOfRangeException">Page index is out of range.</exception>
        public virtual void NextPageByIndex(int pageIndex)
        {
            if (DesignerProperties.GetIsInDesignMode(this) && (pageIndex < 0 || pageIndex >= Pages.Count))
            {
                CurrentPage = null;
                return;
            }
            NextPage(Pages[pageIndex]);
        }

        /// <summary>
        /// Steps to previous page.
        /// </summary>
        public virtual void PreviousPage()
        {
            if (pageHistory.Count > 0)
            {
                if (CurrentPage != null && CurrentPage.RollbackPage())
                {
                    CurrentPage = pageHistory.Pop();
                }
            }
        }

        /// <summary>
        /// Finishes the wizard.
        /// </summary>
        public virtual void Finish()
        {
            OnFinished(new RoutedEventArgs());
        }

        /// <summary>
        /// Cancels the wizard.
        /// </summary>
        public virtual void Cancel()
        {
            OnCancelled(new RoutedEventArgs());
        }

        #endregion [Public Methods]

        #region [Dependency Properties]

        #region [BackButtonContent]

        /// <summary>
        /// Gets or sets the content of the Back button.
        /// </summary>
        public Object BackButtonContent
        {
            get { return (Object)GetValue(BackButtonContentProperty); }
            set { SetValue(BackButtonContentProperty, value); }
        }

        /// <summary>
        /// Identifies <see cref="BackButtonContent"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty BackButtonContentProperty =
            DependencyProperty.Register("BackButtonContent", typeof(Object), typeof(Wizard), 
                new UIPropertyMetadata(Properties.Resources.BackButtonText));

        #endregion [BackButtonContent]

        #region [NextButtonContent]
        
        /// <summary>
        /// Gets or sets the content of the Next button.
        /// </summary>
        public Object NextButtonContent
        {
            get { return (Object)GetValue(NextButtonContentProperty); }
            set { SetValue(NextButtonContentProperty, value); }
        }
        
        /// <summary>
        /// Identifies <see cref="NextButtonContent"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty NextButtonContentProperty =
            DependencyProperty.Register("NextButtonContent", typeof(Object), typeof(Wizard),
                new UIPropertyMetadata(Properties.Resources.NextButtonText));

        #endregion [NextButtonContent]

        #region [FinishButtonContent]

        /// <summary>
        /// Gets or sets the content of the Finish button.
        /// </summary>
        public Object FinishButtonContent
        {
            get { return (Object)GetValue(FinishButtonContentProperty); }
            set { SetValue(FinishButtonContentProperty, value); }
        }

        /// <summary>
        /// Identifies <see cref="FinishButtonContent"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty FinishButtonContentProperty =
            DependencyProperty.Register("FinishButtonContent", typeof(Object), typeof(Wizard),
                new UIPropertyMetadata(Properties.Resources.FinishButtonText));

        #endregion [FinishButtonContent]

        #region [CancelButtonContent]

        /// <summary>
        /// Gets or sets the content of the Cancel button.
        /// </summary>
        public Object CancelButtonContent
        {
            get { return (Object)GetValue(CancelButtonContentProperty); }
            set { SetValue(CancelButtonContentProperty, value); }
        }

        /// <summary>
        /// Identifies <see cref="CancelButtonContent"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CancelButtonContentProperty =
            DependencyProperty.Register("CancelButtonContent", typeof(Object), typeof(Wizard),
                new UIPropertyMetadata(Properties.Resources.CancelButtonText));

        #endregion [CancelButtonContent]

        #region [CurrentPage]
        
        /// <summary>
        /// Gets the current page.
        /// </summary>
        public WizardPage CurrentPage
        {
            get
            {
                return (WizardPage)GetValue(CurrentPageProperty);
            }
            private set
            {
                SetValue(CurrentPagePropertyKey, value);
            }
        }

        private static readonly DependencyPropertyKey CurrentPagePropertyKey =
            DependencyProperty.RegisterReadOnly("CurrentPage", typeof(WizardPage), typeof(Wizard), 
                new UIPropertyMetadata(null, OnCurrentPageChanged, CoerceCurrentPage));

        /// <summary>
        /// Identifies <see cref="CurrentPage"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CurrentPageProperty = CurrentPagePropertyKey.DependencyProperty;

        private static void OnCurrentPageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var wizard = (Wizard)d;
            var oldPage = e.OldValue as WizardPage;
            var newPage = e.NewValue as WizardPage;
            wizard.CurrentPageIndex = wizard.Pages.IndexOf(newPage);

            if (wizard.IsLoaded && newPage != null)
            {
                newPage.InitializePage(oldPage);
            }
            
            UpdateFirstLastPage(wizard);

            wizard.OnCurrentPageChanged(new CurrentPageChangedEventArgs(oldPage, newPage));
            CommandManager.InvalidateRequerySuggested();
        }

        private static object CoerceCurrentPage(DependencyObject d, object basevalue)
        {
            var wizard = (Wizard)d;
            var value = (WizardPage)basevalue;
            
            if (value == null || wizard.Pages.Contains(value))
            {
                return value;
            }

            return DependencyProperty.UnsetValue;
        }

        #endregion [CurrentPage]

        #region [CurrentPageIndex]

        /// <summary>
        /// Gets the current page's index.
        /// </summary>
        public int CurrentPageIndex
        {
            get
            {
                return (int)GetValue(CurrentPageIndexProperty);
            }
            private set
            {
                SetValue(CurrentPageIndexPropertyKey, value);
            }
        }

        private static readonly DependencyPropertyKey CurrentPageIndexPropertyKey =
            DependencyProperty.RegisterReadOnly("CurrentPageIndex", typeof(int), typeof(Wizard),
            new UIPropertyMetadata(-1), ValidateCurrentPageIndex);

        /// <summary>
        /// Identifies <see cref="CurrentPageIndex"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty CurrentPageIndexProperty =
            CurrentPageIndexPropertyKey.DependencyProperty;

        private static bool ValidateCurrentPageIndex(object value)
        {
            return (int)value >= -1;
        }

        #endregion [CurrentPageIndex]

        #region [HasPages]

        /// <summary>
        /// Gets the value indicating whether the wizard has any pages.
        /// </summary>
        public bool HasPages
        {
            get { return (bool)GetValue(HasPagesProperty); }
            private set { SetValue(HasPagesPropertyKey, value); }
        }

        private static readonly DependencyPropertyKey HasPagesPropertyKey =
            DependencyProperty.RegisterReadOnly("HasPages", typeof (bool), typeof (Wizard),
                                                new UIPropertyMetadata(false));

        /// <summary>
        /// Identifies <see cref="HasPages"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HasPagesProperty =
            HasPagesPropertyKey.DependencyProperty;

        #endregion [HasPages]

        #region [IsFirstPage]

        /// <summary>
        /// Gets the value indicating whether the current page is the first page.
        /// </summary>
        public bool IsFirstPage
        {
            get { return (bool)GetValue(IsFirstPageProperty); }
            private set { SetValue(IsFirstPagePropertyKey, value); }
        }

        private static readonly DependencyPropertyKey IsFirstPagePropertyKey =
            DependencyProperty.RegisterReadOnly("IsFirstPage", typeof (bool), typeof (Wizard),
                                                new UIPropertyMetadata(true));

        /// <summary>
        /// Identifies <see cref="IsFirstPage"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsFirstPageProperty =
            IsFirstPagePropertyKey.DependencyProperty;

        #endregion [IsFirstPage]

        #region [IsLastPage]

        /// <summary>
        /// Gets the value indicating whether the current page is either last page or
        /// has the <see cref="WizardPage.IsFinishPage"/> property set to <c>true</c>.
        /// </summary>
        public bool IsLastPage
        {
            get { return (bool)GetValue(IsLastPageProperty); }
            private set { SetValue(IsLastPagePropertyKey, value); }
        }

        private static readonly DependencyPropertyKey IsLastPagePropertyKey =
            DependencyProperty.RegisterReadOnly("IsLastPage", typeof (bool), typeof (Wizard),
                                                new UIPropertyMetadata(true));

        /// <summary>
        /// Identifies <see cref="IsLastPage"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty IsLastPageProperty =
            IsLastPagePropertyKey.DependencyProperty;

        #endregion [IsLastPage]

        #region [WizardStyle]

        /// <summary>
        /// Gets or sets the current <see cref="WizardStyle"/>.
        /// </summary>
        public WizardStyle WizardStyle
        {
            get { return (WizardStyle)GetValue(WizardStyleProperty); }
            set { SetValue(WizardStyleProperty, value); }
        }

        /// <summary>
        /// Identifies <see cref="WizardStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty WizardStyleProperty =
            DependencyProperty.Register("WizardStyle", typeof(WizardStyle), typeof(Wizard), 
                new UIPropertyMetadata(WizardStyle.Auto, OnWizardStyleChanged));

        private static void OnWizardStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var wizard = d as Wizard;
            if (wizard != null)
            {
                var style = (WizardStyle)e.NewValue;
                wizard.UpdateEffectiveWizardStyle(style);
            }
        }

        #endregion [WizardStyle]

        #region [EffectiveWizardStyle]

        /// <summary>
        /// Gets the effective wizard style.
        /// </summary>
        /// <remarks>
        /// <para>
        /// If the <see cref="Wizard.WizardStyle"/> is set to <see cref="AvalonWizard.WizardStyle.Auto"/>
        /// then the effective value is <see cref="AvalonWizard.WizardStyle.Wizard97"/> on Windows XP and Windows 2003 Server,
        /// and <see cref="AvalonWizard.WizardStyle.Aero"/> on Windows Vista or higher.
        /// </para>
        /// <para>
        /// Otherwise the effective value is equal to <see cref="Wizard.WizardStyle"/>.
        /// </para>
        /// </remarks>
        public WizardStyle EffectiveWizardStyle
        {
            get { return (WizardStyle)GetValue(EffectiveWizardStyleProperty); }
            private set { SetValue(EffectiveWizardStylePropertyKey, value); }
        }

        private static readonly DependencyPropertyKey EffectiveWizardStylePropertyKey =
            DependencyProperty.RegisterReadOnly("EffectiveWizardStyle", typeof (WizardStyle), typeof (Wizard),
                                                new UIPropertyMetadata(WizardStyle.Auto));

        /// <summary>
        /// Identifies <see cref="EffectiveWizardStyle"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty EffectiveWizardStyleProperty =
            EffectiveWizardStylePropertyKey.DependencyProperty;

        #endregion [EffectiveWizardStyle]

        #endregion

        #region [Public Properties]

        /// <summary>
        /// Gets the list of wizard pages.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content), Bindable(true)]
        public WizardPageCollection Pages
        {
            get
            {
                return pages;
            }
        }

        /// <summary>
        /// Gets or sets the navigation strategy.
        /// </summary>
        /// <seealso cref="INavigationStrategy"/>
        public INavigationStrategy NavigationStrategy
        {
            get
            {
                return strategy;
            }
            set
            {
                strategy = value ?? new DefaultNavigationStrategy();
            }
        }

        #endregion [Public Properties]

        #region [Public Events]

        #region [Finished Event]

        /// <summary>
        /// Identifies <see cref="Finished"/> routed event.
        /// </summary>
        public static readonly RoutedEvent FinishedEvent = EventManager.RegisterRoutedEvent(
            "Finished", RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (Wizard));

        /// <summary>
        /// Raised when the wizard is successfully finished.
        /// </summary>
        public event RoutedEventHandler Finished
        {
            add { AddHandler(FinishedEvent, value); }
            remove { RemoveHandler(FinishedEvent, value); }
        }

        #endregion [Finished Event]

        #region [Cancelled Event]

        /// <summary>
        /// Identifies <see cref="Cancelled"/> routed event.
        /// </summary>
        public static readonly RoutedEvent CancelledEvent = EventManager.RegisterRoutedEvent(
            "Cancelled", RoutingStrategy.Bubble, typeof (RoutedEventHandler), typeof (Wizard));

        /// <summary>
        /// Raised when the wizard is cancelled.
        /// </summary>
        public event RoutedEventHandler Cancelled
        {
            add { AddHandler(CancelledEvent, value); }
            remove { RemoveHandler(CancelledEvent, value); }
        }

        #endregion [Cancelled Event]

        #region [CurrentPageChanged Event]

        /// <summary>
        /// Identifies <see cref="CurrentPageChanged"/> routed event.
        /// </summary>
        public static readonly RoutedEvent CurrentPageChangedEvent = EventManager.RegisterRoutedEvent(
            "CurrentPageChanged", RoutingStrategy.Bubble, typeof (EventHandler<CurrentPageChangedEventArgs>), typeof (Wizard));

        /// <summary>
        /// Raised when the current page is changed.
        /// </summary>
        public event EventHandler<CurrentPageChangedEventArgs> CurrentPageChanged
        {
            add { AddHandler(CurrentPageChangedEvent, value); }
            remove { RemoveHandler(CurrentPageChangedEvent, value); }
        }

        #endregion [CurrentPageChanged Event]

        #endregion [Public Events]

        #region [Protected Methods]

        /// <summary>
        /// Invoked when the wizard is successfully finished.
        /// </summary>
        protected virtual void OnFinished(RoutedEventArgs args)
        {
            args.RoutedEvent = FinishedEvent;
            RaiseEvent(args);
        }

        /// <summary>
        /// Invoked when the wizard is cancelled.
        /// </summary>
        protected virtual void OnCancelled(RoutedEventArgs args)
        {
            args.RoutedEvent = CancelledEvent;
            RaiseEvent(args);
        }

        /// <summary>
        /// Invoked when the current page is changed.
        /// </summary>
        protected virtual void OnCurrentPageChanged(CurrentPageChangedEventArgs args)
        {
            args.RoutedEvent = CurrentPageChangedEvent;
            RaiseEvent(args);
        }

        #endregion [Protected Methods]

        #region [Commands]

        private static void OnBackCommand(Object sender, ExecutedRoutedEventArgs e)
        {
            var wizard = sender as Wizard;
            if (wizard != null)
            {
                wizard.PreviousPage();
            }
        }

        private static void OnBackCommandCanExecute(Object sender, CanExecuteRoutedEventArgs e)
        {
            var wizard = sender as Wizard;
            if (wizard != null)
            {
                e.CanExecute = !wizard.IsFirstPage && wizard.CurrentPage != null && wizard.CurrentPage.AllowBack;
            }
        }

        private static void OnNextCommand(Object sender, ExecutedRoutedEventArgs e)
        {
            var wizard = sender as Wizard;
            if (wizard != null)
            {
                if (e.Parameter == null)
                {
                    wizard.NextPage();                    
                }
                else if (e.Parameter is WizardPage)
                {
                    wizard.NextPage((WizardPage)e.Parameter);
                }
                else if (e.Parameter is String &&
                         wizard.Pages.Any(page => page.Name == (String)e.Parameter))
                {
                    wizard.NextPageByName((String)e.Parameter);
                }
                else
                {
                    var converter = TypeDescriptor.GetConverter(typeof(int));
                    if (converter.CanConvertFrom(e.Parameter.GetType()))
                    {
                        int index = (int)converter.ConvertFrom(e.Parameter);
                        if (index >= 0 && index < wizard.Pages.Count)
                        {
                            wizard.NextPage(wizard.Pages[index]);
                        }
                    }
                }
            }
        }

        private static void OnNextCommandCanExecute(Object sender, CanExecuteRoutedEventArgs e)
        {
            var wizard = sender as Wizard;
            if (wizard != null)
            {
                e.CanExecute = wizard.CurrentPage != null && wizard.CurrentPage.AllowNext;
                if (e.CanExecute && e.Parameter != null)
                {
                    if (e.Parameter is WizardPage)
                    {
                        e.CanExecute = wizard.Pages.Contains((WizardPage)e.Parameter);
                    }
                    else if (e.Parameter is String &&
                             wizard.Pages.Any(page => page.Name == (String)e.Parameter))
                    {
                        e.CanExecute = true;
                    }
                    else
                    {
                        var converter = TypeDescriptor.GetConverter(typeof(int));
                        if (converter.CanConvertFrom(e.Parameter.GetType()))
                        {
                            int index = (int)converter.ConvertFrom(e.Parameter);
                            e.CanExecute = (index >= 0 && index < wizard.Pages.Count);
                        }
                        else
                        {
                            e.CanExecute = false;
                        }
                    }
                }
            }
        }

        private static void OnFinishCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var wizard = sender as Wizard;
            if (wizard != null)
            {
                wizard.Finish();
            }
        }

        private static void OnFinishCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var wizard = sender as Wizard;
            if (wizard != null)
            {
                e.CanExecute = wizard.IsLastPage && 
                    (wizard.CurrentPage == null || wizard.CurrentPage.AllowFinish);
            }
        }

        private static void OnCancelCommand(object sender, ExecutedRoutedEventArgs e)
        {
            var wizard = sender as Wizard;
            if (wizard != null)
            {
                wizard.Cancel();
            }
        }

        private static void OnCancelCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            var wizard = sender as Wizard;
            if (wizard != null)
            {
                e.CanExecute = wizard.CurrentPage == null || wizard.CurrentPage.AllowCancel;
            }
        }

        #endregion [Commands]

        #region [Private Methods]

        private void HandlePagesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (Pages.Count == 0)
            {
                CurrentPage = null;
            }
            else if (CurrentPage == null)
            {
                if (DesignerProperties.GetIsInDesignMode(this))
                {
                    NextPageByIndex(Designer.GetPageIndex(this));
                }
                else
                {
                    CurrentPage = Pages.FirstOrDefault();
                }
            }
            else
            {
                int index = Math.Min(CurrentPageIndex, Pages.Count);
                CurrentPage = index >= 0 ? Pages[index] : null;
            }

            HasPages = Pages.Count > 0;
            UpdateFirstLastPage(this);

            if (e.Action.In(NotifyCollectionChangedAction.Add, NotifyCollectionChangedAction.Replace))
            {
                foreach (WizardPage wizardPage in e.NewItems)
                {
                    DependencyPropertyDescriptor desc = DependencyPropertyDescriptor.FromProperty(
                        WizardPage.IsFinishPageProperty, typeof (WizardPage));
                    desc.AddValueChanged(wizardPage, OnWizardPageIsFinishChanged);
                }
            }
            else if (e.Action.In(NotifyCollectionChangedAction.Remove, NotifyCollectionChangedAction.Replace))
            {
                foreach (WizardPage wizardPage in e.OldItems)
                {
                    DependencyPropertyDescriptor desc = DependencyPropertyDescriptor.FromProperty(
                        WizardPage.IsFinishPageProperty, typeof(WizardPage));
                    desc.RemoveValueChanged(wizardPage, OnWizardPageIsFinishChanged);
                }
            }
        }

        private void OnWizardPageIsFinishChanged(object sender, EventArgs e)
        {
            UpdateFirstLastPage(this);
            CommandManager.InvalidateRequerySuggested();
        }

        private static void UpdateFirstLastPage(Wizard wizard)
        {
            wizard.IsFirstPage = wizard.CurrentPage == wizard.Pages.FirstOrDefault();
            wizard.IsLastPage = wizard.CurrentPage == wizard.Pages.LastOrDefault() ||
                                (wizard.CurrentPage != null && wizard.CurrentPage.IsFinishPage);
        }

        private void UpdateEffectiveWizardStyle(WizardStyle style)
        {
            if (style == WizardStyle.Auto)
            {
                EffectiveWizardStyle = Environment.OSVersion.Version.Major >= 6 ?
                    WizardStyle.Aero : WizardStyle.Wizard97;
            }
            else
            {
                EffectiveWizardStyle = style;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (CurrentPage != null)
            {
                CurrentPage.InitializePage(null);
            }
        }

        #endregion [Private Methods]

        #region [Private Fields]

        private readonly WizardPageCollection pages;

        private readonly Stack<WizardPage> pageHistory = new Stack<WizardPage>();

        private INavigationStrategy strategy = new DefaultNavigationStrategy(); 

        #endregion [Private Fields]

        #region [Implementation of IAddChild]

        /// <summary>
        /// Adds a child object. 
        /// </summary>
        /// <param name="value">The child object to add.</param>
        /// <exception cref="ArgumentException">The argument is not a <see cref="WizardPage"/>.</exception>
        void IAddChild.AddChild(object value)
        {
            var page = value as WizardPage;
            if (page == null)
                throw new ArgumentException("The object must be derived from WizardPage", "value");
            
            Pages.Add(page);
        }

        /// <summary>
        /// Adds the text content of a node to the object. 
        /// </summary>
        /// <param name="text">The text to add to the object.</param>
        /// <remarks>The method is not implemented and will throw <see cref="NotImplementedException"/>.</remarks>
        /// <exception cref="NotImplementedException">Always.</exception>
        void IAddChild.AddText(string text)
        {
            throw new NotImplementedException();
        }

        #endregion [Implementation of IAddChild]
    }
}
