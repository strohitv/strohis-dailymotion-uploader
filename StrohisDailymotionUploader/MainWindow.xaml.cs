using StrohisUploader.Controls;
using StrohisUploadLib.Dailymotion;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Web;
using System.Collections.Generic;
using StrohisUploader.Dialogs;
using System.Xml;

namespace StrohisUploader
{
	/// <summary>
	/// Interaktionslogik für MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window, INotifyPropertyChanged
	{
		public MainWindow()
		{
			this.DataContext = DmUploader;

			InitializeComponent();

			//if (File.Exists("accounts.xml"))
			//{
			//	PasswordDialog passwordDialog = new PasswordDialog();
			//	//passwordDialog.Owner = this;
			//	passwordDialog.ShowDialog();

			//	if (passwordDialog.EnteredPasswort && !string.IsNullOrEmpty(passwordDialog.Password))
			//	{
			//		DmUploader.LoadAccounts(passwordDialog.Password);
			//	}
			//}

			//UploadElements = new ObservableCollection<UploadElement>();
			//Queue = new ObservableCollection<UploadElement>();
			//Accounts = new ObservableCollection<Account>();

			//Authenticator.Accounts.Add(new Account("StrohiZock", "abc00012"));
		}

		public Uploader DmUploader = new Uploader();

		//private ObservableCollection<UploadElement> uploadElements;
		//public ObservableCollection<UploadElement> UploadElements
		//{
		//	get
		//	{
		//		return uploadElements;
		//	}
		//	set
		//	{
		//		uploadElements = value;
		//		OnPropertyChanged("UploadElements");
		//	}
		//}

		//private ObservableCollection<UploadElement> queue;
		//public ObservableCollection<UploadElement> Queue
		//{
		//	get
		//	{
		//		return queue;
		//	}
		//	set
		//	{
		//		queue = value;
		//		OnPropertyChanged("Queue");
		//	}
		//}

		////private ObservableCollection<Account> accounts;
		//public IList<Account> Accounts
		//{
		//	get
		//	{
		//		//return accounts;
		//		return Authenticator.Accounts;
		//	}
		//	//set
		//	//{
		//	//	accounts = value;
		//	//	OnPropertyChanged("Accounts");
		//	//}
		//}

		public string[] LanguageStrings
		{
			get
			{
				return DmUploadConstants.Language;
			}
		}

		public string[] ChannelStrings
		{
			get
			{
				return DmUploadConstants.Channels;
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;
		public void OnPropertyChanged(string name)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(name));
			}
		}

		private void btnRemoveCurrentClick(object sender, RoutedEventArgs e)
		{
			DmUploader.UploadElements.Remove((UploadElement)cbxUploadElements.SelectedItem);
			cbxUploadElements.SelectedIndex = 0;
		}

		private void btnApplyToQueueClick(object sender, RoutedEventArgs e)
		{
			bool isAlredyInQueue = false;

			foreach (var singleElement in DmUploader.Queue)
			{
				if ((UploadElement)cbxUploadElements.SelectedItem == singleElement)
				{
					isAlredyInQueue = true;
					break;
				}
			}
			if (!isAlredyInQueue)
			{
				int index = cbxUploadElements.SelectedIndex;
				DmUploader.Queue.Add((UploadElement)cbxUploadElements.SelectedItem);
				DmUploader.UploadElements.Remove((UploadElement)cbxUploadElements.SelectedItem);
				if (index == cbxUploadElements.Items.Count)
				{
					cbxUploadElements.SelectedIndex = index - 1;
				}
				else
				{
					cbxUploadElements.SelectedIndex = index;
				}

				foreach (var singleItem in DmUploader.Queue)
				{
					singleItem.UploadFinished -= this.ReactToSingleElementUploadFinished;
					singleItem.UploadFinished += this.ReactToSingleElementUploadFinished;
				}
			}
			else
			{
				int index = cbxUploadElements.SelectedIndex;

				DmUploader.UploadElements.Remove((UploadElement)cbxUploadElements.SelectedItem);
				if (index == cbxUploadElements.Items.Count)
				{
					cbxUploadElements.SelectedIndex = index - 1;
				}
				else
				{
					cbxUploadElements.SelectedIndex = index;
				}
			}

			//((UploadElement)cbxUploadElements.SelectedItem).Start();
			//Queue[0].CurrentState.Percentage = 40.0;
		}

