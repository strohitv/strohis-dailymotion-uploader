using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace StrohisUploader.ValueConverters
{
	class NotNullToBoolMultiConverter : IMultiValueConverter
	{
		public object Convert(object[] value, Type targetType,
			object parameter, CultureInfo culture)
		{
			if (value != null)
			{
				foreach (var singleValue in value)
				{
					if (singleValue == null)
					{
						return false;
					}
				}
			}
			return true;
		}


		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			return new object[targetTypes.Length];
		}
	}
}
