using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

namespace StrohisUploadLib.Dailymotion
{
	public class UploadElement : INotifyPropertyChanged
	{
		#region Private Fields

		//private string accessToken;

		private UploadResponse response;
		private UploadResponse response2;

		private Thread uploadThread;

		private string path;
		private string title;
		private bool isOfficial;
		private string tags;
		private string description;
		private Language videoLanguage;
		private Channel videoChannel;
		private string recordDate; // taken_time
		private string location;
		private bool isCommentingAllowed;
		private bool isPrivate;
		private bool isPublished;
		private bool isNewVideostar;
		private string groupId;
		private string playListId;
		private bool isExplicit;
		private string thumbnailUrl;
		private string videoId;
		private bool seperateJob;
		private Account uploadAccount;
		//private IList<PlaylistElement> playlistsToAdd;

		private int maxKbits;
		private BitUnit bitType;
		private bool isRunning;
		private bool finished;
		private double percentage;
		private string message;
		private bool failed;
		private double bytesTransferred;
		private double bytesToTransfer;
		private TimeSpan duration;
		private string videoUrl;
		private string logPath;

		#endregion Private Fiels

		#region Public Properties

		// Video-Information // filepath,title,official/creative,tags,description,language,channel,recordDate,location,comments,privacy,videostar,group,play list
		public string Path { get { return path; } set { path = value; OnPropertyChanged("Path"); } }
		public string Title { get { return title; } set { title = value; OnPropertyChanged("Title"); } }
		public bool IsOfficial { get { return isOfficial; } set { isOfficial = value; OnPropertyChanged("IsOfficial"); } }
		public string Tags { get { return tags; } set { tags = value; OnPropertyChanged("Tags"); } }
		public string Description { get { return description; } set { description = value; OnPropertyChanged("Description"); } }
		public Language VideoLanguage { get { return videoLanguage; } set { videoLanguage = value; OnPropertyChanged("VideoLanguage"); } }
		public Channel VideoChannel { get { return videoChannel; } set { videoChannel = value; OnPropertyChanged("VideoChannel"); } }
		public string RecordDate { get { return recordDate; } set { recordDate = value; OnPropertyChanged("RecordDate"); } } // taken_time
		public string Location { get { return location; } set { location = value; OnPropertyChanged("Location"); } }
		public bool IsCommentingAllowed { get { return isCommentingAllowed; } set { isCommentingAllowed = value; OnPropertyChanged("IsCommentingAllowed"); } }
		public bool IsPrivate { get { return isPrivate; } set { isPrivate = value; OnPropertyChanged("IsPrivate"); } }
		public bool IsPublished { get { return isPublished; } set { isPublished = value; OnPropertyChanged("IsPublished"); } }
		public bool IsNewVideostar { get { return isNewVideostar; } set { isNewVideostar = value; OnPropertyChanged("IsNewVideostar"); } }
		public string GroupId { get { return groupId; } set { groupId = value; OnPropertyChanged("GroupId"); } }
		public string PlayListId { get { return playListId; } set { playListId = value; OnPropertyChanged("PlayListId"); } }
		public bool IsExplicit { get { return isExplicit; } set { isExplicit = value; OnPropertyChanged("IsExplicit"); } }
		public string ThumbnailUrl { get { return thumbnailUrl; } set { thumbnailUrl = value; OnPropertyChanged("ThumbnailUrl"); } }
		public string VideoId { get { return videoId; } set { videoId = value; OnPropertyChanged("Id"); } }
		public bool SeperateJob { get { return seperateJob; } set { seperateJob = value; OnPropertyChanged("SeperateJob"); } }
		public Account UploadAccount { get { return uploadAccount; } set { uploadAccount = value.Clone(); OnPropertyChanged("UploadAccount"); } }
		//public IList<PlaylistElement> PlaylistsToAdd { get { return playlistsToAdd; } set { playlistsToAdd = value; OnPropertyChanged("PlaylistsToAdd "); } }

