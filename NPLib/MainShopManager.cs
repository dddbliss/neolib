using NPLib.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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

        public void GetItemsInShop(int object_id, Action<List<MainShopItem>> callback)
        {
            string _shopName = GetAllMainShops()[object_id];
            Task.Delay(ClientManager.Instance.GetRandomMS(ClientManager.Instance.Settings.GeneralWaitMin, ClientManager.Instance.Settings.GeneralWaitMin)).Wait();
            _client.SendMessage(String.Format("Checking products in stock at {0}.", _shopName));
            _client.Get("http://www.neopets.com/objects.phtml?type=shop&obj_type=" + object_id.ToString(), "http://www.neopets.com/objects.phtml", new Action<object>((response) =>
            {
                List<MainShopItem> _item_list = new List<MainShopItem>();

                var _result = (string)response;
                var _response = _result.ToHtmlDocument();

                if (_result.Contains("sold out of"))
                {
                    // Out of stock... do nothing.
                    _client.SendMessage(String.Format("{0} appears to be sold out.", _shopName), LogLevel.Info);
                    callback(new List<MainShopItem>());
                }
                else
                {
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
                    _client.SendMessage(String.Format("{0} appears to have {1} products in stock.", _shopName, _item_list.Count), LogLevel.Info);
                    callback.Invoke(_item_list);
                }
            }), ClientManager.Instance.GetRandomMS(ClientManager.Instance.Settings.GeneralWaitMin, ClientManager.Instance.Settings.GeneralWaitMin));

        }

        public void BuyItem(MainShopItem item, int haggle, decimal haggle_percent, int attempts, decimal reduction, Action<MainShopTransaction> callback)
        {
            bool is_attempting = false;
            for (int attempt = 0; attempt < attempts; attempt++)
            {
                if (!is_attempting)
                {
                    is_attempting = true;
                    if (attempt > 0)
                    {
                        // Recalculate haggle.
                        haggle = item.Cost  - Convert.ToInt32(item.Cost / (haggle_percent + (reduction * attempt)));
                        _client.SendMessage(string.Format("Adjusted haggle price for {0} to {1:n0} NP.", item.Name, haggle), LogLevel.Info);
                    }
                    else
                        _client.SendMessage(string.Format("Attempting to purchase {0} for {1:n0} NP.", item.Name, haggle), LogLevel.Info);

                    _client.Get(item.HaggleUri, item.RefererUri, new Action<object>((response) =>
                    {
                        var _response = (string)response;

                        if (_response.Contains("one item every"))
                        {
                            _client.SendMessage(string.Format("Attempted to purchase {0} too fast.", item.Name), LogLevel.Failure);
                            Task.Delay(5000).Wait();
                            callback.Invoke(new MainShopTransaction() { Name = item.Name, Image = item.Image, Date = DateTime.Now, PurchasePrice = haggle, WasSuccessful = false });
                        }
                        else if (_response.Contains("carry a maximum of"))
                        {
                            _client.SendMessage(string.Format("Attempted to buy item with a full inventory."), LogLevel.Failure);
                            callback.Invoke(new MainShopTransaction() { Name = item.Name, Image = item.Image, Date = DateTime.Now, PurchasePrice = haggle, WasSuccessful = false });
                        }
                        else if (_response.ToLower().Contains("leave this shop!!!"))
                        {
                            _client.SendMessage(string.Format("We have upset the shopkeeper."), LogLevel.Failure);
                            callback.Invoke(new MainShopTransaction() { Name = item.Name, Image = item.Image, Date = DateTime.Now, PurchasePrice = haggle, WasSuccessful = false });
                        }
                        else
                        {
                            var captcha_url = "http://www.neopets.com/" + _response.Substring("<input type=\"image\" src=\"", "\" style=\"border");

                            _client.GetBinary(captcha_url, item.RefererUri, new Action<object>((binary_data) =>
                            {
                                var _image_data = (byte[])binary_data;
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

                                    _client.Post("http://www.neopets.com/haggle.phtml", item.HaggleUri, _post_data, new Action<object>((buy_response) =>
                                    {
                                        var _response_purchase = (string)buy_response;

                                        if (_response_purchase.Contains("has been added to your inventory"))
                                        {
                                            _client.SendMessage(string.Format("Successful purchase of {0} for {1} NP.", item.Name, haggle.ToString("N0")), LogLevel.Success);
                                            callback.Invoke(new MainShopTransaction()
                                            {
                                                Name = item.Name,
                                                PurchasePrice = haggle,
                                                WasSuccessful = true,
                                                Image = item.Image,
                                                Date = DateTime.Now
                                            });

                                            attempt = attempts;
                                        }
                                        else
                                        {
                                            if (attempt != (attempts - 1))
                                                _client.SendMessage(string.Format("Failed to purchase {0} for {1:n0} NP. Going to attempt again?", item.Name, haggle), LogLevel.Warning);

                                            is_attempting = false;
                                        }

                                    }), ClientManager.Instance.GetRandomMS(ClientManager.Instance.Settings.OCRWaitMin, ClientManager.Instance.Settings.OCRWaitMax));
                                }
                            }));
                        }
                    }), ClientManager.Instance.GetRandomMS(ClientManager.Instance.Settings.PreHaggleWaitMin, ClientManager.Instance.Settings.PreHaggleWaitMax));
                }
                else
                {
                    attempt--;
                    Task.Delay(100).Wait();
                }
            }

            _client.SendMessage(string.Format("Failed to purchase {0} for {1:n0} NP. Not attempting again.", item.Name, haggle), LogLevel.Failure);
            callback.Invoke(new MainShopTransaction() { Name = item.Name, Image = item.Image, Date = DateTime.Now, PurchasePrice = haggle, WasSuccessful = false });
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

        internal static Point FastCaptchaOCR(Bitmap img)
        {
            var pixels = new byte[img.Width * img.Height];

            var lockedBits = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly,
                img.PixelFormat);
            Marshal.Copy(lockedBits.Scan0, pixels, 0, pixels.Length);
            var bitDepth = Image.GetPixelFormatSize(img.PixelFormat) / 8;

            var darkestPoint = new Point();
            var darkestPixelBrightness = 1.0f;
            for (var i = 0; i < pixels.Length; i += bitDepth)
            {
                var currentPixelBrightness =
                    Color.FromArgb(pixels[i], pixels[i + 1], pixels[i + 2], pixels[i + 3]).GetBrightness();
                if (currentPixelBrightness > darkestPixelBrightness) continue;
                darkestPoint = new Point(i / bitDepth % img.Width, i / bitDepth / img.Height);
                darkestPixelBrightness = currentPixelBrightness;
            }

            img.UnlockBits(lockedBits);

            return darkestPoint;
        }

        public static Dictionary<int, string> GetAllMainShops()
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
            }.OrderBy(m => m.Key).ToDictionary(v => v.Key, v=> v.Value);
        }
    }
}

    


