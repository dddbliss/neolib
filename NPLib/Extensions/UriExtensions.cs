using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLib
{
    public static class UriExtensions
    {
        public async static Task<MemoryStream> DownloadIntoMemory(this Uri @this)
        {
            var client = ClientManager.Instance;

            var bytes = await client.GetBinary(@this.ToString(), "http://www.neopets.com");

            return new MemoryStream(bytes);
        }
    }
}
