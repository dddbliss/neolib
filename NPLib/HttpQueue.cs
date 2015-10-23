using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLib
{
    public enum HttpRequestType
    {
        Get,
        Post,
        Binary
    }

    public class HttpQueueItem
    {
        public HttpRequestType Type { get; set; }
        public Uri Url { get; set; }
        public Uri Referer { get; set; }

        public int PreDelay { get; set; }
        public int PostDelay { get; set; }

        public Action<object> Callback { get; set; }
        public Dictionary<string, string> PostData { get; set; }

        public HttpQueueItem()
        {

        }
        public HttpQueueItem(string url, Action<object> callback, string referer = null, Dictionary<string, string> post_data = null, bool is_binary = false, int pre_delay = 0, int post_delay = 0)
        {
            if(post_data != null)
            {
                Type = HttpRequestType.Post;
                PostData = post_data;
            }
            else if(is_binary)
            {
                Type = HttpRequestType.Binary;
            }
            else
            {
                Type = HttpRequestType.Get;
            }

            Url = new Uri(url);

            if(!string.IsNullOrWhiteSpace(referer))
                Referer = new Uri(referer);

            if (pre_delay > 0)
                PreDelay = pre_delay;
            if (post_delay > 0)
                PostDelay = post_delay;

            Callback = callback;
        }
    }
    public class HttpQueue
    {
        public ConcurrentQueue<HttpQueueItem> Queue { get; set; }
        public Task Runner { get; set; }
        private bool IsQueueActive { get; set; }

        public HttpQueue()
        {
            Queue = new ConcurrentQueue<HttpQueueItem>();
            IsQueueActive = false;
        }

        public async void StartQueue()
        {
            IsQueueActive = true;
            Runner = new Task(() =>
            {
                while(IsQueueActive)
                {
                    // Item in queue.
                    HttpQueueItem item = new HttpQueueItem();
                    if (Queue.TryDequeue(out item))
                    {
                        //Task.Delay(item.PreDelay).Wait();
                        ClientManager.Instance.ProcessQueueItem(item).Start();
                        
                        //Task.Delay(item.PostDelay);
                    }
                    else
                    {
                        // Nothing to do, we wait.
                        Task.Delay(100).Wait();
                    }
                }
                
            });

            Runner.Start();
        }

        public void StopQueue()
        {
            IsQueueActive = false;
        }

        public void AddQueueItem(HttpQueueItem item)
        {
            if(!(Queue.Where(i => i.Url == item.Url && i.Referer == item.Referer).Count() > 0))
                Queue.Enqueue(item);
        }
    }
}
