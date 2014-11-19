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
	public class ChannelToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			Channel chan = (Channel)value;
			return DmUploadConstants.Channels[(int)chan];
		}

		public object ConvertBack(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			string chan = (string)value;
			return (Channel)Array.IndexOf(DmUploadConstants.Channels, chan);
		}
	}
}
