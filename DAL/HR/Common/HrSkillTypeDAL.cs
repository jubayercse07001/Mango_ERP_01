using ERP.Models.HR.Common.HrSkillType;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Common
{

    public class HrSkillTypeDAL
    {
        public static HrSkillTypeInfo GetSkillTypeByID(string RowId)
        {
            HrSkillTypeInfo HrSkillType = null;
            using (var repo = new GMContext())
            {
                HrSkillType = repo.Database.SqlQuery<HrSkillTypeInfo>(@"
                   SELECT [RowId]
                         ,[SkillTypeId]
                         ,[SkillTypeName]
                         ,[DomainStatus]
                     FROM [dbo].[HrSkillType] a
                     WHERE a.[RowId] = @RowId and a.DomainStatus = 1
               ", new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }
            return HrSkillType;
        }

        public static HrSkillTypeInfo GetHrSkillTypeByName(string SkillTypeName, string RowId)
        {
            HrSkillTypeInfo HrSkillType = null;
            using (var repo = new GMContext())
            {
                HrSkillType = repo.Database.SqlQuery<HrSkillTypeInfo>(@"
                    SELECT [RowId]
                         ,[SkillTypeId]
                         ,[SkillTypeName]
                         ,[DomainStatus]
                     FROM [dbo].[HrSkillType] 
                     WHERE [SkillTypeName] = @SkillTypeName
              AND (@RowId IS NULL OR [RowId] <> @RowId)
              AND DomainStatus = 1", new SqlParameter("@SkillTypeName", SkillTypeName)
                       , new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }
            return HrSkillType;
        }


        public static List<HrSkillTypeInfo> GetAllHrSkillTypes()
        {
            List<HrSkillTypeInfo> HrSkillTypes = null;
            using (var repo = new GMContext())
            {
                HrSkillTypes = repo.Database.SqlQuery<HrSkillTypeInfo>(@"
                   SELECT [RowId]
                         ,[SkillTypeId]
                         ,[SkillTypeName]
                         ,[DomainStatus]
                     FROM [dbo].[HrSkillType] a
                       WHERE a.[DomainStatus] = 1
                     ORDER BY [SkillTypeId]
               ").ToList();
            }
            return HrSkillTypes;
        }
        public static void AddHrSkillType(HrSkillTypeInfo HrSkillType, string loggedInUser)
        {
            HrSkillTypeInfo loc;
            using (var repo = new GMContext())
            {
                loc = repo.Database.SqlQuery<HrSkillTypeInfo>(@"
                        SELECT ISNULL(MAX([SkillTypeId]), 0) + 1 AS SkillTypeId
                        FROM [dbo].[HrSkillType]
                         WHERE [DomainStatus] = 1
                        ").FirstOrDefault();


                repo.Database.ExecuteSqlCommand(@"
                   INSERT INTO [dbo].[HrSkillType]
                                  ([RowId]
                                  ,[SkillTypeId]
                                  ,[SkillTypeName]
                                  ,[CreateUser]
                                  ,[CreateDate]
                                  ,[DomainStatus])
                            VALUES
                                  (@RowId
                                  ,@SkillTypeId
                                  ,@SkillTypeName
                                  ,@CreateUser
                                  ,getdate() 
                                  ,1)
               ", new SqlParameter("@RowId", HrSkillType.RowId)
                , new SqlParameter("@SkillTypeId", loc.SkillTypeId)
                , new SqlParameter("@SkillTypeName", HrSkillType.SkillTypeName)
                , new SqlParameter("@CreateUser", loggedInUser));
            }

        }

        public static void UpdateHrSkillType(HrSkillTypeInfo HrSkillType, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE [dbo].[HrSkillType]
                              SET [SkillTypeName] = @SkillTypeName
                                 ,[UpdateUser] = @UpdateUser
                                 ,[UpdateDate] = getdate()
                            WHERE [RowId] = @RowId
               ", new SqlParameter("@RowId", HrSkillType.RowId)
                , new SqlParameter("@SkillTypeName", HrSkillType.SkillTypeName)
                , new SqlParameter("@UpdateUser", loggedInUser));
            }

        }
        public static void DeleteHrSkillType(HrSkillTypeInfo HrSkillType, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                   UPDATE [dbo].[HrSkillType]
                      SET [DomainStatus] = 0
                         ,[UpdateUser] = @UpdateUser
                         ,[UpdateDate] = getdate() 
                    WHERE [RowId] = @RowId
               ", new SqlParameter("@RowId", HrSkillType.RowId)
                 , new SqlParameter("@UpdateUser", loggedInUser));

            }

        }

        public static HrSkillTypeResponse GetFilteredHrSkillTypes(HttpRequestBase Request)
        {
            HrSkillTypeResponse resp = new HrSkillTypeResponse();
            GMDataTableDAL gmdt = new GMDataTableDAL(Request);
            resp.HrSkillTypes = null;
            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@" FROM [dbo].[HrSkillType] a
                        WHERE a.[DomainStatus] = 1");

                resp.TotalRecordCount = repo.Database.SqlQuery<int>(string.Format(@"
                    SELECT count(*) {0} ", sb.ToString())).Single();

                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                List<SqlParameter> sqlParameters1 = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.AppendLine(" AND  CHARINDEX(@SearchValue, SkillTypeName) > 0 ");
                    sqlParameters.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));

                }

                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(String.Format(@"
                    SELECT count(*) {0}", sb.ToString()), sqlParameters.ToArray()).Single();


                sqlParameters1.Add(new SqlParameter("@Skip", gmdt.Start));
                sqlParameters1.Add(new SqlParameter("@Take", gmdt.Length));

                resp.HrSkillTypes = repo.Database.SqlQuery<HrSkillTypeInfo>(string.Format(@"
                          SELECT 
                            a.[RowId] 
                           ,a.[SkillTypeId]
                           ,a.[SkillTypeName]
                        {0}
                        ORDER BY {1} {2}
                        OFFSET (@Skip) ROWS FETCH NEXT (@Take) ROWS ONLY
                ", sb.ToString(), gmdt.SortColum.ToString(), gmdt.SortDir), sqlParameters1.ToArray()).ToList();
            }
            return resp;
        }
    }
}