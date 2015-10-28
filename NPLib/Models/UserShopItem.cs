using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NPLib.Models
{
    public class UserShopItem : Item
    {
        public int ObjectID { get; set; }
        public int OldPrice { get; set; }
        public int Cost { get; set; }
        public int BackToInvID { get; set; }
    }
}
