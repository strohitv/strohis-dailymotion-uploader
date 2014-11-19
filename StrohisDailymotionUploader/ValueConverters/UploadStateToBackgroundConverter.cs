using StrohisUploadLib.Dailymotion;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace StrohisUploader.ValueConverters
{
	public class UploadStateToBackgroundConverter : IMultiValueConverter
	{
		//public object Convert(object value, Type targetType,
		//	object parameter, CultureInfo culture)
		//{
		//	UploadElement element = (UploadElement)parameter;

		//	if (!element.IsRunning && !element.Finished && !element.Failed)
		//	{
		//		// Noch nicht begonnen -> Weiß
		//		return new SolidColorBrush(Colors.White);
		//	}
		//	else if (!element.IsRunning && element.Finished && !element.Failed)
		//	{
		//		// Erfolgreich Abgeschlossen -> Hellgrün
		//		return new SolidColorBrush(Colors.LightGreen);
		//	}
		//	else if (!element.IsRunning && element.Failed)
		//	{
		//		// Fehler -> Hellrot
		//		return new BrushConverter().ConvertFromString("#FFFFBFBF");
		//	}
		//	else if (element.IsRunning)
		//	{
		//		// 
		//		GradientStopCollection collection = new GradientStopCollection();
		//		collection.Add(new GradientStop(Colors.LightSkyBlue, element.Percentage / 100));
		//		collection.Add(new GradientStop(Colors.White, element.Percentage / 100));

		//		LinearGradientBrush progressBrush = new LinearGradientBrush(collection, new Point(0.0, 0.0), new Point(1.0, 0.0));

		//		return progressBrush;
		//	}

		//	// Create a LinearGradientBrush and use it to
		//	// paint the rectangle.
		//	//myBrush.GradientStops.Add(new GradientStop(Colors.Yellow, 0.0));
		//	//myBrush.GradientStops.Add(new GradientStop(Colors.Orange, 0.5));
		//	//myBrush.GradientStops.Add(new GradientStop(Colors.Red, 1.0));

		//	return null;
		//}

		//public object ConvertBack(object value, Type targetType,
		//	object parameter, CultureInfo culture)
		//{
		//	return null;
		//}

		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			UploadElement element = (UploadElement)values[1];

			if (!element.IsRunning && !element.Finished && !element.Failed)
			{
				// Noch nicht begonnen -> Weiß
				return new SolidColorBrush(Colors.White);
			}
			else if (!element.IsRunning && element.Finished && !element.Failed)
			{
				// Erfolgreich Abgeschlossen -> Hellgrün
				return new SolidColorBrush(Colors.LightGreen);
			}
			else if (!element.IsRunning && element.Failed)
			{
				// Fehler -> Hellrot
				return new BrushConverter().ConvertFromString("#FFFFBFBF");
			}
			else if (element.IsRunning)
			{
				// 
				GradientStopCollection collection = new GradientStopCollection();
				collection.Add(new GradientStop(Colors.LightSkyBlue, element.Percentage / 100));
				collection.Add(new GradientStop(Colors.White, element.Percentage / 100));

				LinearGradientBrush progressBrush = new LinearGradientBrush(collection, new Point(0.0, 0.0), new Point(1.0, 0.0));

				return progressBrush;
			}

			// Create a LinearGradientBrush and use it to
			// paint the rectangle.
			//myBrush.GradientStops.Add(new GradientStop(Colors.Yellow, 0.0));
			//myBrush.GradientStops.Add(new GradientStop(Colors.Orange, 0.5));
			//myBrush.GradientStops.Add(new GradientStop(Colors.Red, 1.0));

			return new SolidColorBrush(Colors.White);
		}

		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
