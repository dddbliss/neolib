using NPLib.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLib
{
	public class MainShopManager
	{
		private ClientManager _client { get; set; }

		public MainShopManager()
		{
			_client = ClientManager.Instance;
		}

		public List<MainShopItem> GetItemsInShop(int object_id)
		{
			List<MainShopItem> _item_list = new List<MainShopItem>();
			var _response = _client.Get("http://www.neopets.com/objects.phtml?type=shop&obj_type=" + object_id.ToString(), "http://www.neopets.com/objects.phtml").ToHtmlDocument();

			var _items_in_stock = _response.DocumentNode.SelectNodes("//td[@width='120']");
			foreach (var item in _items_in_stock)
			{
				var name = item.InnerHtml.Substring("</a><b>", "</b><br>");
				var haggle_url = item.InnerHtml.Substring("this.href='", "';if (!confirm").Replace("'+'", "");
				var cost = item.InnerHtml.Substring("<br>Cost: ", " NP<br>");
				var in_stock = item.InnerHtml.Substring("</b><br>", " in stock");
				var image = item.InnerHtml.Substring("<img src=\"", "\" width=\"80\"");

				_item_list.Add(new MainShopItem()
				{
					Name = name,
					HaggleUri = "http://www.neopets.com/" + haggle_url,
					RefererUri = "http://www.neopets.com/objects.phtml?type=shop&obj_type=" + object_id.ToString(),
					Cost = int.Parse(cost.Replace(",", "")),
					InStock = int.Parse(in_stock.Replace(",", "")),
					Image = new Uri(image)
				});
			}

			return _item_list;
		}

		public Dictionary<int, string> GetMainShops()
		{
			return new Dictionary<int, string>() 
			{
				{1,"Food Shop"},
				{14,"Chocolate Factory"},
				{15,"Bakery"},
				{16,"Healtly Food"},
				{18,"Smoothie Store"},
				{20,"Tropical Food"},
				{22,"Grundos"},
				{30,"Spooky Food"},
				{34,"Ye Olde Coffee Shop"},
				{35,"Slushie Shop"},
				{37,"Icy Fun Snow Shop"},
				{39,"Faerie Food"},
				{42,"Tyrannian Food"},
				{46,"Hubert's Hot Dog"},
				{47,"Pizzaroo"},
				{49,"Food of the Lost Desert"}, 
				{56,"Merifoods"},
				{62,"Jelly food"},
				{63,"Refreshments"},
				{66,"Kiko Lake Treats"},
				{72,"Cafe Kreludor"},
				{81,"Brightvale Fruits"},
				{90,"Qasalan Delights"},
				{95,"Exquisite Ambrosia"},
				{101,"Exotic Foods"},
				{105,"The Crumpetmonger"},
				{112,"Molten Morsels"},
				{7,"Book Shop"},
				{38,"Faerieland Bookshop"},
				{51,"Sutek's Scrolls"},
				{70,"Booktastic Books"},
				{77,"Brightvale Books"},
				{92,"Words of Antiquity"},
				{106,"Neovian Printing Press"},
				{114,"Moltaran Books"},
				{4,"Clothes Shop"},
				{107,"Prigpants & Swolthy, Tailors"},
				{108,"Mystical Surroundings"},
				{111,"Cog's Togs"},
				{117,"Ugga Shinies"},
				{25,"Neopian Petpet Shop"},
				{26,"Robopet Shop"},
				{27,"Rock Pool"},
				{31,"Spooky Pets"},
				{40,"Faerieland Petpets"},
				{44,"Tyrannian Petpets"},
				{50,"Peopatra's Pet Pets"},
				{57,"Ye olde Petpets"},
				{61,"Wintery Petpet"},
				{69,"Petpet Supplies"},
				{88,"Maraquan Petpet"},
				{89,"Geraptiku Petpets"},
				{97,"Legendary Petpets"},
				{103,"Fanciful Fauna"},
				{113,"Moltaran Petpets"},
				{12,"Garden Centre"},
				{41,"Neopian Furniture"},
				{43,"Tyrannian Furniture"},
				{55,"Osiri's Pottery"},
				{60,"Spooky Furniture"},
				{67,"Kiko Lake Carpentry"},
				{71,"Kreludan Homes"},
				{75,"Faerie Furniture"},
				{76,"Roo Island Marchandise Shop"},
				{79,"Brightvale Glaziers"},
				{104,"Chesterdrawers' Antiques"},
				{110,"Lampwyck's Lights Fantastic"},
				{2,"Magical Shop"},
				{9,"Battle Magic Shop"},
				{10,"Defense Magic Shop"},
				{23,"Space Weaponry"},
				{24,"Space Armour"},
				{36,"Ice Crystal Shop"},
				{45,"Tyrannian Weaponry"},
				{54,"Battle Supplies"},
				{59,"Haunted Weaponry"},
				{73,"Kayla's Potion Shop"},
				{78,"The Scrollery"},
				{80,"Brightvale Armoury"},
				{82,"Brightvale Motery"},
				{83,"Royal Potionery"},
				{87,"Maractite Marvels"},
				{91,"Desert Arms"},
				{93,"Faerie Weapon Shop"},
				{94,"Illustrious Armoury"},
				{96,"Magical Marvels"},
				{100,"Wonderous Weaponry"},
				{3,"Toy Shop"},
				{48,"Usuki Land"},
				{74,"Darigan Toys"},
				{98,"Plushie Palace"},
				{116,"Springy Things"},
				{13,"Neopian Pharmacy"},
				{85,"Lost Desert Medicine"},
				{102,"Remarkable Restoratives"},
				{8,"Collectable Card Shop"},
				{58,"Neopian Post Office Kiosk"},
				{68,"Collectable Coins"},
				{86,"Collectable Sea Shells"},
				{5,"Grooming Shop"},
				{17,"Neopian Gift Shop"},
				{21,"Tiki Tack"},
				{53,"Back to School Shop"},
				{84,"Neopian Music Shop"},
				{99,"Altador Cup Souvenirs"},
				{109,"Petpetpet Habitat"}
			};
		}

		public MainShopTransaction DoBuyProcess(MainShopItem item, int haggle)
		{
			var _response = _client.Get(item.HaggleUri, item.RefererUri);

			var captcha_url = "http://www.neopets.com/" + _response.Substring("<input type=\"image\" src=\"", "\" style=\"border");

			var _image_data = _client.GetBinary(captcha_url, item.RefererUri);

			using (var ms = new MemoryStream(_image_data))
			{
				var bm = new Bitmap(ms);

				Point darkestPixel = CaptchaOCR(bm);

				var _post_data = new Dictionary<string, string>()
				{
					{"current_offer", haggle.ToString()},
					{"x", darkestPixel.X.ToString()},
					{"y", darkestPixel.Y.ToString()}
				};

				var _response_purchase = _client.Post("http://www.neopets.com/haggle.phtml", item.HaggleUri, _post_data);

				if (_response_purchase.Contains("successful"))
				{
					return new MainShopTransaction()
					{
						Name = item.Name,
						PurchasePrice = haggle,
					};
				}
				else
				{
					return null;
				}
				
			}
		}

		private Point CaptchaOCR(Bitmap img)
		{
			Point oS = new Point();
			int width = img.Width;
			int height = img.Height;
			float darkPixel = 1;
			for (int x = 0; x < width; x += 10)
			{
				for (int y = 0; y < height; y += 10)
				{
					float curPixel = img.GetPixel(x, y).GetBrightness();
					if (curPixel < darkPixel)
					{
						darkPixel = curPixel;
						oS.X = x;
						oS.Y = y;
					}
				}
			}
			return oS;
		}
	}
}
