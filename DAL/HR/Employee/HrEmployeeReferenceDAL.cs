using ERP.Models.HR.Employee.HrEmployeeReference;
using ERP.Models.HR.Employee.HrEmployeeReferenceReference;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Common
{
    public class HrEmployeeReferenceDAL
    {
        public static HrEmployeeReferenceInfo GetEmployeeReferenceByID(string RowId)
        {
            HrEmployeeReferenceInfo reference = null;

            using (var repo = new GMContext())
            {
                reference = repo.Database.SqlQuery<HrEmployeeReferenceInfo>(@"
                    SELECT
                        RowId,
                        EmployeeReferenceId,
                        EmployeeId,
                        ReferenceName,
                        Occupation,
                        Designation,
                        Company,
                        Relation,
                        ContactNo,
                        PresentAddress,
                        PermanentAddress,
                        Email,
                        NId,
                        Web,
                        DomainStatus,
                        CreateUser,
                        UpdateUser,
                        CreateDate,
                        UpdateDate
                    FROM HrEmployeeReference
                    WHERE RowId = @RowId
                    AND DomainStatus = 1
                ", new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }

            return reference;
        }

        public static HrEmployeeReferenceInfo GetHrEmployeeReferenceByName(string ReferenceName, string RowId)
        {
            HrEmployeeReferenceInfo reference = null;

            using (var repo = new GMContext())
            {
                reference = repo.Database.SqlQuery<HrEmployeeReferenceInfo>(@"
                    SELECT *
                    FROM [dbo].[HrEmployeeReference]
                    WHERE ReferenceName = @ReferenceName
                    AND (@RowId IS NULL OR RowId <> @RowId)
                    AND DomainStatus = 1
                ",
                new SqlParameter("@ReferenceName", ReferenceName),
                new SqlParameter("@RowId", (object)RowId ?? DBNull.Value)
                ).FirstOrDefault();
            }

            return reference;
        }

        public static List<HrEmployeeReferenceInfo> GetAllHrEmployeeReferencesByEmployee(string employeeId)
        {
            List<HrEmployeeReferenceInfo> references = null;

            using (var repo = new GMContext())
            {
                references = repo.Database.SqlQuery<HrEmployeeReferenceInfo>(@"
                    SELECT
                        RowId,
                        EmployeeReferenceId,
                        EmployeeId,
                        ReferenceName,
                        Occupation,
                        Designation,
                        Company,
                        Relation,
                        ContactNo,
                        PresentAddress,
                        PermanentAddress,
                        Email,
                        NId,
                        Web
                    FROM [dbo].[HrEmployeeReference]
                    WHERE EmployeeId = @EmployeeId
                    AND DomainStatus = 1
                    ORDER BY EmployeeReferenceId
                ", new SqlParameter("@EmployeeId", employeeId)).ToList();
            }

            return references;
        }

        public static void AddHrEmployeeReference(HrEmployeeReferenceInfo reference, string loggedInUser)
        {
            HrEmployeeReferenceInfo obj;

            using (var repo = new GMContext())
            {
                // Generates auto-incremented reference identification per active sequence
                obj = repo.Database.SqlQuery<HrEmployeeReferenceInfo>(@"
                    SELECT ISNULL(MAX(EmployeeReferenceId), 0) + 1 AS EmployeeReferenceId
                    FROM HrEmployeeReference
                    WHERE DomainStatus = 1
                ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO [dbo].[HrEmployeeReference]
                    (
                        RowId,
                        EmployeeReferenceId,
                        EmployeeId,
                        ReferenceName,
                        Occupation,
                        Designation,
                        Company,
                        Relation,
                        ContactNo,
                        PresentAddress,
                        PermanentAddress,
                        Email,
                        NId,
                        Web,
                        CreateUser,
                        CreateDate,
                        DomainStatus
                    )
                    VALUES
                    (
                        @RowId,
                        @EmployeeReferenceId,
                        @EmployeeId,
                        @ReferenceName,
                        @Occupation,
                        @Designation,
                        @Company,
                        @Relation,
                        @ContactNo,
                        @PresentAddress,
                        @PermanentAddress,
                        @Email,
                        @NId,
                        @Web,
                        @CreateUser,
                        GETDATE(),
                        1
                    )
                ",
                new SqlParameter("@RowId", reference.RowId),
                new SqlParameter("@EmployeeReferenceId", obj.EmployeeReferenceId),
                new SqlParameter("@EmployeeId", reference.EmployeeId),
                new SqlParameter("@ReferenceName", reference.ReferenceName),
                new SqlParameter("@Occupation", reference.Occupation ?? (object)DBNull.Value),
                new SqlParameter("@Designation", reference.Designation ?? (object)DBNull.Value),
                new SqlParameter("@Company", reference.Company ?? (object)DBNull.Value),
                new SqlParameter("@Relation", reference.Relation ?? (object)DBNull.Value),
                new SqlParameter("@ContactNo", reference.ContactNo),
                new SqlParameter("@PresentAddress", reference.PresentAddress),
                new SqlParameter("@PermanentAddress", reference.PermanentAddress ?? (object)DBNull.Value),
                new SqlParameter("@Email", reference.Email ?? (object)DBNull.Value),
                new SqlParameter("@NId", reference.NId ?? (object)DBNull.Value),
                new SqlParameter("@Web", reference.Web ?? (object)DBNull.Value),
                new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        public static void UpdateHrEmployeeReference(HrEmployeeReferenceInfo reference, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE HrEmployeeReference
                    SET
                        EmployeeId = @EmployeeId,
                        ReferenceName = @ReferenceName,
                        Occupation = @Occupation,
                        Designation = @Designation,
                        Company = @Company,
                        Relation = @Relation,
                        ContactNo = @ContactNo,
                        PresentAddress = @PresentAddress,
                        PermanentAddress = @PermanentAddress,
                        Email = @Email,
                        NId = @NId,
                        Web = @Web,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", reference.RowId),
                new SqlParameter("@EmployeeId", reference.EmployeeId),
                new SqlParameter("@ReferenceName", reference.ReferenceName),
                new SqlParameter("@Occupation", reference.Occupation ?? (object)DBNull.Value),
                new SqlParameter("@Designation", reference.Designation ?? (object)DBNull.Value),
                new SqlParameter("@Company", reference.Company ?? (object)DBNull.Value),
                new SqlParameter("@Relation", reference.Relation ?? (object)DBNull.Value),
                new SqlParameter("@ContactNo", reference.ContactNo),
                new SqlParameter("@PresentAddress", reference.PresentAddress),
                new SqlParameter("@PermanentAddress", reference.PermanentAddress ?? (object)DBNull.Value),
                new SqlParameter("@Email", reference.Email ?? (object)DBNull.Value),
                new SqlParameter("@NId", reference.NId ?? (object)DBNull.Value),
                new SqlParameter("@Web", reference.Web ?? (object)DBNull.Value),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static void DeleteHrEmployeeReference(HrEmployeeReferenceInfo reference, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE HrEmployeeReference
                    SET
                        DomainStatus = 0,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", reference.RowId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static HrEmployeeReferenceResponse GetFilteredHrEmployeeReferences(HttpRequestBase Request)
        {
            HrEmployeeReferenceResponse resp = new HrEmployeeReferenceResponse();

            GMDataTableDAL gmdt = new GMDataTableDAL(Request);

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
                    FROM HrEmployeeReference a
                    WHERE a.DomainStatus = 1
                ");

                List<SqlParameter> parameters = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.Append(@"
                        AND (
                            a.ReferenceName LIKE '%' + @SearchValue + '%'
                            OR a.Company LIKE '%' + @SearchValue + '%'
                            OR a.EmployeeId LIKE '%' + @SearchValue + '%'
                        )");

                    parameters.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));
                }

                resp.TotalRecordCount = repo.Database.SqlQuery<int>(
                    "SELECT COUNT(*) FROM HrEmployeeReference WHERE DomainStatus = 1"
                ).Single();

                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(
                    "SELECT COUNT(*) " + sb.ToString(),
                    parameters.ToArray()
                ).Single();

                parameters.Add(new SqlParameter("@Skip", gmdt.Start));
                parameters.Add(new SqlParameter("@Take", gmdt.Length));

                // Clone query state to apply order rules safely
                var searchParams = parameters.Select(p => ((ICloneable)p).Clone() as SqlParameter).ToArray();

                resp.HrEmployeeReferences = repo.Database.SqlQuery<HrEmployeeReferenceInfo>(@"
                    SELECT
                        a.RowId,
                        a.EmployeeReferenceId,
                        a.EmployeeId,
                        a.ReferenceName,
                        a.Occupation,
                        a.Designation,
                        a.Company,
                        a.Relation,
                        a.ContactNo,
                        a.PresentAddress,
                        a.PermanentAddress,
                        a.Email,
                        a.NId,
                        a.Web
                    " + sb.ToString() + @"
                    ORDER BY " + gmdt.SortColum + " " + gmdt.SortDir + @"
                    OFFSET @Skip ROWS
                    FETCH NEXT @Take ROWS ONLY
                    ",
                    searchParams).ToList();
            }

            return resp;
        }
    }
}