		// Upload
		public int MaxKbits { get { return maxKbits; } set { maxKbits = value; OnPropertyChanged("MaxKbits"); } }
		public BitUnit BitType { get { return bitType; } set { bitType = value; OnPropertyChanged("BitType"); } }
		public bool IsRunning { get { return isRunning; } set { isRunning = value; OnPropertyChanged("IsRunning"); } }
		public bool Finished { get { return finished; } set { finished = value; OnPropertyChanged("Finished"); } }
		public double Percentage { get { return percentage; } set { percentage = value; OnPropertyChanged("Percentage"); } }
		public string Message { get { return message; } set { message = value; OnPropertyChanged("Message"); } }
		public bool Failed { get { return failed; } set { failed = value; OnPropertyChanged("Failed"); } }
		public double BytesTransferred { get { return bytesTransferred; } set { bytesTransferred = value; OnPropertyChanged("BytesTransferred"); } }
		public double BytesToTransfer { get { return bytesToTransfer; } set { bytesToTransfer = value; OnPropertyChanged("BytesToTransfer"); } }
		public TimeSpan Duration { get { return duration; } set { duration = value; OnPropertyChanged("Duration"); } }
		public string VideoUrl { get { return videoUrl; } set { videoUrl = value; OnPropertyChanged("VideoUrl"); } }
		public string LogPath { get { return logPath; } set { logPath = value; OnPropertyChanged("LogPath"); } }

		#endregion Public Properties

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

		Stopwatch s1 = new Stopwatch();

		public void Start()
		{
			string token = Authenticator.RefreshToken(UploadAccount);
			s1.Start();

			//accessToken = GetAccessToken();


			//Authorize(token);


			//--Console.WriteLine("Access token is " + Authenticator.AccessToken);
			//var fileToUpload = @"C:\Program Files\Common Files\Microsoft Shared\ink\en-US\join.avi";
			//--Console.WriteLine("File to upload is " + Path);

			var uploadUrl = GetFileUploadUrl(token);

			//--Console.WriteLine("Posting to " + uploadUrl);

			var response = GetFileUploadResponse(Path, token, uploadUrl);

			if (response != null)
			{
				token = Authenticator.RefreshToken(UploadAccount);

				//--Console.WriteLine("Response:\n");
				//--Console.WriteLine(response + "\n");

				//--Console.WriteLine("Publishing video.\n");
				var uploadedResponse = PublishVideo(response, token);

				//Finished = true;
				//IsRunning = false;
				//Failed = false;

				//--Console.WriteLine(uploadedResponse);
				if (this.IsNewVideostar)
				{
					Feature(token, uploadedResponse.id);
				}

				if (!string.IsNullOrWhiteSpace(ThumbnailUrl) && File.Exists(ThumbnailUrl) && new FileInfo(ThumbnailUrl).Length <= 600 * 1024)
				{
					UploadThumb(token, ThumbnailUrl);
				}

				foreach (Playlist singlePlaylist in UploadAccount.Playlists)
				{
					if (singlePlaylist.ShouldBeAddedToVideo)
					{
						AddToPlaylist(token, uploadedResponse.id, singlePlaylist.Id);
					}
				}

				foreach (Group singleGroup in UploadAccount.Groups)
				{
					if (singleGroup.ShouldBeAddedToVideo)
					{
						AddToGroup(token, uploadedResponse.id, singleGroup.Id);
					}
				}

				this.Message = "Fertig. Video verfügbar unter";
				this.Percentage = 100.0;
				this.BytesTransferred = this.BytesToTransfer;
				this.VideoUrl = string.Format("http://www.dailymotion.com/video/{0}", uploadedResponse.id);
				Finished = true;
				IsRunning = false;
				Failed = false;

				this.OnRaiseUploadFinishedEvent(new UploadCompletedEventArgs(this.Message, this.Duration));
			}
		}

		public void StartAsync()
		{
			uploadThread = new Thread(Start);
			uploadThread.Name = string.Format("UploadThread of {0}", Location);
			uploadThread.Start();
		}

		public void Abort()
		{
			if (uploadThread != null && uploadThread.IsAlive)
			{
				uploadThread.Abort();

				this.Failed = true;
				this.IsRunning = false;
				this.Finished = false;
				this.Percentage = 0.0;
				this.Message = "Abbruch durch den Benutzer.";
				this.VideoUrl = string.Empty;
			}
		}

		#region UploadMethods

		private string GetAccessToken()
		{
			var request = WebRequest.Create("https://api.dailymotion.com/oauth/token");
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";

			var requestString = String.Format("grant_type=password&client_id={0}&client_secret={1}&username={2}&password={3}",
				HttpUtility.UrlEncode(SettingsProvider.Key),
				HttpUtility.UrlEncode(SettingsProvider.Secret),
				HttpUtility.UrlEncode(SettingsProvider.Username),
				HttpUtility.UrlEncode(SettingsProvider.Password));

			var requestBytes = Encoding.UTF8.GetBytes(requestString);

			var requestStream = request.GetRequestStream();

			requestStream.Write(requestBytes, 0, requestBytes.Length);

			WebResponse response = null;
			try
			{
				response = request.GetResponse();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
			}

			var responseStream = response.GetResponseStream();
			string responseString;
			using (var reader = new StreamReader(responseStream))
			{
				responseString = reader.ReadToEnd();
			}

			var oauthResponse = JsonConvert.DeserializeObject<OAuthResponse>(responseString);

			return oauthResponse.access_token;
		}

