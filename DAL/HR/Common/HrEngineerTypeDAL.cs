using ERP.Models.HR.Common.HrEngineerType;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Common
{
    
    public class HrEngineerTypeDAL
    {
        public static HrEngineerTypeInfo GetEngineerTypeByID(string RowId)
        {
            HrEngineerTypeInfo HrEngineerType = null;
            using (var repo = new GMContext())
            {
                HrEngineerType = repo.Database.SqlQuery<HrEngineerTypeInfo>(@"
                   SELECT [RowId]
                         ,[EngineerTypeId]
                         ,[EngineerTypeName]
                         ,[DomainStatus]
                     FROM [dbo].[HrEngineerType] a
                     WHERE a.[RowId] = @RowId and a.DomainStatus = 1
               ", new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }
            return HrEngineerType;
        }

        public static HrEngineerTypeInfo GetHrEngineerTypeByName(string EngineerTypeName, string RowId)
        {
            HrEngineerTypeInfo HrEngineerType = null;
            using (var repo = new GMContext())
            {
                HrEngineerType = repo.Database.SqlQuery<HrEngineerTypeInfo>(@"
                     SELECT [RowId]
                         ,[EngineerTypeId]
                         ,[EngineerTypeName]
                         ,[DomainStatus]
                     FROM [dbo].[HrEngineerType] 
                     WHERE [EngineerTypeName] = @EngineerTypeName
              AND (@RowId IS NULL OR [RowId] <> @RowId)
              AND DomainStatus = 1", new SqlParameter("@EngineerTypeName", EngineerTypeName)
                       , new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }
            return HrEngineerType;
        }

        
        public static List<HrEngineerTypeInfo> GetAllHrEngineerTypes()
        {
            List<HrEngineerTypeInfo> HrEngineerTypes = null;
            using (var repo = new GMContext())
            {
                HrEngineerTypes = repo.Database.SqlQuery<HrEngineerTypeInfo>(@"
                    SELECT [RowId]
                         ,[EngineerTypeId]
                         ,[EngineerTypeName]
                         ,[DomainStatus]
                     FROM [dbo].[HrEngineerType] a
                       WHERE a.[DomainStatus] = 1
                     ORDER BY [EngineerTypeId]
               ").ToList();
            }
            return HrEngineerTypes;
        }
        public static void AddHrEngineerType(HrEngineerTypeInfo HrEngineerType, string loggedInUser)
        {
            HrEngineerTypeInfo loc;
            using (var repo = new GMContext())
            {
                loc = repo.Database.SqlQuery<HrEngineerTypeInfo>(@"
                        SELECT ISNULL(MAX([EngineerTypeId]), 0) + 1 AS EngineerTypeId
                        FROM [dbo].[HrEngineerType]
                         WHERE [DomainStatus] = 1
                        ").FirstOrDefault();


                repo.Database.ExecuteSqlCommand(@"
                   INSERT INTO [dbo].[HrEngineerType]
                                  ([RowId]
                                  ,[EngineerTypeId]
                                  ,[EngineerTypeName]
                                  ,[CreateUser]
                                  ,[CreateDate]
                                  ,[DomainStatus])
                            VALUES
                                  (@RowId
                                  ,@EngineerTypeId
                                  ,@EngineerTypeName
                                  ,@CreateUser
                                  ,getdate() 
                                  ,1)
               ", new SqlParameter("@RowId", HrEngineerType.RowId)
                , new SqlParameter("@EngineerTypeId", loc.EngineerTypeId)
                , new SqlParameter("@EngineerTypeName", HrEngineerType.EngineerTypeName)
                , new SqlParameter("@CreateUser", loggedInUser));
            }

        }

        public static void UpdateHrEngineerType(HrEngineerTypeInfo HrEngineerType, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE [dbo].[HrEngineerType]
                              SET [EngineerTypeName] = @EngineerTypeName
                                 ,[UpdateUser] = @UpdateUser
                                 ,[UpdateDate] = getdate()
                            WHERE [RowId] = @RowId
               ", new SqlParameter("@RowId", HrEngineerType.RowId)
                , new SqlParameter("@EngineerTypeName", HrEngineerType.EngineerTypeName)
                , new SqlParameter("@UpdateUser", loggedInUser));
            }

        }
        public static void DeleteHrEngineerType(HrEngineerTypeInfo HrEngineerType, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                   UPDATE [dbo].[HrEngineerType]
                      SET [DomainStatus] = 0
                         ,[UpdateUser] = @UpdateUser
                         ,[UpdateDate] = getdate() 
                    WHERE [RowId] = @RowId
               ", new SqlParameter("@RowId", HrEngineerType.RowId)
                 , new SqlParameter("@UpdateUser", loggedInUser));

            }

        }

        public static HrEngineerTypeResponse GetFilteredHrEngineerTypes(HttpRequestBase Request)
        {
            HrEngineerTypeResponse resp = new HrEngineerTypeResponse();
            GMDataTableDAL gmdt = new GMDataTableDAL(Request);
            resp.HrEngineerTypes = null;
            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@" FROM [dbo].[HrEngineerType] a
                        WHERE a.[DomainStatus] = 1");

                resp.TotalRecordCount = repo.Database.SqlQuery<int>(string.Format(@"
                    SELECT count(*) {0} ", sb.ToString())).Single();

                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                List<SqlParameter> sqlParameters1 = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.AppendLine(" AND  CHARINDEX(@SearchValue, EngineerTypeName) > 0 ");
                    sqlParameters.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));

                }

                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(String.Format(@"
                    SELECT count(*) {0}", sb.ToString()), sqlParameters.ToArray()).Single();


                sqlParameters1.Add(new SqlParameter("@Skip", gmdt.Start));
                sqlParameters1.Add(new SqlParameter("@Take", gmdt.Length));

                resp.HrEngineerTypes = repo.Database.SqlQuery<HrEngineerTypeInfo>(string.Format(@"
                          SELECT 
                            a.[RowId] 
                           ,a.[EngineerTypeId]
                           ,a.[EngineerTypeName]
                        {0}
                        ORDER BY {1} {2}
                        OFFSET (@Skip) ROWS FETCH NEXT (@Take) ROWS ONLY
                ", sb.ToString(), gmdt.SortColum.ToString(), gmdt.SortDir), sqlParameters1.ToArray()).ToList();
            }
            return resp;
        }
    }
}