using ERP.Models.HR.Organization.HrApprovedPosition;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.HR.Common
{
    public class HrApprovedPositionDAL
    {
        public static List<HrApprovedPositionInfo> GetDivisionByCompanyId(string CompanyId)
        {
            using (var repo = new GMContext())
            {
                return repo.Database.SqlQuery<HrApprovedPositionInfo>(@"
            SELECT
                RowId,
                DivisionName
            FROM HrDivision
            WHERE ParentId = @ParentId
            AND DomainStatus = 1
            ORDER BY [Rank]
        ",
                new SqlParameter("@ParentId", CompanyId))
                .ToList();
            }
        }
        public static List<HrApprovedPositionInfo> GetGetBranchByDivisionId(string DivisionId)
        {
            using (var repo = new GMContext())
            {
                return repo.Database.SqlQuery<HrApprovedPositionInfo>(@"
            SELECT
                RowId,
                BranchName
            FROM HrBranch
            WHERE DivisionId = @DivisionId
            AND DomainStatus = 1
            ORDER BY [Rank]
        ",
                new SqlParameter("@DivisionId", DivisionId))
                .ToList();
            }
        }
        public static List<HrApprovedPositionInfo> GetDepartmentByBranchId(string BranchId)
        {
            using (var repo = new GMContext())
            {
                return repo.Database.SqlQuery<HrApprovedPositionInfo>(@"
            SELECT
                RowId,
                DepartmentName
            FROM HrDepartment
            WHERE BranchId = @BranchId
            AND DomainStatus = 1
            ORDER BY [Rank]
        ",
                new SqlParameter("@BranchId", BranchId))
                .ToList();
            }
        }
        public static List<HrApprovedPositionInfo> GetDesignationByGroupId(string groupId)
        {
            using (var repo = new GMContext())
            {
                // Explicitly defining the raw sql parameter array
                var param = new SqlParameter("@DesignationGroupId", groupId ?? (object)System.DBNull.Value);

                return repo.Database.SqlQuery<HrApprovedPositionInfo>(@"
            SELECT
                RowId,
                DesignationName
            FROM HrDesignation
            WHERE DesignationGroupId = @DesignationGroupId
              AND DomainStatus = 1
            ORDER BY [Rank]
        ", param).ToList();
            }
        }
        
        
        public static HrApprovedPositionInfo GetApprovedPositionByID(string RowId)
        {
            HrApprovedPositionInfo position = null;

            using (var repo = new GMContext())
            {
                position = repo.Database.SqlQuery<HrApprovedPositionInfo>(@"

                    SELECT
                        a.RowId,
                        a.PositionId,
                        a.CompanyId,
                        a.BranchId,
                        a.MainDivisionId,
                        a.DivisionId,
                        a.DepartmentId,
                        a.SectionId,
                        a.EmployeeTypeId,
                        a.DesignationGroupId,
                        a.DesignationId,
                        a.ApprovedPost,
                        a.PositionRank,

                        b.CompanyName,
                        c.BranchName,
                        d.MainDivisionName,
                        e.DivisionName,
                        f.DepartmentName,
                        g.SectionName,
                        h.EmployeeTypeName,
                        i.DesignationGroup,
                        j.DesignationName

                    FROM HrApprovedPosition a
                    LEFT JOIN HrCompanyInformation b ON b.RowId = a.CompanyId
                    LEFT JOIN HrBranch c ON c.RowId = a.BranchId
                    LEFT JOIN HrMainDivision d ON d.RowId = a.MainDivisionId
                    LEFT JOIN HrDivision e ON e.RowId = a.DivisionId
                    LEFT JOIN HrDepartment f ON f.RowId = a.DepartmentId
                    LEFT JOIN HrSection g ON g.RowId = a.SectionId
                    LEFT JOIN HrEmployeeType h ON h.RowId = a.EmployeeTypeId
                    LEFT JOIN HrDesignationGroup i ON i.RowId = a.DesignationGroupId
                    LEFT JOIN HrDesignation j ON j.RowId = a.DesignationId

                    WHERE a.RowId = @RowId
                    AND a.DomainStatus = 1

                ", new SqlParameter("@RowId", RowId))
                .FirstOrDefault();
            }

            return position;
        }
        public static HrApprovedPositionInfo GetAllPosition()
        {
            HrApprovedPositionInfo position = null;
            using (var repo = new GMContext())
            {
                position = repo.Database.SqlQuery<HrApprovedPositionInfo>(@"
            SELECT [RowId]
                  ,[PositionId]  
                  ,[ApprovedPost]
                  ,(SELECT ISNULL(MAX([PositionRank]), 0) + 1 FROM [HrApprovedPosition]) AS NextPositionRank
              FROM [HrApprovedPosition]
              where [DomainStatus] = 1;
        ").FirstOrDefault();
            }
            return position;
        }
        public static int GetNextPositionRank(string companyId, string branchId, string departmentId)
        {
            using (var repo = new GMContext())
            {
                // Query looks for the highest existing rank within the identical structural boundary
                int maxRank = repo.Database.SqlQuery<int?>(@"
            SELECT MAX(PositionRank) 
            FROM HrApprovedPosition 
            WHERE CompanyId = @CompanyId 
              AND BranchId = @BranchId 
              AND DepartmentId = @DepartmentId
              AND DomainStatus = 1
        ",
                new SqlParameter("@CompanyId", companyId ?? (object)DBNull.Value),
                new SqlParameter("@BranchId", branchId ?? (object)DBNull.Value),
                new SqlParameter("@DepartmentId", departmentId ?? (object)DBNull.Value)).FirstOrDefault() ?? 0;

                // If max is 0 (no positions exist yet), the next rank starts at 1
                return maxRank + 1;
            }
        }
        public static List<HrApprovedPositionInfo> GetAllHrApprovedPositions()
        {
            List<HrApprovedPositionInfo> positions = null;

            using (var repo = new GMContext())
            {
                positions = repo.Database.SqlQuery<HrApprovedPositionInfo>(@"

                    SELECT
                        a.RowId,
                        a.PositionId,
                        a.ApprovedPost,
                        a.PositionRank,

                        b.CompanyName,
                        c.BranchName,
                        e.DivisionName,
                        f.DepartmentName,
                        h.EmployeeTypeName,
                        j.DesignationName

                    FROM HrApprovedPosition a
                    LEFT JOIN HrCompanyInformation b ON b.RowId = a.CompanyId
                    LEFT JOIN HrBranch c ON c.RowId = a.BranchId
                    LEFT JOIN HrDivision e ON e.RowId = a.DivisionId
                    LEFT JOIN HrDepartment f ON f.RowId = a.DepartmentId
                    LEFT JOIN HrEmployeeType h ON h.RowId = a.EmployeeTypeId
                    LEFT JOIN HrDesignation j ON j.RowId = a.DesignationId

                    WHERE a.DomainStatus = 1
                    ORDER BY a.PositionId

                ").ToList();
            }

            return positions;
        }

        public static void AddHrApprovedPosition(
            HrApprovedPositionInfo position,
            string loggedInUser)
        {
            HrApprovedPositionInfo obj;

            using (var repo = new GMContext())
            {
                obj = repo.Database.SqlQuery<HrApprovedPositionInfo>(@"
                    SELECT ISNULL(MAX(PositionId),0)+1 AS PositionId
                    FROM HrApprovedPosition
                    WHERE DomainStatus = 1
                ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"

                    INSERT INTO HrApprovedPosition
                    (
                        RowId,
                        PositionId,
                        CompanyId,
                        BranchId,
                        MainDivisionId,
                        DivisionId,
                        DepartmentId,
                        SectionId,
                        EmployeeTypeId,
                        DesignationGroupId,
                        DesignationId,
                        ApprovedPost,
                        PositionRank,
                        CreateUser,
                        CreateDate,
                        DomainStatus
                    )
                    VALUES
                    (
                        @RowId,
                        @PositionId,
                        @CompanyId,
                        @BranchId,
                        @MainDivisionId,
                        @DivisionId,
                        @DepartmentId,
                        @SectionId,
                        @EmployeeTypeId,
                        @DesignationGroupId,
                        @DesignationId,
                        @ApprovedPost,
                        @PositionRank,
                        @CreateUser,
                        GETDATE(),
                        1
                    )

                ",
                new SqlParameter("@RowId", position.RowId),
                new SqlParameter("@PositionId", obj.PositionId),
                new SqlParameter("@CompanyId", position.CompanyId ?? ""),
                new SqlParameter("@BranchId", position.BranchId ?? ""),
                new SqlParameter("@MainDivisionId", position.MainDivisionId ?? ""),
                new SqlParameter("@DivisionId", position.DivisionId ?? ""),
                new SqlParameter("@DepartmentId", position.DepartmentId ?? ""),
                new SqlParameter("@SectionId", position.SectionId ?? ""),
                new SqlParameter("@EmployeeTypeId", position.EmployeeTypeId ?? ""),
                new SqlParameter("@DesignationGroupId", position.DesignationGroupId ?? ""),
                new SqlParameter("@DesignationId", position.DesignationId ?? ""),
                new SqlParameter("@ApprovedPost", position.ApprovedPost),
                new SqlParameter("@PositionRank", position.PositionRank),
                new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        public static void UpdateHrApprovedPosition(HrApprovedPositionInfo position, string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
            UPDATE HrApprovedPosition
            SET
                CompanyId = @CompanyId,
                BranchId = @BranchId,
                MainDivisionId = @MainDivisionId,
                DivisionId = @DivisionId,
                DepartmentId = @DepartmentId,
                SectionId = @SectionId,
                EmployeeTypeId = @EmployeeTypeId,
                DesignationGroupId = @DesignationGroupId,
                DesignationId = @DesignationId,
                ApprovedPost = @ApprovedPost,
                PositionRank = @PositionRank,
                UpdateUser = @UpdateUser,
                UpdateDate = GETDATE()
            WHERE RowId = @RowId
        ",
                new SqlParameter("@RowId", (object)position.RowId ?? DBNull.Value),
                new SqlParameter("@CompanyId", (object)position.CompanyId ?? DBNull.Value),
                new SqlParameter("@BranchId", (object)position.BranchId ?? DBNull.Value),
                new SqlParameter("@MainDivisionId", (object)position.MainDivisionId ?? DBNull.Value),
                new SqlParameter("@DivisionId", (object)position.DivisionId ?? DBNull.Value),
                new SqlParameter("@DepartmentId", (object)position.DepartmentId ?? DBNull.Value),
                new SqlParameter("@SectionId", (object)position.SectionId ?? DBNull.Value),
                new SqlParameter("@EmployeeTypeId", (object)position.EmployeeTypeId ?? DBNull.Value),
                new SqlParameter("@DesignationGroupId", (object)position.DesignationGroupId ?? DBNull.Value),
                new SqlParameter("@DesignationId", (object)position.DesignationId ?? DBNull.Value),
                new SqlParameter("@ApprovedPost", (object)position.ApprovedPost ?? DBNull.Value),
                new SqlParameter("@PositionRank", (object)position.PositionRank ?? DBNull.Value),
                new SqlParameter("@UpdateUser", (object)loggedInUser ?? DBNull.Value));
            }
        }

        public static void DeleteHrApprovedPosition(
            HrApprovedPositionInfo position,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"

                    UPDATE HrApprovedPosition
                    SET
                        DomainStatus = 0,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId

                ",
                new SqlParameter("@RowId", position.RowId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static HrApprovedPositionResponse
            GetFilteredHrApprovedPositions(HttpRequestBase Request)
        {
            HrApprovedPositionResponse resp =
                new HrApprovedPositionResponse();

            GMDataTableDAL gmdt =
                new GMDataTableDAL(Request);

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"

                    FROM HrApprovedPosition a

                    LEFT JOIN HrCompanyInformation b
                        ON b.RowId = a.CompanyId

                   -- LEFT JOIN HrBranch c ON c.RowId = a.BranchId

                    LEFT JOIN HrDivision d
                        ON d.RowId = a.DivisionId

                    LEFT JOIN HrDepartment e
                        ON e.RowId = a.DepartmentId

                   -- LEFT JOIN HrEmployeeType f ON f.RowId = a.EmployeeTypeId

                    LEFT JOIN HrDesignation g
                        ON g.RowId = a.DesignationId

                    WHERE a.DomainStatus = 1

                ");

                List<SqlParameter> parameters =
                    new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.Append(@"

                        AND
                        (
                            b.CompanyName LIKE '%' + @SearchValue + '%'
                            OR d.DivisionName LIKE '%' + @SearchValue + '%'
                         --   OR c.BranchName LIKE '%' + @SearchValue + '%'
                            OR e.DepartmentName LIKE '%' + @SearchValue + '%'
                         --   OR f.EmployeeTypeName LIKE '%' + @SearchValue + '%'
                            OR g.DesignationName LIKE '%' + @SearchValue + '%'
                        )

                    ");

                    parameters.Add(
                        new SqlParameter(
                            "@SearchValue",
                            gmdt.SearchValue));
                }

                resp.TotalRecordCount =
                    repo.Database.SqlQuery<int>(@"
                        SELECT COUNT(*)
                        FROM HrApprovedPosition
                        WHERE DomainStatus = 1
                    ").Single();

                resp.FilteredRecordCount =
                    repo.Database.SqlQuery<int>(
                        "SELECT COUNT(*) " + sb.ToString(),
                        parameters.ToArray()
                    ).Single();

                parameters.Add(
                    new SqlParameter("@Skip", gmdt.Start));

                parameters.Add(
                    new SqlParameter("@Take", gmdt.Length));

                resp.HrApprovedPositions =
                    repo.Database.SqlQuery<HrApprovedPositionInfo>(@"

                    SELECT
                        a.RowId,
                        a.PositionId,
                        b.CompanyName,
                        d.DivisionName,
                      --  c.BranchName,
                        e.DepartmentName,
                       -- f.EmployeeTypeName,
                        g.DesignationName,
                        a.ApprovedPost,
                        a.PositionRank

                    " + sb.ToString() + @"

                    ORDER BY " + gmdt.SortColum + " " + gmdt.SortDir + @"

                    OFFSET @Skip ROWS
                    FETCH NEXT @Take ROWS ONLY

                    ",
                    parameters.ToArray())
                    .ToList();
            }

            return resp;
        }
    }
}