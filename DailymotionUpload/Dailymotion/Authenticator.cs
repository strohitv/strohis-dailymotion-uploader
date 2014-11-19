using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace StrohisUploadLib.Dailymotion
{
	public static class Authenticator
	{
		private static string publicApiKey = "d7b976fb4ac33d3b8d7a";
		private static string secretApiKey = "e4d8327ee265b6f2589a8632e82aaa4540d694de";

		private static IList<Account> accounts = new List<Account>();

		internal static string PublicApiKey { get { return publicApiKey; } set { publicApiKey = value; } }
		internal static string SecretApiKey { get { return secretApiKey; } set { secretApiKey = value; } }

		public static string RefreshToken(Account account)
		{
			return GetAccessToken(account);
		}

		private static string GetAccessToken(Account account)
		{
			var request = WebRequest.Create("https://api.dailymotion.com/oauth/token");
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";

			var requestString = String.Format("grant_type=password&client_id={0}&client_secret={1}&username={2}&password={3}",
				HttpUtility.UrlEncode(PublicApiKey),
				HttpUtility.UrlEncode(SecretApiKey),
				HttpUtility.UrlEncode(account.User),
				HttpUtility.UrlEncode(account.Password));

			var requestBytes = Encoding.UTF8.GetBytes(requestString);

			var requestStream = request.GetRequestStream();

			requestStream.Write(requestBytes, 0, requestBytes.Length);

			var response = request.GetResponse();

			var responseStream = response.GetResponseStream();
			string responseString;
			using (var reader = new StreamReader(responseStream))
			{
				responseString = reader.ReadToEnd();
			}

			var oauthResponse = JsonConvert.DeserializeObject<OAuthResponse>(responseString);

			return oauthResponse.access_token;
		}

		public static string GetAuthorizationUrl(Account account)
		{
			string token = RefreshToken(account);

			if (!string.IsNullOrWhiteSpace(token))
			{
				var authorizeUrl = String.Format("https://api.dailymotion.com/oauth/authorize?response_type=code&client_id={0}&scope=read+write+manage_videos+manage_playlists+manage_groups+delete&redirect_uri={1}",
					HttpUtility.UrlEncode(SettingsProvider.Key),
					HttpUtility.UrlEncode("http://strohi.tv/"));

				//--Console.WriteLine("We need permissions to upload. Press enter to open web browser.");
				//--Console.ReadLine();

				return authorizeUrl;
			}

			return string.Empty;

			//var client = new WebClient();
			//client.Headers.Add("Authorization", "OAuth " + AccessToken);

			//--Console.WriteLine("Press enter once you have authenticated and been redirected to your callback URL");
			//--Console.ReadLine();
		}
	}
}