		private void Authorize(string accessToken)
		{
			var authorizeUrl = String.Format("https://api.dailymotion.com/oauth/authorize?response_type=code&client_id={0}&scope=read+write+manage_videos+manage_playlists+manage_groups+delete&redirect_uri={1}",
				HttpUtility.UrlEncode(SettingsProvider.Key),
				HttpUtility.UrlEncode("http://strohi.tv/"));

			//--Console.WriteLine("We need permissions to upload. Press enter to open web browser.");
			//--Console.ReadLine();

			Process.Start(authorizeUrl);

			//var client = new WebClient();
			//client.Headers.Add("Authorization", "OAuth " + accessToken);

			//--Console.WriteLine("Press enter once you have authenticated and been redirected to your callback URL");
			//--Console.ReadLine();
		}

		public UploadResponse HttpUploadFile(string accessToken, string url, string file, string paramName, string contentType, NameValueCollection nvc, string type = "Video")
		{
			IsRunning = true;

			//--Console.WriteLine(string.Format("Uploading {0} to {1}", file, url));
			string boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x");
			byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

			HttpWebRequest wr = (HttpWebRequest)WebRequest.Create(url);
			wr.Proxy = null;
			wr.ContentType = "multipart/form-data; boundary=" + boundary;
			wr.Method = "POST";
			wr.KeepAlive = true;
			wr.Headers.Add("Authorization", "OAuth " + accessToken);
			wr.Credentials = System.Net.CredentialCache.DefaultCredentials;
			wr.ProtocolVersion = HttpVersion.Version10;

			wr.ServicePoint.SetTcpKeepAlive(true, 10000, 1000);

			long lenghtOfBytes = 0;

			string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
			foreach (string key in nvc.Keys)
			{
				lenghtOfBytes += boundarybytes.Length;
				string formitem = string.Format(formdataTemplate, key, nvc[key]);
				byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
				lenghtOfBytes += formitembytes.Length;
			}
			lenghtOfBytes += boundarybytes.Length;

			string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
			string header = string.Format(headerTemplate, paramName, file, contentType);
			byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
			lenghtOfBytes += headerbytes.Length;

			long fileLength = new FileInfo(file).Length;
			lenghtOfBytes += fileLength;

			byte[] trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
			lenghtOfBytes += trailer.Length;

			wr.AllowWriteStreamBuffering = false;
			wr.ContentLength = lenghtOfBytes;

			//--Console.WriteLine(wr.AuthenticationLevel.ToString());
			wr.Timeout = int.MaxValue;

			Stream rs = wr.GetRequestStream();

			rs.WriteTimeout = -1;

			formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
			foreach (string key in nvc.Keys)
			{
				rs.Write(boundarybytes, 0, boundarybytes.Length);
				lenghtOfBytes += boundarybytes.Length;
				string formitem = string.Format(formdataTemplate, key, nvc[key]);
				byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
				rs.Write(formitembytes, 0, formitembytes.Length);
				lenghtOfBytes += formitembytes.Length;
			}
			rs.Write(boundarybytes, 0, boundarybytes.Length);
			lenghtOfBytes += boundarybytes.Length;

			headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\nContent-Type: {2}\r\n\r\n";
			header = string.Format(headerTemplate, paramName, file, contentType);
			headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
			rs.Write(headerbytes, 0, headerbytes.Length);
			lenghtOfBytes += headerbytes.Length;

			rs.WriteTimeout = int.MaxValue;

			FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
			this.BytesToTransfer = fileStream.Length;
			this.BytesTransferred = 0;
			byte[] buffer = new byte[4 * 1024];
			int bytesRead = 0;
			while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
			{
				try
				{
					rs.Write(buffer, 0, bytesRead);
					BytesTransferred += bytesRead;
					Percentage = (double)fileStream.Position / lenghtOfBytes * 100;
					Message = string.Format("Hochladen des {0}s: {1:0.00}% fertig, Hochgeladen: {2:0.00} MB, Hochzuladen: {3:0.00} MB", type, Percentage, BytesTransferred / 1024 / 1024, BytesToTransfer / 1024 / 1024);
					//--Console.WriteLine("{0} von {1} Bytes hochgeladen.", fileStream.Position, fileStream.Length);
				}
				catch (Exception ex)
				{
					this.Message = string.Format("Upload des {0}s fehlgeschlagen: {0}", type, ex.Message);
					this.Failed = true;
					this.IsRunning = false;
					this.Finished = true;

					rs.Close();
					return null;
				}
			}
			fileStream.Close();

			trailer = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "--\r\n");
			rs.Write(trailer, 0, trailer.Length);
			rs.Close();

