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

        private string last_response { get; set; }
        private byte[] last_binary_response { get; set; }

        public string LastResponse { get { return last_response; } }

        public HttpWrapper()
		{
			_cookie_jar = new CookieContainer();
		}

		#region Private

        private HttpClient PrepareWebClient(string url, string referer)
        {

            HttpClientHandler _client_handler = new HttpClientHandler();

            if(ClientManager.Instance.Settings.UseProxy)
            {
                _client_handler = new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip,
                    CookieContainer = _cookie_jar,
                    UseProxy = ClientManager.Instance.Settings.UseProxy,
                    Proxy = new WebProxy(ClientManager.Instance.Settings.ProxyUri, false, null, new NetworkCredential(ClientManager.Instance.Settings.ProxyUser, ClientManager.Instance.Settings.ProxyPass))

                };
            }
            else
            {
                _client_handler = new HttpClientHandler()
                {
                    CookieContainer = _cookie_jar,
                };
            }

            var client = new HttpClient(_client_handler) { BaseAddress = new Uri(url) };
			client.DefaultRequestHeaders.UserAgent.ParseAdd(ClientManager.Instance.Settings.UserAgent);
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
                last_response = result;
				return result;
			}
			catch (Exception ex)
			{
				ClientManager.Instance.SendMessage("Request failed to complete.", Models.LogLevel.Error);
                throw new HttpRequestException("HTTP GET Request failed. Connection failure?", ex);
                
			}
		}

        public async Task<byte[]> GetBinary(string url, string referer)
        {
			HttpClient me = PrepareWebClient(url, referer);
			try
			{
				byte[] response = await me.GetByteArrayAsync(url);
                last_binary_response = response;
				return response;
			}
			catch (Exception ex)
			{
				ClientManager.Instance.SendMessage("Request failed to complete.", Models.LogLevel.Error);
                throw new HttpRequestException("HTTP GET Binary Request failed. Connection failure?", ex);
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
                last_response = result;
				return result;
			}
			catch (Exception ex)
			{
                ClientManager.Instance.SendMessage("Request failed to complete.", Models.LogLevel.Error);
                throw new HttpRequestException("HTTP POST Request failed. Connection failure?", ex);
            }
        }

  

		#endregion
	}
}
