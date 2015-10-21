using NPLib.Models;
using NPLib.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLib
{
	public class UserManager
	{
		private ClientManager _client { get; set; }
		public User CurrentUser { get; set; }

		public UserManager()
		{
			_client = ClientManager.Instance;
			CurrentUser = new User();
		}

        public async Task<bool> Login(string username, string password)
        {
            var post_data = new Dictionary<string, string>() {
                {"destination", "/index.phtml"},
                {"username", username},
                {"password", password}
            };

            var _result = await _client.Post("http://www.neopets.com/login.phtml", "http://www.neopets.com/index.phtml", post_data);
            var _response = _result.ToHtmlDocument();

            string _np = _response.DocumentNode.SelectNodes("//a[@id='npanchor']")[0].InnerText;
            string _nc = _response.DocumentNode.SelectNodes("//a[@id='ncanchor']")[0].InnerText;

            if (true)
            {
                CurrentUser = new User()
                {
                    username = username,
                    NP = int.Parse(_np.Replace(",", "")),
                    NC = int.Parse(_nc.Replace(",", ""))
                };

                return true;
            }
        }
		//public bool Login(string username, string password)
		//{
		//	var post_data = new Dictionary<string, string>() {
		//		{"destination", "/index.phtml"},
		//		{"username", username},
		//		{"password", password}
		//	};

		//	var _response = _client.Post("http://www.neopets.com/login.phtml", "http://www.neopets.com/index.phtml", post_data).ToHtmlDocument();

		//	string _np = _response.DocumentNode.SelectNodes("//a[@id='npanchor']")[0].InnerText;
		//	string _nc = _response.DocumentNode.SelectNodes("//a[@id='ncanchor']")[0].InnerText;

		//	if(true)
		//	{
		//		CurrentUser = new User()
		//		{
		//			username = username,
		//			NP = int.Parse(_np.Replace(",", "")),
		//			NC = int.Parse(_nc.Replace(",", ""))
		//		};

		//		_client.RegisterCallback(UpdateCurrencies);

		//		return true;
		//	}
			
		//} 

		public async Task<bool> Logout()
		{
			await _client.Get("http://www.neopets.com/logout.phtml", "http://www.neopets.com/");

			return true;
		}
	}
}
