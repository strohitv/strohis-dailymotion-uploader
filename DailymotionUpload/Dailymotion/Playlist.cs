using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace StrohisUploadLib.Dailymotion
{
	[Serializable]
	public class Playlist : INotifyPropertyChanged
	{
		// created_time,description,id,name,videos_total,&owner=StrohiZock
		private int created_time;
		private string description;
		private string id;
		private string name;
		private int videos_total;
		private string owner;
		private bool visible = true;
		private bool shouldBeAddedToVideo;
		private List<Video> videos = new List<Video>();

		public int Created_time { get { return created_time; } set { created_time = value; OnPropertyChanged("Created"); } }
		public string Description { get { return description; } set { description = value; OnPropertyChanged("Description"); } }
		public string Id { get { return id; } set { id = value; OnPropertyChanged("Id"); } }
		public string Name { get { return name; } set { name = value; OnPropertyChanged("Name"); } }
		public int Videos_total { get { return videos_total; } set { videos_total = value; OnPropertyChanged("VideoCount"); } }
		public string Owner { get { return owner; } set { owner = value; OnPropertyChanged("Owner"); } }
		public bool Visible { get { return visible; } set { visible = value; OnPropertyChanged("Visible"); } }
		public bool ShouldBeAddedToVideo { get { return shouldBeAddedToVideo; } set { shouldBeAddedToVideo = value; OnPropertyChanged("ShouldBeAddedToVideo"); } }
		public List<Video> Videos { get { return videos; } set { videos = value; OnPropertyChanged("Videos"); } }

		public static Playlist CreateNewPlaylist(Account account, string name, string description)
		{
			//https://api.dailymotion.com/me/playlists?fields=created_time,description%2Cid%2Cname%2Cowner%2Cvideos_total%2C&description=Random+Description&name=Gothic+%5BLP+%23003%5D&access_token=bGwCS01ZVgZYFhIATlhRSxIOBglcBFUf

			string token = account.AccessToken;

			Playlist uploadedResponse = null;
			var request = WebRequest.Create(string.Format("https://api.dailymotion.com/me/playlists?fields=created_time,description,id,name,owner,videos_total&description={0}&name={1}&access_token={2}", description, name, token));
			request.Method = "POST";

			try
			{
				var response = request.GetResponse();

				var responseStream = response.GetResponseStream();
				string responseString;
				using (var reader = new StreamReader(responseStream))
				{
					responseString = reader.ReadToEnd();
				}
				responseStream.Close();

				uploadedResponse = JsonConvert.DeserializeObject<Playlist>(responseString);

				//Authenticator.RefreshInstantly(account);

				return uploadedResponse;
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("503"))
				{
					Console.WriteLine("Eine solche Playlist existiert bereits!");
				}
				return null;
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
	}
}
