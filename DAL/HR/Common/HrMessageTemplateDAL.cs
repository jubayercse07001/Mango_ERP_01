using ERP.Models.HR.Common.HrMessageTemplate;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Common
{
    
    public class HrMessageTemplateDAL
    {
        public static HrMessageTemplateInfo GetTemplateByID(string RowId)
        {
            HrMessageTemplateInfo HrMessageTemplate = null;
            using (var repo = new GMContext())
            {
                HrMessageTemplate = repo.Database.SqlQuery<HrMessageTemplateInfo>(@"
                   SELECT [RowId]
                         ,[TemplateId]
                         ,[TemplateName]
                         ,[Subject]
                         ,[Body]
                         ,[DomainStatus]
                     FROM [dbo].[HrMessageTemplate] a
                     WHERE a.[RowId] = @RowId and a.DomainStatus = 1
               ", new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }
            return HrMessageTemplate;
        }

        public static HrMessageTemplateInfo GetHrMessageTemplateByName(string TemplateName, string RowId)
        {
            HrMessageTemplateInfo HrMessageTemplate = null;
            using (var repo = new GMContext())
            {
                HrMessageTemplate = repo.Database.SqlQuery<HrMessageTemplateInfo>(@"
                    SELECT [RowId]
                         ,[TemplateId]
                         ,[TemplateName]
                         ,[Subject]
                         ,[Body]
                         ,[DomainStatus]
                     FROM [dbo].[HrMessageTemplate] 
                     WHERE [TemplateName] = @TemplateName
              AND (@RowId IS NULL OR [RowId] <> @RowId)
              AND DomainStatus = 1", new SqlParameter("@TemplateName", TemplateName)
                       , new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }
            return HrMessageTemplate;
        }

        
        public static List<HrMessageTemplateInfo> GetAllHrMessageTemplates()
        {
            List<HrMessageTemplateInfo> HrMessageTemplates = null;
            using (var repo = new GMContext())
            {
                HrMessageTemplates = repo.Database.SqlQuery<HrMessageTemplateInfo>(@"
                   SELECT [RowId]
                         ,[TemplateId]
                         ,[TemplateName]
                         ,[Subject]
                         ,[Body]
                         ,[DomainStatus]
                     FROM [dbo].[HrMessageTemplate] a
                       WHERE a.[DomainStatus] = 1
                     ORDER BY [TemplateId]
               ").ToList();
            }
            return HrMessageTemplates;
        }
        public static void AddHrMessageTemplate(HrMessageTemplateInfo HrMessageTemplate, string loggedInUser)
        {
            HrMessageTemplateInfo loc;
            using (var repo = new GMContext())
            {
                loc = repo.Database.SqlQuery<HrMessageTemplateInfo>(@"
                        SELECT ISNULL(MAX([TemplateId]), 0) + 1 AS TemplateId
                        FROM [dbo].[HrMessageTemplate]
                         WHERE [DomainStatus] = 1
                        ").FirstOrDefault();


                repo.Database.ExecuteSqlCommand(@"
                   INSERT INTO [dbo].[HrMessageTemplate]
                                  ([RowId]
                                    ,[TemplateId]
                                    ,[TemplateName]
                                    ,[Subject]
                                    ,[Body]
                                  ,[CreateUser]
                                  ,[CreateDate]
                                  ,[DomainStatus])
                            VALUES
                                  (@RowId
                                  ,@TemplateId
                                  ,@TemplateName
                                  ,@Subject
                                  ,@Body
                                  ,@CreateUser
                                  ,getdate() 
                                  ,1)
               ", new SqlParameter("@RowId", HrMessageTemplate.RowId)
                , new SqlParameter("@TemplateId", loc.TemplateId)
                , new SqlParameter("@TemplateName", HrMessageTemplate.TemplateName)
                , new SqlParameter("@Subject", HrMessageTemplate.Subject)
                , new SqlParameter("@Body", HrMessageTemplate.Body)
                , new SqlParameter("@CreateUser", loggedInUser));
            }

        }

        public static void UpdateHrMessageTemplate(HrMessageTemplateInfo HrMessageTemplate, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE [dbo].[HrMessageTemplate]
                              SET [TemplateName] = @TemplateName
                                 ,[Subject] = @Subject
                                 ,[Body] = @Body
                                 ,[UpdateUser] = @UpdateUser
                                 ,[UpdateDate] = getdate()
                            WHERE [RowId] = @RowId
               ", new SqlParameter("@RowId", HrMessageTemplate.RowId)
                , new SqlParameter("@TemplateName", HrMessageTemplate.TemplateName)
                , new SqlParameter("@Subject", HrMessageTemplate.Subject)
                , new SqlParameter("@Body", HrMessageTemplate.Body)
                , new SqlParameter("@UpdateUser", loggedInUser));
            }

        }
        public static void DeleteHrMessageTemplate(HrMessageTemplateInfo HrMessageTemplate, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                   UPDATE [dbo].[HrMessageTemplate]
                      SET [DomainStatus] = 0
                         ,[UpdateUser] = @UpdateUser
                         ,[UpdateDate] = getdate() 
                    WHERE [RowId] = @RowId
               ", new SqlParameter("@RowId", HrMessageTemplate.RowId)
                 , new SqlParameter("@UpdateUser", loggedInUser));

            }

        }

        public static HrMessageTemplateResponse GetFilteredHrMessageTemplates(HttpRequestBase Request)
        {
            HrMessageTemplateResponse resp = new HrMessageTemplateResponse();
            GMDataTableDAL gmdt = new GMDataTableDAL(Request);
            resp.HrMessageTemplates = null;
            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@" FROM [dbo].[HrMessageTemplate] a
                        WHERE a.[DomainStatus] = 1");

                resp.TotalRecordCount = repo.Database.SqlQuery<int>(string.Format(@"
                    SELECT count(*) {0} ", sb.ToString())).Single();

                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                List<SqlParameter> sqlParameters1 = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.AppendLine(" AND  CHARINDEX(@SearchValue, TemplateName) > 0 ");
                    sb.AppendLine(" AND  CHARINDEX(@SearchValue, Subject) > 0 ");
                    sqlParameters.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));

                }

                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(String.Format(@"
                    SELECT count(*) {0}", sb.ToString()), sqlParameters.ToArray()).Single();


                sqlParameters1.Add(new SqlParameter("@Skip", gmdt.Start));
                sqlParameters1.Add(new SqlParameter("@Take", gmdt.Length));

                resp.HrMessageTemplates = repo.Database.SqlQuery<HrMessageTemplateInfo>(string.Format(@"
                          SELECT [RowId]
                         ,[TemplateId]
                         ,[TemplateName]
                         ,[Subject]
                         ,[Body]
                        {0}
                        ORDER BY {1} {2}
                        OFFSET (@Skip) ROWS FETCH NEXT (@Take) ROWS ONLY
                ", sb.ToString(), gmdt.SortColum.ToString(), gmdt.SortDir), sqlParameters1.ToArray()).ToList();
            }
            return resp;
        }
    }
}