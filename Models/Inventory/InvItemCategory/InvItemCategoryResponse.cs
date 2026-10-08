using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ERP.Models.Inventory.InvItemCategory
{
    public class InvItemCategoryResponse : WebResponse
    {
        public InvItemCategoryResponse()
        {
            ResponseTS = DateTime.Now;
            InvItemCategorys = new List<InvItemCategoryInfo>();
            ResponseMessages = new List<string>();
        }
        public List<string> ResponseMessages { get; set; }
        public string ResponseMessage { get; set; }
        public InvItemCategoryInfo InvItemCategory;
        public List<InvItemCategoryInfo> InvItemCategorys;
    }
}