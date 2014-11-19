using System;

namespace StrohisUploadLib.Dailymotion
{
	public class UploadCompletedEventArgs
	{
		public UploadCompletedEventArgs(string message, TimeSpan duration)
		{
			this.message = message;
			this.duration = duration;
		}

		private string message;
		private bool error;
		private TimeSpan duration;

		public string Message
		{
			get { return message; }
			set { message = value; }
		}
		public bool Error
		{
			get { return error; }
			set { error = value; }
		}
		public TimeSpan Duration
		{
			get { return duration; }
			set { duration = value; }
		}
	}

	public class UploadStateChangedEventArgs
	{
		public UploadStateChangedEventArgs(string message)
		{
			this.message = message;
		}

		private string message;

		public string Message
		{
			get { return message; }
			set { message = value; }
		}
	}
}
