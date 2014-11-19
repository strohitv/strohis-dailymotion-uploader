using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
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
		private ObservableCollection<Playlist> playlists;
		[XmlIgnore]
		private ObservableCollection<Group> groups;

		[XmlElement]
		public string User { get { return user; } set { user = value; } }
		[XmlElement]
		public string Password { get { return password; } set { password = value; } }

		[XmlElement, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
		public ObservableCollection<Playlist> Playlists { get { return playlists; } set { playlists = value; } }
		[XmlElement, Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
		public ObservableCollection<Group> Groups { get { return groups; } set { groups = value; } }

		public Account() { }

		public Account(string user, string password)
		{
			this.User = user;
			this.Password = password;

			this.Playlists = new ObservableCollection<Playlist>();

			LoadPlaylists();
			LoadGroups();
		}

		public void LoadPlaylists(string token = null)
		{
			if (Playlists == null)
			{
				Playlists = new ObservableCollection<Playlist>();
			}

			if (string.IsNullOrWhiteSpace(token))
			{
				token = Authenticator.RefreshToken(this);
			}

			PlaylistGetResponse uploadedResponse = null;
			int page = 0;

			do
			{
				page++;

				var request = WebRequest.Create(string.Format("https://api.dailymotion.com/playlists?owner={0}&page={1}&limit=100&sort=alphaaz&access_token={2}", User, page, token));
				request.Method = "GET";

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

			string token = Authenticator.RefreshToken(this);

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

			LoadPlaylists(token);
		}

		public void LoadGroups(string token = null)
		{
			if (Groups == null)
			{
				Groups = new ObservableCollection<Group>();
			}

			if (string.IsNullOrWhiteSpace(token))
			{
				token = Authenticator.RefreshToken(this);
			}

			GroupGetResponse uploadedResponse = null;
			int page = 0;

			do
			{
				page++;

				// https://api.dailymotion.com/user/me/groups?fields=created_time,description%2Cid%2Cname%2Cowner%2Curl_name%2C&access_token=ZmwDCxRdBQRBFQdXQlsXXx9CEwEYEQ9E
				// &owner={0}
				var request = WebRequest.Create(string.Format("https://api.dailymotion.com/user/me/groups?fields=created_time,description%2Cid%2Cname%2Cowner%2Curl_name%2C&page={1}&access_token={2}", "x1b4bbe" /*User*/, page, token));
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

			string token = Authenticator.RefreshToken(this);

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
	}
	/// <summary>
	/// Reference Article http://www.codeproject.com/KB/tips/SerializedObjectCloner.aspx
	/// Provides a method for performing a deep copy of an object.
	/// Binary Serialization is used to perform the copy.
	/// </summary>
	public static class ObjectCopier
	{
		/// <summary>
		/// Perform a deep Copy of the object.
		/// </summary>
		/// <typeparam name="T">The type of object being copied.</typeparam>
		/// <param name="source">The object instance to copy.</param>
		/// <returns>The copied object.</returns>
		public static Account Clone<Account>(this Account source)
		{
			if (!typeof(Account).IsSerializable)
			{
				throw new ArgumentException("The type must be serializable.", "source");
			}

			// Don't serialize a null object, simply return the default for that object
			if (Object.ReferenceEquals(source, null))
			{
				return default(Account);
			}

			IFormatter formatter = new BinaryFormatter();
			Stream stream = new MemoryStream();
			using (stream)
			{
				formatter.Serialize(stream, source);
				stream.Seek(0, SeekOrigin.Begin);
				var account = (Account)formatter.Deserialize(stream);
				return account;
			}
		}
	}
}
