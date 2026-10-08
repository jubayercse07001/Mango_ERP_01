using ERP.Models.HR.Common.HrReligion;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Common
{
    
    public class HrReligionDAL
    {
        public static HrReligionInfo GetReligionByID(string RowId)
        {
            HrReligionInfo HrReligion = null;
            using (var repo = new GMContext())
            {
                HrReligion = repo.Database.SqlQuery<HrReligionInfo>(@"
                   SELECT [RowId]
                         ,[ReligionId]
                         ,[ReligionName]
                         ,[Rank]
                         ,[DomainStatus]
                     FROM [dbo].[HrReligion] a
                     WHERE a.[RowId] = @RowId and a.DomainStatus = 1
               ", new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }
            return HrReligion;
        }

        public static HrReligionInfo GetHrReligionByName(string ReligionName, string RowId)
        {
            HrReligionInfo HrReligion = null;
            using (var repo = new GMContext())
            {
                HrReligion = repo.Database.SqlQuery<HrReligionInfo>(@"
                    SELECT [RowId]
                         ,[ReligionId]
                         ,[ReligionName]
                         ,[Rank]
                         ,[DomainStatus]
                     FROM [dbo].[HrReligion] 
                     WHERE [ReligionName] = @ReligionName
              AND (@RowId IS NULL OR [RowId] <> @RowId)
              AND DomainStatus = 1", new SqlParameter("@ReligionName", ReligionName)
                       , new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }
            return HrReligion;
        }

        
        public static List<HrReligionInfo> GetAllHrReligions()
        {
            List<HrReligionInfo> HrReligions = null;
            using (var repo = new GMContext())
            {
                HrReligions = repo.Database.SqlQuery<HrReligionInfo>(@"
                   SELECT [RowId]
                         ,[ReligionId]
                         ,[ReligionName]
                         ,[Rank]
                         ,[DomainStatus]
                     FROM [dbo].[HrReligion] a
                       WHERE a.[DomainStatus] = 1
                     ORDER BY [ReligionId]
               ").ToList();
            }
            return HrReligions;
        }
        public static void AddHrReligion(HrReligionInfo HrReligion, string loggedInUser)
        {
            HrReligionInfo loc;
            using (var repo = new GMContext())
            {
                loc = repo.Database.SqlQuery<HrReligionInfo>(@"
                        SELECT ISNULL(MAX([ReligionId]), 0) + 1 AS ReligionId
                        FROM [dbo].[HrReligion]
                         WHERE [DomainStatus] = 1
                        ").FirstOrDefault();


                repo.Database.ExecuteSqlCommand(@"
                   INSERT INTO [dbo].[HrReligion]
                                  ([RowId]
                                  ,[ReligionId]
                                  ,[ReligionName]
                                  ,[Rank]
                                  ,[CreateUser]
                                  ,[CreateDate]
                                  ,[DomainStatus])
                            VALUES
                                  (@RowId
                                  ,@ReligionId
                                  ,@ReligionName
                                  ,@Rank
                                  ,@CreateUser
                                  ,getdate() 
                                  ,1)
               ", new SqlParameter("@RowId", HrReligion.RowId)
                , new SqlParameter("@ReligionId", loc.ReligionId)
                , new SqlParameter("@ReligionName", HrReligion.ReligionName)
                , new SqlParameter("@Rank", HrReligion.Rank)
                , new SqlParameter("@CreateUser", loggedInUser));
            }

        }

        public static void UpdateHrReligion(HrReligionInfo HrReligion, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE [dbo].[HrReligion]
                              SET [ReligionName] = @ReligionName
                                 ,[ReligionCode] = @ReligionCode
                                 ,[UpdateUser] = @UpdateUser
                                 ,[UpdateDate] = getdate()
                            WHERE [RowId] = @RowId
               ", new SqlParameter("@RowId", HrReligion.RowId)
                , new SqlParameter("@ReligionName", HrReligion.ReligionName)
                , new SqlParameter("@Rank", HrReligion.Rank)
                , new SqlParameter("@UpdateUser", loggedInUser));
            }

        }
        public static void DeleteHrReligion(HrReligionInfo HrReligion, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                   UPDATE [dbo].[HrReligion]
                      SET [DomainStatus] = 0
                         ,[UpdateUser] = @UpdateUser
                         ,[UpdateDate] = getdate() 
                    WHERE [RowId] = @RowId
               ", new SqlParameter("@RowId", HrReligion.RowId)
                 , new SqlParameter("@UpdateUser", loggedInUser));

            }

        }

        public static HrReligionResponse GetFilteredHrReligions(HttpRequestBase Request)
        {
            HrReligionResponse resp = new HrReligionResponse();
            GMDataTableDAL gmdt = new GMDataTableDAL(Request);
            resp.HrReligions = null;
            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@" FROM [dbo].[HrReligion] a
                        WHERE a.[DomainStatus] = 1");

                resp.TotalRecordCount = repo.Database.SqlQuery<int>(string.Format(@"
                    SELECT count(*) {0} ", sb.ToString())).Single();

                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                List<SqlParameter> sqlParameters1 = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.AppendLine(" AND  CHARINDEX(@SearchValue, ReligionName) > 0 ");
                    sb.AppendLine(" AND  CHARINDEX(@SearchValue, Rank) > 0 ");
                    sqlParameters.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));

                }

                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(String.Format(@"
                    SELECT count(*) {0}", sb.ToString()), sqlParameters.ToArray()).Single();


                sqlParameters1.Add(new SqlParameter("@Skip", gmdt.Start));
                sqlParameters1.Add(new SqlParameter("@Take", gmdt.Length));

                resp.HrReligions = repo.Database.SqlQuery<HrReligionInfo>(string.Format(@"
                          SELECT 
                            a.[RowId] 
                           ,a.[ReligionId]
                           ,a.[ReligionName]
                           ,a.[Rank]
                        {0}
                        ORDER BY {1} {2}
                        OFFSET (@Skip) ROWS FETCH NEXT (@Take) ROWS ONLY
                ", sb.ToString(), gmdt.SortColum.ToString(), gmdt.SortDir), sqlParameters1.ToArray()).ToList();
            }
            return resp;
        }
    }
}