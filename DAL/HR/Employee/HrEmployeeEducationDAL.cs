using ERP.Models.HR.Employee.HrEmployeeEducation;
using Mango_ERP.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Common
{
    public class HrEmployeeEducationDAL
    {
        public static HrEmployeeEducationInfo GetEducationByID(string RowId)
        {
            HrEmployeeEducationInfo education = null;

            using (var repo = new GMContext())
            {
                education = repo.Database.SqlQuery<HrEmployeeEducationInfo>(@"
                    SELECT
                        a.RowId,
                        a.EmployeeEducationId,
                        a.EmployeeId,
                        a.EducationalDegreeId,
                        deg.EducationalDegreeName,
                        a.EducationalInstituteId,
                        inst.EducationalInstituteName,
                        a.Session,
                        a.PassingYear,
                        a.Board,
                        a.Result,
                        a.Rank
                    FROM HrEmployeeEducation a
                    LEFT JOIN HrEducationalDegree deg ON deg.RowId = a.EducationalDegreeId
                    LEFT JOIN HrEducationalInstitute inst ON inst.RowId = a.EducationalInstituteId
                    WHERE a.RowId = @RowId
                    AND a.DomainStatus = 1
                ", new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }

            return education;
        }

        public static HrEmployeeEducationInfo GetHrEmployeeEducationDuplicate(string EmployeeId, string EducationalDegreeId, string RowId)
        {
            HrEmployeeEducationInfo education = null;

            using (var repo = new GMContext())
            {
                education = repo.Database.SqlQuery<HrEmployeeEducationInfo>(@"
                    SELECT *
                    FROM [dbo].[HrEmployeeEducation]
                    WHERE EmployeeId = @EmployeeId
                    AND EducationalDegreeId = @EducationalDegreeId
                    AND (@RowId IS NULL OR RowId <> @RowId)
                    AND DomainStatus = 1
                ",
                new SqlParameter("@EmployeeId", EmployeeId),
                new SqlParameter("@EducationalDegreeId", EducationalDegreeId),
                new SqlParameter("@RowId", (object)RowId ?? DBNull.Value)
                ).FirstOrDefault();
            }

            return education;
        }

        public static List<HrEmployeeEducationInfo> GetAllHrEmployeeEducations()
        {
            List<HrEmployeeEducationInfo> educations = null;

            using (var repo = new GMContext())
            {
                educations = repo.Database.SqlQuery<HrEmployeeEducationInfo>(@"
                    SELECT
                        [RowId],
                        [EmployeeEducationId],
                        [EmployeeId],
                        [EducationalDegreeId],
                        [EducationalInstituteId],
                        [Session],
                        [PassingYear],
                        [Board],
                        [Result],
                        [Rank]
                    FROM [dbo].[HrEmployeeEducation]
                    WHERE DomainStatus = 1
                    ORDER BY Rank, EmployeeEducationId
                ").ToList();
            }

            return educations;
        }

        public static void AddHrEmployeeEducation(HrEmployeeEducationInfo education, string loggedInUser)
        {
            HrEmployeeEducationInfo obj;

            using (var repo = new GMContext())
            {
                obj = repo.Database.SqlQuery<HrEmployeeEducationInfo>(@"
                    SELECT ISNULL(MAX(EmployeeEducationId),0)+1 AS EmployeeEducationId
                    FROM HrEmployeeEducation
                    WHERE DomainStatus = 1
                ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO [dbo].[HrEmployeeEducation]
                    (
                        RowId,
                        EmployeeEducationId,
                        EmployeeId,
                        EducationalDegreeId,
                        EducationalInstituteId,
                        Session,
                        PassingYear,
                        Board,
                        Result,
                        Rank,
                        CreateUser,
                        CreateDate,
                        DomainStatus
                    )
                    VALUES
                    (
                        @RowId,
                        @EmployeeEducationId,
                        @EmployeeId,
                        @EducationalDegreeId,
                        @EducationalInstituteId,
                        @Session,
                        @PassingYear,
                        @Board,
                        @Result,
                        @Rank,
                        @CreateUser,
                        GETDATE(),
                        1
                    )
                ",
                new SqlParameter("@RowId", education.RowId),
                new SqlParameter("@EmployeeEducationId", obj.EmployeeEducationId),
                new SqlParameter("@EmployeeId", education.EmployeeId),
                new SqlParameter("@EducationalDegreeId", education.EducationalDegreeId),
                new SqlParameter("@EducationalInstituteId", education.EducationalInstituteId ?? ""),
                new SqlParameter("@Session", education.Session ?? ""),
                new SqlParameter("@PassingYear", education.PassingYear ?? ""),
                new SqlParameter("@Board", education.Board ?? ""),
                new SqlParameter("@Result", education.Result ?? ""),
                new SqlParameter("@Rank", education.Rank),
                new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        public static void UpdateHrEmployeeEducation(HrEmployeeEducationInfo education, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE HrEmployeeEducation
                    SET
                        EmployeeId = @EmployeeId,
                        EducationalDegreeId = @EducationalDegreeId,
                        EducationalInstituteId = @EducationalInstituteId,
                        Session = @Session,
                        PassingYear = @PassingYear,
                        Board = @Board,
                        Result = @Result,
                        Rank = @Rank,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", education.RowId),
                new SqlParameter("@EmployeeId", education.EmployeeId),
                new SqlParameter("@EducationalDegreeId", education.EducationalDegreeId),
                new SqlParameter("@EducationalInstituteId", education.EducationalInstituteId ?? ""),
                new SqlParameter("@Session", education.Session ?? ""),
                new SqlParameter("@PassingYear", education.PassingYear ?? ""),
                new SqlParameter("@Board", education.Board ?? ""),
                new SqlParameter("@Result", education.Result ?? ""),
                new SqlParameter("@Rank", education.Rank),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static void DeleteHrEmployeeEducation(HrEmployeeEducationInfo education, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE HrEmployeeEducation
                    SET
                        DomainStatus = 0,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", education.RowId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static HrEmployeeEducationResponse GetFilteredHrEmployeeEducations(HttpRequestBase Request)
        {
            HrEmployeeEducationResponse resp = new HrEmployeeEducationResponse();
            GMDataTableDAL gmdt = new GMDataTableDAL(Request);

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
                    FROM HrEmployeeEducation a
                    LEFT JOIN HrEducationalDegree deg ON deg.RowId = a.EducationalDegreeId
                    LEFT JOIN HrEducationalInstitute inst ON inst.RowId = a.EducationalInstituteId
                    WHERE a.DomainStatus = 1
                ");

                List<SqlParameter> parameters = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.Append(@"
                        AND (
                            a.EmployeeId LIKE '%' + @SearchValue + '%'
                            OR deg.EducationalDegreeName LIKE '%' + @SearchValue + '%'
                            OR inst.EducationalInstituteName LIKE '%' + @SearchValue + '%'
                            OR a.PassingYear LIKE '%' + @SearchValue + '%'
                        )");

                    parameters.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));
                }

                resp.TotalRecordCount = repo.Database.SqlQuery<int>(
                    "SELECT COUNT(*) FROM HrEmployeeEducation WHERE DomainStatus = 1"
                ).Single();

                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(
                    "SELECT COUNT(*) " + sb.ToString(),
                    parameters.ToArray()
                ).Single();

                parameters.Add(new SqlParameter("@Skip", gmdt.Start));
                parameters.Add(new SqlParameter("@Take", gmdt.Length));

                resp.HrEmployeeEducations = repo.Database.SqlQuery<HrEmployeeEducationInfo>(@"
                    SELECT
                        a.RowId,
                        a.EmployeeEducationId,
                        a.EmployeeId,
                        deg.EducationalDegreeName,
                        inst.EducationalInstituteName,
                        a.PassingYear,
                        a.Result,
                        a.Rank
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