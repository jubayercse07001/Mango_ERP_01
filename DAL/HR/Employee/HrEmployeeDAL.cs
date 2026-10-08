using ERP.Models;
using ERP.Models.HR.Employee.EmployeeDetail;
using ERP.Models.HR.Employee.HrEmployeeEducation;
using ERP.Models.HR.Employee.HrEmployeeProfessionalKnowledge;
using ERP.Models.HR.Employee.HrEmployeeTraining;
using Mango_ERP.Models; 
using Mango_ERP.Models.HR.Employee;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Employee
{
    public class HrEmployeeDAL
    {
        public static List<HrEmployeeInfo> GetDivisionByCompanyId(string CompanyId)
        {
            using (var repo = new GMContext())
            {
                return repo.Database.SqlQuery<HrEmployeeInfo>(@"
            SELECT DISTINCT
                D.RowId,
                D.DivisionName,
                D.Rank
            FROM HrApprovedPosition AP
            INNER JOIN HrDivision D
                ON AP.DivisionId = D.RowId
            WHERE AP.CompanyId = @CompanyId
                AND AP.DomainStatus = 1
                AND D.DomainStatus = 1
            ORDER BY D.Rank,D.DivisionName;
        ",
                new SqlParameter("@CompanyId", CompanyId))
                .ToList();
            }
        }

        public static List<HrEmployeeInfo> GetBranchByCompanyIdDivisionId(string CompanyId, string DivisionId)
        {
            using (var repo = new GMContext())
            {
                return repo.Database.SqlQuery<HrEmployeeInfo>(@"
            SELECT DISTINCT
                B.RowId,
                B.BranchName,
                B.Rank
            FROM HrApprovedPosition AP
            INNER JOIN HrBranch B
                ON AP.BranchId = B.RowId
            WHERE AP.CompanyId = @CompanyId
              AND AP.DivisionId = @DivisionId
              AND AP.DomainStatus = 1
              AND B.DomainStatus = 1
            ORDER BY B.Rank, B.BranchName;",
                    new SqlParameter("@CompanyId", CompanyId),
                    new SqlParameter("@DivisionId", DivisionId)
                ).ToList();
            }
        }

        public static List<HrEmployeeInfo> GetDepartmentByCompanyIdDivisionIdBranchId(
    string CompanyId,
    string DivisionId,
    string BranchId)
        {
            using (var repo = new GMContext())
            {
                return repo.Database.SqlQuery<HrEmployeeInfo>(@"
            SELECT DISTINCT
                D.RowId,
                D.DepartmentName,
                D.Rank
            FROM HrApprovedPosition AP
            INNER JOIN HrDepartment D
                ON AP.DepartmentId = D.RowId
            WHERE AP.CompanyId = @CompanyId
              AND AP.DivisionId = @DivisionId
              AND AP.BranchId = @BranchId
              AND AP.DomainStatus = 1
              AND D.DomainStatus = 1
            ORDER BY D.Rank, D.DepartmentName;",
                    new SqlParameter("@CompanyId", CompanyId),
                    new SqlParameter("@DivisionId", DivisionId),
                    new SqlParameter("@BranchId", BranchId)
                ).ToList();
            }
        }

        public static List<HrEmployeeInfo> GetDesignationByCompanyIdDivisionIdBranchIdDepartmentId(
    string CompanyId,
    string DivisionId,
    string BranchId,
    string DepartmentId)
        {
            using (var repo = new GMContext())
            {
                return repo.Database.SqlQuery<HrEmployeeInfo>(@"
            SELECT DISTINCT
                D.RowId,
                D.DesignationName,
                D.Rank
            FROM HrApprovedPosition AP
            INNER JOIN HrDesignation D
                ON AP.DesignationId = D.RowId
            WHERE AP.CompanyId = @CompanyId
              AND AP.DivisionId = @DivisionId
              AND AP.BranchId = @BranchId
              AND AP.DepartmentId = @DepartmentId
              AND AP.DomainStatus = 1
              AND D.DomainStatus = 1
            ORDER BY D.Rank, D.DesignationName;",
                    new SqlParameter("@CompanyId", CompanyId),
                    new SqlParameter("@DivisionId", DivisionId),
                    new SqlParameter("@BranchId", BranchId),
                    new SqlParameter("@DepartmentId", DepartmentId)
                ).ToList();
            }
        }
        public static List<HrEmployeeInfo> GetEmployeeTypeByCompanyIdDivisionIdBranchIdDepartmentId(
    string CompanyId,
    string DivisionId,
    string BranchId,
    string DepartmentId)
        {
            using (var repo = new GMContext())
            {
                return repo.Database.SqlQuery<HrEmployeeInfo>(@"
            SELECT DISTINCT
                ET.RowId,
                ET.EmployeeTypeName,
                ET.Rank
            FROM HrApprovedPosition AP
            INNER JOIN HrEmployeeType ET
                ON AP.EmployeeTypeId = ET.RowId
            WHERE AP.CompanyId = @CompanyId
              AND AP.DivisionId = @DivisionId
              AND AP.BranchId = @BranchId
              AND AP.DepartmentId = @DepartmentId
              AND AP.DomainStatus = 1
              AND ET.DomainStatus = 1
            ORDER BY ET.Rank, ET.EmployeeTypeName;",
                    new SqlParameter("@CompanyId", CompanyId),
                    new SqlParameter("@DivisionId", DivisionId),
                    new SqlParameter("@BranchId", BranchId),
                    new SqlParameter("@DepartmentId", DepartmentId)
                ).ToList();
            }
        }

        public static HrEmployeeInfo GetEmployeeByID(string RowId)
        {
            HrEmployeeInfo employee = null;

            using (var repo = new GMContext())
            {
                employee = repo.Database.SqlQuery<HrEmployeeInfo>(@"
                    SELECT 
                        e.*,
                        c.CompanyName,
                        d.DepartmentName,
                        deg.DesignationName
                    FROM HrEmployee e
                    LEFT JOIN HrCompanyInformation c ON c.RowId = e.CompanyId
                    LEFT JOIN HrDepartment d ON d.RowId = e.DepartmentId
                    LEFT JOIN HrDesignation deg ON deg.RowId = e.DesignationId
                    WHERE e.RowId = @RowId
                    AND e.DomainStatus = 1
                ", new SqlParameter("@RowId", RowId)).FirstOrDefault();
            }

            return employee;
        }

        public static HrEmployeeInfo GetEmployeeByStaffCode(string StaffCode, string RowId)
        {
            HrEmployeeInfo employee = null;

            using (var repo = new GMContext())
            {
                employee = repo.Database.SqlQuery<HrEmployeeInfo>(@"
                    SELECT   RowId, EmployeeId, StaffCode, FirstName, MiddleName, LastName, 
                PresentAddress, PermanentAddress, BloodGroupId, DateOfBirth, NationalityId, 
                HomePhone, CellPhone, OfficePhone, PrimaryEmail, AlternateEmail, Gender, 
                ReligionId, MaritalStatus, DateOfJoin, DateOfRetirement, DateOfPermanent, 
                CompanyId, DivisionId, BranchId, DepartmentId, SectionId, DesignationId, 
                IsSalaried, BasicSalary, CurrentSalary, IsConfirmed, ConfirmationDate, 
                IsTerminated, TerminateDate, TerminateBy, TerminateReason, EmployeeTypeId, 
                GradeId, BankAccountNumber, UserId, NationalIdNumber, IsPoliceVerified, 
                PoliceVerificationNumber, PeferredJob, JobType, EngineerTypeId, 
                IsAdminEmployee, IsEngineer, Rank, CreateUser, CreateDate, DomainStatus FROM HrEmployee
                    WHERE StaffCode = @StaffCode
                    AND (@RowId IS NULL OR RowId <> @RowId)
                    AND DomainStatus = 1
                ",
                new SqlParameter("@StaffCode", StaffCode),
                new SqlParameter("@RowId", (object)RowId ?? DBNull.Value)
                ).FirstOrDefault();
            }

            return employee;
        }

        public static List<HrEmployeeInfo> GetAllHrEmployees()
        {
            List<HrEmployeeInfo> employees = null;

            using (var repo = new GMContext())
            {
                employees = repo.Database.SqlQuery<HrEmployeeInfo>(@"
                    SELECT RowId, EmployeeId, StaffCode, FirstName, MiddleName, LastName, 
                PresentAddress, PermanentAddress, BloodGroupId, DateOfBirth, NationalityId, 
                HomePhone, CellPhone, OfficePhone, PrimaryEmail, AlternateEmail, Gender, 
                ReligionId, MaritalStatus, DateOfJoin, DateOfRetirement, DateOfPermanent, 
                CompanyId, DivisionId, BranchId, DepartmentId, SectionId, DesignationId, 
                IsSalaried, BasicSalary, CurrentSalary, IsConfirmed, ConfirmationDate, 
                IsTerminated, TerminateDate, TerminateBy, TerminateReason, EmployeeTypeId, 
                GradeId, BankAccountNumber, UserId, NationalIdNumber, IsPoliceVerified, 
                PoliceVerificationNumber, PeferredJob, JobType, EngineerTypeId, 
                IsAdminEmployee, IsEngineer, Rank, CreateUser, CreateDate, DomainStatus
                        FROM HrEmployee
                         WHERE DomainStatus = 1
                            ORDER BY StaffCode
                ").ToList();
            }

            return employees;
        }
       /* public static void AddHrEmployee(HrEmployeeInfo employee, string loggedInUser)
        {
            HrEmployeeInfo obj;

               using (var repo = new GMContext())
             {
                 // Generate Next EmployeeId
                 obj = repo.Database.SqlQuery<HrEmployeeInfo>(@"
             SELECT ISNULL(MAX(EmployeeId),0) + 1 AS EmployeeId
             FROM HrEmployee
             WHERE DomainStatus = 1
         ").FirstOrDefault();

                 employee.EmployeeId = obj != null ? obj.EmployeeId : 1;
                 string targetEmployeeFk = employee.RowId;

                repo.Database.ExecuteSqlCommand(@"
            INSERT INTO dbo.HrEmployee
             (
                         RowId,
                 EmployeeId,
                 CompanyId,
                 DivisionId,
                 BranchId,
                 DepartmentId,
                 SectionId,
                 DesignationId,
                 EmployeeTypeId,
                 EngineerTypeId,
                 JobType,
                 StaffCode,
                 FirstName,
                 MiddleName,
                 LastName,
                 PresentAddress,
                 PermanentAddress,
                 BloodGroupId,
                 DateOfBirth,
                 NationalityId,
                 HomePhone,
                 CellPhone,
                 OfficePhone,
                 PrimaryEmail,
                 AlternateEmail,
                 Gender,
                 ReligionId,
                 MaritalStatus,
                 DateOfJoin,
                 DateOfRetirement,
                 DateOfPermanent,
                 IsSalaried,
                 BasicSalary,
                 CurrentSalary,
                 IsConfirmed,
                 ConfirmationDate,
                 IsTerminated,
                 TerminateDate,
                 TerminateBy,
                 TerminateReason,
                 GradeId,
                 BankAccountNumber,
                 UserId,
                 NationalIdNumber,
                 IsPoliceVerified,
                 PoliceVerificationNumber,
                 PeferredJob,
                 IsAdminEmployee,
                 IsEngineer,
                 IsInLPR,
                 Rank,
                 CreateUser,
                 CreateDate,
                 DomainStatus
             )
             VALUES
             (
                 @RowId,
                 @EmployeeId,
                 @CompanyId,
                 @DivisionId,
                 @BranchId,
                 @DepartmentId,
                 @SectionId,
                 @DesignationId,
                 @EmployeeTypeId,
                 @EngineerTypeId,
                 @JobType,
                 @StaffCode,
                 @FirstName,
                 @MiddleName,
                 @LastName,
                 @PresentAddress,
                 @PermanentAddress,
                 @BloodGroupId,
                 @DateOfBirth,
                 @NationalityId,
                 @HomePhone,
                 @CellPhone,
                 @OfficePhone,
                 @PrimaryEmail,
                 @AlternateEmail,
                 @Gender,
                 @ReligionId,
                 @MaritalStatus,
                 @DateOfJoin,
                 @DateOfRetirement,
                 @DateOfPermanent,
                 @IsSalaried,
                 @BasicSalary,
                 @CurrentSalary,
                 @IsConfirmed,
                 @ConfirmationDate,
                 @IsTerminated,
                 @TerminateDate,
                 @TerminateBy,
                 @TerminateReason,
                 @GradeId,
                 @BankAccountNumber,
                 @UserId,
                 @NationalIdNumber,
                 @IsPoliceVerified,
                 @PoliceVerificationNumber,
                 @PeferredJob,
                 @IsAdminEmployee,
                 @IsEngineer,
                 @IsInLPR,
                 @Rank,
                 @CreateUser,
                 GETDATE(),
                 1
             )",

                     new SqlParameter("@RowId", employee.RowId),
                     new SqlParameter("@EmployeeId", employee.EmployeeId),

                     new SqlParameter("@CompanyId", employee.CompanyId ?? ""),
                     new SqlParameter("@DivisionId", employee.DivisionId ?? ""),
                     new SqlParameter("@BranchId", employee.BranchId ?? ""),
                     new SqlParameter("@DepartmentId", employee.DepartmentId ?? ""),
                     new SqlParameter("@SectionId", employee.SectionId ?? ""),
                     new SqlParameter("@DesignationId", employee.DesignationId ?? ""),

                     new SqlParameter("@EmployeeTypeId", employee.EmployeeTypeId ?? ""),
                     new SqlParameter("@EngineerTypeId", employee.EngineerTypeId ?? ""),
                     new SqlParameter("@JobType", employee.JobType ?? ""),

                     new SqlParameter("@StaffCode", employee.StaffCode ?? ""),
                     new SqlParameter("@FirstName", employee.FirstName ?? ""),
                     new SqlParameter("@MiddleName", employee.MiddleName ?? ""),
                     new SqlParameter("@LastName", employee.LastName ?? ""),

                     new SqlParameter("@PresentAddress", employee.PresentAddress ?? ""),
                     new SqlParameter("@PermanentAddress", employee.PermanentAddress ?? ""),

                     new SqlParameter("@BloodGroupId", employee.BloodGroupId ?? ""),

                     new SqlParameter("@DateOfBirth", (object)employee.DateOfBirth ?? DateTime.Parse("1900-01-01")),

                     new SqlParameter("@NationalityId", employee.NationalityId ?? ""),

                     new SqlParameter("@HomePhone", (object)employee.HomePhone ?? DBNull.Value),
                     new SqlParameter("@CellPhone", (object)employee.CellPhone ?? DBNull.Value),
                     new SqlParameter("@OfficePhone", (object)employee.OfficePhone ?? DBNull.Value),

                     new SqlParameter("@PrimaryEmail", employee.PrimaryEmail ?? ""),
                     new SqlParameter("@AlternateEmail", employee.AlternateEmail ?? ""),

                     new SqlParameter("@Gender", employee.Gender ?? ""),
                     new SqlParameter("@ReligionId", employee.ReligionId ?? ""),
                     new SqlParameter("@MaritalStatus", employee.MaritalStatus ?? ""),

                     new SqlParameter("@DateOfJoin", (object)employee.DateOfJoin ?? DateTime.Parse("1900-01-01")),
                     new SqlParameter("@DateOfRetirement", (object)employee.DateOfRetirement ?? DateTime.Parse("1900-01-01")),
                     new SqlParameter("@DateOfPermanent", (object)employee.DateOfPermanent ?? DateTime.Parse("1900-01-01")),

                     new SqlParameter("@IsSalaried", employee.IsSalaried),
                     new SqlParameter("@BasicSalary", employee.BasicSalary),
                     new SqlParameter("@CurrentSalary", employee.CurrentSalary),

                     new SqlParameter("@IsConfirmed", employee.IsConfirmed),
                     new SqlParameter("@ConfirmationDate", (object)employee.ConfirmationDate ?? DateTime.Parse("1900-01-01")),

                     new SqlParameter("@IsTerminated", employee.IsTerminated),
                     new SqlParameter("@TerminateDate", (object)employee.TerminateDate ?? DateTime.Parse("1900-01-01")),

                     new SqlParameter("@TerminateBy", employee.TerminateBy ?? ""),
                     new SqlParameter("@TerminateReason", employee.TerminateReason ?? ""),

                     new SqlParameter("@GradeId", employee.GradeId ?? ""),

                     new SqlParameter("@BankAccountNumber", (object)employee.BankAccountNumber ?? DBNull.Value),

                     new SqlParameter("@UserId", employee.UserId ?? ""),

                     new SqlParameter("@NationalIdNumber", (object)employee.NationalIdNumber ?? DBNull.Value),

                     new SqlParameter("@IsPoliceVerified", employee.IsPoliceVerified),
                     new SqlParameter("@PoliceVerificationNumber", employee.PoliceVerificationNumber ?? ""),

                     new SqlParameter("@PeferredJob", employee.PeferredJob ?? ""),

                     new SqlParameter("@IsAdminEmployee", employee.IsAdminEmployee),
                     new SqlParameter("@IsEngineer", employee.IsEngineer),
                     new SqlParameter("@IsInLPR", employee.IsInLPR),

                     new SqlParameter("@Rank", employee.Rank),

                     new SqlParameter("@CreateUser", loggedInUser)
                 );

                                 // -------------------------------------------------------------
                                 // 2. CHILD TABLE 1: HrEmployeeDetail
                                 // -------------------------------------------------------------
                                 if (employee.EmployeeDetail != null)
                                 {
                                     HrEmployeeDetailInfo detailObj = repo.Database.SqlQuery<HrEmployeeDetailInfo>(@"
                                 SELECT ISNULL(MAX(EmployeeDetailId),0) + 1 AS EmployeeDetailId
                                 FROM HrEmployeeDetail
                             ").FirstOrDefault();

                                     int employeeDetailId = detailObj != null ? detailObj.EmployeeDetailId : 1;

                                     repo.Database.ExecuteSqlCommand(@"
                                 INSERT INTO [dbo].[HrEmployeeDetail]
                                 (
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
                                     [Remarks],
                                     [CreateUser],
                                     [CreateDate],
                                     [DomainStatus]
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
                                 )",
                                         new SqlParameter("@RowId", (object)employee.EmployeeDetail.RowId ?? Guid.NewGuid().ToString()),
                                         new SqlParameter("@EmployeeDetailId", employeeDetailId),
                                         new SqlParameter("@EmployeeId", targetEmployeeFk),
                                         new SqlParameter("@FathersName", (object)employee.EmployeeDetail.FathersName ?? DBNull.Value),
                                         new SqlParameter("@MothersName", (object)employee.EmployeeDetail.MothersName ?? DBNull.Value),
                                         new SqlParameter("@NumberOfChildren", (object)employee.EmployeeDetail.NumberOfChildren ?? DBNull.Value),
                                         new SqlParameter("@SpouseName", (object)employee.EmployeeDetail.SpouseName ?? DBNull.Value),
                                         new SqlParameter("@NationalIdNumber", (object)employee.EmployeeDetail.NationalIdNumber ?? DBNull.Value),
                                         new SqlParameter("@CityOfBirth", (object)employee.EmployeeDetail.CityOfBirth ?? DBNull.Value),
                                         new SqlParameter("@DistrictIdOfBirth", (object)employee.EmployeeDetail.DistrictIdOfBirth ?? DBNull.Value),
                                         new SqlParameter("@CountryIdOfBirth", (object)employee.EmployeeDetail.CountryIdOfBirth ?? DBNull.Value),
                                         new SqlParameter("@CountryIdOfCitizenship", (object)employee.EmployeeDetail.CountryIdOfCitizenship ?? DBNull.Value),
                                         new SqlParameter("@BirthMark", (object)employee.EmployeeDetail.BirthMark ?? DBNull.Value),
                                         new SqlParameter("@Height", (object)employee.EmployeeDetail.Height ?? DBNull.Value),
                                         new SqlParameter("@Weight", (object)employee.EmployeeDetail.Weight ?? DBNull.Value),
                                         new SqlParameter("@EmergencyContactPerson", (object)employee.EmployeeDetail.EmergencyContactPerson ?? DBNull.Value),
                                         new SqlParameter("@EmergenryContactNo", (object)employee.EmployeeDetail.EmergenryContactNo ?? DBNull.Value),
                                         new SqlParameter("@EmergenryContactRelation", (object)employee.EmployeeDetail.EmergenryContactRelation ?? DBNull.Value),
                                         new SqlParameter("@TotalExperience", (object)employee.EmployeeDetail.TotalExperience ?? DBNull.Value),
                                         new SqlParameter("@WillingToTravel", (object)employee.EmployeeDetail.WillingToTravel ?? DBNull.Value),
                                         new SqlParameter("@PassportNo", (object)employee.EmployeeDetail.PassportNo ?? DBNull.Value),
                                         new SqlParameter("@SecurityClearance", (object)employee.EmployeeDetail.SecurityClearance ?? DBNull.Value),
                                         new SqlParameter("@WorkAuthorizationId", (object)employee.EmployeeDetail.WorkAuthorizationId ?? DBNull.Value),
                                         new SqlParameter("@PreferedLocation", (object)employee.EmployeeDetail.PreferedLocation ?? DBNull.Value),
                                         new SqlParameter("@WillingToRelocate", (object)employee.EmployeeDetail.WillingToRelocate ?? DBNull.Value),
                                         new SqlParameter("@Remarks", (object)employee.EmployeeDetail.Remarks ?? DBNull.Value),
                                         new SqlParameter("@CreateUser", loggedInUser)
                                     );
                                 } 
            string targetEmployeeFk = employee.RowId;
        }
    }*/
    public static void AddHrEmployee(HrEmployeeInfo employee, string loggedInUser)
        {
            HrEmployeeInfo obj;

             using (var repo = new GMContext())
            {
                // Generate Next EmployeeId
                obj = repo.Database.SqlQuery<HrEmployeeInfo>(@"
                SELECT ISNULL(MAX(EmployeeId),0) + 1 AS EmployeeId
                FROM HrEmployee
                WHERE DomainStatus = 1
            ").FirstOrDefault();

                employee.EmployeeId = obj != null ? obj.EmployeeId : 1;
                string targetEmployeeFk = employee.RowId;

             repo.Database.ExecuteSqlCommand(@"
             INSERT INTO dbo.HrEmployee
            (
                RowId,
                EmployeeId,
                CompanyId,
                DivisionId,
                BranchId,
                DepartmentId,
                SectionId,
                DesignationId,
                EmployeeTypeId,
                EngineerTypeId,
                JobType,
                StaffCode,
                FirstName,
                MiddleName,
                LastName,
                PresentAddress,
                PermanentAddress,
                BloodGroupId,
                DateOfBirth,
                NationalityId,
                HomePhone,
                CellPhone,
                OfficePhone,
                PrimaryEmail,
                AlternateEmail,
                Gender,
                ReligionId,
                MaritalStatus,
                DateOfJoin,
                DateOfRetirement,
                DateOfPermanent,
                IsSalaried,
                BasicSalary,
                CurrentSalary,
                IsConfirmed,
                ConfirmationDate,
                IsTerminated,
                TerminateDate,
                TerminateBy,
                TerminateReason,
                GradeId,
                BankAccountNumber,
                UserId,
                NationalIdNumber,
                IsPoliceVerified,
                PoliceVerificationNumber,
                PeferredJob,
                IsAdminEmployee,
                IsEngineer,
                IsInLPR,
                Rank,
                CreateUser,
                CreateDate,
                DomainStatus
            )
            VALUES
            (
                @RowId,
                @EmployeeId,
                @CompanyId,
                @DivisionId,
                @BranchId,
                @DepartmentId,
                @SectionId,
                @DesignationId,
                @EmployeeTypeId,
                @EngineerTypeId,
                @JobType,
                @StaffCode,
                @FirstName,
                @MiddleName,
                @LastName,
                @PresentAddress,
                @PermanentAddress,
                @BloodGroupId,
                @DateOfBirth,
                @NationalityId,
                @HomePhone,
                @CellPhone,
                @OfficePhone,
                @PrimaryEmail,
                @AlternateEmail,
                @Gender,
                @ReligionId,
                @MaritalStatus,
                @DateOfJoin,
                @DateOfRetirement,
                @DateOfPermanent,
                @IsSalaried,
                @BasicSalary,
                @CurrentSalary,
                @IsConfirmed,
                @ConfirmationDate,
                @IsTerminated,
                @TerminateDate,
                @TerminateBy,
                @TerminateReason,
                @GradeId,
                @BankAccountNumber,
                @UserId,
                @NationalIdNumber,
                @IsPoliceVerified,
                @PoliceVerificationNumber,
                @PeferredJob,
                @IsAdminEmployee,
                @IsEngineer,
                @IsInLPR,
                @Rank,
                @CreateUser,
                GETDATE(),
                1
            )",

                    new SqlParameter("@RowId", employee.RowId),
                    new SqlParameter("@EmployeeId", employee.EmployeeId),

                    new SqlParameter("@CompanyId", employee.CompanyId ?? ""),
                    new SqlParameter("@DivisionId", employee.DivisionId ?? ""),
                    new SqlParameter("@BranchId", employee.BranchId ?? ""),
                    new SqlParameter("@DepartmentId", employee.DepartmentId ?? ""),
                    new SqlParameter("@SectionId", employee.SectionId ?? ""),
                    new SqlParameter("@DesignationId", employee.DesignationId ?? ""),

                    new SqlParameter("@EmployeeTypeId", employee.EmployeeTypeId ?? ""),
                    new SqlParameter("@EngineerTypeId", employee.EngineerTypeId ?? ""),
                    new SqlParameter("@JobType", employee.JobType ?? ""),

                    new SqlParameter("@StaffCode", employee.StaffCode ?? ""),
                    new SqlParameter("@FirstName", employee.FirstName ?? ""),
                    new SqlParameter("@MiddleName", employee.MiddleName ?? ""),
                    new SqlParameter("@LastName", employee.LastName ?? ""),

                    new SqlParameter("@PresentAddress", employee.PresentAddress ?? ""),
                    new SqlParameter("@PermanentAddress", employee.PermanentAddress ?? ""),

                    new SqlParameter("@BloodGroupId", employee.BloodGroupId ?? ""),

                    new SqlParameter("@DateOfBirth", (object)employee.DateOfBirth ?? DateTime.Parse("1900-01-01")),

                    new SqlParameter("@NationalityId", employee.NationalityId ?? ""),

                    new SqlParameter("@HomePhone", (object)employee.HomePhone ?? DBNull.Value),
                    new SqlParameter("@CellPhone", (object)employee.CellPhone ?? DBNull.Value),
                    new SqlParameter("@OfficePhone", (object)employee.OfficePhone ?? DBNull.Value),

                    new SqlParameter("@PrimaryEmail", employee.PrimaryEmail ?? ""),
                    new SqlParameter("@AlternateEmail", employee.AlternateEmail ?? ""),

                    new SqlParameter("@Gender", employee.Gender ?? ""),
                    new SqlParameter("@ReligionId", employee.ReligionId ?? ""),
                    new SqlParameter("@MaritalStatus", employee.MaritalStatus ?? ""),

                    new SqlParameter("@DateOfJoin", (object)employee.DateOfJoin ?? DateTime.Parse("1900-01-01")),
                    new SqlParameter("@DateOfRetirement", (object)employee.DateOfRetirement ?? DateTime.Parse("1900-01-01")),
                    new SqlParameter("@DateOfPermanent", (object)employee.DateOfPermanent ?? DateTime.Parse("1900-01-01")),

                    new SqlParameter("@IsSalaried", employee.IsSalaried),
                    new SqlParameter("@BasicSalary", employee.BasicSalary),
                    new SqlParameter("@CurrentSalary", employee.CurrentSalary),

                    new SqlParameter("@IsConfirmed", employee.IsConfirmed),
                    new SqlParameter("@ConfirmationDate", (object)employee.ConfirmationDate ?? DateTime.Parse("1900-01-01")),

                    new SqlParameter("@IsTerminated", employee.IsTerminated),
                    new SqlParameter("@TerminateDate", (object)employee.TerminateDate ?? DateTime.Parse("1900-01-01")),

                    new SqlParameter("@TerminateBy", employee.TerminateBy ?? ""),
                    new SqlParameter("@TerminateReason", employee.TerminateReason ?? ""),

                    new SqlParameter("@GradeId", employee.GradeId ?? ""),

                    new SqlParameter("@BankAccountNumber", (object)employee.BankAccountNumber ?? DBNull.Value),

                    new SqlParameter("@UserId", employee.UserId ?? ""),

                    new SqlParameter("@NationalIdNumber", (object)employee.NationalIdNumber ?? DBNull.Value),

                    new SqlParameter("@IsPoliceVerified", employee.IsPoliceVerified),
                    new SqlParameter("@PoliceVerificationNumber", employee.PoliceVerificationNumber ?? ""),

                    new SqlParameter("@PeferredJob", employee.PeferredJob ?? ""),

                    new SqlParameter("@IsAdminEmployee", employee.IsAdminEmployee),
                    new SqlParameter("@IsEngineer", employee.IsEngineer),
                    new SqlParameter("@IsInLPR", employee.IsInLPR),

                    new SqlParameter("@Rank", employee.Rank),

                    new SqlParameter("@CreateUser", loggedInUser)
                );
                
                                // -------------------------------------------------------------
                                // 2. CHILD TABLE 1: HrEmployeeDetail
                                // -------------------------------------------------------------
                                if (employee.EmployeeDetail != null)
                                {
                                    HrEmployeeDetailInfo detailObj = repo.Database.SqlQuery<HrEmployeeDetailInfo>(@"
                                SELECT ISNULL(MAX(EmployeeDetailId),0) + 1 AS EmployeeDetailId
                                FROM HrEmployeeDetail
                            ").FirstOrDefault();

                                    int employeeDetailId = detailObj != null ? detailObj.EmployeeDetailId : 1;

                                    repo.Database.ExecuteSqlCommand(@"
                                INSERT INTO [dbo].[HrEmployeeDetail]
                                (
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
                                    [Remarks],
                                    [CreateUser],
                                    [CreateDate],
                                    [DomainStatus]
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
                                )",
                                        new SqlParameter("@RowId", (object)employee.EmployeeDetail.RowId ?? Guid.NewGuid().ToString()),
                                        new SqlParameter("@EmployeeDetailId", employeeDetailId),
                                        new SqlParameter("@EmployeeId", targetEmployeeFk),
                                        new SqlParameter("@FathersName", (object)employee.EmployeeDetail.FathersName ?? DBNull.Value),
                                        new SqlParameter("@MothersName", (object)employee.EmployeeDetail.MothersName ?? DBNull.Value),
                                        new SqlParameter("@NumberOfChildren", (object)employee.EmployeeDetail.NumberOfChildren ?? DBNull.Value),
                                        new SqlParameter("@SpouseName", (object)employee.EmployeeDetail.SpouseName ?? DBNull.Value),
                                        new SqlParameter("@NationalIdNumber", (object)employee.EmployeeDetail.NationalIdNumber ?? DBNull.Value),
                                        new SqlParameter("@CityOfBirth", (object)employee.EmployeeDetail.CityOfBirth ?? DBNull.Value),
                                        new SqlParameter("@DistrictIdOfBirth", (object)employee.EmployeeDetail.DistrictIdOfBirth ?? DBNull.Value),
                                        new SqlParameter("@CountryIdOfBirth", (object)employee.EmployeeDetail.CountryIdOfBirth ?? DBNull.Value),
                                        new SqlParameter("@CountryIdOfCitizenship", (object)employee.EmployeeDetail.CountryIdOfCitizenship ?? DBNull.Value),
                                        new SqlParameter("@BirthMark", (object)employee.EmployeeDetail.BirthMark ?? DBNull.Value),
                                        new SqlParameter("@Height", (object)employee.EmployeeDetail.Height ?? DBNull.Value),
                                        new SqlParameter("@Weight", (object)employee.EmployeeDetail.Weight ?? DBNull.Value),
                                        new SqlParameter("@EmergencyContactPerson", (object)employee.EmployeeDetail.EmergencyContactPerson ?? DBNull.Value),
                                        new SqlParameter("@EmergenryContactNo", (object)employee.EmployeeDetail.EmergenryContactNo ?? DBNull.Value),
                                        new SqlParameter("@EmergenryContactRelation", (object)employee.EmployeeDetail.EmergenryContactRelation ?? DBNull.Value),
                                        new SqlParameter("@TotalExperience", (object)employee.EmployeeDetail.TotalExperience ?? DBNull.Value),
                                        new SqlParameter("@WillingToTravel", (object)employee.EmployeeDetail.WillingToTravel ?? DBNull.Value),
                                        new SqlParameter("@PassportNo", (object)employee.EmployeeDetail.PassportNo ?? DBNull.Value),
                                        new SqlParameter("@SecurityClearance", (object)employee.EmployeeDetail.SecurityClearance ?? DBNull.Value),
                                        new SqlParameter("@WorkAuthorizationId", (object)employee.EmployeeDetail.WorkAuthorizationId ?? DBNull.Value),
                                        new SqlParameter("@PreferedLocation", (object)employee.EmployeeDetail.PreferedLocation ?? DBNull.Value),
                                        new SqlParameter("@WillingToRelocate", (object)employee.EmployeeDetail.WillingToRelocate ?? DBNull.Value),
                                        new SqlParameter("@Remarks", (object)employee.EmployeeDetail.Remarks ?? DBNull.Value),
                                        new SqlParameter("@CreateUser", loggedInUser)
                                    );
                                } 
               
            // -------------------------------------------------------------
            // 3. CHILD TABLE 2: HrEmployeeEducation
            // -------------------------------------------------------------
            if (employee.Educations != null && employee.Educations.Any())
            {
                foreach (var edu in employee.Educations)
                {
                    // 1. Get the next available EmployeeEducationId
                    HrEmployeeEducationInfo eduObj = repo.Database.SqlQuery<HrEmployeeEducationInfo>(@"
                                SELECT ISNULL(MAX(EmployeeEducationId), 0) + 1 AS EmployeeEducationId
                                FROM HrEmployeeEducation
                     ").FirstOrDefault();

                    int employeeEducationId = eduObj != null ? eduObj.EmployeeEducationId : 1;

                    // 2. Execute SQL Insert
            repo.Database.ExecuteSqlCommand(@"
            INSERT INTO [dbo].[HrEmployeeEducation]
            (
                [RowId],
                [EmployeeEducationId],
                [EmployeeId],
                [EducationalDegreeId],
                [EducationalInstituteId],
                [Session],
                [PassingYear],
                [Board],
                [Result],
                [Rank],
                [CreateUser],
                [CreateDate],
                [DomainStatus]
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
            )",
                        new SqlParameter("@RowId", (object)edu.RowId ?? Guid.NewGuid().ToString()),
                        new SqlParameter("@EmployeeEducationId", employeeEducationId),
                        new SqlParameter("@EmployeeId", targetEmployeeFk),
                        new SqlParameter("@EducationalDegreeId", (object)edu.EducationalDegreeId ?? DBNull.Value),
                        new SqlParameter("@EducationalInstituteId", (object)edu.EducationalInstituteId ?? DBNull.Value),
                        new SqlParameter("@Session", (object)edu.Session ?? DBNull.Value),
                        new SqlParameter("@PassingYear", (object)edu.PassingYear ?? DBNull.Value),
                        new SqlParameter("@Board", (object)edu.Board ?? DBNull.Value),
                        new SqlParameter("@Result", (object)edu.Result ?? DBNull.Value),
                        new SqlParameter("@Rank", edu.Rank),
                        new SqlParameter("@CreateUser", loggedInUser)
                    );
                }
            }

                // -------------------------------------------------------------
                // 4. CHILD TABLE 3: HrEmployeeExperience
                // -------------------------------------------------------------
                if (employee.Experiences != null && employee.Experiences.Any())
                {
                    foreach (var exp in employee.Experiences)
                    {
                        // 1. Get the next available EmployeeExperienceId
                        var expObj = repo.Database.SqlQuery<int>(@"
            SELECT ISNULL(MAX(EmployeeExperienceId), 0) + 1 
            FROM HrEmployeeExperience
        ").FirstOrDefault();

                        int employeeExperienceId = expObj > 0 ? expObj : 1;

                        // 2. Execute SQL Insert
                        repo.Database.ExecuteSqlCommand(@"
            INSERT INTO [dbo].[HrEmployeeExperience]
            (
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
                [FromDate],
                [ToDate],
                [IsCurrentlyWorking],
                [Duration],
                [Responsibilities],
                [Rank],
                [DomainStatus],
                [CreateUser],
                [CreateDate]
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
                @FromDate,
                @ToDate,
                @IsCurrentlyWorking,
                @Duration,
                @Responsibilities,
                @Rank,
                1,
                @CreateUser,
                GETDATE()
            )",
                            new SqlParameter("@RowId", (object)exp.RowId ?? Guid.NewGuid().ToString()),
                            new SqlParameter("@EmployeeExperienceId", employeeExperienceId),
                            new SqlParameter("@EmployeeId", targetEmployeeFk),
                            new SqlParameter("@CompanyName", (object)exp.CompanyName ?? DBNull.Value),
                            new SqlParameter("@CompanyAddress", (object)exp.CompanyAddress ?? DBNull.Value),
                            new SqlParameter("@CompanyContactNo", (object)exp.CompanyContactNo ?? DBNull.Value),
                            new SqlParameter("@Mobile", (object)exp.Mobile ?? DBNull.Value),
                            new SqlParameter("@Email", (object)exp.Email ?? DBNull.Value),
                            new SqlParameter("@Web", (object)exp.Web ?? DBNull.Value),
                            new SqlParameter("@Designation", (object)exp.Designation ?? DBNull.Value),
                            new SqlParameter("@Department", (object)exp.Department ?? DBNull.Value),
                            new SqlParameter("@FromDate", exp.FromDate != DateTime.MinValue ? (object)exp.FromDate : DBNull.Value),
                            new SqlParameter("@ToDate", (object)exp.ToDate ?? DBNull.Value),
                            new SqlParameter("@IsCurrentlyWorking", exp.IsCurrentlyWorking),
                            new SqlParameter("@Duration", (object)exp.Duration ?? DBNull.Value),
                            new SqlParameter("@Responsibilities", (object)exp.Responsibilities ?? DBNull.Value),
                            new SqlParameter("@Rank", exp.Rank),
                            new SqlParameter("@CreateUser", loggedInUser)
                        );
                        
                            }
                }
                // -------------------------------------------------------------
                // CHILD TABLE: HrEmployeeTraining
                // -------------------------------------------------------------
                if (employee.Trainings != null && employee.Trainings.Any())
                {
                    foreach (var train in employee.Trainings)
                    {
                        // 1. Get the next available TrainingId
                        HrEmployeeTrainingInfo trainObj = repo.Database.SqlQuery<HrEmployeeTrainingInfo>(@"
                                SELECT ISNULL(MAX(TrainingId), 0) + 1 AS TrainingId
                                FROM [dbo].[HrEmployeeTraining]
                            ").FirstOrDefault();

                        int trainingId = trainObj != null ? trainObj.TrainingId : 1;

                        // 2. Execute SQL Insert
                        repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO [dbo].[HrEmployeeTraining]
                    (
                        [RowId],
                        [TrainingId],
                        [EmployeeId],
                        [TrainingSource],
                        [TrainingListId],
                        [Trainer],
                        [InstituteName],
                        [StartDate],
                        [EndDate],
                        [Duration],
                        [DomainStatus],
                        [CreateUser],
                        [CreateDate]
                    )
                    VALUES
                    (
                        @RowId,
                        @TrainingId,
                        @EmployeeId,
                        @TrainingSource,
                        @TrainingListId,
                        @Trainer,
                        @InstituteName,
                        @StartDate,
                        @EndDate,
                        @Duration,
                        1,
                        @CreateUser,
                        GETDATE()
                    )",
                                    new SqlParameter("@RowId", (object)train.RowId ?? Guid.NewGuid().ToString()),
                                    new SqlParameter("@TrainingId", trainingId),
                                    new SqlParameter("@EmployeeId", targetEmployeeFk),
                                    new SqlParameter("@TrainingSource", (object)train.TrainingSource ?? DBNull.Value),
                                    new SqlParameter("@TrainingListId", (object)train.TrainingListId ?? DBNull.Value), // or train.TrainingTitle if mapped
                                    new SqlParameter("@Trainer", (object)train.Trainer ?? DBNull.Value),
                                    new SqlParameter("@InstituteName", (object)train.InstituteName ?? DBNull.Value),
                                    new SqlParameter("@StartDate", train.StartDate != DateTime.MinValue ? (object)train.StartDate : DBNull.Value),
                                    new SqlParameter("@EndDate", (object)train.EndDate ?? DBNull.Value),
                                    new SqlParameter("@Duration", (object)train.Duration ?? DBNull.Value),
                                    new SqlParameter("@CreateUser", loggedInUser)
                                );
                    }
                }
                // -------------------------------------------------------------
                // CHILD TABLE: HrEmployeeProfessionalKnowledge
                // -------------------------------------------------------------
                if (employee.ProfessionalKnowledges != null && employee.ProfessionalKnowledges.Any())
                {
                    foreach (var knowledge in employee.ProfessionalKnowledges)
                    {
                        // 1. Generate Next Id
                        HrEmployeeProfessionalKnowledgeInfo knowledgeObj = repo.Database.SqlQuery<HrEmployeeProfessionalKnowledgeInfo>(@"
                                SELECT ISNULL(MAX(ProfessionalKnowledgeId), 0) + 1 AS ProfessionalKnowledgeId
                                FROM HrEmployeeProfessionalKnowledge
                            ").FirstOrDefault();

                        int professionalKnowledgeId = knowledgeObj != null ? knowledgeObj.ProfessionalKnowledgeId : 1;

                        // 2. Execute Insert Command
                        repo.Database.ExecuteSqlCommand(@"
                            INSERT INTO [dbo].[HrEmployeeProfessionalKnowledge]
                            (
                                [RowId],
                                [ProfessionalKnowledgeId],
                                [EmployeeId],
                                [SkillId],
                                [CreateUser],
                                [CreateDate],
                                [DomainStatus]
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
                            )",
                                            new SqlParameter("@RowId", (object)knowledge.RowId ?? Guid.NewGuid().ToString()),
                                            new SqlParameter("@ProfessionalKnowledgeId", professionalKnowledgeId),
                                            new SqlParameter("@EmployeeId", targetEmployeeFk),
                                            new SqlParameter("@SkillId", (object)knowledge.SkillId ?? DBNull.Value),
                                            new SqlParameter("@CreateUser", loggedInUser)
                                        );
                    }
                }
                // -------------------------------------------------------------
                // CHILD TABLE: HrEmployeeReference
                // -------------------------------------------------------------
                if (employee.References != null && employee.References.Any())
                {
                    foreach (var reference in employee.References)
                    {
                        // 1. Generate Next EmployeeReferenceId
                        int nextReferenceId = repo.Database.SqlQuery<int>(@"
            SELECT ISNULL(MAX(EmployeeReferenceId), 0) + 1 
            FROM HrEmployeeReference
        ").FirstOrDefault();

                        // 2. Execute Insert Command
                        repo.Database.ExecuteSqlCommand(@"
            INSERT INTO [dbo].[HrEmployeeReference]
            (
                [RowId],
                [EmployeeReferenceId],
                [EmployeeId],
                [ReferenceName],
                [Occupation],
                [Designation],
                [Company],
                [Relation],
                [ContactNo],
                [PresentAddress],
                [PermanentAddress],
                [Email],
                [NId],
                [Web],
                [CreateUser],
                [CreateDate],
                [DomainStatus]
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
            )",
                            new SqlParameter("@RowId", (object)reference.RowId ?? Guid.NewGuid().ToString()),
                            new SqlParameter("@EmployeeReferenceId", nextReferenceId),
                            new SqlParameter("@EmployeeId", targetEmployeeFk),
                            new SqlParameter("@ReferenceName", (object)reference.ReferenceName ?? DBNull.Value),
                            new SqlParameter("@Occupation", (object)reference.Occupation ?? DBNull.Value),
                            new SqlParameter("@Designation", (object)reference.Designation ?? DBNull.Value),
                            new SqlParameter("@Company", (object)reference.Company ?? DBNull.Value),
                            new SqlParameter("@Relation", (object)reference.Relation ?? DBNull.Value),
                            new SqlParameter("@ContactNo", reference.ContactNo),
                            new SqlParameter("@PresentAddress", (object)reference.PresentAddress ?? DBNull.Value),
                            new SqlParameter("@PermanentAddress", (object)reference.PermanentAddress ?? DBNull.Value),
                            new SqlParameter("@Email", (object)reference.Email ?? DBNull.Value),
                            new SqlParameter("@NId", (object)reference.NId ?? DBNull.Value),
                            new SqlParameter("@Web", (object)reference.Web ?? DBNull.Value),
                            new SqlParameter("@CreateUser", loggedInUser)
                        );
                    }
                }
                // -------------------------------------------------------------
                // CHILD TABLE: HrEmployeePicture (Database Save / Upsert)
                // -------------------------------------------------------------
                // =========================================================================
                // 1. PROCESS EMPLOYEE PICTURE & SIGNATURE (UPSERT)
                // =========================================================================
                if (employee.EmployeePicture != null)
                {
                    var pictureInfo = employee.EmployeePicture;

                    bool hasDataToSave = pictureInfo.Picture != null ||
                                         pictureInfo.Signature != null ||
                                         !string.IsNullOrEmpty(pictureInfo.PicturePath) ||
                                         !string.IsNullOrEmpty(pictureInfo.SignaturePath);

                    if (hasDataToSave)
                    {
                        // Check if a picture record already exists for this Employee
                        string existingRowId = repo.Database.SqlQuery<string>(@"
                    SELECT TOP 1 RowId 
                    FROM HrEmployeePicture 
                    WHERE EmployeeId = @EmployeeId OR (RowId = @RowId AND ISNULL(@RowId, '') <> '')",
                            new SqlParameter("@EmployeeId", (object)targetEmployeeFk ?? DBNull.Value),
                            new SqlParameter("@RowId", (object)pictureInfo.RowId ?? DBNull.Value)
                        ).FirstOrDefault();

                        if (!string.IsNullOrEmpty(existingRowId))
                        {
                            // UPDATE existing record without overwriting unprovided fields
                            var pPicture = new SqlParameter("@Picture", System.Data.SqlDbType.VarBinary, -1) { Value = (object)pictureInfo.Picture ?? DBNull.Value };
                            var pPicturePath = new SqlParameter("@PicturePath", (object)pictureInfo.PicturePath ?? DBNull.Value);
                            var pSignature = new SqlParameter("@Signature", System.Data.SqlDbType.VarBinary, -1) { Value = (object)pictureInfo.Signature ?? DBNull.Value };
                            var pSignaturePath = new SqlParameter("@SignaturePath", (object)pictureInfo.SignaturePath ?? DBNull.Value);
                            var pUpdateUser = new SqlParameter("@UpdateUser", (object)loggedInUser ?? DBNull.Value);
                            var pRowId = new SqlParameter("@RowId", existingRowId);

                            repo.Database.ExecuteSqlCommand(@"
                        UPDATE [dbo].[HrEmployeePicture]
                        SET 
                            [Picture]       = CASE WHEN @Picture IS NOT NULL THEN @Picture ELSE [Picture] END,
                            [PicturePath]   = CASE WHEN @PicturePath IS NOT NULL AND @PicturePath <> '' THEN @PicturePath ELSE [PicturePath] END,
                            [Signature]     = CASE WHEN @Signature IS NOT NULL THEN @Signature ELSE [Signature] END,
                            [SignaturePath] = CASE WHEN @SignaturePath IS NOT NULL AND @SignaturePath <> '' THEN @SignaturePath ELSE [SignaturePath] END,
                            [UpdateUser]    = @UpdateUser,
                            [UpdateDate]    = GETDATE()
                        WHERE [RowId] = @RowId",
                                pPicture,
                                pPicturePath,
                                pSignature,
                                pSignaturePath,
                                pUpdateUser,
                                pRowId
                            );
                        }
                        else
                        {
                            // INSERT new picture record
                            int nextPictureId = repo.Database.SqlQuery<int>(@"
                        SELECT ISNULL(MAX(HrEmployeePictureId), 0) + 1 
                        FROM HrEmployeePicture
                    ").FirstOrDefault();

                            string newRowId = !string.IsNullOrEmpty(pictureInfo.RowId)
                                              ? pictureInfo.RowId
                                              : Guid.NewGuid().ToString();

                            var pRowId = new SqlParameter("@RowId", newRowId);
                            var pHrEmployeePictureId = new SqlParameter("@HrEmployeePictureId", nextPictureId);
                            var pEmployeeId = new SqlParameter("@EmployeeId", (object)targetEmployeeFk ?? DBNull.Value);
                            var pPicture = new SqlParameter("@Picture", System.Data.SqlDbType.VarBinary, -1) { Value = (object)pictureInfo.Picture ?? DBNull.Value };
                            var pPicturePath = new SqlParameter("@PicturePath", (object)pictureInfo.PicturePath ?? DBNull.Value);
                            var pSignature = new SqlParameter("@Signature", System.Data.SqlDbType.VarBinary, -1) { Value = (object)pictureInfo.Signature ?? DBNull.Value };
                            var pSignaturePath = new SqlParameter("@SignaturePath", (object)pictureInfo.SignaturePath ?? DBNull.Value);
                            var pCreateUser = new SqlParameter("@CreateUser", (object)loggedInUser ?? DBNull.Value);

                            repo.Database.ExecuteSqlCommand(@"
                        INSERT INTO [dbo].[HrEmployeePicture]
                        (
                            [RowId],
                            [HrEmployeePictureId],
                            [EmployeeId],
                            [Picture],
                            [PicturePath],
                            [Signature],
                            [SignaturePath],
                            [CreateUser],
                            [CreateDate],
                            [DomainStatus]
                        )
                        VALUES
                        (
                            @RowId,
                            @HrEmployeePictureId,
                            @EmployeeId,
                            @Picture,
                            @PicturePath,
                            @Signature,
                            @SignaturePath,
                            @CreateUser,
                            GETDATE(),
                            1
                        )",
                                pRowId,
                                pHrEmployeePictureId,
                                pEmployeeId,
                                pPicture,
                                pPicturePath,
                                pSignature,
                                pSignaturePath,
                                pCreateUser
                            );
                        }
                    }
                }
                        ////////End
                    }
        }

        public static void UpdateHrEmployee(HrEmployeeInfo employee, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE HrEmployee
                    SET
                        StaffCode = @StaffCode, FirstName = @FirstName, MiddleName = @MiddleName, LastName = @LastName,
                        PresentAddress = @PresentAddress, PermanentAddress = @PermanentAddress, BloodGroupId = @BloodGroupId,
                        DateOfBirth = @DateOfBirth, NationalityId = @NationalityId, HomePhone = @HomePhone, CellPhone = @CellPhone,
                        OfficePhone = @OfficePhone, PrimaryEmail = @PrimaryEmail, AlternateEmail = @AlternateEmail,
                        Gender = @Gender, ReligionId = @ReligionId, MaritalStatus = @MaritalStatus, DateOfJoin = @DateOfJoin,
                        DateOfRetirement = @DateOfRetirement, DateOfPermanent = @DateOfPermanent, CompanyId = @CompanyId,
                        DivisionId = @DivisionId, BranchId = @BranchId, DepartmentId = @DepartmentId, SectionId = @SectionId,
                        DesignationId = @DesignationId, IsSalaried = @IsSalaried, BasicSalary = @BasicSalary,
                        CurrentSalary = @CurrentSalary, IsConfirmed = @IsConfirmed, ConfirmationDate = @ConfirmationDate,
                        IsTerminated = @IsTerminated, TerminateDate = @TerminateDate, TerminateBy = @TerminateBy,
                        TerminateReason = @TerminateReason, EmployeeTypeId = @EmployeeTypeId, GradeId = @GradeId,
                        BankAccountNumber = @BankAccountNumber, UserId = @UserId, NationalIdNumber = @NationalIdNumber,
                        IsPoliceVerified = @IsPoliceVerified, PoliceVerificationNumber = @PoliceVerificationNumber,
                        PeferredJob = @PeferredJob, JobType = @JobType, EngineerTypeId = @EngineerTypeId,
                        IsAdminEmployee = @IsAdminEmployee, IsEngineer = @IsEngineer, Rank = @Rank,
                        UpdateUser = @UpdateUser, UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", employee.RowId),
                new SqlParameter("@StaffCode", employee.StaffCode ?? ""),
                new SqlParameter("@FirstName", employee.FirstName ?? ""),
                new SqlParameter("@MiddleName", employee.MiddleName ?? ""),
                new SqlParameter("@LastName", employee.LastName ?? ""),
                new SqlParameter("@PresentAddress", employee.PresentAddress ?? ""),
                new SqlParameter("@PermanentAddress", employee.PermanentAddress ?? ""),
                new SqlParameter("@BloodGroupId", employee.BloodGroupId ?? ""),
                new SqlParameter("@DateOfBirth", employee.DateOfBirth),
                new SqlParameter("@NationalityId", employee.NationalityId ?? ""),
                new SqlParameter("@HomePhone", employee.HomePhone),
                new SqlParameter("@CellPhone", employee.CellPhone),
                new SqlParameter("@OfficePhone", employee.OfficePhone),
                new SqlParameter("@PrimaryEmail", employee.PrimaryEmail ?? ""),
                new SqlParameter("@AlternateEmail", employee.AlternateEmail ?? ""),
                new SqlParameter("@Gender", employee.Gender ?? ""),
                new SqlParameter("@ReligionId", employee.ReligionId ?? ""),
                new SqlParameter("@MaritalStatus", employee.MaritalStatus ?? ""),
                new SqlParameter("@DateOfJoin", employee.DateOfJoin),
                new SqlParameter("@DateOfRetirement", employee.DateOfRetirement),
                new SqlParameter("@DateOfPermanent", employee.DateOfPermanent),
                new SqlParameter("@CompanyId", employee.CompanyId ?? DBNull.Value.ToString()),
                new SqlParameter("@DivisionId", employee.DivisionId ?? DBNull.Value.ToString()),
                new SqlParameter("@BranchId", employee.BranchId ?? DBNull.Value.ToString()),
                new SqlParameter("@DepartmentId", employee.DepartmentId ?? DBNull.Value.ToString()),
                new SqlParameter("@SectionId", employee.SectionId ?? DBNull.Value.ToString()),
                new SqlParameter("@DesignationId", employee.DesignationId ?? DBNull.Value.ToString()),
                new SqlParameter("@IsSalaried", employee.IsSalaried),
                new SqlParameter("@BasicSalary", employee.BasicSalary),
                new SqlParameter("@CurrentSalary", employee.CurrentSalary),
                new SqlParameter("@IsConfirmed", employee.IsConfirmed),
                new SqlParameter("@ConfirmationDate", employee.ConfirmationDate),
                new SqlParameter("@IsTerminated", employee.IsTerminated),
                new SqlParameter("@TerminateDate", (object)employee.TerminateDate ?? DBNull.Value),
                new SqlParameter("@TerminateBy", employee.TerminateBy ?? ""),
                new SqlParameter("@TerminateReason", employee.TerminateReason ?? ""),
                new SqlParameter("@EmployeeTypeId", employee.EmployeeTypeId ?? DBNull.Value.ToString()),
                new SqlParameter("@GradeId", employee.GradeId ?? DBNull.Value.ToString()),
                new SqlParameter("@BankAccountNumber", employee.BankAccountNumber),
                new SqlParameter("@UserId", employee.UserId ?? ""),
                new SqlParameter("@NationalIdNumber", employee.NationalIdNumber),
                new SqlParameter("@IsPoliceVerified", employee.IsPoliceVerified),
                new SqlParameter("@PoliceVerificationNumber", employee.PoliceVerificationNumber ?? ""),
                new SqlParameter("@PeferredJob", employee.PeferredJob ?? ""),
                new SqlParameter("@JobType", employee.JobType ?? ""),
                new SqlParameter("@EngineerTypeId", employee.EngineerTypeId ?? DBNull.Value.ToString()),
                new SqlParameter("@IsAdminEmployee", employee.IsAdminEmployee),
                new SqlParameter("@IsEngineer", employee.IsEngineer),
                new SqlParameter("@Rank", employee.Rank),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static void DeleteHrEmployee(string RowId, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE HrEmployee
                    SET
                        DomainStatus = 0,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", RowId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static HrEmployeeResponse GetFilteredHrEmployees(HttpRequestBase Request)
        {
            HrEmployeeResponse resp = new HrEmployeeResponse();
            GMDataTableDAL gmdt = new GMDataTableDAL(Request);

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
            FROM HrEmployee e
            LEFT JOIN HrCompanyInformation c ON c.RowId = e.CompanyId
            LEFT JOIN HrDepartment d ON d.RowId = e.DepartmentId
            LEFT JOIN HrDesignation deg ON deg.RowId = e.DesignationId
            WHERE e.DomainStatus = 1
        ");

                List<SqlParameter> countParameters = new List<SqlParameter>();
                List<SqlParameter> dataParameters = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.Append(@"
                AND (
                    e.FirstName LIKE '%' + @SearchValue + '%'
                    OR e.LastName LIKE '%' + @SearchValue + '%'
                    OR e.StaffCode LIKE '%' + @SearchValue + '%'
                    OR c.CompanyName LIKE '%' + @SearchValue + '%'
                )");

                    // Add to both lists since both queries will require the search string
                    countParameters.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));
                    dataParameters.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));
                }

                // 1. Total Record Count
                resp.TotalRecordCount = repo.Database.SqlQuery<int>(
                    "SELECT COUNT(*) FROM HrEmployee WHERE DomainStatus = 1"
                ).Single();

                // 2. Filtered Record Count (Using its own dedicated parameter array)
                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(
                    "SELECT COUNT(*) " + sb.ToString(),
                    countParameters.ToArray()
                ).Single();

                // 3. Setup Pagination Parameters for the main data query
                dataParameters.Add(new SqlParameter("@Skip", gmdt.Start));
                dataParameters.Add(new SqlParameter("@Take", gmdt.Length));

                // Prefix sorting column properly if it maps directly to table fields
                string sortColumn = gmdt.SortColum.ToString(); ; // Watch out for this typo in your class property ("SortColum")
                if (sortColumn == "RowId" || sortColumn == "Rank")
                {
                    sortColumn = "e." + sortColumn;
                }

                // 4. Fetch Paginated Records
                resp.HrEmployees = repo.Database.SqlQuery<HrEmployeeInfo>(@"
            SELECT
                e.RowId, e.EmployeeId, e.StaffCode, e.FirstName, e.MiddleName, e.LastName,
                e.CellPhone, e.PrimaryEmail, e.DateOfJoin, e.CurrentSalary, e.Rank,
                c.CompanyName, d.DepartmentName, deg.DesignationName
            " + sb.ToString() + @"
            ORDER BY " + sortColumn + " " + gmdt.SortDir + @"
            OFFSET @Skip ROWS
            FETCH NEXT @Take ROWS ONLY
            ",
                    dataParameters.ToArray()).ToList();
            }

            return resp;
        }
    }

    // Supporting Response wrapper to mimic your pattern 
    public class HrEmployeeResponse
    {
        public int TotalRecordCount { get; set; }
        public int FilteredRecordCount { get; set; }
        public List<HrEmployeeInfo> HrEmployees { get; set; }
    }
}