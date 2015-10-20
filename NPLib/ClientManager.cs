using NPLib.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NPLib
{
	public sealed class ClientManager
	{
		private static readonly ClientManager _instance = new ClientManager();
		private HttpWrapper _wrapper { get; set; }

		public string Status { get; set; }
		public bool IsProcessing { get; set; }

		private ClientManager() 
		{
			_wrapper = new HttpWrapper();
		}

		public static ClientManager Instance
		{
			get
			{
				return _instance;
			}
		}

		public string Get(string url, string referer, int delay_ms = 0)
		{	
			if(delay_ms > 0)
				Thread.Sleep(delay_ms);

			return _wrapper.Get(url, referer);
		}

		public byte[] GetBinary(string url, string referer)
		{
			return _wrapper.GetBinary(url, referer);
		}

		public string Post(string url, string referer, Dictionary<string, string> post_data, int delay_ms = 0)
		{
			Task<string> _request = new Task<string>(() =>
			{
				IsProcessing = true;

				if(delay_ms > 0)
					Thread.Sleep(delay_ms);
				
				return _wrapper.Post(url, referer, post_data);
			});

			_request.Start();

			IsProcessing = false;

			return _request.Result;
		}

		public void RegisterCallback(WebRequestCompleteEventHandler callback)
		{
			_wrapper.WebRequestCompleted += callback;
		}

		public void UnRegisterCallback(WebRequestCompleteEventHandler callback)
		{
			_wrapper.WebRequestCompleted -= callback;
		}
	}
}
