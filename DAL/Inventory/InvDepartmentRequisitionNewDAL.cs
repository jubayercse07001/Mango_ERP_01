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
    public class InvDepartmentRequisitionNewDAL
    {
        // ---------------------------------------------------------------
        // Requisition Status values (used by the workflow methods)
        // ---------------------------------------------------------------
        private const int STATUS_NEW = 0;
        private const int STATUS_FORWARDED = 1;
        private const int STATUS_APPROVED = 2;
        private const int STATUS_REJECTED = 3;
        private const int STATUS_ISSUED = 4;

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
                    AND a.IsRemoved = 0
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
                    AND a.DomainStatus = 1
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
        // Add  (requisition header + details + initial workflow transaction)
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
                            @Status,
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
                    new SqlParameter("@Status", STATUS_NEW),
                    new SqlParameter("@CreateUser", loggedInUser));

                    InsertDetails(repo, requisition.RowId, requisition.Details, loggedInUser);

                    // Initial workflow transaction (header + detail snapshot)
                    decimal totalAmount = 0;

                    if (requisition.Details != null)
                        totalAmount = requisition.Details.Sum(x => x.Quantity * x.UnitPriceEst);

                    InsertTransaction(
                        repo,
                        requisition.RowId,
                        "RequisitionCreate",
                        "Initial requisition created",
                        totalAmount,
                        0,
                        requisition.DepartmentId,
                        requisition.DepartmentId,
                        loggedInUser);

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
                        AND IsRemoved = 0
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
                new SqlParameter("@UnitId", (object)line.UnitId ?? DBNull.Value),
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
        // Workflow: Forward / Approve / Reject / Issue
        // Each one updates the requisition header and records a row in
        // InvDepartmentTransaction (+ a snapshot of the current details)
        // ---------------------------------------------------------------
        public static void ForwardRequisition(string rowId, string loggedInUser, string remarks)
        {
            ExecuteWorkflow(rowId, loggedInUser, remarks, "Forward", STATUS_FORWARDED, 0, 0);
        }

        public static void ApproveRequisition(string rowId, string loggedInUser, string remarks)
        {
            ExecuteWorkflow(rowId, loggedInUser, remarks, "Approve", STATUS_APPROVED, 1, 0);
        }

        public static void RejectRequisition(string rowId, string loggedInUser, string remarks)
        {
            ExecuteWorkflow(rowId, loggedInUser, remarks, "Reject", STATUS_REJECTED, 0, 0);
        }

        public static void IssueRequisition(string rowId, string loggedInUser, string remarks)
        {
            ExecuteWorkflow(rowId, loggedInUser, remarks, "Issue", STATUS_ISSUED, 1, 1);
        }

        private static void ExecuteWorkflow(
            string rowId,
            string loggedInUser,
            string remarks,
            string transactionType,
            int status,
            int isApproved,
            int isIssued)
        {
            using (var repo = new GMContext())
            using (var dbTransaction = repo.Database.BeginTransaction())
            {
                try
                {
                    int affected = repo.Database.ExecuteSqlCommand(@"
                        UPDATE InvDepartmentRequisition
                        SET
                            Status = @Status,
                            IsApproved = @IsApproved,
                            IsIssued = @IsIssued,
                            UpdateUser = @UpdateUser,
                            UpdateDate = GETDATE()
                        WHERE RowId = @RowId
                        AND IsRemoved = 0
                    ",
                    new SqlParameter("@RowId", rowId),
                    new SqlParameter("@Status", status),
                    new SqlParameter("@IsApproved", isApproved),
                    new SqlParameter("@IsIssued", isIssued),
                    new SqlParameter("@UpdateUser", loggedInUser));

                    if (affected == 0)
                        throw new Exception("Requisition not found.");

                    InsertTransaction(
                        repo,
                        rowId,
                        transactionType,
                        string.IsNullOrWhiteSpace(remarks) ? transactionType : remarks,
                        0,
                        isIssued,
                        null,
                        null,
                        loggedInUser);

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
        // Writes one InvDepartmentTransaction row and copies the current
        // requisition details into InvDepartmentTransactionDetail (history)
        // ---------------------------------------------------------------
        private static void InsertTransaction(
            GMContext repo,
            string requisitionRowId,
            string transactionType,
            string remarks,
            decimal totalAmount,
            int isIssued,
            string fromDepartment,
            string toDepartment,
            string loggedInUser)
        {
            string transactionRowId = Guid.NewGuid().ToString();

            int nextTransactionId = repo.Database.SqlQuery<int>(@"
                SELECT ISNULL(MAX(TransactionId),0)+1
                FROM InvDepartmentTransaction
            ").Single();

            string transactionCode = "TRX-" + nextTransactionId.ToString("D6");

            repo.Database.ExecuteSqlCommand(@"
                INSERT INTO InvDepartmentTransaction
                (
                    RowId,
                    TransactionId,
                    TransactionCode,
                    TransactionDate,
                    RequisitionId,
                    MaterialReveiveId,
                    MaterialIssueId,
                    TransactionType,
                    FromDepartment,
                    ToDepartment,
                    TotalAmount,
                    Remarks,
                    IsIssued,
                    IsReceived,
                    IsRemoved,
                    DomainStatus,
                    CreateUser,
                    CreateDate
                )
                VALUES
                (
                    @RowId,
                    @TransactionId,
                    @TransactionCode,
                    GETDATE(),
                    @RequisitionId,
                    '',
                    '',
                    @TransactionType,
                    @FromDepartment,
                    @ToDepartment,
                    @TotalAmount,
                    @Remarks,
                    @IsIssued,
                    0,
                    0,
                    1,
                    @CreateUser,
                    GETDATE()
                )
            ",
            new SqlParameter("@RowId", transactionRowId),
            new SqlParameter("@TransactionId", nextTransactionId),
            new SqlParameter("@TransactionCode", transactionCode),
            new SqlParameter("@RequisitionId", requisitionRowId),
            new SqlParameter("@TransactionType", transactionType),
            new SqlParameter("@FromDepartment", (object)fromDepartment ?? ""),
            new SqlParameter("@ToDepartment", (object)toDepartment ?? ""),
            new SqlParameter("@TotalAmount", totalAmount),
            new SqlParameter("@Remarks", (object)remarks ?? ""),
            new SqlParameter("@IsIssued", isIssued),
            new SqlParameter("@CreateUser", loggedInUser));

            // Snapshot of the current requisition detail lines
            repo.Database.ExecuteSqlCommand(@"
                INSERT INTO InvDepartmentTransactionDetail
                (
                    RowId,
                    TransactionDetailId,
                    TransactionId,
                    ItemId,
                    UnitId,
                    Quantity,
                    MRRQty,
                    UnitPrice,
                    TotalPrice,
                    DomainStatus,
                    CreateUser,
                    CreateDate
                )
                SELECT
                    CAST(NEWID() AS VARCHAR(36)),
                    ROW_NUMBER() OVER (ORDER BY RequisitionDetailId),
                    @TransactionId,
                    ItemId,
                    UnitId,
                    Quantity,
                    0,
                    UnitPriceEst,
                    TotalPriceEst,
                    1,
                    @CreateUser,
                    GETDATE()
                FROM InvDepartmentRequisitionDetail
                WHERE RequisitionId = @RequisitionId
                AND DomainStatus = 1
            ",
            new SqlParameter("@TransactionId", transactionRowId),
            new SqlParameter("@RequisitionId", requisitionRowId),
            new SqlParameter("@CreateUser", loggedInUser));
        }

        // ---------------------------------------------------------------
        // Filtered list for the DataTables grid (Index page)
        // Supports the Advance Search panel: Code, Name, Department,
        // Job No, Approval Status, From/To Date — plus the free-text
        // "Search..." box (gmdt.SearchValue).
        // ---------------------------------------------------------------
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
                        ,a.RequisitionCode
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

    }
}
