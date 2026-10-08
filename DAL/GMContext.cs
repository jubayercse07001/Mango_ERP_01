using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace ERP.DAL
{
    public class GMContext : DbContext
    {
        
            //public GMContext() : base(@"Data Source=DESKTOP-ME82O9N\SQLEXPRESS;Initial Catalog=BGFCL_STORE_WEB;Integrated Security=True")
            // public GMContext() : base(@"Data Source=103.221.255.115;Initial Catalog=BGFCL_STORE_WEB;Integrated Security=false;User Id=sa;Password=Du@ict@cell;")
            //public GMContext() : base(@"Data Source=DESKTOP-TF7LP71\MSSQLSERVER1;Initial Catalog=Mango_HR;Integrated Security=True")
            //public GMContext() : base(@"Data Source=DESKTOP-TKI8NPV\SQLEXPRESS;Initial Catalog=Mango_HR;Integrated Security=True")
            //public GMContext() : base(@"Data Source=DESKTOP-CN9TSCI;Initial Catalog=BGFCL_STORE_WEB;Integrated Security=false;User Id=sa;Password=07001;")
            //public GMContext() : base(@"Data Source=DESKTOP-CN9TSCI;Initial Catalog=Mango_HR;Integrated Security=True")
            //public GMContext() : base(@"Data Source=192.168.4.195;Initial Catalog=Mango_HR;Integrated Security=false;User Id=sa;Password=07001;")
            public GMContext() : base(@"Data Source=LAPTOP-O1C0SV33\SQLEXPRESS;Initial Catalog=Mango_ERP;Integrated Security=false;User Id=sa;Password=07001;")
            {

            }
        
    }
}