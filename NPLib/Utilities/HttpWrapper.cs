using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace NPLib.Utilities
{
	public class HttpWrapper
	{

		private HttpWebRequest _request { get; set; }
        private HttpClient _client { get; set; }
		private CookieContainer _cookie_jar { get; set; }

		public HttpWrapper()
		{
			_cookie_jar = new CookieContainer();
		}

		#region Private

        private HttpClient PrepareWebClient(string url, string referer)
        {
			
            HttpMessageHandler _client_handler = new HttpClientHandler()
            {
                CookieContainer = _cookie_jar,
            };

            var client = new HttpClient(_client_handler) { BaseAddress = new Uri(url) };
			client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.71 Safari/537.36");
			client.DefaultRequestHeaders.Referrer = new Uri(referer);

			return client;
        }

		#endregion

		#region Public

		public async Task<string> Get(string url, string referer)
		{
			HttpClient me = PrepareWebClient(url, referer);
			try
			{
				HttpResponseMessage response = await me.GetAsync(url);
				response.EnsureSuccessStatusCode();
				string result = await response.Content.ReadAsStringAsync();
				return result;
			}
			catch (Exception ex)
			{
				ClientManager.Instance.SendMessage("Request failed to complete.");
				return string.Empty;
			}
		}

        public async Task<byte[]> GetBinary(string url, string referer)
        {
			HttpClient me = PrepareWebClient(url, referer);
			try
			{
				byte[] response = await me.GetByteArrayAsync(url);
				return response;
			}
			catch (Exception ex)
			{
				ClientManager.Instance.SendMessage("Request failed to complete.");
				return new byte[0];
			}
        }

        public async Task<string> Post(string url, string referer, Dictionary<string,string> post_data)
        {
			HttpClient me = PrepareWebClient(url, referer);
			try
			{
				HttpResponseMessage response = await me.PostAsync(url, new FormUrlEncodedContent(post_data));
				response.EnsureSuccessStatusCode();
				string result = await response.Content.ReadAsStringAsync();
				return result;
			}
			catch (Exception ex)
			{
				ClientManager.Instance.SendMessage("Request failed to complete.");
				return string.Empty;
			}
        }

  

		#endregion
	}
}
