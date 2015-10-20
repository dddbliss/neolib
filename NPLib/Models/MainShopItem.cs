using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLib.Models
{
	public class MainShopItem
	{
		public string Name { get; set; }
		public Uri Image { get; set; }
		public string HaggleUri { get; set; }
		public string RefererUri { get; set; }
		public int InStock { get; set; }
		public int Cost { get; set; }
	}
}