		private void OpenFile(string[] filenames)
		{
			foreach (string singlePath in filenames)
			{
				var openedFileFileInfo = new FileInfo(singlePath);
				var extension = openedFileFileInfo.Extension;

				if (extension.ToLower().Equals(".csv"))
				{
					var videosOfCsv = DmVideoImporter.ImportCSV(singlePath);
					foreach (UploadElement singleUploadElementOfCsv in videosOfCsv)
					{
						DmUploader.UploadElements.Add(singleUploadElementOfCsv);
					}
				}
				else
				{
					DmUploader.UploadElements.Add(DmVideoImporter.ImportSingleVideo(singlePath));
					if (DmUploader.Accounts.Count > 0)
					{
						foreach (var singleVideo in DmUploader.UploadElements)
						{
							if (singleVideo.UploadAccount == null)
							{
								singleVideo.UploadAccount = DmUploader.Accounts[0];
							}
						}
					}
				}
			}
		}

		private void btnStartQueueClick(object sender, RoutedEventArgs e)
		{
			if (DmUploader.Queue.Count > 0 && allRunningElements.Count == 0)
			{
				foreach (var singleElement in DmUploader.Queue)
				{
					if (!singleElement.IsRunning && !singleElement.Failed && !singleElement.Finished)
					{
						singleElement.StartAsync();
						allRunningElements.Add(singleElement);
						break;
					}
				}
			}
		}

		ObservableCollection<UploadElement> allRunningElements = new ObservableCollection<UploadElement>();

		private void ReactToSingleElementUploadFinished(object sender, UploadCompletedEventArgs e)
		{
			allRunningElements.Remove((UploadElement)sender);

			foreach (var singleElement in DmUploader.Queue)
			{
				if (singleElement.IsRunning)
				{
					allRunningElements.Add(singleElement);
				}
			}

			if (allRunningElements.Count == 0)
			{
				foreach (var singleElement in DmUploader.Queue)
				{
					if (!singleElement.IsRunning && !singleElement.Failed && !singleElement.Finished)
					{
						singleElement.StartAsync();
						break;
					}
				}
			}
		}

		private void mainwindowClosing(object sender, CancelEventArgs e)
		{
			foreach (var singleItem in allRunningElements)
			{
				singleItem.Abort();
			}
		}

		private void btnAbortClick(object sender, RoutedEventArgs e)
		{
			foreach (var singleItem in allRunningElements)
			{
				singleItem.Abort();
			}
		}

		private void btnApplyAllToQueueClick(object sender, RoutedEventArgs e)
		{
			while (DmUploader.UploadElements.Count > 0)
			{
				bool isAlredyInQueue = false;

				foreach (var singleElement in DmUploader.Queue)
				{
					if (DmUploader.UploadElements[0] == singleElement)
					{
						isAlredyInQueue = true;
						break;
					}
				}
				if (!isAlredyInQueue)
				{
					DmUploader.Queue.Add(DmUploader.UploadElements[0]);
					DmUploader.UploadElements.Remove(DmUploader.UploadElements[0]);

					foreach (var singleItem in DmUploader.Queue)
					{
						singleItem.UploadFinished -= this.ReactToSingleElementUploadFinished;
						singleItem.UploadFinished += this.ReactToSingleElementUploadFinished;
					}
				}
				else
				{
					DmUploader.UploadElements.Remove(DmUploader.UploadElements[0]);
				}
			}
		}

		private void btnEditQueueItemClick(object sender, RoutedEventArgs e)
		{
			Button sendingbutton = (Button)sender;
			UploadElement sendingElement = null;

			foreach (var singleElement in DmUploader.Queue)
			{
				if (singleElement.Path.Equals(sendingbutton.Tag))
				{
					sendingElement = singleElement;
					break;
				}
			}

			if (sendingElement != null)
			{
				DmUploader.UploadElements.Add(sendingElement);
				cbxUploadElements.SelectedItem = sendingElement;

				mainTabControl.SelectedItem = newUploadTab;
			}
		}

