using StrohisUploadLib.Dailymotion;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace StrohisUploader.ValueConverters
{
	class NullToFirstAccountConverter : IMultiValueConverter
	{
		public object Convert(object[] value, Type targetType,
			object parameter, CultureInfo culture)
		{
			if (value[0] != DependencyProperty.UnsetValue)
			{
				Account selectedAccount = (Account)value[0];
				if (selectedAccount == null)
				{
					ObservableCollection<Account> accounts = (ObservableCollection<Account>)value[1];
					if (accounts != null && accounts.Count > 0)
					{
						return accounts[0];
					}
				}
			}
			return null;
		}


		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
