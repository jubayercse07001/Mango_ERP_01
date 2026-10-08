using ERP.Models.HR.Employee.HrEmployeeProfessionalKnowledge;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Common
{
    public class HrEmployeeProfessionalKnowledgeDAL
    {
        public static HrEmployeeProfessionalKnowledgeInfo GetProfessionalKnowledgeByID(string RowId)
        {
            HrEmployeeProfessionalKnowledgeInfo knowledge = null;

            using (var repo = new GMContext())
            {
                knowledge = repo.Database.SqlQuery<HrEmployeeProfessionalKnowledgeInfo>(@"
                    SELECT
                        a.RowId,
                        a.ProfessionalKnowledgeId,
                        a.EmployeeId,
                        a.SkillId,
                        b.SkillName,
                        a.DomainStatus
                    FROM HrEmployeeProfessionalKnowledge a
                    INNER JOIN HrSkillInformation b on b.RowId = a.SkillId
                    WHERE a.RowId = @RowId
                    AND a.DomainStatus = 1
                ", new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }

            return knowledge;
        }

        public static List<HrEmployeeProfessionalKnowledgeInfo> GetKnowledgeByEmployeeId(string EmployeeId)
        {
            List<HrEmployeeProfessionalKnowledgeInfo> items = null;

            using (var repo = new GMContext())
            {
                items = repo.Database.SqlQuery<HrEmployeeProfessionalKnowledgeInfo>(@"
                    SELECT
                        a.RowId,
                        a.ProfessionalKnowledgeId,
                        a.EmployeeId,
                        a.SkillId,
                        b.SkillName,
                        a.DomainStatus
                    FROM HrEmployeeProfessionalKnowledge a
                    INNER JOIN HrSkillInformation b on b.RowId = a.SkillId
                    WHERE a.EmployeeId = @EmployeeId
                    AND a.DomainStatus = 1
                    ORDER BY a.ProfessionalKnowledgeId
                ", new SqlParameter("@EmployeeId", EmployeeId)).ToList();
            }

            return items;
        }

        public static void AddHrEmployeeProfessionalKnowledge(HrEmployeeProfessionalKnowledgeInfo knowledge, string loggedInUser)
        {
            HrEmployeeProfessionalKnowledgeInfo obj;

            using (var repo = new GMContext())
            {
                obj = repo.Database.SqlQuery<HrEmployeeProfessionalKnowledgeInfo>(@"
                    SELECT ISNULL(MAX(ProfessionalKnowledgeId),0)+1 AS ProfessionalKnowledgeId
                    FROM HrEmployeeProfessionalKnowledge
                    WHERE DomainStatus = 1
                ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO [dbo].[HrEmployeeProfessionalKnowledge]
                    (
                        RowId,
                        ProfessionalKnowledgeId,
                        EmployeeId,
                        SkillId,
                        CreateUser,
                        CreateDate,
                        DomainStatus
                    )
                    VALUES
                    (
                        @RowId,
                        @ProfessionalKnowledgeId,
                        @EmployeeId,
                        @SkillId,
                        @CreateUser,
                        GETDATE(),
                        1
                    )
                ",
                new SqlParameter("@RowId", knowledge.RowId),
                new SqlParameter("@ProfessionalKnowledgeId", obj.ProfessionalKnowledgeId),
                new SqlParameter("@EmployeeId", knowledge.EmployeeId),
                new SqlParameter("@SkillId", knowledge.SkillId),
                new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        public static void UpdateHrEmployeeProfessionalKnowledge(HrEmployeeProfessionalKnowledgeInfo knowledge, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE HrEmployeeProfessionalKnowledge
                    SET
                        EmployeeId = @EmployeeId,
                        SkillId = @SkillId,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", knowledge.RowId),
                new SqlParameter("@EmployeeId", knowledge.EmployeeId),
                new SqlParameter("@SkillId", knowledge.SkillId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static void DeleteHrEmployeeProfessionalKnowledge(HrEmployeeProfessionalKnowledgeInfo knowledge, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE HrEmployeeProfessionalKnowledge
                    SET
                        DomainStatus = 0,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", knowledge.RowId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static HrEmployeeProfessionalKnowledgeResponse GetFilteredHrEmployeeProfessionalKnowledge(HttpRequestBase Request)
        {
            HrEmployeeProfessionalKnowledgeResponse resp = new HrEmployeeProfessionalKnowledgeResponse();
            GMDataTableDAL gmdt = new GMDataTableDAL(Request);

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
                    FROM HrEmployeeProfessionalKnowledge a
                    INNER JOIN HrSkillInformation b on b.RowId = a.SkillId
                    WHERE a.DomainStatus = 1
                ");

                List<SqlParameter> parameters = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.Append(@"
                        AND (
                            b.SkillName LIKE '%' + @SearchValue + '%'
                            OR a.EmployeeId LIKE '%' + @SearchValue + '%'
                        )");

                    parameters.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));
                }

                resp.TotalRecordCount = repo.Database.SqlQuery<int>(
                    "SELECT COUNT(*) FROM HrEmployeeProfessionalKnowledge WHERE DomainStatus = 1"
                ).Single();

                // Clone parameters for counting query execution
                var countParameters = parameters.Select(p => ((ICloneable)p).Clone() as SqlParameter).ToArray();

                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(
                    "SELECT COUNT(*) " + sb.ToString(),
                    countParameters
                ).Single();

                parameters.Add(new SqlParameter("@Skip", gmdt.Start));
                parameters.Add(new SqlParameter("@Take", gmdt.Length));

                resp.HrEmployeeProfessionalKnowledges = repo.Database.SqlQuery<HrEmployeeProfessionalKnowledgeInfo>(@"
                    SELECT
                        a.RowId,
                        a.ProfessionalKnowledgeId,
                        a.EmployeeId,
                        a.SkillId,
                        b.SkillName
                    " + sb.ToString() + @"
                    ORDER BY " + gmdt.SortColum + " " + gmdt.SortDir + @"
                    OFFSET @Skip ROWS
                    FETCH NEXT @Take ROWS ONLY
                    ",
                    parameters.ToArray()).ToList();
            }

            return resp;
        }
    }
}