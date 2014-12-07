using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;

namespace StrohisUploadLib.Dailymotion
{
	public static class Authenticator
	{
		private static IList<TokenInformation> tokens = new List<TokenInformation>();
		private static Thread refreshThread = new Thread(RefreshToken) { Name = "RefreshThread" };

		private static string publicApiKey = "d7b976fb4ac33d3b8d7a";
		private static string secretApiKey = "e4d8327ee265b6f2589a8632e82aaa4540d694de";
		private static bool shouldRefreshAuthToken = true;

		internal static string PublicApiKey { get { return publicApiKey; } set { publicApiKey = value; } }
		internal static string SecretApiKey { get { return secretApiKey; } set { secretApiKey = value; } }

		private static void RefreshToken()
		{
			do
			{
				for (int i = 0; i < tokens.Count; i++)
				{
					tokens[i].TimeUntilRefresh--;
					if (tokens[i].TimeUntilRefresh <= 0)
					{
						RefreshAccessToken(tokens[i].RefreshToken);
					}
				}
				Thread.Sleep(1000);
			} while (shouldRefreshAuthToken);
		}

		internal static void RefreshInstantly(Account account)
		{
			foreach (var singleTokenInformation in tokens)
			{
				if (account.AccessToken.Equals(singleTokenInformation.AccessToken))
				{
					account.AccessToken = RefreshAccessToken(singleTokenInformation.RefreshToken);
					break;
				}
			}
		}

		public static void Logout(Account account)
		{
			account.ShutThreadsDown();

			string token = account.AccessToken;

			for (int i = 0; i < tokens.Count; i++)
			{
				if (token.Equals(tokens[i].AccessToken))
				{
					tokens.RemoveAt(i);
					if (tokens.Count == 0)
					{
						shouldRefreshAuthToken = false;
					}
					break;
				}
			}

			var request = WebRequest.Create(string.Format("https://api.dailymotion.com/logout?access_token={0}", token));
			request.Method = "GET";

			var response3 = request.GetResponse();

			var responseStream = response3.GetResponseStream();
			string responseString;
			using (var reader = new StreamReader(responseStream))
			{
				responseString = reader.ReadToEnd();
			}
			responseStream.Close();
		}

		public static string GetAccessToken(Account account, bool shouldRefresh = true)
		{
			if (!account.IsRefreshingAccessToken)
			{
				account.IsRefreshingAccessToken = true;

				var request = WebRequest.Create("https://api.dailymotion.com/oauth/token");
				request.Method = "POST";
				request.ContentType = "application/x-www-form-urlencoded";

				var requestString = String.Format("grant_type=password&client_id={0}&client_secret={1}&username={2}&password={3}&scope=email+userinfo+manage_videos+manage_playlists+manage_groups",
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

				if (shouldRefresh)
				{
					AddToRefreshTimer(oauthResponse.access_token, oauthResponse.refresh_token);

					if (!refreshThread.IsAlive)
					{
						try
						{
							refreshThread.Start();
						}
						catch (Exception exec)
						{
							Console.WriteLine(exec.Message);
						}
					}
				}

				return oauthResponse.access_token;
			}
			else
			{
				while (string.IsNullOrEmpty(account.GetAccessToken()))
				{
					Thread.Sleep(1000);
				}
				return account.GetAccessToken();
			}
		}

		private static void AddToRefreshTimer(string accessToken, string refreshToken)
		{
			bool isContained = false;

			foreach (var singleToken in tokens)
			{
				if (singleToken.AccessToken.Equals(accessToken))
				{
					isContained = true;
					break;
				}
			}

			if (!isContained)
			{
				tokens.Add(new TokenInformation() { AccessToken = accessToken, RefreshToken = refreshToken, TimeUntilRefresh = 50 * 60 });
			}
		}

		public static void Stop()
		{
			shouldRefreshAuthToken = false;
		}

		private static string RefreshAccessToken(string refreshToken)
		{
			var request = WebRequest.Create("https://api.dailymotion.com/oauth/token");
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";

			var requestString = String.Format("grant_type=refresh_token&client_id={0}&client_secret={1}&refresh_token={2}",
				HttpUtility.UrlEncode(PublicApiKey),
				HttpUtility.UrlEncode(SecretApiKey),
				HttpUtility.UrlEncode(refreshToken));

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

			if (oauthResponse.access_token != null)
			{
				for (int i = 0; i < tokens.Count; i++)
				{
					if (tokens[i].RefreshToken.Equals(oauthResponse.refresh_token))
					{
						tokens[i].AccessToken = oauthResponse.access_token;
						tokens[i].TimeUntilRefresh = 50 * 60;
						return oauthResponse.access_token;
					}
				}
			}
			else
			{
				Console.WriteLine("Scheibenkleister!");
			}
			return null;
		}

		public static string GetAuthorizationUrl(Account account)
		{
			string token = account.AccessToken;

			if (!string.IsNullOrEmpty(token))
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

		private class TokenInformation
		{
			public string AccessToken { get; set; }
			public string RefreshToken { get; set; }
			public int TimeUntilRefresh { get; set; }
		}
	}
}
