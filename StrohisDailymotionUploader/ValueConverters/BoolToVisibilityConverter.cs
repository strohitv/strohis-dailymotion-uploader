﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace StrohisUploader.ValueConverters
{
	class BoolToVisibilityConverter : IValueConverter
	{
		public object Convert(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			if (parameter == null)
			{
				if ((bool)value)
				{
					return Visibility.Visible;
				}
				else
				{
					return Visibility.Collapsed;
				}
			}
			else
			{
				if ((bool)value)
				{
					return Visibility.Collapsed;
				}
				else
				{
					return Visibility.Visible;
				}
			}
		}

		public object ConvertBack(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			if (parameter == null)
			{
				if ((Visibility)value == Visibility.Visible)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			else
			{
				if ((Visibility)value == Visibility.Visible)
				{
					return false;
				}
				else
				{
					return true;
				}
			}
		}
	}
}
