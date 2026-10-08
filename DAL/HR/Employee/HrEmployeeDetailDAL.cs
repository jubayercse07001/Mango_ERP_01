using ERP.Models.HR.Employee.EmployeeDetail;
using Mango_ERP.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Common
{
    public class HrEmployeeDetailDAL
    {
        public static HrEmployeeDetailInfo GetEmployeeDetailByID(string RowId)
        {
            HrEmployeeDetailInfo employeeDetail = null;

            using (var repo = new GMContext())
            {
                employeeDetail = repo.Database.SqlQuery<HrEmployeeDetailInfo>(@"
                    SELECT
                        [RowId],
                        [EmployeeDetailId],
                        [EmployeeId],
                        [FathersName],
                        [MothersName],
                        [NumberOfChildren],
                        [SpouseName],
                        [NationalIdNumber],
                        [CityOfBirth],
                        [DistrictIdOfBirth],
                        [CountryIdOfBirth],
                        [CountryIdOfCitizenship],
                        [BirthMark],
                        [Height],
                        [Weight],
                        [EmergencyContactPerson],
                        [EmergenryContactNo],
                        [EmergenryContactRelation],
                        [TotalExperience],
                        [WillingToTravel],
                        [PassportNo],
                        [SecurityClearance],
                        [WorkAuthorizationId],
                        [PreferedLocation],
                        [WillingToRelocate],
                        [Remarks]
                    FROM [dbo].[HrEmployeeDetail]
                    WHERE RowId = @RowId
                    AND DomainStatus = 1
                ", new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }

            return employeeDetail;
        }

        public static HrEmployeeDetailInfo GetHrEmployeeDetailByEmployeeId(string EmployeeId, string RowId)
        {
            HrEmployeeDetailInfo employeeDetail = null;

            using (var repo = new GMContext())
            {
                employeeDetail = repo.Database.SqlQuery<HrEmployeeDetailInfo>(@"
                    SELECT *
                    FROM [dbo].[HrEmployeeDetail]
                    WHERE EmployeeId = @EmployeeId
                    AND (@RowId IS NULL OR RowId <> @RowId)
                    AND DomainStatus = 1
                ",
                new SqlParameter("@EmployeeId", EmployeeId),
                new SqlParameter("@RowId", (object)RowId ?? DBNull.Value)
                ).FirstOrDefault();
            }

            return employeeDetail;
        }

        public static List<HrEmployeeDetailInfo> GetAllHrEmployeeDetails()
        {
            List<HrEmployeeDetailInfo> employeeDetails = null;

            using (var repo = new GMContext())
            {
                employeeDetails = repo.Database.SqlQuery<HrEmployeeDetailInfo>(@"
                    SELECT
                        [RowId],
                        [EmployeeDetailId],
                        [EmployeeId],
                        [FathersName],
                        [MothersName],
                        [NumberOfChildren],
                        [SpouseName],
                        [NationalIdNumber],
                        [CityOfBirth],
                        [DistrictIdOfBirth],
                        [CountryIdOfBirth],
                        [CountryIdOfCitizenship],
                        [BirthMark],
                        [Height],
                        [Weight],
                        [EmergencyContactPerson],
                        [EmergenryContactNo],
                        [EmergenryContactRelation],
                        [TotalExperience],
                        [WillingToTravel],
                        [PassportNo],
                        [SecurityClearance],
                        [WorkAuthorizationId],
                        [PreferedLocation],
                        [WillingToRelocate],
                        [Remarks]
                    FROM [dbo].[HrEmployeeDetail]
                    WHERE DomainStatus = 1
                    ORDER BY EmployeeDetailId
                ").ToList();
            }

            return employeeDetails;
        }

        public static void AddHrEmployeeDetail(HrEmployeeDetailInfo detail, string loggedInUser)
        {
            HrEmployeeDetailInfo obj;

            using (var repo = new GMContext())
            {
                obj = repo.Database.SqlQuery<HrEmployeeDetailInfo>(@"
                    SELECT ISNULL(MAX(EmployeeDetailId),0)+1 AS EmployeeDetailId
                    FROM HrEmployeeDetail
                    WHERE DomainStatus = 1
                ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO [dbo].[HrEmployeeDetail]
                    (
                        RowId,
                        EmployeeDetailId,
                        EmployeeId,
                        FathersName,
                        MothersName,
                        NumberOfChildren,
                        SpouseName,
                        NationalIdNumber,
                        CityOfBirth,
                        DistrictIdOfBirth,
                        CountryIdOfBirth,
                        CountryIdOfCitizenship,
                        BirthMark,
                        Height,
                        Weight,
                        EmergencyContactPerson,
                        EmergenryContactNo,
                        EmergenryContactRelation,
                        TotalExperience,
                        WillingToTravel,
                        PassportNo,
                        SecurityClearance,
                        WorkAuthorizationId,
                        PreferedLocation,
                        WillingToRelocate,
                        Remarks,
                        CreateUser,
                        CreateDate,
                        DomainStatus
                    )
                    VALUES
                    (
                        @RowId,
                        @EmployeeDetailId,
                        @EmployeeId,
                        @FathersName,
                        @MothersName,
                        @NumberOfChildren,
                        @SpouseName,
                        @NationalIdNumber,
                        @CityOfBirth,
                        @DistrictIdOfBirth,
                        @CountryIdOfBirth,
                        @CountryIdOfCitizenship,
                        @BirthMark,
                        @Height,
                        @Weight,
                        @EmergencyContactPerson,
                        @EmergenryContactNo,
                        @EmergenryContactRelation,
                        @TotalExperience,
                        @WillingToTravel,
                        @PassportNo,
                        @SecurityClearance,
                        @WorkAuthorizationId,
                        @PreferedLocation,
                        @WillingToRelocate,
                        @Remarks,
                        @CreateUser,
                        GETDATE(),
                        1
                    )
                ",
                new SqlParameter("@RowId", detail.RowId),
                new SqlParameter("@EmployeeDetailId", obj.EmployeeDetailId),
                new SqlParameter("@EmployeeId", detail.EmployeeId),
                new SqlParameter("@FathersName", detail.FathersName ?? ""),
                new SqlParameter("@MothersName", detail.MothersName ?? ""),
                new SqlParameter("@NumberOfChildren", detail.NumberOfChildren ?? ""),
                new SqlParameter("@SpouseName", detail.SpouseName ?? ""),
                new SqlParameter("@NationalIdNumber", detail.NationalIdNumber ?? ""),
                new SqlParameter("@CityOfBirth", detail.CityOfBirth ?? ""),
                new SqlParameter("@DistrictIdOfBirth", detail.DistrictIdOfBirth ?? ""),
                new SqlParameter("@CountryIdOfBirth", detail.CountryIdOfBirth ?? ""),
                new SqlParameter("@CountryIdOfCitizenship", detail.CountryIdOfCitizenship ?? ""),
                new SqlParameter("@BirthMark", detail.BirthMark ?? ""),
                new SqlParameter("@Height", detail.Height ?? ""),
                new SqlParameter("@Weight", detail.Weight ?? ""),
                new SqlParameter("@EmergencyContactPerson", detail.EmergencyContactPerson ?? ""),
                new SqlParameter("@EmergenryContactNo", detail.EmergenryContactNo ?? ""),
                new SqlParameter("@EmergenryContactRelation", detail.EmergenryContactRelation ?? ""),
                new SqlParameter("@TotalExperience", detail.TotalExperience ?? ""),
                new SqlParameter("@WillingToTravel", detail.WillingToTravel ?? ""),
                new SqlParameter("@PassportNo", detail.PassportNo ?? ""),
                new SqlParameter("@SecurityClearance", detail.SecurityClearance ?? ""),
                new SqlParameter("@WorkAuthorizationId", detail.WorkAuthorizationId ?? ""),
                new SqlParameter("@PreferedLocation", detail.PreferedLocation ?? ""),
                new SqlParameter("@WillingToRelocate", detail.WillingToRelocate ?? ""),
                new SqlParameter("@Remarks", detail.Remarks ?? ""),
                new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        public static void UpdateHrEmployeeDetail(HrEmployeeDetailInfo detail, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE [dbo].[HrEmployeeDetail]
                    SET
                        EmployeeId = @EmployeeId,
                        FathersName = @FathersName,
                        MothersName = @MothersName,
                        NumberOfChildren = @NumberOfChildren,
                        SpouseName = @SpouseName,
                        NationalIdNumber = @NationalIdNumber,
                        CityOfBirth = @CityOfBirth,
                        DistrictIdOfBirth = @DistrictIdOfBirth,
                        CountryIdOfBirth = @CountryIdOfBirth,
                        CountryIdOfCitizenship = @CountryIdOfCitizenship,
                        BirthMark = @BirthMark,
                        Height = @Height,
                        Weight = @Weight,
                        EmergencyContactPerson = @EmergencyContactPerson,
                        EmergenryContactNo = @EmergenryContactNo,
                        EmergenryContactRelation = @EmergenryContactRelation,
                        TotalExperience = @TotalExperience,
                        WillingToTravel = @WillingToTravel,
                        PassportNo = @PassportNo,
                        SecurityClearance = @SecurityClearance,
                        WorkAuthorizationId = @WorkAuthorizationId,
                        PreferedLocation = @PreferedLocation,
                        WillingToRelocate = @WillingToRelocate,
                        Remarks = @Remarks,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", detail.RowId),
                new SqlParameter("@EmployeeId", detail.EmployeeId),
                new SqlParameter("@FathersName", detail.FathersName ?? ""),
                new SqlParameter("@MothersName", detail.MothersName ?? ""),
                new SqlParameter("@NumberOfChildren", detail.NumberOfChildren ?? ""),
                new SqlParameter("@SpouseName", detail.SpouseName ?? ""),
                new SqlParameter("@NationalIdNumber", detail.NationalIdNumber ?? ""),
                new SqlParameter("@CityOfBirth", detail.CityOfBirth ?? ""),
                new SqlParameter("@DistrictIdOfBirth", detail.DistrictIdOfBirth ?? ""),
                new SqlParameter("@CountryIdOfBirth", detail.CountryIdOfBirth ?? ""),
                new SqlParameter("@CountryIdOfCitizenship", detail.CountryIdOfCitizenship ?? ""),
                new SqlParameter("@BirthMark", detail.BirthMark ?? ""),
                new SqlParameter("@Height", detail.Height ?? ""),
                new SqlParameter("@Weight", detail.Weight ?? ""),
                new SqlParameter("@EmergencyContactPerson", detail.EmergencyContactPerson ?? ""),
                new SqlParameter("@EmergenryContactNo", detail.EmergenryContactNo ?? ""),
                new SqlParameter("@EmergenryContactRelation", detail.EmergenryContactRelation ?? ""),
                new SqlParameter("@TotalExperience", detail.TotalExperience ?? ""),
                new SqlParameter("@WillingToTravel", detail.WillingToTravel ?? ""),
                new SqlParameter("@PassportNo", detail.PassportNo ?? ""),
                new SqlParameter("@SecurityClearance", detail.SecurityClearance ?? ""),
                new SqlParameter("@WorkAuthorizationId", detail.WorkAuthorizationId ?? ""),
                new SqlParameter("@PreferedLocation", detail.PreferedLocation ?? ""),
                new SqlParameter("@WillingToRelocate", detail.WillingToRelocate ?? ""),
                new SqlParameter("@Remarks", detail.Remarks ?? ""),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static void DeleteHrEmployeeDetail(HrEmployeeDetailInfo detail, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE [dbo].[HrEmployeeDetail]
                    SET
                        DomainStatus = 0,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", detail.RowId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static HrEmployeeDetailResponse GetFilteredHrEmployeeDetails(HttpRequestBase Request)
        {
            HrEmployeeDetailResponse resp = new HrEmployeeDetailResponse();
            GMDataTableDAL gmdt = new GMDataTableDAL(Request);

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
                    FROM [dbo].[HrEmployeeDetail] a
                    WHERE a.DomainStatus = 1
                ");

                List<SqlParameter> parameters = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.Append(@"
                        AND (
                            a.EmployeeId LIKE '%' + @SearchValue + '%'
                            OR a.FathersName LIKE '%' + @SearchValue + '%'
                            OR a.SpouseName LIKE '%' + @SearchValue + '%'
                            OR a.NationalIdNumber LIKE '%' + @SearchValue + '%'
                        )");

                    parameters.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));
                }

                resp.TotalRecordCount = repo.Database.SqlQuery<int>(
                    "SELECT COUNT(*) FROM [dbo].[HrEmployeeDetail] WHERE DomainStatus = 1"
                ).Single();

                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(
                    "SELECT COUNT(*) " + sb.ToString(),
                    parameters.ToArray()
                ).Single();

                parameters.Add(new SqlParameter("@Skip", gmdt.Start));
                parameters.Add(new SqlParameter("@Take", gmdt.Length));

                resp.HrEmployeeDetails = repo.Database.SqlQuery<HrEmployeeDetailInfo>(@"
                    SELECT
                        a.RowId,
                        a.EmployeeDetailId,
                        a.EmployeeId,
                        a.FathersName,
                        a.SpouseName,
                        a.NationalIdNumber,
                        a.EmergencyContactPerson,
                        a.EmergenryContactNo
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