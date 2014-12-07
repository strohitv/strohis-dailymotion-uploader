using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Threading;
using System.Web;

namespace StrohisUploadLib.Dailymotion
{
	public class Uploader : INotifyPropertyChanged
	{
		public Uploader()
		{
			VideosToEdit = new BindingList<Video>();
			Queue = new BindingList<Video>();
			Accounts = new BindingList<Account>();
			currentTasks = new BindingList<string>();

			HttpRequestCachePolicy policy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
			HttpWebRequest.DefaultCachePolicy = policy;
		}

		private BindingList<Video> videosToEdit;
		public BindingList<Video> VideosToEdit
		{
			get
			{
				return videosToEdit;
			}
			set
			{
				videosToEdit = value;
				OnPropertyChanged("UploadElements");
			}
		}

		private BindingList<Video> queue;
		public BindingList<Video> Queue
		{
			get
			{
				return queue;
			}
			set
			{
				queue = value;
				OnPropertyChanged("Queue");
			}
		}

		private BindingList<Account> accounts;
		public BindingList<Account> Accounts
		{
			get
			{
				return accounts;
			}
			set
			{
				accounts = value;
				OnPropertyChanged("Accounts");
			}
		}

		private bool isBusy;
		public bool IsBusy
		{
			get
			{
				return isBusy;
			}
			set
			{
				isBusy = value;
				OnPropertyChanged("IsBusy");
			}
		}

		private string tasks;
		public string Tasks
		{
			get
			{
				return tasks;
			}
			set
			{
				tasks = value;
				OnPropertyChanged("Tasks");
			}
		}

		private BindingList<string> currentTasks;

		#region Public Delegates

		public delegate void ProblemEventHandler(ProblemEventArgs e);
		public delegate void CurrentTasksChangedEventHandler(CurrentTasksChangedEventArgs e);

		#endregion Public Delegates

		#region Public Events

		public event ProblemEventHandler ProblemOccured;
		public void OnProblemOccured(string message, ErrorCodes errorCode, Exception exception)
		{
			if (ProblemOccured != null)
			{
				ProblemOccured(new ProblemEventArgs(message, errorCode, exception));
			}
		}

		public event CurrentTasksChangedEventHandler CurrentTasksChanged;
		public void OnCurrentTasksChanged(BindingList<Task> message)
		{
			if (CurrentTasksChanged != null)
			{
				CurrentTasksChanged(new CurrentTasksChangedEventArgs(message));
			}
		}

		#endregion Public Events

