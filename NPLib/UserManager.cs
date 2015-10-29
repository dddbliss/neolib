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

            _client.SendMessage("Attempting to log in to Neopets.", LogLevel.Info);
            _client.Post("http://www.neopets.com/login.phtml", "http://www.neopets.com/index.phtml", post_data, new Action<object>((response) =>
            {
                var _result = (string)response;

                var _response = _result.ToHtmlDocument();

                string _np = _response.DocumentNode.SelectNodes("//a[@id='npanchor']")[0].InnerText;

                string _nc = "0";
                
                var _nc_node = _response.DocumentNode.SelectNodes("//a[@id='ncanchor']");

                if (_nc_node.Count > 0) 
                    _nc = _nc_node[0].InnerText;

                if (_result.ToLower().Contains("welcome, "))
                {
                    _client.SendMessage("Logged into Neopets.");
                    CurrentUser = new User()
                    {
                        is_authenticated = true,
                        username = username,
                        NP = int.Parse(_np.Replace(",", "")),
                        NC = int.Parse(_nc.Replace(",", ""))
                    };

                    _client.SendMessage("Log into Neopets was successful.", LogLevel.Info);
                    callback.Invoke(true);
                }
                else
                {
                    _client.SendMessage("Log into Neopets was not successful.", LogLevel.Error);
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
