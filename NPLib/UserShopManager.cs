using NPLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLib
{
    public class UserShopManager
    {

        //public void GetItemsInShop(Action<List<UserShopItem>> callback)
        //{
        //    List<UserShopItem> _item_list = new List<UserShopItem>();
        //    _client.Get("http://www.neopets.com/market.phtml?type=your", "http://www.neopets.com/market.phtml", new Action<object>((response) =>
        //    {
        //        var _result = (string)response;
        //        var _response = _result.ToHtmlDocument();

        //        var links = _response.DocumentNode.SelectNodes("//*[@id=\"content\"]/table/tbody/tr/td[2]/p[3]/a");
        //        links.RemoveAt(0);

        //        var items = _response.DocumentNode.SelectNodes("//*[@id=\"content\"]/table/tbody/tr/td[2]/form[2]/table/tbody/tr");
        //        items.RemoveAt(0);

        //        foreach (var _item in items)
        //        {
        //            var name = _item.InnerText;
        //            var image = new Uri(_item.SelectSingleNode("a/img").GetAttributeValue("src", "about:none"));
        //            _item_list.Add(new InventoryItem() { Name = name, Image = image });
        //        }

        //        callback.Invoke(_item_list);
        //    }), ClientManager.Instance.GetRandomMS(ClientManager.Instance.Settings.GeneralWaitMin, ClientManager.Instance.Settings.GeneralWaitMin));
        //}
    }
}
