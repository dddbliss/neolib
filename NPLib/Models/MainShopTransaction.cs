using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLib.Models
{
	public class MainShopTransaction
	{
		public string Name { get; set; }
        public Uri Image { get; set; }
		public int PurchasePrice { get; set; }
        public bool WasSuccessful { get; set; }

        public DateTime Date { get; set; }

        public string DisplayPrice { get { return string.Format("{0:n0} NP", PurchasePrice);  } }
	}
}
