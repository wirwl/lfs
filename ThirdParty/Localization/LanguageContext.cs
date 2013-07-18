using System;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.ComponentModel;
using System.Windows.Data;

namespace Tomers.WPF.Localization
{
	public sealed class LanguageContext : INotifyPropertyChanged
	{
		#region Fields

		public static readonly LanguageContext Instance = new LanguageContext();

		private CultureInfo _cultureInfo;
		private LanguageDictionary _dictionary; 

		#endregion

		#region Properties

		public CultureInfo Culture
		{
			get
			{
				if (_cultureInfo == null)
				{
					_cultureInfo = CultureInfo.CurrentUICulture;
				}
				return _cultureInfo;
			}
			set
			{
				if (value == null)
				{
					throw new ArgumentNullException("Culture must not be null");
				}
				if (value == _cultureInfo)
				{
					return;
				}
				if (_cultureInfo != null)
				{
					LanguageDictionary currentDictionary = LanguageDictionary.GetDictionary(_cultureInfo);
					currentDictionary.Unload();
				}
				_cultureInfo = value;
				LanguageDictionary newDictionary = LanguageDictionary.GetDictionary(_cultureInfo);
				Thread.CurrentThread.CurrentUICulture = _cultureInfo;
				newDictionary.Load();
				Dictionary = newDictionary;
				OnPropertyChanged("Culture");
			}
		}

		public LanguageDictionary Dictionary
		{
			get { return _dictionary; }
			set
			{
				if (value != null && value != _dictionary)
				{
					_dictionary = value;
					OnPropertyChanged("Dictionary");
				}
			}
		}
 
		#endregion

		#region Initialization

		private LanguageContext() { } 

		#endregion

		#region INotifyPropertyChanged Members

		private void OnPropertyChanged(string property)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(property));
			}
		}
		
		public event PropertyChangedEventHandler PropertyChanged;

		#endregion
        public static void Binder(string id, string propname, string defvalue, DependencyObject depo, DependencyProperty depp)
        {
            Binding binding = new Binding("Dictionary");
            binding.Source = LanguageContext.Instance;
            LanguageConverter converter = new LanguageConverter(id, propname, defvalue);
            binding.Converter = converter;
            BindingOperations.SetBinding(depo, depp, binding);            
        }
	}
}
