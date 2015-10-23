using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLib.Models
{
	public class ClientSettings
	{
		public decimal GeneralWaitMin { get; set; }
		public decimal GeneralWaitMax { get; set; }

		public decimal PreHaggleWaitMin { get; set; }
		public decimal PreHaggleWaitMax { get; set; }

		public decimal OCRWaitMin { get; set; }
		public decimal OCRWaitMax { get; set; }
	}
}
