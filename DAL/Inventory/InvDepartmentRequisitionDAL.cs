using ERP.Models.Inventory.InvDepartmentRequisition;
using ERP.Models.Inventory.InvDepartmentRequisitionDetail;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.Inventory
{
    public class InvDepartmentRequisitionDAL
    {
        // ---------------------------------------------------------------
        // Single record (Edit / Delete screens)
        // ---------------------------------------------------------------
        public static InvDepartmentRequisitionInfo GetRequisitionByID(string RowId)
        {
            InvDepartmentRequisitionInfo requisition = null;

            using (var repo = new GMContext())
            {
                requisition = repo.Database.SqlQuery<InvDepartmentRequisitionInfo>(@"
                    SELECT
                         a.RowId
                        ,a.RequisitionId
                        ,a.RequisitionDate
                        ,a.RequisitionCode
                        ,a.RequisitionName
                        ,a.DepartmentId
                        ,b.DepartmentName
                        ,a.RequestedBy
                        ,a.JobId
                        ,a.JobNo
                        ,a.RequiredFor
                        ,a.Status
                        ,a.IsApproved
                        ,a.IsIssued
                        ,a.IsRemoved
                        ,c.LastName AS RequestedByName
                        ,d.JobName
                    FROM InvDepartmentRequisition a
                    LEFT JOIN HrDepartment b ON b.RowId = a.DepartmentId
                    LEFT JOIN HrEmployee c ON c.RowId = a.RequestedBy
                    LEFT JOIN InvJob d ON d.RowId = a.JobId
                    WHERE a.RowId = @RowId
                    AND a.DomainStatus = 1
                ",
                new SqlParameter("@RowId", RowId))
                .FirstOrDefault();

                if (requisition != null)
                    requisition.Details = GetRequisitionDetailsByRequisitionId(requisition.RowId);
            }

            return requisition;
        }

        public static List<InvDepartmentRequisitionDetailInfo> GetRequisitionDetailsByRequisitionId(string RequisitionId)
        {
            List<InvDepartmentRequisitionDetailInfo> details = null;

            using (var repo = new GMContext())
            {
                details = repo.Database.SqlQuery<InvDepartmentRequisitionDetailInfo>(@"
                    SELECT
                         a.RowId
                        ,a.RequisitionDetailId
                        ,a.RequisitionId
                        ,a.ItemId
                        ,a.Quantity
                        ,a.UnitId
                        ,a.UnitPriceEst
                        ,a.TotalPriceEst
                        ,a.ApprovedQuantity
                        ,a.Remarks
                        ,b.ItemName
                        ,c.MinUnitName
                    FROM InvDepartmentRequisitionDetail a
                    INNER JOIN InvItem b ON b.RowId = a.ItemId
                    INNER JOIN InvUnit c ON c.RowId = a.UnitId
                    WHERE a.RequisitionId = @RequisitionId
                    ORDER BY a.RequisitionDetailId
                ",
                new SqlParameter("@RequisitionId", RequisitionId))
                .ToList();
            }

            return details;
        }

        public static InvDepartmentRequisitionInfo GetHrRequisitionByCode(
            string RequisitionCode,
            string RowId)
        {
            InvDepartmentRequisitionInfo requisition = null;

            using (var repo = new GMContext())
            {
                requisition = repo.Database.SqlQuery<InvDepartmentRequisitionInfo>(@"
                    SELECT *
                    FROM InvDepartmentRequisition
                    WHERE RequisitionCode = @RequisitionCode
                    AND (@RowId IS NULL OR RowId <> @RowId)
                    AND IsRemoved = 0
                ",
                new SqlParameter("@RequisitionCode", RequisitionCode),
                new SqlParameter("@RowId", (object)RowId ?? DBNull.Value))
                .FirstOrDefault();
            }

            return requisition;
        }

        // ---------------------------------------------------------------
        // Next display code, e.g. REQ-0006, shown read-only on the Add form
        // ---------------------------------------------------------------
        public static string GetNextRequisitionCode()
        {
            using (var repo = new GMContext())
            {
                int nextId = repo.Database.SqlQuery<int>(@"
                    SELECT ISNULL(MAX(RequisitionId),0)+1
                    FROM InvDepartmentRequisition
                ").Single();

                return "REQ-" + nextId.ToString("D4");
            }
        }

        // ---------------------------------------------------------------
        // Add
        // ---------------------------------------------------------------
        public static void AddRequisition(
            InvDepartmentRequisitionInfo requisition,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            using (var dbTransaction = repo.Database.BeginTransaction())
            {
                try
                {
                    InvDepartmentRequisitionInfo obj = repo.Database.SqlQuery<InvDepartmentRequisitionInfo>(@"
                        SELECT
                            ISNULL(MAX(RequisitionId),0)+1 AS RequisitionId
                        FROM InvDepartmentRequisition
                    ").FirstOrDefault();

                    repo.Database.ExecuteSqlCommand(@"
                        INSERT INTO InvDepartmentRequisition
                        (
                            RowId,
                            RequisitionId,
                            RequisitionDate,
                            RequisitionCode,
                            RequisitionName,
                            DepartmentId,
                            RequestedBy,
                            JobId,
                            JobNo,
                            RequiredFor,
                            Status,
                            IsApproved,
                            IsIssued,
                            IsRemoved,
                            CreateUser,
                            CreateDate,
                            DomainStatus
                        )
                        VALUES
                        (
                            @RowId,
                            @RequisitionId,
                            @RequisitionDate,
                            @RequisitionCode,
                            @RequisitionName,
                            @DepartmentId,
                            @RequestedBy,
                            @JobId,
                            @JobNo,
                            @RequiredFor,
                            1,
                            0,
                            0,
                            0,
                            @CreateUser,
                            GETDATE(),
                            1
                        )
                    ",
                    new SqlParameter("@RowId", requisition.RowId),
                    new SqlParameter("@RequisitionId", obj.RequisitionId),
                    new SqlParameter("@RequisitionDate", requisition.RequisitionDate),
                    new SqlParameter("@RequisitionCode", requisition.RequisitionCode),
                    new SqlParameter("@RequisitionName", requisition.RequisitionName),
                    new SqlParameter("@DepartmentId", requisition.DepartmentId),
                    new SqlParameter("@RequestedBy", requisition.RequestedBy),
                    new SqlParameter("@JobId", (object)requisition.JobId ?? DBNull.Value),
                    new SqlParameter("@JobNo", (object)requisition.JobNo ?? ""),
                    new SqlParameter("@RequiredFor", (object)requisition.RequiredFor ?? ""),
                    new SqlParameter("@CreateUser", loggedInUser));

                    InsertDetails(repo, requisition.RowId, requisition.Details, loggedInUser);

                    dbTransaction.Commit();
                }
                catch
                {
                    dbTransaction.Rollback();
                    throw;
                }
            }
        }

        // ---------------------------------------------------------------
        // Update — delete existing detail lines and reinsert (simplest
        // approach that matches the header/detail grid submitted from the UI)
        // ---------------------------------------------------------------
        public static void UpdateRequisition(
            InvDepartmentRequisitionInfo requisition,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            using (var dbTransaction = repo.Database.BeginTransaction())
            {
                try
                {
                    repo.Database.ExecuteSqlCommand(@"
                        UPDATE InvDepartmentRequisition
                        SET
                            RequisitionDate = @RequisitionDate,
                            RequisitionName = @RequisitionName,
                            DepartmentId = @DepartmentId,
                            RequestedBy = @RequestedBy,
                            JobId = @JobId,
                            JobNo = @JobNo,
                            RequiredFor = @RequiredFor,
                            UpdateUser = @UpdateUser,
                            UpdateDate = GETDATE()
                        WHERE RowId = @RowId
                    ",
                    new SqlParameter("@RowId", requisition.RowId),
                    new SqlParameter("@RequisitionDate", requisition.RequisitionDate),
                    new SqlParameter("@RequisitionName", requisition.RequisitionName),
                    new SqlParameter("@DepartmentId", requisition.DepartmentId),
                    new SqlParameter("@RequestedBy", requisition.RequestedBy),
                    new SqlParameter("@JobId", (object)requisition.JobId ?? DBNull.Value),
                    new SqlParameter("@JobNo", (object)requisition.JobNo ?? ""),
                    new SqlParameter("@RequiredFor", (object)requisition.RequiredFor ?? ""),
                    new SqlParameter("@UpdateUser", loggedInUser));

                    repo.Database.ExecuteSqlCommand(@"
                        DELETE FROM InvDepartmentRequisitionDetail
                        WHERE RequisitionId = @RequisitionId
                    ",
                    new SqlParameter("@RequisitionId", requisition.RowId));

                    InsertDetails(repo, requisition.RowId, requisition.Details, loggedInUser);

                    dbTransaction.Commit();
                }
                catch
                {
                    dbTransaction.Rollback();
                    throw;
                }
            }
        }

        private static void InsertDetails(
            GMContext repo,
            string requisitionRowId,
            List<InvDepartmentRequisitionDetailInfo> details,
            string loggedInUser)
        {
            if (details == null)
                return;

            foreach (var line in details)
            {
                InvDepartmentRequisitionDetailInfo obj = repo.Database.SqlQuery<InvDepartmentRequisitionDetailInfo>(@"
                    SELECT
                        ISNULL(MAX(RequisitionDetailId),0)+1 AS RequisitionDetailId
                    FROM InvDepartmentRequisitionDetail
                ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO InvDepartmentRequisitionDetail
                    (
                        RowId,
                        RequisitionDetailId,
                        RequisitionId,
                        ItemId,
                        Quantity,
                        UnitId,
                        UnitPriceEst,
                        TotalPriceEst,
                        Remarks,
                        CreateUser,
                        CreateDate,
                        DomainStatus
                    )
                    VALUES
                    (
                        @RowId,
                        @RequisitionDetailId,
                        @RequisitionId,
                        @ItemId,
                        @Quantity,
                        @UnitId,
                        @UnitPriceEst,
                        @TotalPriceEst,
                        @Remarks,
                        @CreateUser,
                        GETDATE(),
                        1
                    )
                ",
                new SqlParameter("@RowId", Guid.NewGuid().ToString()),
                new SqlParameter("@RequisitionDetailId", obj.RequisitionDetailId),
                new SqlParameter("@RequisitionId", requisitionRowId),
                new SqlParameter("@ItemId", line.ItemId),
                new SqlParameter("@Quantity", line.Quantity),
                new SqlParameter("@UnitId", line.UnitId),
                new SqlParameter("@UnitPriceEst", line.UnitPriceEst),
                new SqlParameter("@TotalPriceEst", line.Quantity * line.UnitPriceEst),
                new SqlParameter("@Remarks", (object)line.Remarks ?? ""),
                new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        // ---------------------------------------------------------------
        // Delete (soft delete)
        // ---------------------------------------------------------------
        public static void DeleteRequisition(
            InvDepartmentRequisitionInfo requisition,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvDepartmentRequisition
                    SET
                        IsRemoved = 1,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", requisition.RowId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        // ---------------------------------------------------------------
        // Filtered list for the DataTables grid (Index page)
        // Supports the Advance Search panel: Code, Name, Department,
        // Job No, Approval Status, From/To Date — plus the free-text
        // "Search..." box (gmdt.SearchValue).
        // ---------------------------------------------------------------
        public class InvDepartmentApprovedLineInfo
        {
            public string RowId { get; set; }

            public decimal ApprovedQuantity { get; set; }
        }
        public static InvDepartmentRequisitionResponse GetFilteredRequisitions(HttpRequestBase Request)
        {
            InvDepartmentRequisitionResponse resp = new InvDepartmentRequisitionResponse();

            GMDataTableDAL gmdt = new GMDataTableDAL(Request);

            resp.Requisitions = null;

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
                    FROM InvDepartmentRequisition a
                    LEFT JOIN InvDepartment b ON b.RowId = a.DepartmentId
                    LEFT JOIN HrEmployee c ON c.RowId = a.RequestedBy
                    WHERE a.IsRemoved = 0
                ");

                List<SqlParameter> sqlParameters = new List<SqlParameter>();

                string requisitionCode = Request["RequisitionCode"];
                string requisitionName = Request["RequisitionName"];
                string departmentId = Request["DepartmentId"];
                string jobNo = Request["JobNo"];
                string approvalStatus = Request["ApprovalStatus"];
                string fromDate = Request["FromDate"];
                string toDate = Request["ToDate"];

                if (!string.IsNullOrEmpty(requisitionCode))
                {
                    sb.AppendLine(" AND a.RequisitionCode LIKE '%' + @RequisitionCode + '%' ");
                    sqlParameters.Add(new SqlParameter("@RequisitionCode", requisitionCode));
                }

                if (!string.IsNullOrEmpty(requisitionName))
                {
                    sb.AppendLine(" AND a.RequisitionName LIKE '%' + @RequisitionName + '%' ");
                    sqlParameters.Add(new SqlParameter("@RequisitionName", requisitionName));
                }

                if (!string.IsNullOrEmpty(departmentId))
                {
                    sb.AppendLine(" AND a.DepartmentId = @DepartmentId ");
                    sqlParameters.Add(new SqlParameter("@DepartmentId", departmentId));
                }

                if (!string.IsNullOrEmpty(jobNo))
                {
                    sb.AppendLine(" AND a.JobNo LIKE '%' + @JobNo + '%' ");
                    sqlParameters.Add(new SqlParameter("@JobNo", jobNo));
                }

                if (!string.IsNullOrEmpty(approvalStatus) && approvalStatus != "All")
                {
                    sb.AppendLine(" AND a.IsApproved = @IsApproved ");
                    sqlParameters.Add(new SqlParameter("@IsApproved", approvalStatus == "Approved" ? 1 : 0));
                }

                if (!string.IsNullOrEmpty(fromDate))
                {
                    sb.AppendLine(" AND a.RequisitionDate >= @FromDate ");
                    sqlParameters.Add(new SqlParameter("@FromDate", Convert.ToDateTime(fromDate)));
                }

                if (!string.IsNullOrEmpty(toDate))
                {
                    sb.AppendLine(" AND a.RequisitionDate <= @ToDate ");
                    sqlParameters.Add(new SqlParameter("@ToDate", Convert.ToDateTime(toDate)));
                }

                resp.TotalRecordCount = repo.Database.SqlQuery<int>(string.Format(@"
                    SELECT COUNT(*) {0}
                ", sb.ToString()), sqlParameters.ToArray()).Single();

                List<SqlParameter> sqlParameters1 = new List<SqlParameter>();
                sqlParameters1.AddRange(sqlParameters);

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.AppendLine(@"
                        AND
                        (
                            a.RequisitionCode LIKE '%' + @SearchValue + '%'
                            OR a.RequisitionName LIKE '%' + @SearchValue + '%'
                            OR b.DepartmentName LIKE '%' + @SearchValue + '%'
                            OR c.LastName LIKE '%' + @SearchValue + '%'
                        )");

                    sqlParameters1.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));
                }

                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(string.Format(@"
                    SELECT COUNT(*) {0}
                ", sb.ToString()), sqlParameters1.ToArray()).Single();

                List<SqlParameter> sqlParameters2 = new List<SqlParameter>();
                sqlParameters2.AddRange(sqlParameters1);
                sqlParameters2.Add(new SqlParameter("@Skip", gmdt.Start));
                sqlParameters2.Add(new SqlParameter("@Take", gmdt.Length));

                string sortColumn = gmdt.SortColum.ToString();
                if (sortColumn == "RowId" || sortColumn == "RequisitionId" || sortColumn == "RequisitionDate"
                    || sortColumn == "RequisitionCode" || sortColumn == "RequisitionName" || sortColumn == "Status"
                    || sortColumn == "IsApproved" || sortColumn == "IsIssued")
                {
                    sortColumn = "a." + sortColumn;
                }

                resp.Requisitions = repo.Database.SqlQuery<InvDepartmentRequisitionInfo>(string.Format(@"
                    SELECT
                         a.RowId
                        ,a.RequisitionId
                        ,a.RequisitionDate
                        -- ,a.RequisitionCode
                        -- ,a.RequisitionName
                        ,c.LastName AS RequestedByName
                        ,a.JobNo
                        ,a.Status
                        ,a.IsApproved
                        ,a.IsIssued

                    {0}

                    ORDER BY {1} {2}

                    OFFSET (@Skip) ROWS
                    FETCH NEXT (@Take) ROWS ONLY
                ",
                sb.ToString(),
                sortColumn,
                gmdt.SortDir),
                sqlParameters2.ToArray()).ToList();
            }

            return resp;
        }

        public class InvDepartmentRequisitionResponse
        {
            public int TotalRecordCount { get; set; }
            public int FilteredRecordCount { get; set; }
            public List<InvDepartmentRequisitionInfo> Requisitions { get; set; }
        }
        public bool ApproveRequisition(
    string requisitionRowId,
    int toDepartmentId,
    string remarks,
    List<InvDepartmentApprovedLineInfo> approvedLines,
    string loggedInUser,
    out string responseMessage)
        {
            responseMessage = "";

            if (string.IsNullOrWhiteSpace(requisitionRowId))
            {
                responseMessage = "Invalid requisition.";
                return false;
            }

            if (toDepartmentId <= 0)
            {
                responseMessage = "Please select To Department.";
                return false;
            }

            if (approvedLines == null || approvedLines.Count == 0)
            {
                responseMessage = "No requisition details were submitted.";
                return false;
            }

            if (!approvedLines.Any(x => x.ApprovedQuantity > 0))
            {
                responseMessage = "At least one approved quantity must be greater than zero.";
                return false;
            }

            using (var db = new GMContext())
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // 1. Lock and validate the requisition.
                    // Only a forwarded requisition (Status = 1) can be approved.

                    var requisitionStatus = db.Database.SqlQuery<int>(
                        @"SELECT Status
                  FROM InvDepartmentRequisition WITH (UPDLOCK, HOLDLOCK)
                  WHERE RowId = @RowId
                    AND IsRemoved = 0",
                        new SqlParameter("@RowId", requisitionRowId)
                    ).FirstOrDefault();

                    if (requisitionStatus != 1)
                    {
                        transaction.Rollback();

                        responseMessage =
                            "Only forwarded requisitions can be approved.";

                        return false;
                    }

                    // 2. Validate that the receiving department exists.

                    var departmentExists = db.Database.SqlQuery<int>(
                        @"SELECT COUNT(1)
                  FROM HrDepartment
                  WHERE RowId = @DepartmentId",
                        new SqlParameter("@DepartmentId", toDepartmentId)
                    ).FirstOrDefault();

                    if (departmentExists == 0)
                    {
                        transaction.Rollback();

                        responseMessage = "Invalid receiving department.";

                        return false;
                    }

                    // 3. Validate and save approved quantities.
                    // Requested quantity is checked again in the database.

                    foreach (var line in approvedLines)
                    {
                        if (string.IsNullOrWhiteSpace(line.RowId) ||
                            line.ApprovedQuantity < 0)
                        {
                            transaction.Rollback();

                            responseMessage = "Invalid approval detail.";

                            return false;
                        }

                        int affected = db.Database.ExecuteSqlCommand(
                            @"UPDATE InvDepartmentRequisitionDetail
                      SET ApprovedQuantity = @ApprovedQuantity
                      WHERE RowId = @DetailRowId
                        AND RequisitionId = @RequisitionRowId
                        AND @ApprovedQuantity <= Quantity",
                            new SqlParameter("@ApprovedQuantity", line.ApprovedQuantity),
                            new SqlParameter("@DetailRowId", line.RowId),
                            new SqlParameter("@RequisitionRowId", requisitionRowId)
                        );

                        if (affected != 1)
                        {
                            transaction.Rollback();

                            responseMessage =
                                "A requisition detail was not found, or its approved quantity exceeds the requested quantity.";

                            return false;
                        }
                    }

                    // 4. Update the related transaction.
                    // This assumes one transaction header per requisition.

                    int transactionAffected = db.Database.ExecuteSqlCommand(
                        @"UPDATE InvDepartmentTransaction
                  SET ToDepartmentId = @ToDepartmentId,
                      Status = 2,
                      Remarks = @Remarks
                  WHERE RequisitionId = @RequisitionRowId
                    AND IsRemoved = 0",
                        new SqlParameter("@ToDepartmentId", toDepartmentId),
                        new SqlParameter("@Remarks",
                            (object)remarks ?? DBNull.Value),
                        new SqlParameter("@RequisitionRowId", requisitionRowId)
                    );

                    if (transactionAffected != 1)
                    {
                        transaction.Rollback();

                        responseMessage =
                            "The related transaction was not found, or multiple transactions exist for this requisition.";

                        return false;
                    }

                    // 5. Update requisition status to Approved.

                    int requisitionAffected = db.Database.ExecuteSqlCommand(
                        @"UPDATE InvDepartmentRequisition
                  SET Status = 2
                  WHERE RowId = @RowId
                    AND Status = 1
                    AND IsRemoved = 0",
                        new SqlParameter("@RowId", requisitionRowId)
                    );

                    if (requisitionAffected != 1)
                    {
                        transaction.Rollback();

                        responseMessage =
                            "The requisition status could not be updated.";

                        return false;
                    }

                    // 6. Commit all changes together.

                    transaction.Commit();

                    responseMessage = "Requisition approved successfully.";

                    return true;
                }
                catch (Exception ex)
                {
                    transaction.Rollback();

                    responseMessage = "Approval failed: " + ex.Message;

                    return false;
                }
            }
        }
    }
}
