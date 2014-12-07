using System;
using System.ComponentModel;

namespace StrohisUploadLib.Dailymotion
{
	public class UploadCompletedEventArgs : EventArgs
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

	public class ProblemEventArgs : EventArgs
	{
		public ProblemEventArgs(string message, ErrorCodes errorCode, Exception exception)
		{
			this.message = message;
			this.exception = exception;
			this.errorCode = errorCode;
		}

		private string message;
		private Exception exception;
		private ErrorCodes errorCode;

		public string Message
		{
			get { return message; }
			set { message = value; }
		}
		public Exception Exception
		{
			get { return exception; }
			set { exception = value; }
		}
		public ErrorCodes ErrorCode
		{
			get { return errorCode; }
			set { errorCode = value; }
		}
	}

	public class CurrentTasksChangedEventArgs : EventArgs
	{
		public CurrentTasksChangedEventArgs(BindingList<Task> tasks)
		{
			this.Tasks = tasks;
		}

		private BindingList<Task> tasks;
		public BindingList<Task> Tasks
		{
			get { return tasks; }
			set { tasks = value; }
		}
	}

	public class UploadStateChangedEventArgs : EventArgs
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
