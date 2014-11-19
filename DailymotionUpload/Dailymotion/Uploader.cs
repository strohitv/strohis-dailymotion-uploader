using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace StrohisUploadLib.Dailymotion
{
	public class Uploader : INotifyPropertyChanged
	{
		public Uploader()
		{
			UploadElements = new ObservableCollection<UploadElement>();
			Queue = new ObservableCollection<UploadElement>();
			Accounts = new ObservableCollection<Account>();
		}

		private ObservableCollection<UploadElement> uploadElements;
		public ObservableCollection<UploadElement> UploadElements
		{
			get
			{
				return uploadElements;
			}
			set
			{
				uploadElements = value;
				OnPropertyChanged("UploadElements");
			}
		}

		private ObservableCollection<UploadElement> queue;
		public ObservableCollection<UploadElement> Queue
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

		private ObservableCollection<Account> accounts;
		public ObservableCollection<Account> Accounts
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

		public void SaveAccounts(string password, string fileLocation = "accounts.xml")
		{
			XmlManager.WriteDecryptedAccountXml(Accounts, password, fileLocation);
		}

		public void LoadAccounts(string password, string fileLocation = "accounts.xml")
		{
			var loadedAccounts = XmlManager.ReadEncryptedAccountXml(password, fileLocation);

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
					singleLoadedAccount.LoadPlaylists();
					singleLoadedAccount.LoadGroups();
					Accounts.Add(singleLoadedAccount);
				}
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
	}
}
