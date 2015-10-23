using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLib.Models
{
	public class User
	{
        public bool is_authenticated { get; set; }
		public string username { get; set; }
		public int NP { get; set; }
		public int NC { get; set; }

        public string DisplayNP
        {
            get
            {
                return NP.ToString("N") + " NP";
            }
        }
	}
}
