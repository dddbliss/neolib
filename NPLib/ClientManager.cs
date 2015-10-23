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
	public sealed class ClientManager : IDisposable
	{
		private static readonly ClientManager _instance = new ClientManager();
		private HttpWrapper _wrapper { get; set; }
        private HttpQueue _queue { get; set; }

        private Action<string> MessageReceiver { get; set; }

		public string Status { get; set; }
		public bool IsProcessing { get; set; }

		private ClientManager() 
		{
			_wrapper = new HttpWrapper();
            _queue = new HttpQueue();

            _queue.StartQueue();
		}

		public static ClientManager Instance
		{
			get
			{
				return _instance;
			}
		}

		public void Get(string url, string referer, Action<object> action, int delay_ms = 0)
		{	
            _queue.AddQueueItem(new HttpQueueItem(url, referer: referer, pre_delay: delay_ms, callback: action));
		}

		public void GetBinary(string url, string referer, Action<object> action)
		{
            _queue.AddQueueItem(new HttpQueueItem(url, referer: referer, is_binary: true, callback: action));
        }

		public void Post(string url, string referer, Dictionary<string, string> post_data, Action<object> action, int delay_ms = 0)
		{
            _queue.AddQueueItem(new HttpQueueItem(url, referer: referer, post_data: post_data, pre_delay: delay_ms, callback: action));
        }

        public Task ProcessQueueItem(HttpQueueItem item)
        {
            IsProcessing = true;

            return new Task(() =>
            {
                object data = new object();
                switch (item.Type)
                {
                    case HttpRequestType.Get:
                        Task.Delay(item.PreDelay * 1000).Wait();
                        data = _wrapper.Get(item.Url.ToString(), item.Referer.ToString()).Result;
                        break;
                    case HttpRequestType.Post:
                        data =  _wrapper.Post(item.Url.ToString(), item.Referer.ToString(), item.PostData).Result;
                        break;
                    case HttpRequestType.Binary:
                        data = _wrapper.GetBinary(item.Url.ToString(), item.Referer.ToString()).Result;
                        break;
                }

                IsProcessing = false;
                item.Callback.Invoke(data);
                
            });
            
            
        }

        public void RegisterMessageAction(Action<string> messageReceiver)
        {
            MessageReceiver = messageReceiver;
        }

        public void SendMessage(string message)
        {
            MessageReceiver.Invoke(message);
        }

        #region IDisposable Support
        private bool disposedValue = false;

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    //_queue.StopQueue();
                }
                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~ClientManager() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            Instance.Dispose(true);
        }
        #endregion
    }
}