			WebResponse wresp = null;
			try
			{
				var writerresponse = wr.GetResponse();
				Stream stream2 = writerresponse.GetResponseStream();
				StreamReader reader2 = new StreamReader(stream2);
				var test = reader2.ReadToEnd();
				response = JsonConvert.DeserializeObject<UploadResponse>(test);
				//--Console.WriteLine(response.ToString());
				//--Console.WriteLine(string.Format("File uploaded, server response is: {0}", test));
			}
			catch (Exception ex)
			{
				//--Console.WriteLine("Error uploading file", ex);
				this.Message = ex.Message;
				this.Failed = true;

				if (wresp != null)
				{
					wresp.Close();
					wresp = null;
				}
			}
			finally
			{
				wr = null;
			}

			return response;
		}

		private string GetFileUploadUrl(string accessToken)
		{
			var client = new WebClient();

			client.Headers.Add("Authorization", "OAuth " + accessToken);

			var urlResponse = client.DownloadString("https://api.dailymotion.com/file/upload");

			var response = JsonConvert.DeserializeObject<UploadRequestResponse>(urlResponse).upload_url;

			return response;
		}

		private void UploadThumb(string accessToken, string fileUrl)
		{
			var uploadUrl = GetFileUploadUrl(accessToken);
			NameValueCollection nvc = new NameValueCollection();
			nvc.Add("Authorization", "OAuth " + accessToken);

			HttpUploadFile(accessToken, uploadUrl, fileUrl, "file", "image/png", nvc, "Thumbnail");

			var request = WebRequest.Create(string.Format("https://api.dailymotion.com/video/{0}?thumbnail_url={1}", VideoId, HttpUtility.UrlEncode(response.url)));
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			request.Headers.Add("Authorization", "OAuth " + accessToken);

			//var requestBytes = Encoding.UTF8.GetBytes(response.url);

			//var requestStream = request.GetRequestStream();

			//requestStream.Write(requestBytes, 0, requestBytes.Length);

			var response3 = request.GetResponse();

			var responseStream = response3.GetResponseStream();
			string responseString;
			using (var reader = new StreamReader(responseStream))
			{
				responseString = reader.ReadToEnd();
			}
			//var uploadedResponse = JsonConvert.DeserializeObject<UploadedResponse>(responseString);
			//this.Message = "Fertig. Video verfügbar unter";
			//this.Percentage = 100.0;
			//this.BytesTransferred = this.BytesToTransfer;
			//Message = string.Format("Am Hochladen: {0:0.00}% fertig, Hochgeladen: {1:0.00} MB, Hochzuladen: {2:0.00} MB", Percentage, BytesToTransfer / 1024 / 1024, BytesTransferred / 1024 / 1024);
			//this.VideoUrl = string.Format("http://www.dailymotion.com/video/{0}", uploadedResponse.id);

		}

		private UploadResponse GetFileUploadResponse(string fileToUpload, string accessToken, string uploadUrl)
		{
			//client = new WebClient();

			//client.Headers.Add("Authorization", "OAuth " + accessToken);

			//client.UploadProgressChanged += new UploadProgressChangedEventHandler(OnUploadProgressChanged);
			//client.UploadFileCompleted += new UploadFileCompletedEventHandler(OnUploadProgressCompleted);

			NameValueCollection nvc = new NameValueCollection();
			nvc.Add("Authorization", "OAuth " + accessToken);

			var response = HttpUploadFile(accessToken, uploadUrl, fileToUpload, "file", "video/mp4", nvc);
			if (response != null)
			{
				this.Percentage = 100.0;
				return response;
			}
			else
			{
				return null;
			}
		}

