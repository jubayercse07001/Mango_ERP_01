using ERP.Models.User;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;

namespace ERP.DAL
{
    public class UserDAL
    {
        public static UserInfo GetUserByUsername(string username)
        {
            UserInfo user = null;
            using (var repo = new GMContext())
            {
                user = repo.Database.SqlQuery<UserInfo>(@"
                     SELECT [UserCode]
                          ,[UserName]
                          ,[Password]
                          ,[DisplayName]
                          ,[PermissionGroup]
                      FROM [dbo].[User]
	                  WHERE UserName = @UserName AND DomainStatus = 1
                ", new SqlParameter("@UserName", username)).FirstOrDefault();
            }
            return user;
        }
        public static UserInfo GetUserByUserCode(string usercode)
        {
            UserInfo user = null;
            using (var repo = new GMContext())
            {
                user = repo.Database.SqlQuery<UserInfo>(@"
                     SELECT [UserCode]
                          ,[UserName]
                          ,[DisplayName]
                          ,[PermissionGroup]
                      FROM [dbo].[User]
	                  WHERE UserCode = @UserCode AND DomainStatus = 1
                ", new SqlParameter("@UserCode", usercode)).FirstOrDefault();
            }
            return user;
        }
    }
}