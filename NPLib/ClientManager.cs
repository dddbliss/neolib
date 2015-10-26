using NPLib.Events;
using NPLib.Models;
using NPLib.Processors;
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

		private List<IProcessor> _processors { get; set; }

        private Action<LogMessage> MessageReceiver { get; set; }
		private Action<Event> EventReceiver { get; set; }

		public ClientSettings Settings { get; set; }

		private ClientManager() 
		{
			_wrapper = new HttpWrapper();
            _queue = new HttpQueue();

			_processors = new List<IProcessor>();
			_processors.Add(new CurrencyProcessor());

			Settings = new ClientSettings()
			{
				GeneralWaitMin = 3.0m,
				GeneralWaitMax = 6.0m,
				PreHaggleWaitMin = 0.5m,
				PreHaggleWaitMax = 0.9m,
				OCRWaitMin = 0.5m,
				OCRWaitMax = 0.9m,
			};

            _queue.StartQueue();
		}

		public static ClientManager Instance
		{
			get
			{
				return _instance;
			}
		}

        public string GetLastRequestBody()
        {
            return _wrapper.LastResponse;
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

        public async void ProcessQueueItem(HttpQueueItem item)
        {
				object data = new object();
                
				// Wait for pre-defined time.
				Task.Delay(item.PreDelay).Wait();
			
				switch (item.Type)
                {
                    case HttpRequestType.Get:
                        data = await _wrapper.Get(item.Url.ToString(), item.Referer.ToString());
                        break;
                    case HttpRequestType.Post:
                        data = await _wrapper.Post(item.Url.ToString(), item.Referer.ToString(), item.PostData);
                        break;
                    case HttpRequestType.Binary:
                        data = await _wrapper.GetBinary(item.Url.ToString(), item.Referer.ToString());
                        break;
                }
                item.Callback.Invoke(data);

				if(item.Type == HttpRequestType.Get || item.Type == HttpRequestType.Post)
					_processors.ForEach(processor => processor.Process((string)data));
        }

        public void RegisterMessageAction(Action<LogMessage> messageReceiver)
        {
            MessageReceiver = messageReceiver;
        }

        public void SendMessage(string message, LogLevel level = LogLevel.Info)
        {
            MessageReceiver.Invoke(new LogMessage()
            {
                Level = level,
                Message = message,
                Date = DateTime.Now
            });
        }

		public void RegisterEventHandler(Action<Event> eventReceiver)
		{
			EventReceiver = eventReceiver;
		}

		public void SendEvent(Event _event)
		{
			EventReceiver.Invoke(_event);
		}

		public int GetRandomMS(decimal min, decimal max)
		{
			int min_int = Convert.ToInt32(min * 1000m);
			int max_int = Convert.ToInt32(max * 1000m);

			var rand = new Random(new System.DateTime().Millisecond);
			var value = rand.Next(min_int, max_int);

			return value;
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
