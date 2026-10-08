using ERP.Models.HR.Organization.HrDesignationGroup;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
namespace ERP.DAL.HR.Organization
{
    public class HrDesignationGroupDAL
    {
        public static HrDesignationGroupInfo GetDesignationGroupByID(string RowId)
        {
            HrDesignationGroupInfo HrDesignationGroup = null;
            using (var repo = new GMContext())
            {
                HrDesignationGroup = repo.Database.SqlQuery<HrDesignationGroupInfo>(@"
                   SELECT [RowId]
                            ,[DesignationGroupId]
                            ,[DesignationGroup]
                            ,[Rank]
                     FROM [dbo].[HrDesignationGroup] 
                     WHERE [RowId] = @RowId and DomainStatus = 1
               ", new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }
            return HrDesignationGroup;
        }

        public static HrDesignationGroupInfo GetHrDesignationGroupByName(string DesignationGroup, string RowId)
        {
            HrDesignationGroupInfo HrDesignationGroup = null;
            using (var repo = new GMContext())
            {
                HrDesignationGroup = repo.Database.SqlQuery<HrDesignationGroupInfo>(@"
                     SELECT [RowId]
                            ,[DesignationGroupId]
                            ,[DesignationGroup]
                            ,[Rank]
                     FROM [dbo].[HrDesignationGroup] 
                     WHERE [DesignationGroup] = @DesignationGroup
              AND (@RowId IS NULL OR [RowId] <> @RowId)
              AND DomainStatus = 1", new SqlParameter("@DesignationGroup", DesignationGroup)
                       , new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }
            return HrDesignationGroup;
        }

        
        public static List<HrDesignationGroupInfo> GetAllHrDesignationGroups()
        {
            List<HrDesignationGroupInfo> HrDesignationGroups = null;
            using (var repo = new GMContext())
            {
                HrDesignationGroups = repo.Database.SqlQuery<HrDesignationGroupInfo>(@"
                   SELECT [RowId]
                            ,[DesignationGroupId]
                            ,[DesignationGroup]
                            ,[Rank]
                     FROM [dbo].[HrDesignationGroup] 
                       WHERE [DomainStatus] = 1
                     ORDER BY [Rank]
               ").ToList();
            }
            return HrDesignationGroups;
        }
        public static void AddHrDesignationGroup(HrDesignationGroupInfo HrDesignationGroup, string loggedInUser)
        {
            HrDesignationGroupInfo loc;
            using (var repo = new GMContext())
            {
                loc = repo.Database.SqlQuery<HrDesignationGroupInfo>(@"
                        SELECT ISNULL(MAX([DesignationGroupId]), 0) + 1 AS DesignationGroupId
                        FROM [dbo].[HrDesignationGroup]
                         WHERE [DomainStatus] = 1
                        ").FirstOrDefault();


                repo.Database.ExecuteSqlCommand(@"
                   INSERT INTO [dbo].[HrDesignationGroup]
                                  ([RowId]
                                  ,[DesignationGroupId]
                                  ,[DesignationGroup]
                                  ,[Rank]
                                  ,[CreateUser]
                                  ,[CreateDate]
                                  ,[DomainStatus])
                            VALUES
                                  (@RowId
                                  ,@DesignationGroupId
                                  ,@DesignationGroup
                                  ,@Rank
                                  ,@CreateUser
                                  ,getdate() 
                                  ,1)
               ", new SqlParameter("@RowId", HrDesignationGroup.RowId)
                , new SqlParameter("@DesignationGroupId", loc.DesignationGroupId)
                , new SqlParameter("@DesignationGroup", HrDesignationGroup.DesignationGroup)
                , new SqlParameter("@Rank", HrDesignationGroup.Rank)
                , new SqlParameter("@CreateUser", loggedInUser));
            }

        }

        public static void UpdateHrDesignationGroup(HrDesignationGroupInfo HrDesignationGroup, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE [dbo].[HrDesignationGroup]
                              SET [DesignationGroup] = @DesignationGroup
                                 ,[Rank] = @Rank
                                 ,[UpdateUser] = @UpdateUser
                                 ,[UpdateDate] = getdate()
                            WHERE [RowId] = @RowId
               ", new SqlParameter("@RowId", HrDesignationGroup.RowId)
                , new SqlParameter("@DesignationGroup", HrDesignationGroup.DesignationGroup)
                , new SqlParameter("@Rank", HrDesignationGroup.Rank)
                , new SqlParameter("@UpdateUser", loggedInUser));
            }

        }
        public static void DeleteHrDesignationGroup(HrDesignationGroupInfo HrDesignationGroup, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                   UPDATE [dbo].[HrDesignationGroup]
                      SET [DomainStatus] = 0
                         ,[UpdateUser] = @UpdateUser
                         ,[UpdateDate] = getdate() 
                    WHERE [RowId] = @RowId
               ", new SqlParameter("@RowId", HrDesignationGroup.RowId)
                 , new SqlParameter("@UpdateUser", loggedInUser));

            }

        }

        public static HrDesignationGroupResponse GetFilteredHrDesignationGroups(HttpRequestBase Request)
        {
            HrDesignationGroupResponse resp = new HrDesignationGroupResponse();
            GMDataTableDAL gmdt = new GMDataTableDAL(Request);
            resp.HrDesignationGroups = null;
            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@" FROM [dbo].[HrDesignationGroup] a
                        WHERE a.[DomainStatus] = 1");

                resp.TotalRecordCount = repo.Database.SqlQuery<int>(string.Format(@"
                    SELECT count(*) {0} ", sb.ToString())).Single();

                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                List<SqlParameter> sqlParameters1 = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.AppendLine(" AND  CHARINDEX(@SearchValue, DesignationGroup) > 0 ");
                    sb.AppendLine(" AND  CHARINDEX(@SearchValue, Rank) > 0 ");
                    sqlParameters.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));

                }

                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(String.Format(@"
                    SELECT count(*) {0}", sb.ToString()), sqlParameters.ToArray()).Single();


                sqlParameters1.Add(new SqlParameter("@Skip", gmdt.Start));
                sqlParameters1.Add(new SqlParameter("@Take", gmdt.Length));

                resp.HrDesignationGroups = repo.Database.SqlQuery<HrDesignationGroupInfo>(string.Format(@"
                          SELECT 
                            a.[RowId] 
                           ,a.[DesignationGroupId]
                           ,a.[DesignationGroup]
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