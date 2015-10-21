using NPLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLib
{
	public class InventoryManager
	{
		private ClientManager _client { get; set; }

		public InventoryManager()
		{
			_client = ClientManager.Instance;
		}

		public async Task<List<InventoryItem>> GetInventory()
		{
			List<InventoryItem> _item_list = new List<InventoryItem>();
            var _result = await _client.Get("http://www.neopets.com/inventory.phtml", "http://www.neopets.com/index.phtml");
                
             var _response = _result.ToHtmlDocument();

			var _td_items = _response.DocumentNode.SelectNodes("//td[@class='']");
			foreach (var _item in _td_items)
			{
				var name = _item.InnerText;
				var image = new Uri(_item.SelectSingleNode("a/img").GetAttributeValue("src", "about:none"));
				_item_list.Add(new InventoryItem() { Name = name, Image = image });
			}

			return _item_list;
		}
	}
}
