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
using System.Threading;

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
			DmUploader.ProblemOccured += this.ReactOnProblemOccured;
			refreshWorker.DoWork += refreshWorkerDoWork;
			refreshWorker.RunWorkerCompleted += refreshWorkerRunWorkerCompleted;

			InitializeComponent();
		}

		void refreshWorkerRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			foreach (var singleAccount in refreshWorkerLoadedAccounts)
			{
				DmUploader.Accounts.Add(singleAccount);
			}
		}

		private Account[] refreshWorkerLoadedAccounts = null;

		void refreshWorkerDoWork(object sender, DoWorkEventArgs e)
		{
			refreshWorkerLoadedAccounts = DmUploader.LoadAndReturnAccounts((string)e.Argument);
		}

		private void ReactOnProblemOccured(ProblemEventArgs e)
		{
			switch (e.ErrorCode)
			{
				case ErrorCodes.IncorrectPassword:
					MessageBox.Show(this, "Das Passwort ist falsch. Die Accounts werden nicht geladen.", "Falsches Passwort", MessageBoxButton.OK, MessageBoxImage.Error);
					break;
				default:
					break;
			}
		}

		public Uploader DmUploader = new Uploader();

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

		private void BtnRemoveCurrentClick(object sender, RoutedEventArgs e)
		{
			DmUploader.VideosToEdit.Remove((Video)cbxUploadElements.SelectedItem);
			cbxUploadElements.SelectedIndex = 0;
		}

		private void BtnApplyToQueueClick(object sender, RoutedEventArgs e)
		{
			var index = cbxUploadElements.SelectedIndex;

			DmUploader.ApplySingleItemToQueue((Video)cbxUploadElements.SelectedItem);

			if (index > cbxUploadElements.Items.Count - 1)
			{
				cbxUploadElements.SelectedIndex = cbxUploadElements.Items.Count - 1;
			}
			else
			{
				cbxUploadElements.SelectedIndex = index;
			}
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
					foreach (Video singleUploadElementOfCsv in videosOfCsv)
					{
						DmUploader.VideosToEdit.Add(singleUploadElementOfCsv);
					}
				}
				else
				{
					DmUploader.VideosToEdit.Add(DmVideoImporter.ImportSingleVideo(singlePath));
					if (DmUploader.Accounts.Count > 0)
					{
						foreach (var singleVideo in DmUploader.VideosToEdit)
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

		private void BtnStartQueueClick(object sender, RoutedEventArgs e)
		{
			DmUploader.Start();
		}

		private void MainwindowClosing(object sender, CancelEventArgs e)
		{
			DmUploader.ShutdownThreaded();
		}

		private void BtnAbortClick(object sender, RoutedEventArgs e)
		{
			DmUploader.Stop();
		}

		private void BtnApplyAllToQueueClick(object sender, RoutedEventArgs e)
		{
			DmUploader.ApplyAllToQueue();
		}

		private void BtnEditQueueItemClick(object sender, RoutedEventArgs e)
		{
			Button sendingbutton = (Button)sender;
			Video sendingElement = DmUploader.SearchForVideoInQueue((string)sendingbutton.Tag);

			if (sendingElement != null)
			{
				DmUploader.EditVideo(sendingElement);
				cbxUploadElements.SelectedItem = sendingElement;

				mainTabControl.SelectedItem = newUploadTab;
			}
		}

		private void BtnAbortQueueItemClick(object sender, RoutedEventArgs e)
		{
			Button sendingbutton = (Button)sender;
			Video sendingElement = DmUploader.SearchForVideoInQueue((string)sendingbutton.Tag);

			if (sendingElement != null)
			{
				DmUploader.AbortVideo(sendingElement);
			}
		}

		private void BtnRemoveQueueItemClick(object sender, RoutedEventArgs e)
		{
			Button sendingbutton = (Button)sender;
			Video sendingElement = DmUploader.SearchForVideoInQueue((string)sendingbutton.Tag);

			if (sendingElement != null)
			{
				DmUploader.RemoveVideoFromQueue(sendingElement);
			}
		}

		private void BtnStartQueueItemClick(object sender, RoutedEventArgs e)
		{
			Button sendingbutton = (Button)sender;
			Video sendingElement = DmUploader.SearchForVideoInQueue((string)sendingbutton.Tag);

			if (sendingElement != null)
			{
				DmUploader.Start(sendingElement, true);
			}
		}

		private void BtnAddNewAccountClick(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrWhiteSpace(txtbxUsername.Text) && !string.IsNullOrWhiteSpace(pwbxPassword.Password))
			{
				Account newAccount = new Account(txtbxUsername.Text, pwbxPassword.Password);
				if ((bool)chbxAuthenticateAddedUser.IsChecked)
				{
					Dialogs.Browser AuthBrowser = new Dialogs.Browser() { UrlToNavigate = Authenticator.GetAuthorizationUrl(newAccount), User = newAccount.User };
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
		}

		private void HyperlinkClick(object sender, RoutedEventArgs e)
		{
			var something = (Hyperlink)sender;
			Process.Start(something.NavigateUri.AbsoluteUri);
		}

		private void TxtbxDescriptionTextChanged(object sender, TextChangedEventArgs e)
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

		private void TxtbxThumbnailTextChanged(object sender, TextChangedEventArgs e)
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

		private void BtnSelectThumbnailClick(object sender, RoutedEventArgs e)
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

		private void BtnAddVideosClick(object sender, RoutedEventArgs e)
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

		private void BtnSavePasswordClick(object sender, RoutedEventArgs e)
		{
			((Account)lstbxAccounts.SelectedItem).Password = pwbxPassword.Password;
		}

		private void BtnSaveEditAccountClick(object sender, RoutedEventArgs e)
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

		private void LstbxAccountsSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			Account accountToEdit = (Account)e.AddedItems[0];
			txtbxEditUsername.Text = accountToEdit.User;
			pwbxEditPassword.Password = accountToEdit.Password;
		}

		private void BtnSaveAccountXmlClick(object sender, RoutedEventArgs e)
		{
			PasswordDialog passwordDialog = new PasswordDialog();
			passwordDialog.Owner = this;
			passwordDialog.ShowDialog();

			if (passwordDialog.EnteredPassword && !string.IsNullOrEmpty(passwordDialog.Password))
			{
				DmUploader.SaveAccounts(passwordDialog.Password);
				refreshWorker.RunWorkerAsync(passwordDialog.Password);
			}
		}

		private void BtnLoadAccountXmlClick(object sender, RoutedEventArgs e)
		{
			if (File.Exists("accounts.xml"))
			{
				PasswordDialog passwordDialog = new PasswordDialog();
				passwordDialog.Owner = this;
				passwordDialog.ShowDialog();

				if (passwordDialog.EnteredPassword && !string.IsNullOrEmpty(passwordDialog.Password))
				{
					refreshWorker.RunWorkerAsync(passwordDialog.Password);
					//DmUploader.LoadAccounts(passwordDialog.Password);
				}
			}
			else
			{
				MessageBox.Show("Keine Datei mit Accountdaten gefunden. Die Datei muss 'accounts.xml' heißen und im selben Verzeichnis wie der Uploader liegen.", "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private readonly BackgroundWorker refreshWorker = new BackgroundWorker();

		private void MainwindowLoaded(object sender, RoutedEventArgs e)
		{
			if (File.Exists("accounts.xml"))
			{
				PasswordDialog passwordDialog = new PasswordDialog();
				passwordDialog.Owner = this;
				passwordDialog.ShowDialog();

				if (passwordDialog.EnteredPassword && !string.IsNullOrEmpty(passwordDialog.Password))
				{
					refreshWorker.RunWorkerAsync(passwordDialog.Password);
					//DmUploader.LoadAccounts(passwordDialog.Password);
				}
			}
		}

		private void BtnDeletePlaylistItemClick(object sender, RoutedEventArgs e)
		{
			Account accountToDeleteFrom = (Account)cmbbxAccountForPlaylistTab.SelectedItem;
			DmUploader.DeletePlaylistOfAccount(accountToDeleteFrom, (string)((Button)sender).Tag);
		}

		private void BtnCreateNewPlaylistClick(object sender, RoutedEventArgs e)
		{
			if (cmbbxAccountForPlaylistTab.SelectedItem != null && !string.IsNullOrWhiteSpace(txtbxNewPlaylistName.Text) && !string.IsNullOrWhiteSpace(txtbxNewPlaylistDescription.Text))
			{
				DmUploader.CreateNewPlaylistForAccount((Account)cmbbxAccountForPlaylistTab.SelectedItem, txtbxNewPlaylistName.Text, txtbxNewPlaylistDescription.Text);
			}
		}

		private void BtnRefreshPlaylistsClick(object sender, RoutedEventArgs e)
		{
			DmUploader.RefreshPlaylistsOfAccount((Account)cmbbxAccountForPlaylistTab.SelectedItem);
		}

		private void BtnCreateNewGroupClick(object sender, RoutedEventArgs e)
		{
			if (cmbbxAccountForGroupTab.SelectedItem != null && !string.IsNullOrWhiteSpace(txtbxNewGroupName.Text) && !string.IsNullOrWhiteSpace(txtbxNewGroupDescription.Text) && !string.IsNullOrWhiteSpace(txtbxNewGroupShortUrl.Text))
			{
				DmUploader.CreateNewGroupForAccount((Account)cmbbxAccountForGroupTab.SelectedItem, txtbxNewGroupName.Text, txtbxNewGroupDescription.Text, txtbxNewGroupShortUrl.Text);
			}
		}

		private void BtnRefreshGroupsClick(object sender, RoutedEventArgs e)
		{
			DmUploader.RefreshGroupsOfAccount((Account)cmbbxAccountForGroupTab.SelectedItem);
		}

		private void BtnDeleteGroupItemClick(object sender, RoutedEventArgs e)
		{
			Account accountToDeleteFrom = (Account)cmbbxAccountForPlaylistTab.SelectedItem;
			DmUploader.DeletePlaylistOfAccount(accountToDeleteFrom, (string)((Button)sender).Tag);
		}

		private void ChbxVideoShouldBeAddedToPlaylistChecked(object sender, RoutedEventArgs e)
		{
			var checkbox = (CheckBox)sender;
			Playlist playlist = (Playlist)checkbox.Tag;
			Video video = (Video)cbxUploadElements.SelectedItem;

			if ((bool)checkbox.IsChecked && !playlist.Videos.Contains(video))
			{
				playlist.Videos.Add(video);
			}
		}

		private void ChbxVideoShouldBeAddedToPlaylistUnchecked(object sender, RoutedEventArgs e)
		{
			var checkbox = (CheckBox)sender;
			Playlist playlist = (Playlist)checkbox.Tag;
			Video video = (Video)cbxUploadElements.SelectedItem;

			if (!(bool)checkbox.IsChecked && playlist.Videos.Contains(video))
			{
				playlist.Videos.Remove(video);
			}
		}

		private void ChbxVideoShouldBeAddedToGroupChecked(object sender, RoutedEventArgs e)
		{
			var checkbox = (CheckBox)sender;
			Group group = (Group)checkbox.Tag;
			Video video = (Video)cbxUploadElements.SelectedItem;

			if ((bool)checkbox.IsChecked && !group.Videos.Contains(video))
			{
				group.Videos.Add(video);
			}
		}

		private void ChbxVideoShouldBeAddedToGroupUnchecked(object sender, RoutedEventArgs e)
		{
			var checkbox = (CheckBox)sender;
			Group group = (Group)checkbox.Tag;
			Video video = (Video)cbxUploadElements.SelectedItem;

			if (!(bool)checkbox.IsChecked && group.Videos.Contains(video))
			{
				group.Videos.Remove(video);
			}
		}

		private void BtnAccountLogoffClick(object sender, RoutedEventArgs e)
		{
			DmUploader.LogAccountOut((Account)((Button)sender).Tag);
		}
	}
}
