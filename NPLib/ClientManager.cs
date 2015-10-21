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

		public async Task<string> Get(string url, string referer, int delay_ms = 0)
		{	
			if(delay_ms > 0)
				Thread.Sleep(delay_ms);

			return await _wrapper.Get(url, referer);
		}

		public async Task<byte[]> GetBinary(string url, string referer)
		{
			return await _wrapper.GetBinary(url, referer);
		}

		public async Task<string> Post(string url, string referer, Dictionary<string, string> post_data, int delay_ms = 0)
		{
			return await _wrapper.Post(url, referer, post_data);
		}
	}
}
