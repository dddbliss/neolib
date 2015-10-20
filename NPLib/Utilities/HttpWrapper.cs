using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NPLib.Utilities
{
	public delegate void WebRequestCompleteEventHandler(object sender, WebRequestEventArgs e);
	public delegate void WebRequestStartEventHandler(object sender, EventArgs e);

	public class HttpWrapper
	{
		public event WebRequestCompleteEventHandler WebRequestCompleted;
		public event WebRequestStartEventHandler WebRequestStarted;

		private HttpWebRequest _request { get; set; }
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

		#endregion

		#region Protected

		protected virtual void OnWebRequestCompleted(WebRequestEventArgs e)
		{
			if (WebRequestCompleted != null)
			{
				WebRequestCompleted(this, e);
			}
		}

		protected virtual void OnWebRequestStarted(EventArgs e)
		{
			if (WebRequestStarted != null)
			{
				WebRequestStarted(this, e);
			}
		}

		#endregion

		#region Public

		public string Get(string url, string referer)
		{
			PrepareWebRequest(url, referer);

			var _thread = Task.Factory.FromAsync<WebResponse>(_request.BeginGetResponse, _request.EndGetResponse, null)
				.ContinueWith(task =>
				{
					var _res = (HttpWebResponse)task.Result;
					
					return _res;
				});

			var _response = _thread.Result;
			var _response_stream = _response.GetResponseStream();
			var _content = new StreamReader(_response_stream).ReadToEnd();

			return _content;
		}

		public byte[] GetBinary(string url, string referer)
		{
			PrepareWebRequest(url, referer);

			OnWebRequestStarted(new EventArgs());

			var _response = _request.GetResponse();
			var _response_stream = _response.GetResponseStream();
			var _content = new BinaryReader(_response_stream).ReadBytes((int)_response.ContentLength);

			OnWebRequestCompleted(new WebRequestEventArgs(null));
			return _content;
		}

		public string Post(string url, string referer, Dictionary<string, string> post_data)
		{
			PrepareWebRequest(url, referer);
			_request.Method = "POST";
			_request.ContentType = "application/x-www-form-urlencoded";

			var _post_data_array = Encoding.UTF8.GetBytes(post_data.ToPostData());
			_request.ContentLength = _post_data_array.Length;
			var _request_stream = _request.GetRequestStream();
			_request_stream.Write(_post_data_array, 0, _post_data_array.Length);
			_request_stream.Close();

			OnWebRequestStarted(new EventArgs());

			var _response_stream = _request.GetResponse().GetResponseStream();
			var _content = new StreamReader(_response_stream).ReadToEnd();
			var _document = _content.ToHtmlDocument();

			OnWebRequestCompleted(new WebRequestEventArgs(_document));
			return _content;
		}

		#endregion
	}

#region Events


	public class WebRequestEventArgs : EventArgs
	{
		private readonly HtmlDocument _document;

		public WebRequestEventArgs(HtmlDocument document)
		{
			_document = document;
		}

		public HtmlDocument Document
		{
			get
			{
				return _document;
			}
		}
	}
#endregion
}