		private UploadedResponse PublishVideo(UploadResponse uploadResponse, string accessToken)
		{
			//"https://api.dailymotion.com/me/videos?url=http://upload-07.dailymotion.com/files/f4f65be7a2bcc575283a62ec7fe8cdb8.mkv#c2VhbD0wMzE3Y2UxN2Y1YjQ4MDM0OWFhMDI0YzhmMTY4ODAzYyZhY29kZWM9RkxBQyZiaXRyYXRlPTYxMjEyODYmZGltZW5zaW9uPTMyMDB4MTgwMCZkdXJhdGlvbj0xNTQ1MjM0JmZvcm1hdD1NYXRyb3NrYSZoYXNoPTRjNmE3YjU1MDQ1NGZjZjEwN2NiN2MwNGUwYjM4YWJlZTMzMjg0OGImbmFtZT1nb3RoaWNfMDAxLW11eGVkJnNpemU9MTE4MjM1MjQxNCZzdHJlYW1hYmxlPSZ2Y29kZWM9QVZD"
			var request = WebRequest.Create("https://api.dailymotion.com/me/videos?url=" + HttpUtility.UrlEncode(uploadResponse.url));
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			request.Headers.Add("Authorization", "OAuth " + accessToken);

			var requestString = GetPostStringWithOutDescription();

			var requestBytes = Encoding.UTF8.GetBytes(requestString);

			var requestStream = request.GetRequestStream();

			//byte[] buffer = new byte[4 * 1024];
			//int bytesRead = 0;
			//while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
			//{
			//	try
			//	{
			//		requestStream.Write(buffer, 0, bytesRead);
			//		BytesTransferred += bytesRead;
			//		Percentage = (double)fileStream.Position / lenghtOfBytes * 100;
			//		//--Console.WriteLine("{0} von {1} Bytes hochgeladen.", fileStream.Position, fileStream.Length);
			//	}
			//	catch (Exception ex)
			//	{
			//		Console.WriteLine(ex.Message);
			//	}
			//}

			int position = 0;
			while (position < requestBytes.Length)
			{
				//byte[] sendBytes = new byte[(position < requestBytes.Length - 4*1024)?4*1024:requestBytes.Length - position];
				requestStream.Write(requestBytes, position, (position < requestBytes.Length - 4 * 1024) ? 4 * 1024 : requestBytes.Length - position);
				position += (position < requestBytes.Length - 4 * 1024) ? 4 * 1024 : requestBytes.Length - position;
			}

			//requestStream.Write(requestBytes, 0, requestBytes.Length);
			//requestStream.Flush();

			WebResponse response;

			////accessToken = Authenticator.AccessToken;

			//string somethingstring = GetDescriptionPostString();

			response = request.GetResponse();

			var responseStream = response.GetResponseStream();
			string responseString;
			using (var reader = new StreamReader(responseStream))
			{
				responseString = reader.ReadToEnd();
			}
			var uploadedResponse = JsonConvert.DeserializeObject<UploadedResponse>(responseString);
			this.VideoId = uploadedResponse.id;

			return uploadedResponse;
		}

		private void Feature(string accessToken, string id)
		{
			var request = WebRequest.Create("https://api.dailymotion.com/me/features/" + HttpUtility.UrlEncode(id));
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			request.Headers.Add("Authorization", "OAuth " + accessToken);

			var requestStream = request.GetRequestStream();

			//var requestBytes = Encoding.UTF8.GetBytes(id);

			//requestStream.Write(requestBytes, 0, requestBytes.Length);

			var response = request.GetResponse();

			var responseStream = response.GetResponseStream();
			string responseString;
			using (var reader = new StreamReader(responseStream))
			{
				responseString = reader.ReadToEnd();
			}
		}

		private void AddToPlaylist(string accessToken, string videoId, string playlistId)
		{
			var request = WebRequest.Create(string.Format("https://api.dailymotion.com/video/{0}/playlists/{1}", HttpUtility.UrlEncode(videoId), HttpUtility.UrlEncode(playlistId)));
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			request.Headers.Add("Authorization", "OAuth " + accessToken);

			var requestStream = request.GetRequestStream();

			//var requestBytes = Encoding.UTF8.GetBytes(videoId);

			//requestStream.Write(requestBytes, 0, requestBytes.Length);

			var response = request.GetResponse();

			var responseStream = response.GetResponseStream();
			string responseString;
			using (var reader = new StreamReader(responseStream))
			{
				responseString = reader.ReadToEnd();
			}
		}

