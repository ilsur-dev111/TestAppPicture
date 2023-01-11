using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TestAppPicture.Services
{
	public class ParammRequest_GET
	{
		public string Key { get; set; }
		public string Paramm { get; set; }
	}

	// подтягивается для работы с сетью
	public static class WebRequestProtocol
	{
		public static string WebRequestProtocolJSON_POST(string url, object JsonSer, string encoding = null)
		{
			try
			{
				if (CheckInternet() == ConnectionStatus.NotConnected)
					return null;
				string JsonText = JsonSerializer.Serialize(JsonSer);
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
				request.ServerCertificateValidationCallback = delegate { return true; };
				if (encoding != null)
					request.Headers.Add("Authorization", "Basic " + encoding);
				request.Method = "POST";
				byte[] byteArray = Encoding.UTF8.GetBytes(JsonText);
				request.ContentType = "application/json;charset=UTF-8";
				request.ContentLength = byteArray.Length;
				Stream dataStream = request.GetRequestStream();
				dataStream.Write(byteArray, 0, byteArray.Length);
				dataStream.Close();
				WebResponse response = request.GetResponse();
				string responseFromServer = null;
				using (dataStream = response.GetResponseStream())
				{
					StreamReader reader = new StreamReader(dataStream);
					responseFromServer = reader.ReadToEnd();
				}
				// Close the response.
				response.Close();
				return responseFromServer;
			}
			catch (Exception ex)
			{
				return null;
			}
		}
		public static async Task<string> WebRequestProtocolJSON_POSTAsync(string url, object JsonSer, string encoding = null)
		{
			try
			{
				if (CheckInternet() == ConnectionStatus.NotConnected)
					return null;
				string JsonText = JsonSerializer.Serialize(JsonSer);
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
				request.ServerCertificateValidationCallback = delegate { return true; };
				if (encoding != null)
					request.Headers.Add("Authorization", "Basic " + encoding);
				request.Method = "POST";
				byte[] byteArray = Encoding.UTF8.GetBytes(JsonText);
				request.ContentType = "application/json;charset=UTF-8";
				request.ContentLength = byteArray.Length;
				Stream dataStream = await request.GetRequestStreamAsync();
				dataStream.Write(byteArray, 0, byteArray.Length);
				dataStream.Close();
				WebResponse response = await request.GetResponseAsync();
				string responseFromServer = null;
				using (dataStream = response.GetResponseStream())
				{
					StreamReader reader = new StreamReader(dataStream);
					responseFromServer = reader.ReadToEnd();
				}
				// Close the response.
				response.Close();
				return responseFromServer;
			}
			catch (Exception ex)
			{
				return null;
			}
		}
		public static async Task<string> WebRequestProtocolJSON_GETAsync(string url, List<ParammRequest_GET> Param = null, string encoding = null)
		{
			try
			{
                #region добавляем параметры к юрл
                if (Param != null)
					for (int i = 0; i < Param.Count; i++)
						url += $"{(i>0? "&" : "?")}{Param[i].Key}={Param[i].Paramm}";
                #endregion
                string responseFromServer;
				HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
				request.ServerCertificateValidationCallback = delegate { return true; };
				if (!string.IsNullOrEmpty(encoding))
					request.Headers.Add("Authorization", "Basic " + encoding);
				request.Credentials = CredentialCache.DefaultCredentials;
				WebResponse response = await request.GetResponseAsync();
				using (Stream dataStream = response.GetResponseStream())
				{
					StreamReader reader = new StreamReader(dataStream);
					responseFromServer = reader.ReadToEnd();
				}
				response.Close();
				return responseFromServer;
			}
			catch(Exception ex) { return null; }
		}
		public static string WebRequestProtocoOceteaStream(string url, object JsonSer, Image img, string encoding = null)
		{

			string JsonText = "";
			if(JsonSer!=null)
				JsonText = JsonSerializer.Serialize(JsonSer);
			HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
			request.ServerCertificateValidationCallback = delegate { return true; };
			if (encoding != null)
				request.Headers.Add("Authorization", "Basic " + encoding);
			request.Method = "POST";

			byte[] byteArray = Encoding.UTF8.GetBytes(JsonText);
			request.ContentType = "application/octet-stream";
			request.ContentLength = byteArray.Length;
			Stream dataStream = request.GetRequestStream();
			dataStream.Write(byteArray, 0, byteArray.Length);
			dataStream.Close();
			WebResponse response = request.GetResponse();
			string responseFromServer = null;
			using (dataStream = response.GetResponseStream())
			{
				//StreamReader reader = new StreamReader(dataStream);
				img.Source = ImageSource.FromStream(() => dataStream);
				//responseFromServer = reader.ReadToEnd();
			}
			// Close the response.
			//response.Close();
			return responseFromServer;
		}
		public static string GetFinalRedirect(string url, string encoding = null)
		{
			if (string.IsNullOrWhiteSpace(url))
				return url;

			int maxRedirCount = 8;  // prevent infinite loops
			string newUrl = url;
			do
			{
				HttpWebRequest req = null;
				HttpWebResponse resp = null;
				try
				{
					req = (HttpWebRequest)HttpWebRequest.Create(url);
					if (encoding != null)
						req.Headers.Add("Authorization", "Basic " + encoding);
					req.Method = "HEAD";
					req.AllowAutoRedirect = false;
					resp = (HttpWebResponse)req.GetResponse();
					switch (resp.StatusCode)
					{
						case HttpStatusCode.OK:
							return newUrl;
						case HttpStatusCode.Redirect:
						case HttpStatusCode.MovedPermanently:
						case HttpStatusCode.RedirectKeepVerb:
						case HttpStatusCode.RedirectMethod:
							newUrl = resp.Headers["Location"];
							if (newUrl == null)
								return url;

							if (newUrl.IndexOf("://", System.StringComparison.Ordinal) == -1)
							{
								// Doesn't have a URL Schema, meaning it's a relative or absolute URL
								Uri u = new Uri(new Uri(url), newUrl);
								newUrl = u.ToString();
							}
							break;
						default:
							return newUrl;
					}
					url = newUrl;
				}
				catch (WebException)
				{
					// Return the last known good URL
					return newUrl;
				}
				catch (Exception ex)
				{
					return null;
				}
				finally
				{
					if (resp != null)
						resp.Close();
				}
			} while (maxRedirCount-- > 0);

			return newUrl;
		}

		#region Проверка подключения к интернет

		public enum ConnectionStatus
		{
			NotConnected,
			LimitedAccess,
			Connected
		}
		public static ConnectionStatus CheckInternet()
		{
			// Проверить подключение к dns.msftncsi.com
			try
			{
				IPHostEntry entry = Dns.GetHostEntry("dns.msftncsi.com");
				if (entry.AddressList.Length == 0)
				{
					return ConnectionStatus.NotConnected;
				}
				else
				{
					if (!entry.AddressList[0].ToString().Equals("131.107.255.255"))
					{
						return ConnectionStatus.LimitedAccess;
					}
					return ConnectionStatus.Connected;
				}
			}
			catch
			{
				return ConnectionStatus.NotConnected;
			}

			
		}
		public static bool CheckInternetConnection()
		{
			try
			{
				IPHostEntry entry = Dns.GetHostEntry("dns.msftncsi.com");
				if (entry.AddressList.Length == 0)
				{
					return false ;
				}
				else
				{
					if (!entry.AddressList[0].ToString().Equals("131.107.255.255"))
					{
						return true;
					}
					return true;
				}
			}
			catch
			{
				return false;
			}
		}
		#endregion
	}
}
