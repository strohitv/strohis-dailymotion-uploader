using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Web;

namespace StrohisUploader.ValueConverters
{
	class TextToCountOfEscapedTextConverter : IValueConverter
	{
		public object Convert(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			var text = (string)value;
			return HttpUtility.UrlEncode(text).Length;
		}

		public object ConvertBack(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			return string.Empty;
		}
	}
}
