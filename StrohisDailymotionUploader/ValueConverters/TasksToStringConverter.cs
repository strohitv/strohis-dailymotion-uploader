using StrohisUploadLib.Dailymotion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace StrohisUploader.ValueConverters
{
	public class TasksToStringConverter : IValueConverter
	{
		public object Convert(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			string returnString = string.Empty;

			BindingList<Task> tasks = (BindingList<Task>)value;
			for (int i = 0; i < tasks.Count; i++)
			{
				returnString += tasks[i];

				if (i < tasks.Count - 1)
				{
					returnString += " | ";
				}
			}

			return returnString;
		}

		public object ConvertBack(object value, Type targetType,
			object parameter, CultureInfo culture)
		{
			string[] tasks = ((string)value).Split(new string[] { " | " }, StringSplitOptions.RemoveEmptyEntries);

			BindingList<string> taskList = new BindingList<string>();

			foreach (var singleTask in tasks)
			{
				taskList.Add(singleTask);
			}

			return taskList;
		}
	}
}
