using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace ERP.DAL
{
    public class GMDataTableDAL
    {
        public int Start { get; set; }
        public int Length { get; set; }
        public string SearchValue { get; set; }
        public int SortColum { get; set; }
        public string SortDir { get; set; }

        public GMDataTableDAL(HttpRequestBase Request)
        {
            Start = Convert.ToInt32(Request["start"]);
            Length = Convert.ToInt32(Request["length"]);
            SearchValue = Request["search[value]"];
            SortColum = Convert.ToInt32(Request["order[0][column]"]) + 2;
            SortDir = Request["order[0][dir]"];
        }

    }
}