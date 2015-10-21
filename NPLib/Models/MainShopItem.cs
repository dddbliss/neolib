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
	}
}