		#region NotifyProperty

		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(name));
			}
		}

		#endregion NotifyProperty

		#region TaskMessages

		private const string videoUploadMessage = "Lade Video hoch: {0}";
		private const string loadPlaylistsOfAccountMessage = "Lade Playlists von {0}";
		private const string loadGroupsOfAccountMessage = "Lade Gruppen von {0}";
		private const string refreshAccountsMessage = "Lade Accounts";

		#endregion TaskMessages

		private void AddOrRemoveTask(string message, bool add, params object[] args)
		{
			message = string.Format(message, args);

			if (add)
			{
				currentTasks.Add(message);
				UpdateTaskString();
			}
			else if (currentTasks.Contains(message))
			{
				currentTasks.Remove(message);
				UpdateTaskString();
			}
		}

		private void UpdateTaskString()
		{
			Tasks = string.Empty;

			for (int i = 0; i < currentTasks.Count; i++)
			{
				Tasks += currentTasks[i];

				if (i < currentTasks.Count - 1)
				{
					Tasks += " | ";
				}
			}
		}

		public void SaveAccounts(string password, string fileLocation = "accounts.xml")
		{
			XmlManager.WriteDecryptedAccountXml(Accounts, password, fileLocation);
		}

		public Account[] LoadAndReturnAccounts(string password, string fileLocation = "accounts.xml")
		{
			IsBusy = true;
			AddOrRemoveTask(refreshAccountsMessage, true);

			var loadedAccounts = XmlManager.ReadEncryptedAccountXml(password, fileLocation);

			var accountsToReturn = new List<Account>();

			if (loadedAccounts != null)
			{
				foreach (var singleLoadedAccount in loadedAccounts)
				{
					bool isAlreadyRegistered = false;
					foreach (var singleExistingAccount in Accounts)
					{
						if (singleLoadedAccount.User.ToUpper().Equals(singleExistingAccount.User.ToUpper()))
						{
							isAlreadyRegistered = true;
							break;
						}
					}
					if (!isAlreadyRegistered)
					{
						AddOrRemoveTask(loadPlaylistsOfAccountMessage, true, singleLoadedAccount.User);
						singleLoadedAccount.LoadPlaylists();
						AddOrRemoveTask(loadPlaylistsOfAccountMessage, false, singleLoadedAccount.User);

						AddOrRemoveTask(loadGroupsOfAccountMessage, true, singleLoadedAccount.User);
						singleLoadedAccount.LoadGroups();
						AddOrRemoveTask(loadGroupsOfAccountMessage, false, singleLoadedAccount.User);

						accountsToReturn.Add(singleLoadedAccount);
					}
				}
			}
			else
			{
				OnProblemOccured("Das Passwort ist falsch", ErrorCodes.IncorrectPassword, null);
			}

			//CurrentTasksList.Remove(new Task("Lade Accounts"));
			AddOrRemoveTask(refreshAccountsMessage, false);
			IsBusy = false;

			return accountsToReturn.ToArray();

			//foreach (var singleAccount in Accounts)
			//{
			//	singleAccount.LoadPlaylists();
			//	singleAccount.LoadGroups();
			//}
		}

		public void LoadAccounts(string password, string fileLocation = "accounts.xml")
		{
			var loadedAccounts = XmlManager.ReadEncryptedAccountXml(password, fileLocation);

			if (loadedAccounts != null)
			{
				foreach (var singleLoadedAccount in loadedAccounts)
				{
					bool isAlreadyRegistered = false;
					foreach (var singleExistingAccount in Accounts)
					{
						if (singleLoadedAccount.User.ToUpper().Equals(singleExistingAccount.User.ToUpper()))
						{
							isAlreadyRegistered = true;
							break;
						}
					}
					if (!isAlreadyRegistered)
					{
						Accounts.Add(singleLoadedAccount);
					}
				}
			}
			else
			{
				OnProblemOccured("Das Passwort ist falsch", ErrorCodes.IncorrectPassword, null);
			}

			foreach (var singleAccount in Accounts)
			{
				singleAccount.LoadPlaylists();
				singleAccount.LoadGroups();
			}
		}

		public void CreateNewPlaylistForAccount(Account account, string name, string description)
		{
			name = HttpUtility.UrlEncode(name);
			description = HttpUtility.UrlEncode(description);
			if (name.Length > 50)
			{
				name = name.Remove(50);
			}
			if (description.Length > 2000)
			{
				description = description.Remove(2000);
			}

			Playlist.CreateNewPlaylist(account, name, description);
			Thread.Sleep(2000);
			account.LoadPlaylists();
		}

		public void CreateNewGroupForAccount(Account account, string name, string description, string url_name)
		{
			name = HttpUtility.UrlEncode(name);
			description = HttpUtility.UrlEncode(description);
			url_name = HttpUtility.UrlEncode(url_name);

			if (name.Length > 50)
			{
				name = name.Remove(50);
			}
			if (description.Length > 2000)
			{
				description = description.Remove(2000);
			}
			if (url_name.Length > 35)
			{
				url_name = url_name.Remove(35);
			}

			Group.CreateNewGroup(account, name, description, url_name);
			account.LoadGroups();
		}

		public void DeletePlaylistOfAccount(Account account, string playlistId)
		{
			account.DeletePlaylist(playlistId);
		}

		public void RefreshPlaylistsOfAccount(Account account)
		{
			account.LoadPlaylists();
		}

		public void DeleteGroupOfAccount(Account account, string groupId)
		{
			account.DeleteGroup(groupId);
		}

		public void RefreshGroupsOfAccount(Account account)
		{
			account.LoadGroups();
		}

		public void LogAccountOut(Account account, bool removeFromAccounts = true)
		{
			Authenticator.Logout(account);
			if (removeFromAccounts)
			{
				Accounts.Remove(account);
			}
		}

		public void ShutdownThreaded()
		{
			Thread shutdownThread = new Thread(Shutdown);
			shutdownThread.Name = "ShutdownThread";
			shutdownThread.Start();
		}

		private void Shutdown()
		{
			Stop();

			Authenticator.Stop();

			for (int i = 0; i < Accounts.Count; i++)
			{
				LogAccountOut(Accounts[i], false);
			}
		}

		public void Stop()
		{
			foreach (var singleVideo in Queue)
			{
				if (singleVideo.IsRunning)
				{
					singleVideo.Abort();
				}
			}
		}

		public void ApplySingleItemToQueue(Video video)
		{
			if (video != null)
			{
				bool isAlredyInQueue = false;

				foreach (var singleElement in this.Queue)
				{
					if (video == singleElement)
					{
						isAlredyInQueue = true;
						break;
					}
				}

				if (!isAlredyInQueue)
				{
					this.Queue.Add(video);
				}

				if (this.videosToEdit.Contains(video))
				{
					this.VideosToEdit.Remove(video);
				}
			}
		}

		public void RefreshUploadFinishedEventMethods()
		{
			foreach (var singleItem in this.Queue)
			{
				singleItem.UploadFinished -= this.ReactToSingleElementUploadFinished;
				singleItem.UploadFinished += this.ReactToSingleElementUploadFinished;
			}
		}

		public void ApplyAllToQueue()
		{
			while (this.VideosToEdit.Count > 0)
			{
				bool isAlredyInQueue = false;

				foreach (var singleElement in this.Queue)
				{
					if (this.VideosToEdit[0] == singleElement)
					{
						isAlredyInQueue = true;
						break;
					}
				}
				if (!isAlredyInQueue)
				{
					this.Queue.Add(this.VideosToEdit[0]);
					this.VideosToEdit.Remove(this.VideosToEdit[0]);
				}
				else
				{
					this.VideosToEdit.Remove(this.VideosToEdit[0]);
				}
			}
			RefreshUploadFinishedEventMethods();
		}

		public Video SearchForVideoInQueue(string path)
		{
			Video video = null;

			foreach (var singleVideo in this.Queue)
			{
				if (singleVideo.Path.Equals(path))
				{
					video = singleVideo;
					break;
				}
			}

			return video;
		}

		private void ReactToSingleElementUploadFinished(object sender, UploadCompletedEventArgs e)
		{
			StartFirstWaitingQueueItem();
		}

		public void EditVideo(Video video)
		{
			if (!VideosToEdit.Contains(video))
			{
				VideosToEdit.Add(video);
			}
		}

		public void AbortVideo(Video video)
		{
			video.Abort();

			if (!video.SeperateJob)
			{
				StartFirstWaitingQueueItem();
			}
		}

		public void RemoveVideoFromQueue(Video video)
		{
			if (video.IsRunning)
			{
				AbortVideo(video);
			}

			if (Queue.Contains(video))
			{
				Queue.Remove(video);
			}
		}

		private void StartFirstWaitingQueueItem()
		{
			foreach (var video in this.Queue)
			{
				if (!video.IsRunning && !video.Failed && !video.Finished)
				{
					video.UploadFinished += OnVideoUploadFinished;
					video.StartAsync();
					IsBusy = true;
					AddOrRemoveTask(videoUploadMessage, true, video.Title);
					break;
				}
			}
		}

		public void Start(Video video = null, bool breakAfterUpload = false)
		{
			if (video == null)
			{
				StartFirstWaitingQueueItem();
			}
			else
			{
				if (!Queue.Contains(video))
				{
					Queue.Add(video);
				}

				video.SeperateJob = breakAfterUpload;
				video.UploadFinished += OnVideoUploadFinished;
				video.StartAsync();
				IsBusy = true;
				AddOrRemoveTask(videoUploadMessage, true, video.Title);
			}
		}

		private void OnVideoUploadFinished(object sender, UploadCompletedEventArgs e)
		{
			AddOrRemoveTask(videoUploadMessage, false, ((Video)sender).Title);
			IsBusy = false;
		}
	}
}