		private void AddToGroup(string accessToken, string videoId, string groupId)
		{
			var request = WebRequest.Create(string.Format("https://api.dailymotion.com/video/{0}/groups/{1}", HttpUtility.UrlEncode(videoId), HttpUtility.UrlEncode(groupId)));
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			request.Headers.Add("Authorization", "OAuth " + accessToken);

			var requestStream = request.GetRequestStream();

			//var requestBytes = Encoding.UTF8.GetBytes(videoId);

			//requestStream.Write(requestBytes, 0, requestBytes.Length);

			var response = request.GetResponse();

			var responseStream = response.GetResponseStream();
			string responseString;
			using (var reader = new StreamReader(responseStream))
			{
				responseString = reader.ReadToEnd();
			}
		}

		private string GetDescriptionPostString()
		{
			StringBuilder postString = new StringBuilder("");
			if (!string.IsNullOrWhiteSpace(Description))
			{
				postString.Append(string.Format("&description={0}", HttpUtility.UrlEncode(Description)));
			}
			return postString.ToString();
		}

		private string GetPostStringWithOutDescription()
		{
			StringBuilder postString = new StringBuilder("");

			postString.Append(string.Format("channel={0}", HttpUtility.UrlEncode(DmUploadConstants.Channels[(int)VideoChannel])));
			postString.Append(string.Format("&language={0}", HttpUtility.UrlEncode(DmUploadConstants.Language[(int)VideoLanguage])));

			if (IsCommentingAllowed)
			{
				postString.Append(string.Format("&allow_comments={0}", HttpUtility.UrlEncode("true")));
			}

			if (IsOfficial)
			{
				postString.Append(string.Format("&type={0}", HttpUtility.UrlEncode("official")));
			}

			if (IsExplicit)
			{
				postString.Append(string.Format("&explicit={0}", HttpUtility.UrlEncode("true")));
			}

			if (IsPublished)
			{
				postString.Append(string.Format("&published={0}", HttpUtility.UrlEncode("true")));
			}
			else if (IsPrivate)
			{
				postString.Append(string.Format("&private={0}", HttpUtility.UrlEncode("true")));
				postString.Append(string.Format("&published={0}", HttpUtility.UrlEncode("false")));
			}
			else
			{
				postString.Append(string.Format("&published={0}", HttpUtility.UrlEncode("false")));
			}

			if (!string.IsNullOrWhiteSpace(Title))
			{
				postString.Append(string.Format("&title={0}", HttpUtility.UrlEncode(Title)));
			}

			if (!string.IsNullOrWhiteSpace(Description))
			{

				postString.Append(string.Format("&description={0}", HttpUtility.UrlEncode(Description)));
			}

			if (!string.IsNullOrWhiteSpace(Tags))
			{
				postString.Append(string.Format("&tags={0}", HttpUtility.UrlEncode(Tags)));
			}

			if (!string.IsNullOrWhiteSpace(RecordDate))
			{
				postString.Append(string.Format("&taken_time={0}", HttpUtility.UrlEncode(RecordDate)));
			}

			return postString.ToString();
		}

		#endregion

		#region Events

		public event EventHandler<UploadCompletedEventArgs> UploadFinished;

		protected virtual void OnRaiseUploadFinishedEvent(UploadCompletedEventArgs e)
		{
			EventHandler<UploadCompletedEventArgs> handler = UploadFinished;

			if (handler != null)
			{
				e.Message = this.Message;
				e.Duration = this.Duration;

				handler(this, e);
			}
		}

		public event EventHandler<UploadStateChangedEventArgs> UploadStateChanged;

		protected virtual void OnRaiseUploadStateChangedEvent(UploadStateChangedEventArgs e)
		{
			EventHandler<UploadStateChangedEventArgs> handler = UploadStateChanged;

			if (handler != null)
			{
				e.Message = this.Message;

				handler(this, e);
			}
		}

		#endregion Events

		private void OnUploadProgressChanged(object sender, UploadProgressChangedEventArgs e)
		{
			// Displays the operation identifier, and the transfer progress.
			//--Console.WriteLine("{0}    uploaded {1} of {2} bytes. {3} % complete...", (string)e.UserState, e.BytesSent, e.TotalBytesToSend, e.ProgressPercentage);
		}

		private void OnUploadProgressCompleted(object sender, UploadFileCompletedEventArgs e)
		{
			// Displays the operation identifier, and the transfer progress.
			//--Console.WriteLine("{0}    uploaded {1} of {2} bytes. {3} % complete...", (string)e.UserState, e.Error, e.Result, e.Cancelled);

			var responseBytes = e.Result;

			var responseString = Encoding.UTF8.GetString(responseBytes);

			response2 = JsonConvert.DeserializeObject<UploadResponse>(responseString);

			//ContinueWithPublishing();
		}
	}
}
