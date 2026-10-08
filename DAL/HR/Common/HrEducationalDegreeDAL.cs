using ERP.Models.HR.Common.HrEducationalDegree;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Common
{
   
        public class HrEducationalDegreeDAL
        {
            public static HrEducationalDegreeInfo GetEducationalDegreeByID(string RowId)
            {
                HrEducationalDegreeInfo HrEducationalDegree = null;
                using (var repo = new GMContext())
                {
                    HrEducationalDegree = repo.Database.SqlQuery<HrEducationalDegreeInfo>(@"
                   SELECT [RowId]
                         ,[DegreeId]
                         ,[DegreeName]
                         ,[Rank]
                         ,[DomainStatus]
                     FROM [dbo].[HrEducationalDegree] a
                     WHERE a.[RowId] = @RowId and a.DomainStatus = 1
               ", new SqlParameter("@RowId", RowId)).FirstOrDefault();
                }
                return HrEducationalDegree;
            }

            public static HrEducationalDegreeInfo GetHrEducationalDegreeByName(string EducationalDegreeName, string RowId)
            {
                HrEducationalDegreeInfo HrEducationalDegree = null;
                using (var repo = new GMContext())
                {
                    HrEducationalDegree = repo.Database.SqlQuery<HrEducationalDegreeInfo>(@"
                     SELECT [RowId]
                         ,[DegreeId]
                         ,[DegreeName]
                         ,[Rank]
                         ,[DomainStatus]
                     FROM [dbo].[HrEducationalDegree] 
                     WHERE [DegreeName] = @DegreeName
              AND (@RowId IS NULL OR [RowId] <> @RowId)
              AND DomainStatus = 1", new SqlParameter("@DegreeName", EducationalDegreeName)
                           , new SqlParameter("@RowId", RowId)).FirstOrDefault();
                }
                return HrEducationalDegree;
            }

            
            public static List<HrEducationalDegreeInfo> GetAllHrEducationalDegrees()
            {
                List<HrEducationalDegreeInfo> HrEducationalDegrees = null;
                using (var repo = new GMContext())
                {
                    HrEducationalDegrees = repo.Database.SqlQuery<HrEducationalDegreeInfo>(@"
                    SELECT [RowId]
                         ,[DegreeId]
                         ,[DegreeName]
                         ,[Rank]
                         ,[DomainStatus]
                     FROM [dbo].[HrEducationalDegree] a
                       WHERE a.[DomainStatus] = 1
                     ORDER BY [DegreeId]
               ").ToList();
                }
                return HrEducationalDegrees;
            }
            public static void AddHrEducationalDegree(HrEducationalDegreeInfo HrEducationalDegree, string loggedInUser)
            {
                HrEducationalDegreeInfo loc;
                using (var repo = new GMContext())
                {
                    loc = repo.Database.SqlQuery<HrEducationalDegreeInfo>(@"
                        SELECT ISNULL(MAX([DegreeId]), 0) + 1 AS DegreeId
                        FROM [dbo].[HrEducationalDegree]
                         WHERE [DomainStatus] = 1
                        ").FirstOrDefault();


                    repo.Database.ExecuteSqlCommand(@"
                   INSERT INTO [dbo].[HrEducationalDegree]
                                  ([RowId]
                                  ,[DegreeId]
                                  ,[DegreeName]
                                  ,[Rank]
                                  ,[CreateUser]
                                  ,[CreateDate]
                                  ,[DomainStatus])
                            VALUES
                                  (@RowId
                                  ,@DegreeId
                                  ,@DegreeName
                                  ,@Rank
                                  ,@CreateUser
                                  ,getdate() 
                                  ,1)
               ", new SqlParameter("@RowId", HrEducationalDegree.RowId)
                    , new SqlParameter("@DegreeId", loc.DegreeId)
                    , new SqlParameter("@DegreeName", HrEducationalDegree.DegreeName)
                    , new SqlParameter("@Rank", HrEducationalDegree.Rank)
                    , new SqlParameter("@CreateUser", loggedInUser));
                }

            }

            public static void UpdateHrEducationalDegree(HrEducationalDegreeInfo HrEducationalDegree, string loggedInUser)
            {
                using (var repo = new GMContext())
                {
                    repo.Database.ExecuteSqlCommand(@"
                    UPDATE [dbo].[HrEducationalDegree]
                              SET [DegreeName] = @DegreeName
                                  ,[Rank] = @Rank
                                 ,[UpdateUser] = @UpdateUser
                                 ,[UpdateDate] = getdate()
                            WHERE [RowId] = @RowId
               ", new SqlParameter("@RowId", HrEducationalDegree.RowId)
                    , new SqlParameter("@DegreeName", HrEducationalDegree.DegreeName)
                    , new SqlParameter("@Rank", HrEducationalDegree.Rank)
                    , new SqlParameter("@UpdateUser", loggedInUser));
                }

            }
            public static void DeleteHrEducationalDegree(HrEducationalDegreeInfo HrEducationalDegree, string loggedInUser)
            {
                using (var repo = new GMContext())
                {
                    repo.Database.ExecuteSqlCommand(@"
                   UPDATE [dbo].[HrEducationalDegree]
                      SET [DomainStatus] = 0
                         ,[UpdateUser] = @UpdateUser
                         ,[UpdateDate] = getdate() 
                    WHERE [RowId] = @RowId
               ", new SqlParameter("@RowId", HrEducationalDegree.RowId)
                     , new SqlParameter("@UpdateUser", loggedInUser));

                }

            }

            public static HrEducationalDegreeResponse GetFilteredHrEducationalDegrees(HttpRequestBase Request)
            {
                HrEducationalDegreeResponse resp = new HrEducationalDegreeResponse();
                GMDataTableDAL gmdt = new GMDataTableDAL(Request);
                resp.HrEducationalDegrees = null;
                using (var repo = new GMContext())
                {
                    StringBuilder sb = new StringBuilder(@" FROM [dbo].[HrEducationalDegree] a
                        WHERE a.[DomainStatus] = 1");

                    resp.TotalRecordCount = repo.Database.SqlQuery<int>(string.Format(@"
                    SELECT count(*) {0} ", sb.ToString())).Single();

                    List<SqlParameter> sqlParameters = new List<SqlParameter>();
                    List<SqlParameter> sqlParameters1 = new List<SqlParameter>();

                    if (!string.IsNullOrEmpty(gmdt.SearchValue))
                    {
                        sb.AppendLine(" AND  CHARINDEX(@SearchValue, EducationalDegreeName) > 0 ");
                        sb.AppendLine(" AND  CHARINDEX(@SearchValue, EducationalDegreeCode) > 0 ");
                        sqlParameters.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));

                    }

                    resp.FilteredRecordCount = repo.Database.SqlQuery<int>(String.Format(@"
                    SELECT count(*) {0}", sb.ToString()), sqlParameters.ToArray()).Single();


                    sqlParameters1.Add(new SqlParameter("@Skip", gmdt.Start));
                    sqlParameters1.Add(new SqlParameter("@Take", gmdt.Length));

                    resp.HrEducationalDegrees = repo.Database.SqlQuery<HrEducationalDegreeInfo>(string.Format(@"
                          SELECT 
                            a.[RowId] 
                           ,a.[DegreeId]
                           ,a.[DegreeName]
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
