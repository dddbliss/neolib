using NPLib.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLib.Processors
{
	public class CurrencyProcessor: IProcessor
	{
		public void Process(string _pageSource)
		{
			var _response = _pageSource.ToHtmlDocument();

			string _np = _response.DocumentNode.SelectNodes("//a[@id='npanchor']")[0].InnerText;
			string _nc = _response.DocumentNode.SelectNodes("//a[@id='ncanchor']")[0].InnerText;

			CurrencySet obj = new CurrencySet()
			{
				NP = int.Parse(_np.Replace(",", "")),
				NC = int.Parse(_nc.Replace(",", ""))
			};

			ClientManager.Instance.SendEvent(new CurrencyEvent()
			{
				Message = "CurrencyUpdate",
				Object = obj
			});
		}
	}
}
