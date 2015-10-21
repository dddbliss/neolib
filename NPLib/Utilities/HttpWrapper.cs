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

		private void PrepareWebRequest(string url, string referer)
		{
			_request = (HttpWebRequest)WebRequest.Create(new Uri(url));
			_request.Referer = referer;
			_request.CookieContainer = _cookie_jar;
			_request.UserAgent = "";
		}

        private void PrepareWebClient(string url, string referer)
        {
            HttpMessageHandler _client_handler = new HttpClientHandler()
            {
                CookieContainer = _cookie_jar,
            };

            _client = new HttpClient(_client_handler) { BaseAddress = new Uri(url) };
            _client.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/46.0.2490.71 Safari/537.36");
            _client.DefaultRequestHeaders.Referrer = new Uri(referer);
        }

		#endregion

		#region Public

		public async Task<string> Get(string url, string referer)
		{
			PrepareWebClient(url, referer);
            using (HttpResponseMessage response = await _client.GetAsync(url).ConfigureAwait(false))
            {
                using (HttpContent content = response.Content)
                {
                    string result = await content.ReadAsStringAsync().ConfigureAwait(false);
                    return result;
                }
            }
		}

        public async Task<byte[]> GetBinary(string url, string referer)
        {
            PrepareWebClient(url, referer);
            var response = await _client.GetByteArrayAsync(url).ConfigureAwait(false);
            return response;
        }

        public async Task<string> Post(string url, string referer, Dictionary<string,string> post_data)
        {
            PrepareWebClient(url, referer);
            using (HttpResponseMessage response = await _client.PostAsync(url, new FormUrlEncodedContent(post_data)).ConfigureAwait(false))
            {
                using (HttpContent content = response.Content)
                {
                    string result = await content.ReadAsStringAsync().ConfigureAwait(false);
                    return result;
                }
            }
        }

  //      public string Get(string url, string referer)
  //      {
  //          PrepareWebRequest(url, referer);

  //          var _response = _request.GetResponse();
  //          var _response_stream = _response.GetResponseStream();
  //          var _content = new StreamReader(_response_stream).ReadToEnd();

  //          return _content;
  //      }

		//public byte[] GetBinary(string url, string referer)
		//{
		//	PrepareWebRequest(url, referer);

			
		//	var _response = _request.GetResponse();
		//	var _response_stream = _response.GetResponseStream();
		//	var _content = new BinaryReader(_response_stream).ReadBytes((int)_response.ContentLength);

		//	return _content;
		//}

		//public string Post(string url, string referer, Dictionary<string, string> post_data)
		//{
		//	PrepareWebRequest(url, referer);
		//	_request.Method = "POST";
		//	_request.ContentType = "application/x-www-form-urlencoded";

		//	var _post_data_array = Encoding.UTF8.GetBytes(post_data.ToPostData());
		//	_request.ContentLength = _post_data_array.Length;
		//	var _request_stream = _request.GetRequestStream();
		//	_request_stream.Write(_post_data_array, 0, _post_data_array.Length);
		//	_request_stream.Close();

		//	var _response_stream = _request.GetResponse().GetResponseStream();
		//	var _content = new StreamReader(_response_stream).ReadToEnd();
		//	var _document = _content.ToHtmlDocument();

		//	return _content;
		//}

		#endregion
	}
}
