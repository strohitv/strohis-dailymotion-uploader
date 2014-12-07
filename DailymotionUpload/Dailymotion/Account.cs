using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Serialization;

namespace StrohisUploadLib.Dailymotion
{
	[XmlRoot, Serializable]
	public class Account
	{
		[XmlIgnore]
		private string user;
		[XmlIgnore]
		private string password;

		[XmlIgnore]
		private BindingList<Playlist> playlists;
		[XmlIgnore]
		private BindingList<Group> groups;
		[XmlIgnore]
		private string accessToken;
		[XmlIgnore]
		private Thread grpThread;
		[XmlIgnore]
		private Thread plThread;

		[XmlElement]
		public string User { get { return user; } set { user = value; } }
		[XmlElement]
		public string Password { get { return password; } set { password = value; } }
		[XmlIgnore]
		public string AccessToken
		{
			get
			{
				if (string.IsNullOrEmpty(accessToken))
				{
					accessToken = Authenticator.GetAccessToken(this);
				}
				return accessToken;
			}
			set
			{
				accessToken = value;
			}
		}
		[XmlIgnore]
		public bool IsRefreshingAccessToken { get; set; }

		[XmlElement, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
		public BindingList<Playlist> Playlists { get { return playlists; } set { playlists = value; } }
		[XmlElement, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
		public BindingList<Group> Groups { get { return groups; } set { groups = value; } }

		public Account() { }

		public Account(string user, string password)
		{
			this.User = user;
			this.Password = password;

			this.Playlists = new BindingList<Playlist>();
			this.Groups = new BindingList<Group>();

			LoadPlaylists();
			LoadGroups();
		}

		internal string GetAccessToken()
		{
			return this.accessToken;
		}

		public void LoadPlaylists(object tokenObject = null)
		{
			string token = string.Empty;

			if (Playlists == null)
			{
				Playlists = new BindingList<Playlist>();
			}

			if (tokenObject is string)
			{
				token = (string)tokenObject;
			}

			if (string.IsNullOrEmpty(token))
			{
				token = this.AccessToken;
			}

			//Authenticator.RefreshInstantly(this);

			PlaylistGetResponse uploadedResponse = null;
			int page = 0;

			do
			{
				page++;

				HttpRequestCachePolicy policy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
				var request = WebRequest.Create(string.Format("https://api.dailymotion.com/playlists?owner={0}&page={1}&limit=100&sort=alphaaz&access_token={2}", User, page, token));
				request.CachePolicy = policy;
				request.Method = "GET";

				//NativeMethods.DeleteUrlCacheEntry(request.RequestUri.AbsoluteUri);

				var response3 = request.GetResponse();

				var responseStream = response3.GetResponseStream();
				string responseString;
				using (var reader = new StreamReader(responseStream))
				{
					responseString = reader.ReadToEnd();
				}
				responseStream.Close();

				uploadedResponse = JsonConvert.DeserializeObject<PlaylistGetResponse>(responseString);

				foreach (var singlePlaylist in uploadedResponse.list)
				{
					bool isAlreadyInPlaylists = false;
					foreach (var singleExistingPlaylist in Playlists)
					{
						if (singlePlaylist.Id.Equals(singleExistingPlaylist.Id))
						{
							isAlreadyInPlaylists = true;
							break;
						}
					}
					if (!isAlreadyInPlaylists)
					{
						Playlists.Add(singlePlaylist);
					}
				}

				int singlePlaylistCounter = 0;
				while (singlePlaylistCounter < Playlists.Count)
				{
					bool playlistWasRemoved = true;
					foreach (var singlePlaylist in uploadedResponse.list)
					{
						if (singlePlaylist.Id.Equals(Playlists[singlePlaylistCounter].Id))
						{
							playlistWasRemoved = false;
							break;
						}
					}
					if (playlistWasRemoved)
					{
						Playlists.RemoveAt(singlePlaylistCounter);
					}
					else
					{
						singlePlaylistCounter++;
					}
				}

			} while (uploadedResponse.has_more);
		}

		public void DeletePlaylist(string id)
		{
			// https://api.dailymotion.com/playlist/x3h3z9?access_token=bGwCS01ZVgZYFhIATlhRSxIOBglcBFUf

			string token = this.AccessToken;

			var request = WebRequest.Create(string.Format("https://api.dailymotion.com/playlist/{0}?access_token={1}", id, token));
			request.Method = "DELETE";

			var response3 = request.GetResponse();

			var responseStream = response3.GetResponseStream();
			string responseString;
			using (var reader = new StreamReader(responseStream))
			{
				responseString = reader.ReadToEnd();
			}
			responseStream.Close();

			Authenticator.RefreshInstantly(this);

			LoadPlaylists(token);
		}

		public void ShutThreadsDown()
		{
			if (grpThread != null && !grpThread.IsAlive)
			{
				grpThread.Abort();
				grpThread = null;
			}

			if (plThread != null && !plThread.IsAlive)
			{
				plThread.Abort();
				plThread = null;
			}
		}

		public void LoadGroups(object tokenObject = null)
		{
			string token = string.Empty;

			if (Groups == null)
			{
				Groups = new BindingList<Group>();
			}

			if (tokenObject != null && tokenObject is string)
			{
				token = (string)tokenObject;
			}

			if (string.IsNullOrEmpty(token))
			{
				token = this.AccessToken;
			}

			GroupGetResponse uploadedResponse = null;
			int page = 0;

			do
			{
				page++;

				// https://api.dailymotion.com/user/me/groups?fields=created_time,description%2Cid%2Cname%2Cowner%2Curl_name%2C&access_token=ZmwDCxRdBQRBFQdXQlsXXx9CEwEYEQ9E
				// &owner={0}

				HttpRequestCachePolicy policy = new HttpRequestCachePolicy(HttpRequestCacheLevel.NoCacheNoStore);
				var request = WebRequest.Create(string.Format("https://api.dailymotion.com/user/me/groups?fields=created_time,description%2Cid%2Cname%2Cowner%2Curl_name%2C&page={1}&access_token={2}", "x1b4bbe" /*User*/, page, token));
				request.CachePolicy = policy;
				request.Method = "GET";

				var response3 = request.GetResponse();

				var responseStream = response3.GetResponseStream();
				string responseString;
				using (var reader = new StreamReader(responseStream))
				{
					responseString = reader.ReadToEnd();
				}
				responseStream.Close();

				uploadedResponse = JsonConvert.DeserializeObject<GroupGetResponse>(responseString);

				foreach (var singleGroup in uploadedResponse.list)
				{
					bool isAlreadyInGroups = false;
					foreach (var singleExistingGroup in Groups)
					{
						if (singleGroup.Id.Equals(singleExistingGroup.Id))
						{
							isAlreadyInGroups = true;
							break;
						}
					}
					if (!isAlreadyInGroups)
					{
						Groups.Add(singleGroup);
					}
				}

				int singleGroupCounter = 0;
				while (singleGroupCounter < Groups.Count)
				{
					bool groupWasRemoved = true;
					foreach (var singleGroup in uploadedResponse.list)
					{
						if (singleGroup.Id.Equals(Groups[singleGroupCounter].Id))
						{
							groupWasRemoved = false;
							break;
						}
					}
					if (groupWasRemoved)
					{
						Groups.RemoveAt(singleGroupCounter);
					}
					else
					{
						singleGroupCounter++;
					}
				}

			} while (uploadedResponse.has_more);
		}

		public void DeleteGroup(string id)
		{
			// https://api.dailymotion.com/group/x7eg5?access_token=ZmwDCxRdBQRBFQdXQlsXXx9CEwEYEQ9E

			string token = this.AccessToken;

			var request = WebRequest.Create(string.Format("https://api.dailymotion.com/group/{0}?access_token={1}", id, token));
			request.Method = "DELETE";

			var response3 = request.GetResponse();

			var responseStream = response3.GetResponseStream();
			string responseString;
			using (var reader = new StreamReader(responseStream))
			{
				responseString = reader.ReadToEnd();
			}
			responseStream.Close();

			LoadGroups(token);
		}

		public static Account Clone(Account source)
		{
			StrohisUploadLib.Dailymotion.Account newAccount = new StrohisUploadLib.Dailymotion.Account(source.User, source.Password);

			foreach (var singleGroup in source.Groups)
			{
				Group newGroup = new Group()
				{
					Created_time = singleGroup.Created_time,
					Description = singleGroup.Description,
					Id = singleGroup.Id,
					Name = singleGroup.Name,
					Owner = singleGroup.Owner,
					ShouldBeAddedToVideo = singleGroup.ShouldBeAddedToVideo,
					Url_name = singleGroup.Url_name,
					Visible = singleGroup.Visible
				};
				newAccount.Groups.Add(newGroup);
			}

			foreach (var singlePlaylist in source.Playlists)
			{
				Playlist newGroup = new Playlist()
				{
					Created_time = singlePlaylist.Created_time,
					Description = singlePlaylist.Description,
					Id = singlePlaylist.Id,
					Name = singlePlaylist.Name,
					Owner = singlePlaylist.Owner,
					ShouldBeAddedToVideo = singlePlaylist.ShouldBeAddedToVideo,
					Videos_total = singlePlaylist.Videos_total,
					Visible = singlePlaylist.Visible
				};
				newAccount.Playlists.Add(newGroup);
			}

			return newAccount;
		}

		public override string ToString()
		{
			return string.Format("{0}: {1} Playlists, {2} Groups", User, Playlists.Count, Groups.Count);
		}

		public static class NativeMethods
		{
			[DllImport("WinInet.dll", PreserveSig = true, SetLastError = true)]
			public static extern void DeleteUrlCacheEntry(string url);
		}
	}
}
