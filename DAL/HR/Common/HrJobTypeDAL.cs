using ERP.Models.HR.Common.HrJobType;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Common
{
    
    public class HrJobTypeDAL
    {
        public static HrJobTypeInfo GetJobTypeByID(string RowId)
        {
            HrJobTypeInfo HrJobType = null;
            using (var repo = new GMContext())
            {
                HrJobType = repo.Database.SqlQuery<HrJobTypeInfo>(@"
                   SELECT [RowId]
                         ,[JobTypeId]
                         ,[JobType]
                         ,[DomainStatus]
                     FROM [dbo].[HrJobType] a
                     WHERE a.[RowId] = @RowId and a.DomainStatus = 1
               ", new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }
            return HrJobType;
        }

        public static HrJobTypeInfo GetHrJobTypeByName(string JobType, string RowId)
        {
            HrJobTypeInfo HrJobType = null;
            using (var repo = new GMContext())
            {
                HrJobType = repo.Database.SqlQuery<HrJobTypeInfo>(@"
                     SELECT [RowId]
                         ,[JobTypeId]
                         ,[JobType]
                         ,[DomainStatus]
                     FROM [dbo].[HrJobType] 
                     WHERE [JobType] = @JobType
              AND (@RowId IS NULL OR [RowId] <> @RowId)
              AND DomainStatus = 1", new SqlParameter("@JobType", JobType)
                       , new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }
            return HrJobType;
        }


        public static List<HrJobTypeInfo> GetAllHrJobTypes()
        {
            List<HrJobTypeInfo> HrJobTypes = null;
            using (var repo = new GMContext())
            {
                HrJobTypes = repo.Database.SqlQuery<HrJobTypeInfo>(@"
                    SELECT [RowId]
                         ,[JobTypeId]
                         ,[JobType]
                         ,[DomainStatus]
                     FROM [dbo].[HrJobType] a
                       WHERE a.[DomainStatus] = 1
                     ORDER BY [JobTypeId]
               ").ToList();
            }
            return HrJobTypes;
        }
        public static void AddHrJobType(HrJobTypeInfo HrJobType, string loggedInUser)
        {
            HrJobTypeInfo loc;
            using (var repo = new GMContext())
            {
                loc = repo.Database.SqlQuery<HrJobTypeInfo>(@"
                        SELECT ISNULL(MAX([JobTypeId]), 0) + 1 AS JobTypeId
                        FROM [dbo].[HrJobType]
                         WHERE [DomainStatus] = 1
                        ").FirstOrDefault();


                repo.Database.ExecuteSqlCommand(@"
                   INSERT INTO [dbo].[HrJobType]
                                  ([RowId]
                                  ,[JobTypeId]
                                  ,[JobType]
                                  ,[CreateUser]
                                  ,[CreateDate]
                                  ,[DomainStatus])
                            VALUES
                                  (@RowId
                                  ,@JobTypeId
                                  ,@JobType
                                  ,@CreateUser
                                  ,getdate() 
                                  ,1)
               ", new SqlParameter("@RowId", HrJobType.RowId)
                , new SqlParameter("@JobTypeId", loc.JobTypeId)
                , new SqlParameter("@JobType", HrJobType.JobType)
                , new SqlParameter("@CreateUser", loggedInUser));
            }

        }

        public static void UpdateHrJobType(HrJobTypeInfo HrJobType, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE [dbo].[HrJobType]
                              SET [JobType] = @JobType
                                 ,[UpdateUser] = @UpdateUser
                                 ,[UpdateDate] = getdate()
                            WHERE [RowId] = @RowId
               ", new SqlParameter("@RowId", HrJobType.RowId)
                , new SqlParameter("@JobType", HrJobType.JobType)
                , new SqlParameter("@UpdateUser", loggedInUser));
            }

        }
        public static void DeleteHrJobType(HrJobTypeInfo HrJobType, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                   UPDATE [dbo].[HrJobType]
                      SET [DomainStatus] = 0
                         ,[UpdateUser] = @UpdateUser
                         ,[UpdateDate] = getdate() 
                    WHERE [RowId] = @RowId
               ", new SqlParameter("@RowId", HrJobType.RowId)
                 , new SqlParameter("@UpdateUser", loggedInUser));

            }

        }

        public static HrJobTypeResponse GetFilteredHrJobTypes(HttpRequestBase Request)
        {
            HrJobTypeResponse resp = new HrJobTypeResponse();
            GMDataTableDAL gmdt = new GMDataTableDAL(Request);
            resp.HrJobTypes = null;
            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@" FROM [dbo].[HrJobType] a
                        WHERE a.[DomainStatus] = 1");

                resp.TotalRecordCount = repo.Database.SqlQuery<int>(string.Format(@"
                    SELECT count(*) {0} ", sb.ToString())).Single();

                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                List<SqlParameter> sqlParameters1 = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.AppendLine(" AND  CHARINDEX(@SearchValue, JobType) > 0 ");
                    sqlParameters.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));

                }

                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(String.Format(@"
                    SELECT count(*) {0}", sb.ToString()), sqlParameters.ToArray()).Single();


                sqlParameters1.Add(new SqlParameter("@Skip", gmdt.Start));
                sqlParameters1.Add(new SqlParameter("@Take", gmdt.Length));

                resp.HrJobTypes = repo.Database.SqlQuery<HrJobTypeInfo>(string.Format(@"
                          SELECT 
                            a.[RowId] 
                           ,a.[JobTypeId]
                           ,a.[JobType]
                        {0}
                        ORDER BY {1} {2}
                        OFFSET (@Skip) ROWS FETCH NEXT (@Take) ROWS ONLY
                ", sb.ToString(), gmdt.SortColum.ToString(), gmdt.SortDir), sqlParameters1.ToArray()).ToList();
            }
            return resp;
        }
    }
}