		private void btnAbortQueueItemClick(object sender, RoutedEventArgs e)
		{
			Button sendingbutton = (Button)sender;
			UploadElement sendingElement = null;

			foreach (var singleElement in DmUploader.Queue)
			{
				if (singleElement.Path.Equals(sendingbutton.Tag))
				{
					sendingElement = singleElement;
					break;
				}
			}

			if (sendingElement != null)
			{
				sendingElement.Abort();

				allRunningElements.Remove((UploadElement)sendingElement);

				if (!sendingElement.SeperateJob)
				{
					foreach (var singleElement in DmUploader.Queue)
					{
						if (singleElement.IsRunning)
						{
							allRunningElements.Add(singleElement);
						}
					}

					if (allRunningElements.Count == 0)
					{
						foreach (var singleElement in DmUploader.Queue)
						{
							if (!singleElement.IsRunning && !singleElement.Failed && !singleElement.Finished)
							{
								singleElement.StartAsync();
								break;
							}
						}
					}
				}
			}
		}

		private void btnRemoveQueueItemClick(object sender, RoutedEventArgs e)
		{
			Button sendingbutton = (Button)sender;
			UploadElement sendingElement = null;

			foreach (var singleElement in DmUploader.Queue)
			{
				if (singleElement.Path.Equals(sendingbutton.Tag))
				{
					sendingElement = singleElement;
					break;
				}
			}

			if (sendingElement != null)
			{
				sendingElement.Abort();

				allRunningElements.Remove(sendingElement);
				DmUploader.Queue.Remove(sendingElement);
			}
		}

		private void btnStartQueueItemClick(object sender, RoutedEventArgs e)
		{
			Button sendingbutton = (Button)sender;
			UploadElement sendingElement = null;

			foreach (var singleElement in DmUploader.Queue)
			{
				if (singleElement.Path.Equals(sendingbutton.Tag))
				{
					sendingElement = singleElement;
					break;
				}
			}

			if (sendingElement != null)
			{
				sendingElement.StartAsync();
				sendingElement.SeperateJob = true;
				allRunningElements.Add(sendingElement);
			}
		}

		private void btnAddNewAccountClick(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(txtbxUsername.Text) && !string.IsNullOrWhiteSpace(pwbxPassword.Password))
			{
				try
				{
					Account newAccount = new Account(txtbxUsername.Text, pwbxPassword.Password);
					if ((bool)chbxAuthenticateAddedUser.IsChecked)
					{
						Dialogs.Browser AuthBrowser = new Dialogs.Browser() { UrlToNavigate = Authenticator.GetAuthorizationUrl(newAccount) };
						if (!string.IsNullOrWhiteSpace(AuthBrowser.UrlToNavigate))
						{
							AuthBrowser.ShowDialog();
						}
					}
					bool accountIsAlreadyRegistered = false;
					foreach (var singleAccount in DmUploader.Accounts)
					{
						if (singleAccount.User.Equals(newAccount.User))
						{
							accountIsAlreadyRegistered = true;
							break;
						}
					}
					if (!accountIsAlreadyRegistered)
					{
						DmUploader.Accounts.Add(newAccount);
					}
					txtbxUsername.Text = pwbxPassword.Password = "";
					txtbxUsername.Focus();
				}
				catch (Exception ex)
				{
					MessageBox.Show("Oh nein! Es gab einen Fehler! Die Fehlermeldung wird in einer Datei mit der Endung '.fail' gespeichert, die sich im selben Ordner wie die .exe des Uploaders befindet. Bitte leite sie an @Strohi weiter. Die Fehlermeldung lautet wie folgt: " + ex.Message, "Fehler!", MessageBoxButton.OK, MessageBoxImage.Error);

					int i = 0;
					while (File.Exists(string.Format("Fehler_{0}.fail", i)))
					{
						i++;
					}

					XmlWriter writer = XmlWriter.Create(string.Format("Fehler_{0}.fail", i));

					new ExceptionXElement(ex, true).WriteTo(writer);
					writer.Flush();
					writer.Close();
					Console.WriteLine();
				}
			}

			//DmUploader.Accounts.Add(new Account("Nickname", string.Empty));
			//lstbxAccounts.SelectedItem = DmUploader.Accounts[DmUploader.Accounts.Count - 1];

