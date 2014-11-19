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
	public class LanguageToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			Language lang = (Language)value;
			return DmUploadConstants.Language[(int)lang];
		}

		public object ConvertBack(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			string lang = (string)value;
			return (Language)Array.IndexOf(DmUploadConstants.Language, lang);
		}
	}
}
