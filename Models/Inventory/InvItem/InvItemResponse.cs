using ERP.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvItem
{
    public class InvItemResponse : WebResponse
    {
        public InvItemResponse()
        {
            ResponseTS = DateTime.Now;
            InvItems = new List<InvItemInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public InvItemInfo InvItem;
        public List<InvItemInfo> InvItems;
    }
}