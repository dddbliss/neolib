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
		public ClientManager _client { get; set; }
		public User CurrentUser { get; set; }

        public UserManager()
        {
            _client = ClientManager.Instance;
            CurrentUser = new User() { is_authenticated = false };
        }

        public void Login(string username, string password, Action<bool> callback)
        {
            var post_data = new Dictionary<string, string>() {
                {"destination", "/index.phtml"},
                {"username", username},
                {"password", password}
            };

            _client.SendMessage("Attempting to log in to Neopets.");
            _client.Post("http://www.neopets.com/login.phtml", "http://www.neopets.com/index.phtml", post_data, new Action<object>((response) =>
            {
                var _result = (string)response;

                var _response = _result.ToHtmlDocument();

                string _np = _response.DocumentNode.SelectNodes("//a[@id='npanchor']")[0].InnerText;
                string _nc = _response.DocumentNode.SelectNodes("//a[@id='ncanchor']")[0].InnerText;

                if (true)
                {
                    ClientManager.Instance.SendMessage("Logged into Neopets.");
                    CurrentUser = new User()
                    {
                        is_authenticated = true,
                        username = username,
                        NP = int.Parse(_np.Replace(",", "")),
                        NC = int.Parse(_nc.Replace(",", ""))
                    };

                    callback.Invoke(true);
                }
                else
                {
                    callback.Invoke(false);
                }

            }));
        }		

		public void Logout()
		{
			_client.Get("http://www.neopets.com/logout.phtml", "http://www.neopets.com/", new Action<object>((response) =>
            {
                ClientManager.Instance.SendMessage("Successfully logged out of Neopets.");
            }));
		}
	}
}
