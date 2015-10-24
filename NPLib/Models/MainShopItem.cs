using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLib.Models
{
	public class MainShopItem : Item
	{
		public string HaggleUri { get; set; }
		public string RefererUri { get; set; }
		public int InStock { get; set; }
		public int Cost { get; set; }

        public string DisplayCost
        {
            get
            {
                return Cost.ToString("N0") + " NP";
            }
        }

        public string DisplayInStock
        {
            get
            {
                return "Quantity: " + InStock.ToString("N0");
            }
        }
    }
}
