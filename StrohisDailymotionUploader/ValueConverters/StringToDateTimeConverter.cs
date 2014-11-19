using StrohisUploadLib.Dailymotion;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace StrohisUploader.ValueConverters
{
	class StringToDateTimeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			var dateString = (string)value;
			return DateTime.Parse(dateString);
		}

		public object ConvertBack(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			var dateTime = (DateTime)value;
			return dateTime.ToString("yyyy-MM-dd");
		}
	}
}
