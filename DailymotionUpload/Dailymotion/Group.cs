using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace StrohisUploadLib.Dailymotion
{
	[Serializable]
	public class Group : INotifyPropertyChanged
	{
		private int created_time;
		private string description;
		private string id;
		private string name;
		private string owner;
		private string url_name;
		private bool visible = true;
		private bool shouldBeAddedToVideo;

		public int Created_time { get { return created_time; } set { created_time = value; OnPropertyChanged("Created_time"); } }
		public string Description { get { return description; } set { description = value; OnPropertyChanged("Description"); } }
		public string Id { get { return id; } set { id = value; OnPropertyChanged("Id"); } }
		public string Name { get { return name; } set { name = value; OnPropertyChanged("Name"); } }
		public string Owner { get { return owner; } set { owner = value; OnPropertyChanged("Owner"); } }
		public string Url_name { get { return url_name; } set { url_name = value; OnPropertyChanged("Url_name"); } }
		public bool Visible { get { return visible; } set { visible = value; OnPropertyChanged("Visible"); } }
		public bool ShouldBeAddedToVideo { get { return shouldBeAddedToVideo; } set { shouldBeAddedToVideo = value; OnPropertyChanged("ShouldBeAddedToVideo"); } }

		public static Group CreateNewGroup(Account account, string name, string description, string url_name)
		{
			// https://api.dailymotion.com/me/groups?description={0}&name={1}&url_name={2}&fields=created_time,description%2Cid%2Cname%2Cowner%2Curl_name%2C&access_token={3}
			// Beitritt https://api.dailymotion.com/group/x7dr1/members/me?access_token=bGwCS01ZVgZYFhIATlhRSxIOBglcBFUf
			string token = Authenticator.RefreshToken(account);

			Group uploadedResponse = null;
			var request = WebRequest.Create(string.Format("https://api.dailymotion.com/me/groups?description={0}&name={1}&url_name={2}&fields=created_time,description%2Cid%2Cname%2Cowner%2Curl_name%2C&access_token={3}", description, name, url_name, token));
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

				uploadedResponse = JsonConvert.DeserializeObject<Group>(responseString);

				// Beitritt
				request = WebRequest.Create(string.Format("https://api.dailymotion.com/group/{0}/members/me?access_token={1}", uploadedResponse.Id, token));
				request.Method = "POST";

				response = request.GetResponse();

				responseStream = response.GetResponseStream();
				using (var reader = new StreamReader(responseStream))
				{
					responseString = reader.ReadToEnd();
				}
				responseStream.Close();

				return uploadedResponse;
			}
			catch (Exception ex)
			{
				if (ex.Message.Contains("503"))
				{
					Console.WriteLine("Eine solche Gruppe existiert bereits!");
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
