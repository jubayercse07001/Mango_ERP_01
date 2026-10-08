using ERP.Models.HR.Employee.HrEmployeeExperience;
using Mango_ERP.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Common
{
    public class HrEmployeeExperienceDAL
    {
        public static HrEmployeeExperienceInfo GetExperienceByID(string RowId)
        {
            HrEmployeeExperienceInfo experience = null;

            using (var repo = new GMContext())
            {
                experience = repo.Database.SqlQuery<HrEmployeeExperienceInfo>(@"
                    SELECT
                        a.RowId,
                        a.EmployeeExperienceId,
                        a.EmployeeId,
                        a.CompanyName,
                        a.CompanyAddress,
                        a.CompanyContactNo,
                        a.Mobile,
                        a.Email,
                        a.Web,
                        a.Designation,
                        a.Department,
                        a.Responsibilities,
                        a.Rank
                    FROM HrEmployeeExperience a
                    WHERE a.RowId = @RowId
                    AND a.DomainStatus = 1
                ", new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }

            return experience;
        }

        public static HrEmployeeExperienceInfo GetHrEmployeeExperienceDuplicate(string EmployeeId, string CompanyName, string RowId)
        {
            HrEmployeeExperienceInfo experience = null;

            using (var repo = new GMContext())
            {
                experience = repo.Database.SqlQuery<HrEmployeeExperienceInfo>(@"
                    SELECT *
                    FROM [dbo].[HrEmployeeExperience]
                    WHERE EmployeeId = @EmployeeId
                    AND CompanyName = @CompanyName
                    AND (@RowId IS NULL OR RowId <> @RowId)
                    AND DomainStatus = 1
                ",
                new SqlParameter("@EmployeeId", EmployeeId),
                new SqlParameter("@CompanyName", CompanyName),
                new SqlParameter("@RowId", (object)RowId ?? DBNull.Value)
                ).FirstOrDefault();
            }

            return experience;
        }

        public static List<HrEmployeeExperienceInfo> GetAllHrEmployeeExperiences()
        {
            List<HrEmployeeExperienceInfo> experiences = null;

            using (var repo = new GMContext())
            {
                experiences = repo.Database.SqlQuery<HrEmployeeExperienceInfo>(@"
                    SELECT
                        [RowId],
                        [EmployeeExperienceId],
                        [EmployeeId],
                        [CompanyName],
                        [CompanyAddress],
                        [CompanyContactNo],
                        [Mobile],
                        [Email],
                        [Web],
                        [Designation],
                        [Department],
                        [Responsibilities],
                        [Rank]
                    FROM [dbo].[HrEmployeeExperience]
                    WHERE DomainStatus = 1
                    ORDER BY Rank, EmployeeExperienceId
                ").ToList();
            }

            return experiences;
        }

        public static void AddHrEmployeeExperience(HrEmployeeExperienceInfo experience, string loggedInUser)
        {
            HrEmployeeExperienceInfo obj;

            using (var repo = new GMContext())
            {
                obj = repo.Database.SqlQuery<HrEmployeeExperienceInfo>(@"
                    SELECT ISNULL(MAX(EmployeeExperienceId),0)+1 AS EmployeeExperienceId
                    FROM HrEmployeeExperience
                    WHERE DomainStatus = 1
                ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO [dbo].[HrEmployeeExperience]
                    (
                        RowId,
                        EmployeeExperienceId,
                        EmployeeId,
                        CompanyName,
                        CompanyAddress,
                        CompanyContactNo,
                        Mobile,
                        Email,
                        Web,
                        Designation,
                        Department,
                        Responsibilities,
                        Rank,
                        CreateUser,
                        CreateDate,
                        DomainStatus
                    )
                    VALUES
                    (
                        @RowId,
                        @EmployeeExperienceId,
                        @EmployeeId,
                        @CompanyName,
                        @CompanyAddress,
                        @CompanyContactNo,
                        @Mobile,
                        @Email,
                        @Web,
                        @Designation,
                        @Department,
                        @Responsibilities,
                        @Rank,
                        @CreateUser,
                        GETDATE(),
                        1
                    )
                ",
                new SqlParameter("@RowId", experience.RowId),
                new SqlParameter("@EmployeeExperienceId", obj.EmployeeExperienceId),
                new SqlParameter("@EmployeeId", experience.EmployeeId),
                new SqlParameter("@CompanyName", experience.CompanyName),
                new SqlParameter("@CompanyAddress", experience.CompanyAddress ?? ""),
                new SqlParameter("@CompanyContactNo", experience.CompanyContactNo ?? ""),
                new SqlParameter("@Mobile", experience.Mobile ?? ""),
                new SqlParameter("@Email", experience.Email ?? ""),
                new SqlParameter("@Web", experience.Web ?? ""),
                new SqlParameter("@Designation", experience.Designation ?? ""),
                new SqlParameter("@Department", experience.Department ?? ""),
                new SqlParameter("@Responsibilities", experience.Responsibilities ?? ""),
                new SqlParameter("@Rank", experience.Rank),
                new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        public static void UpdateHrEmployeeExperience(HrEmployeeExperienceInfo experience, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE HrEmployeeExperience
                    SET
                        EmployeeId = @EmployeeId,
                        CompanyName = @CompanyName,
                        CompanyAddress = @CompanyAddress,
                        CompanyContactNo = @CompanyContactNo,
                        Mobile = @Mobile,
                        Email = @Email,
                        Web = @Web,
                        Designation = @Designation,
                        Department = @Department,
                        Responsibilities = @Responsibilities,
                        Rank = @Rank,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", experience.RowId),
                new SqlParameter("@EmployeeId", experience.EmployeeId),
                new SqlParameter("@CompanyName", experience.CompanyName),
                new SqlParameter("@CompanyAddress", experience.CompanyAddress ?? ""),
                new SqlParameter("@CompanyContactNo", experience.CompanyContactNo ?? ""),
                new SqlParameter("@Mobile", experience.Mobile ?? ""),
                new SqlParameter("@Email", experience.Email ?? ""),
                new SqlParameter("@Web", experience.Web ?? ""),
                new SqlParameter("@Designation", experience.Designation ?? ""),
                new SqlParameter("@Department", experience.Department ?? ""),
                new SqlParameter("@Responsibilities", experience.Responsibilities ?? ""),
                new SqlParameter("@Rank", experience.Rank),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static void DeleteHrEmployeeExperience(HrEmployeeExperienceInfo experience, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE HrEmployeeExperience
                    SET
                        DomainStatus = 0,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", experience.RowId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static HrEmployeeExperienceResponse GetFilteredHrEmployeeExperiences(HttpRequestBase Request)
        {
            HrEmployeeExperienceResponse resp = new HrEmployeeExperienceResponse();
            GMDataTableDAL gmdt = new GMDataTableDAL(Request);

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
                    FROM HrEmployeeExperience a
                    WHERE a.DomainStatus = 1
                ");

                List<SqlParameter> parameters = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.Append(@"
                        AND (
                            a.EmployeeId LIKE '%' + @SearchValue + '%'
                            OR a.CompanyName LIKE '%' + @SearchValue + '%'
                            OR a.Designation LIKE '%' + @SearchValue + '%'
                            OR a.Department LIKE '%' + @SearchValue + '%'
                        )");

                    parameters.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));
                }

                resp.TotalRecordCount = repo.Database.SqlQuery<int>(
                    "SELECT COUNT(*) FROM HrEmployeeExperience WHERE DomainStatus = 1"
                ).Single();

                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(
                    "SELECT COUNT(*) " + sb.ToString(),
                    parameters.ToArray()
                ).Single();

                parameters.Add(new SqlParameter("@Skip", gmdt.Start));
                parameters.Add(new SqlParameter("@Take", gmdt.Length));

                resp.HrEmployeeExperiences = repo.Database.SqlQuery<HrEmployeeExperienceInfo>(@"
                    SELECT
                        a.RowId,
                        a.EmployeeExperienceId,
                        a.EmployeeId,
                        a.CompanyName,
                        a.Designation,
                        a.Department,
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