			//Dialogs.Browser AuthBrowser = new Dialogs.Browser() { UrlToNavigate = Authenticator.GetAuthorizationUrl() };
			//if (!string.IsNullOrWhiteSpace(AuthBrowser.UrlToNavigate))
			//{
			//	AuthBrowser.ShowDialog();
			//}
		}

		private void HyperlinkClick(object sender, RoutedEventArgs e)
		{
			var something = (Hyperlink)sender;
			Process.Start(something.NavigateUri.AbsoluteUri);
		}

		private void txtbxDescriptionTextChanged(object sender, TextChangedEventArgs e)
		{
			string textString = txtbxDescription.Text;

			bool changed = false;

			while (HttpUtility.UrlEncode(textString).Length > 2000)
			{
				textString = textString.Remove(textString.Length - 1);
				changed = true;
			}

			if (changed)
			{
				txtbxDescription.Text = textString;
			}
			txtbxDescription.Select(txtbxDescription.Text.Length, 0);
		}

		private void txtbxThumbnailTextChanged(object sender, TextChangedEventArgs e)
		{
			if (File.Exists(txtbxThumbnail.Text))
			{
				FileInfo info = new FileInfo(txtbxThumbnail.Text);
				if (info.Length > 600 * 1024)
				{
					MessageBox.Show("Das Thumbnail ist größer als 600 kB, daher kann ein Upload nicht stattfinden! Es wird beim Upload übersprungen.");
				}
			}
		}

		private void btnSelectThumbnailClick(object sender, RoutedEventArgs e)
		{
			// Configure open file dialog box
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
			dlg.Filter = "Bilder|*.png;*.jpg;*.jpeg;*.gif|Alle Dateien|*.*"; // Filter files by extension
			dlg.Title = "Wählen Sie die zu öffnenden Videos und csv-Dateien aus:";
			dlg.CheckFileExists = true;

			// Show open file dialog box
			Nullable<bool> result = dlg.ShowDialog();

			// Process open file dialog box results
			if (result == true)
			{
				// Open document
				txtbxThumbnail.Text = dlg.FileName;
			}
		}

		private void btnAddVideosClick(object sender, RoutedEventArgs e)
		{
			// Configure open file dialog box
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
			dlg.Filter = "Empfohlene Formate|*.csv;*.mkv;*.mp4;*.avi;*.wmv;*.mov|Comma-separated values|*.csv|Matroska|*.mkv|MPEG-4-Video|*.mp4|Audio Video Interleave|*.avi|Windows Media Video|*.wmv|Quicktime-Movie|*.mov|Alle Dateien|*.*"; // Filter files by extension
			dlg.Multiselect = true;
			dlg.Title = "Wählen Sie die zu öffnenden Videos und csv-Dateien aus:";
			dlg.CheckFileExists = true;

			// Show open file dialog box
			Nullable<bool> result = dlg.ShowDialog();

			// Process open file dialog box results
			if (result == true)
			{
				// Open document
				string[] filenames = dlg.FileNames;
				OpenFile(filenames);
			}
		}

		private void btnSavePasswordClick(object sender, RoutedEventArgs e)
		{
			((Account)lstbxAccounts.SelectedItem).Password = pwbxPassword.Password;
		}

		//private void btnAuthenticateClick(object sender, RoutedEventArgs e)
		//{
		//	Browser AuthBrowser = new Browser() { UrlToNavigate = Authenticator.GetAuthorizationUrl((Account)lstbxAccounts.SelectedItem) };
		//	if (!string.IsNullOrWhiteSpace(AuthBrowser.UrlToNavigate))
		//	{
		//		AuthBrowser.Owner = this;
		//		AuthBrowser.ShowDialog();
		//	}
		//}

		private void btnSaveEditAccountClick(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(txtbxEditUsername.Text) && !string.IsNullOrWhiteSpace(pwbxEditPassword.Password))
			{
				Account accountToEdit = null;
				foreach (var singleAccount in DmUploader.Accounts)
				{
					if (singleAccount.User.Equals(txtbxEditUsername.Text))
					{
						accountToEdit = singleAccount;
						break;
					}
				}
				accountToEdit.Password = pwbxEditPassword.Password;
				if ((bool)chbxAuthenticateAddedUser.IsChecked)
				{
					Browser AuthBrowser = new Browser() { UrlToNavigate = Authenticator.GetAuthorizationUrl(accountToEdit) };
					if (!string.IsNullOrWhiteSpace(AuthBrowser.UrlToNavigate))
					{
						AuthBrowser.Owner = this;
						AuthBrowser.ShowDialog();
					}
				}
			}
		}

		private void lstbxAccountsSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Account accountToEdit = (Account)e.AddedItems[0];
			txtbxEditUsername.Text = accountToEdit.User;
			pwbxEditPassword.Password = accountToEdit.Password;
		}

		private void btnSaveAccountXmlClick(object sender, RoutedEventArgs e)
		{
			PasswordDialog passwordDialog = new PasswordDialog();
			passwordDialog.Owner = this;
			passwordDialog.ShowDialog();

			if (passwordDialog.EnteredPasswort && !string.IsNullOrEmpty(passwordDialog.Password))
			{
				DmUploader.SaveAccounts(passwordDialog.Password);
			}
		}

		private void btnLoadAccountXmlClick(object sender, RoutedEventArgs e)
		{
			if (File.Exists("accounts.xml"))
			{
				PasswordDialog passwordDialog = new PasswordDialog();
				passwordDialog.Owner = this;
				passwordDialog.ShowDialog();

				if (passwordDialog.EnteredPasswort && !string.IsNullOrEmpty(passwordDialog.Password))
				{
					DmUploader.LoadAccounts(passwordDialog.Password);
				}
			}
			else
			{
				MessageBox.Show("Keine Datei mit Accountdaten gefunden. Die Datei muss 'accounts.xml' heißen und im selben Verzeichnis wie der Uploader liegen.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void mainwindowLoaded(object sender, RoutedEventArgs e)
		{
			if (File.Exists("accounts.xml"))
			{
				PasswordDialog passwordDialog = new PasswordDialog();
				passwordDialog.Owner = this;
				passwordDialog.ShowDialog();

				if (passwordDialog.EnteredPasswort && !string.IsNullOrEmpty(passwordDialog.Password))
				{
					DmUploader.LoadAccounts(passwordDialog.Password);
				}
			}
		}

		private void btnDeletePlaylistItemClick(object sender, RoutedEventArgs e)
		{
			Account accountToDeleteFrom = (Account)cmbbxAccountForPlaylistTab.SelectedItem;
			DmUploader.DeletePlaylistOfAccount(accountToDeleteFrom, (string)((Button)sender).Tag);
		}

		private void btnCreateNewPlaylistClick(object sender, RoutedEventArgs e)
		{
			//DmUploader.CreateNewGroupForAccount((Account)cmbbxAccountForPlaylistTab.SelectedItem, "uuurrrlll111222333", "uuurrrlll111222", "uuurrrlll111");
			//return;

			if (cmbbxAccountForPlaylistTab.SelectedItem != null && !string.IsNullOrWhiteSpace(txtbxNewPlaylistName.Text) && !string.IsNullOrWhiteSpace(txtbxNewPlaylistDescription.Text))
			{
				DmUploader.CreateNewPlaylistForAccount((Account)cmbbxAccountForPlaylistTab.SelectedItem, txtbxNewPlaylistName.Text, txtbxNewPlaylistDescription.Text);
			}
		}

		private void btnRefreshPlaylistsClick(object sender, RoutedEventArgs e)
		{
			DmUploader.RefreshPlaylistsOfAccount((Account)cmbbxAccountForPlaylistTab.SelectedItem);
		}

		private void btnCreateNewGroupClick(object sender, RoutedEventArgs e)
		{
			if (cmbbxAccountForGroupTab.SelectedItem != null && !string.IsNullOrWhiteSpace(txtbxNewGroupName.Text) && !string.IsNullOrWhiteSpace(txtbxNewGroupDescription.Text) && !string.IsNullOrWhiteSpace(txtbxNewGroupShortUrl.Text))
			{
				DmUploader.CreateNewGroupForAccount((Account)cmbbxAccountForGroupTab.SelectedItem, txtbxNewGroupName.Text, txtbxNewGroupDescription.Text, txtbxNewGroupShortUrl.Text);
			}
		}

		private void btnRefreshGroupsClick(object sender, RoutedEventArgs e)
		{
			DmUploader.RefreshGroupsOfAccount((Account)cmbbxAccountForGroupTab.SelectedItem);
		}

		private void btnDeleteGroupItemClick(object sender, RoutedEventArgs e)
		{
			Account accountToDeleteFrom = (Account)cmbbxAccountForPlaylistTab.SelectedItem;
			DmUploader.DeletePlaylistOfAccount(accountToDeleteFrom, (string)((Button)sender).Tag);
		}
	}
}
