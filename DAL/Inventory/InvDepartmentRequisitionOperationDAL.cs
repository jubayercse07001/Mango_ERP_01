using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

using ERP.Models.Inventory.InvDepartmentRequisition;
using ERP.Models.Inventory.InvDepartmentRequisitionDetail;

namespace ERP.DAL.Inventory
{
    public class InvDepartmentRequisitionApprovedLine
    {
        public string RowId { get; set; }
        public decimal ApprovedQuantity { get; set; }
    }

    public class InvDepartmentRequisitionResponse
    {
        public List<InvDepartmentRequisitionInfo> Requisitions { get; set; }
        public int TotalRecordCount { get; set; }
        public int FilteredRecordCount { get; set; }
    }

    internal class ApprovalDetailDbInfo
    {
        public string RowId { get; set; }
        public int RequisitionDetailId { get; set; }
        public string RequisitionId { get; set; }
        public string ItemId { get; set; }
        public decimal Quantity { get; set; }
        public string UnitId { get; set; }
        public decimal UnitPriceEst { get; set; }
        public decimal TotalPriceEst { get; set; }
        public decimal ApprovedQuantity { get; set; }
    }

    public class InvDepartmentRequisitionOperationDAL
    {
        private const int STATUS_NEW = 0;
        private const int STATUS_FORWARDED = 1;
        private const int STATUS_APPROVED = 2;
        private const int STATUS_REJECTED = 3;
        private const int STATUS_ISSUED = 4;
        private const int STATUS_RECEIVED = 5;

        #region Index / DataTables

        public static InvDepartmentRequisitionResponse GetFilteredRequisitions(
            HttpRequestBase Request, string loggedInUserID)
        {
            var response = new InvDepartmentRequisitionResponse
            {
                Requisitions = new List<InvDepartmentRequisitionInfo>()
            };

            GMDataTableDAL gmdt = new GMDataTableDAL(Request);

            using (var repo = new GMContext())
            {
                var fromWhere = new StringBuilder(@"
                    FROM InvDepartmentRequisition a
                    LEFT JOIN HrDepartment b ON b.RowId = a.DepartmentId
                    LEFT JOIN HrEmployee c ON c.RowId = a.RequestedBy
                    WHERE a.DomainStatus = 1
                    AND a.Status IN (0, 1)
                ");

                var baseParameters = new List<SqlParameter>();

                string requisitionCode = Request["RequisitionCode"];
                string requisitionName = Request["RequisitionName"];
                string departmentId = Request["DepartmentId"];
                string jobNo = Request["JobNo"];
                string approvalStatus = Request["ApprovalStatus"];
                string fromDate = Request["FromDate"];
                string toDate = Request["ToDate"];

                if (!string.IsNullOrWhiteSpace(requisitionCode))
                {
                    fromWhere.AppendLine(" AND a.RequisitionCode LIKE '%' + @RequisitionCode + '%'");
                    baseParameters.Add(new SqlParameter("@RequisitionCode", requisitionCode));
                }

                if (!string.IsNullOrWhiteSpace(requisitionName))
                {
                    fromWhere.AppendLine(" AND a.RequisitionName LIKE '%' + @RequisitionName + '%'");
                    baseParameters.Add(new SqlParameter("@RequisitionName", requisitionName));
                }

                if (!string.IsNullOrWhiteSpace(departmentId))
                {
                    fromWhere.AppendLine(" AND a.DepartmentId = @DepartmentId");
                    baseParameters.Add(new SqlParameter("@DepartmentId", departmentId));
                }

                if (!string.IsNullOrWhiteSpace(jobNo))
                {
                    fromWhere.AppendLine(" AND a.JobNo LIKE '%' + @JobNo + '%'");
                    baseParameters.Add(new SqlParameter("@JobNo", jobNo));
                }

                int parsedStatus;
                if (!string.IsNullOrWhiteSpace(approvalStatus) &&
                    !approvalStatus.Equals("All", StringComparison.OrdinalIgnoreCase) &&
                    int.TryParse(approvalStatus, out parsedStatus))
                {
                    fromWhere.AppendLine(" AND a.Status = @Status");
                    baseParameters.Add(new SqlParameter("@Status", parsedStatus));
                }
                else if (!string.IsNullOrWhiteSpace(approvalStatus) &&
                         !approvalStatus.Equals("All", StringComparison.OrdinalIgnoreCase))
                {
                    switch (approvalStatus.Trim())
                    {
                        case "New": parsedStatus = STATUS_NEW; break;
                        case "Forwarded": parsedStatus = STATUS_FORWARDED; break;
                        case "Approved": parsedStatus = STATUS_APPROVED; break;
                        case "Rejected": parsedStatus = STATUS_REJECTED; break;
                        case "Issued": parsedStatus = STATUS_ISSUED; break;
                        case "Received": parsedStatus = STATUS_RECEIVED; break;
                        default: parsedStatus = -1; break;
                    }

                    if (parsedStatus >= 0)
                    {
                        fromWhere.AppendLine(" AND a.Status = @Status");
                        baseParameters.Add(new SqlParameter("@Status", parsedStatus));
                    }
                }

                DateTime parsedDate;
                if (!string.IsNullOrWhiteSpace(fromDate) && DateTime.TryParse(fromDate, out parsedDate))
                {
                    fromWhere.AppendLine(" AND a.RequisitionDate >= @FromDate");
                    baseParameters.Add(new SqlParameter("@FromDate", parsedDate.Date));
                }

                if (!string.IsNullOrWhiteSpace(toDate) && DateTime.TryParse(toDate, out parsedDate))
                {
                    DateTime nextDate = parsedDate.Date.AddDays(1);
                    fromWhere.AppendLine(" AND a.RequisitionDate < @ToDateExclusive");
                    baseParameters.Add(new SqlParameter("@ToDateExclusive", nextDate));
                }

                response.TotalRecordCount = repo.Database.SqlQuery<int>(
                    "SELECT COUNT(1) " + fromWhere,
                    CloneParameters(baseParameters).ToArray()).Single();

                var filteredWhere = new StringBuilder(fromWhere.ToString());
                var filteredParameters = CloneParameters(baseParameters);

                if (!string.IsNullOrWhiteSpace(gmdt.SearchValue))
                {
                    filteredWhere.AppendLine(@"
                        AND (
                            a.RequisitionCode LIKE '%' + @SearchValue + '%'
                            OR a.RequisitionName LIKE '%' + @SearchValue + '%'
                            OR b.DepartmentName LIKE '%' + @SearchValue + '%'
                            OR c.LastName LIKE '%' + @SearchValue + '%'
                            OR a.JobNo LIKE '%' + @SearchValue + '%'
                        )");
                    filteredParameters.Add(new SqlParameter("@SearchValue", gmdt.SearchValue));
                }

                response.FilteredRecordCount = repo.Database.SqlQuery<int>(
                    "SELECT COUNT(1) " + filteredWhere,
                    CloneParameters(filteredParameters).ToArray()).Single();

                string sortColumn = Convert.ToString(gmdt.SortColum);
                if (string.IsNullOrWhiteSpace(sortColumn))
                    sortColumn = "a.RequisitionDate";

                switch (sortColumn)
                {
                    case "RowId":
                    case "RequisitionId":
                    case "RequisitionDate":
                    case "RequisitionCode":
                    case "RequisitionName":
                    case "Status":
                    case "IsApproved":
                    case "IsIssued":
                        sortColumn = "a." + sortColumn;
                        break;
                    case "DepartmentName":
                        sortColumn = "b.DepartmentName";
                        break;
                    case "RequestedByName":
                        sortColumn = "c.LastName";
                        break;
                    default:
                        sortColumn = "a.RequisitionDate";
                        break;
                }

                string sortDirection = string.Equals(gmdt.SortDir, "desc", StringComparison.OrdinalIgnoreCase)
                    ? "DESC" : "ASC";

                var listParameters = CloneParameters(filteredParameters);
                listParameters.Add(new SqlParameter("@Skip", gmdt.Start));
                listParameters.Add(new SqlParameter("@Take", gmdt.Length > 0 ? gmdt.Length : 10));

                string sql = @"
                    SELECT
                         a.RowId
                        ,a.RequisitionId
                        ,a.RequisitionDate
                        ,a.RequisitionCode
                        ,a.RequisitionName
                        ,a.DepartmentId
                        ,b.DepartmentName
                        ,a.RequestedBy
                        ,c.LastName AS RequestedByName
                        ,a.JobId
                        ,a.JobNo
                        ,a.Status
                        ,a.IsApproved
                        ,a.IsIssued
                    " + filteredWhere + @"
                    ORDER BY " + sortColumn + " " + sortDirection + @"
                    OFFSET @Skip ROWS
                    FETCH NEXT @Take ROWS ONLY";

                response.Requisitions = repo.Database.SqlQuery<InvDepartmentRequisitionInfo>(
                    sql, listParameters.ToArray()).ToList();
            }

            return response;
        }

        private static List<SqlParameter> CloneParameters(IEnumerable<SqlParameter> source)
        {
            var result = new List<SqlParameter>();
            foreach (SqlParameter p in source)
            {
                result.Add(new SqlParameter
                {
                    ParameterName = p.ParameterName,
                    SqlDbType = p.SqlDbType,
                    Direction = p.Direction,
                    Size = p.Size,
                    Precision = p.Precision,
                    Scale = p.Scale,
                    Value = p.Value ?? DBNull.Value
                });
            }
            return result;
        }

        #endregion

        #region Get

        public static InvDepartmentRequisitionInfo GetRequisitionByID(string rowId)
        {
            if (string.IsNullOrWhiteSpace(rowId))
                return null;

            using (var repo = new GMContext())
            {
                var requisition = repo.Database.SqlQuery<InvDepartmentRequisitionInfo>(@"
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
                ", new SqlParameter("@RowId", rowId)).FirstOrDefault();

                if (requisition != null)
                {
                    requisition.Details = GetRequisitionDetailsByRequisitionId(requisition.RowId);
                }

                return requisition;
            }
        }

        public static List<InvDepartmentRequisitionDetailInfo> GetRequisitionDetailsByRequisitionId(string requisitionId)
        {
            if (string.IsNullOrWhiteSpace(requisitionId))
                return new List<InvDepartmentRequisitionDetailInfo>();

            using (var repo = new GMContext())
            {
                return repo.Database.SqlQuery<InvDepartmentRequisitionDetailInfo>(@"
                    SELECT
                         a.RowId
                        ,a.RequisitionDetailId
                        ,a.RequisitionId
                        ,a.ItemId
                        ,ISNULL(a.Quantity, 0) AS Quantity
                        ,a.UnitId
                        ,ISNULL(a.UnitPriceEst, 0) AS UnitPriceEst
                        ,ISNULL(a.TotalPriceEst, 0) AS TotalPriceEst
                        ,ISNULL(a.ApprovedQuantity, 0) AS ApprovedQuantity
                        ,a.Remarks
                        ,b.ItemName
                        ,c.MinUnitName
                    FROM InvDepartmentRequisitionDetail a
                    LEFT JOIN InvItem b ON b.RowId = a.ItemId
                    LEFT JOIN InvUnit c ON c.RowId = a.UnitId
                    WHERE a.RequisitionId = @RequisitionId
                      AND a.DomainStatus = 1
                    ORDER BY a.RequisitionDetailId
                ", new SqlParameter("@RequisitionId", requisitionId)).ToList();
            }
        }

        public static string GetNextRequisitionCode()
        {
            using (var repo = new GMContext())
            {
                int nextId = repo.Database.SqlQuery<int>(@"
                    SELECT ISNULL(MAX(RequisitionId), 0) + 1
                    FROM InvDepartmentRequisition
                ").Single();

                return "REQ-" + nextId.ToString("D4");
            }
        }

        #endregion

        #region Add / Update / Delete

        public static void AddRequisition(InvDepartmentRequisitionInfo requisition, string loggedInUser)
        {
            if (requisition == null)
                throw new ArgumentNullException("requisition");

            if (string.IsNullOrWhiteSpace(requisition.RowId))
                requisition.RowId = Guid.NewGuid().ToString();

            if (requisition.Details == null || requisition.Details.Count == 0)
                throw new Exception("At least one requisition item is required.");

            if (string.IsNullOrWhiteSpace(loggedInUser))
                throw new Exception("Logged-in user is required.");

            ValidateDetails(requisition.Details);

            using (var repo = new GMContext())
            using (var transaction = repo.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    int nextRequisitionId = GetNextIntegerId(repo, "InvDepartmentRequisition", "RequisitionId");

                    string requisitionCode = string.IsNullOrWhiteSpace(requisition.RequisitionCode)
                        ? "REQ-" + nextRequisitionId.ToString("D4")
                        : requisition.RequisitionCode;

                    repo.Database.ExecuteSqlCommand(@"
                        INSERT INTO InvDepartmentRequisition
                        (
                             RowId, RequisitionId, RequisitionDate, RequisitionCode, RequisitionName,
                             DepartmentId, RequestedBy, JobId, JobNo, RequiredFor,
                             Status, IsApproved, IsIssued, IsRemoved,
                             CreateUser, CreateDate, DomainStatus
                        )
                        VALUES
                        (
                             @RowId, @RequisitionId, @RequisitionDate, @RequisitionCode, @RequisitionName,
                             @DepartmentId, @RequestedBy, @JobId, @JobNo, @RequiredFor,
                             @Status, 0, 0, 0,
                             @CreateUser, GETDATE(), 1
                        )
                    ",
                        new SqlParameter("@RowId", requisition.RowId),
                        new SqlParameter("@RequisitionId", nextRequisitionId),
                        new SqlParameter("@RequisitionDate", requisition.RequisitionDate),
                        new SqlParameter("@RequisitionCode", requisitionCode),
                        new SqlParameter("@RequisitionName", DbValue(requisition.RequisitionName)),
                        new SqlParameter("@DepartmentId", DbValue(requisition.DepartmentId)),
                        new SqlParameter("@RequestedBy", DbValue(requisition.RequestedBy)),
                        new SqlParameter("@JobId", DbValue(requisition.JobId)),
                        new SqlParameter("@JobNo", DbValue(requisition.JobNo)),
                        new SqlParameter("@RequiredFor", DbValue(requisition.RequiredFor)),
                        new SqlParameter("@Status", STATUS_NEW),
                        new SqlParameter("@CreateUser", loggedInUser));

                    InsertRequisitionDetails(repo, requisition.RowId, requisition.Details, loggedInUser);

                    decimal totalAmount = requisition.Details.Sum(x => x.Quantity * x.UnitPriceEst);

                    InsertTransactionHeaderAndDetails(
                        repo, requisition.RowId, "RequisitionCreate",
                        "Initial requisition created", totalAmount,
                        requisition.Details, loggedInUser);

                    transaction.Commit();
                }
                catch
                {
                    try { transaction.Rollback(); } catch { }
                    throw;
                }
            }
        }

        public static void UpdateRequisition(InvDepartmentRequisitionInfo requisition, string loggedInUser)
        {
            if (requisition == null)
                throw new ArgumentNullException("requisition");

            if (string.IsNullOrWhiteSpace(requisition.RowId))
                throw new Exception("Requisition ID is required.");

            if (requisition.Details == null || requisition.Details.Count == 0)
                throw new Exception("At least one requisition item is required.");

            ValidateDetails(requisition.Details);

            using (var repo = new GMContext())
            using (var transaction = repo.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    int affected = repo.Database.ExecuteSqlCommand(@"
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
                          AND DomainStatus = 1
                          AND Status = @Status
                    ",
                        new SqlParameter("@RequisitionDate", requisition.RequisitionDate),
                        new SqlParameter("@RequisitionName", DbValue(requisition.RequisitionName)),
                        new SqlParameter("@DepartmentId", DbValue(requisition.DepartmentId)),
                        new SqlParameter("@RequestedBy", DbValue(requisition.RequestedBy)),
                        new SqlParameter("@JobId", DbValue(requisition.JobId)),
                        new SqlParameter("@JobNo", DbValue(requisition.JobNo)),
                        new SqlParameter("@RequiredFor", DbValue(requisition.RequiredFor)),
                        new SqlParameter("@UpdateUser", loggedInUser),
                        new SqlParameter("@RowId", requisition.RowId),
                        new SqlParameter("@Status", STATUS_NEW));

                    if (affected == 0)
                        throw new Exception("Requisition is not in New status.");

                    repo.Database.ExecuteSqlCommand(@"
                        DELETE FROM InvDepartmentRequisitionDetail
                        WHERE RequisitionId = @RequisitionId
                    ", new SqlParameter("@RequisitionId", requisition.RowId));

                    InsertRequisitionDetails(repo, requisition.RowId, requisition.Details, loggedInUser);

                    transaction.Commit();
                }
                catch
                {
                    try { transaction.Rollback(); } catch { }
                    throw;
                }
            }
        }

        private static void InsertRequisitionDetails(
            GMContext repo, string requisitionRowId,
            List<InvDepartmentRequisitionDetailInfo> details, string loggedInUser)
        {
            foreach (var line in details)
            {
                int detailId = GetNextIntegerId(repo, "InvDepartmentRequisitionDetail", "RequisitionDetailId");

                decimal totalPrice = Math.Round(
                    line.Quantity * line.UnitPriceEst, 2, MidpointRounding.AwayFromZero);

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO InvDepartmentRequisitionDetail
                    (
                         RowId, RequisitionDetailId, RequisitionId, ItemId,
                         Quantity, UnitId, UnitPriceEst, TotalPriceEst, Remarks,
                         CreateUser, CreateDate, DomainStatus
                    )
                    VALUES
                    (
                         @RowId, @RequisitionDetailId, @RequisitionId, @ItemId,
                         @Quantity, @UnitId, @UnitPriceEst, @TotalPriceEst, @Remarks,
                         @CreateUser, GETDATE(), 1
                    )
                ",
                    new SqlParameter("@RowId", Guid.NewGuid().ToString()),
                    new SqlParameter("@RequisitionDetailId", detailId),
                    new SqlParameter("@RequisitionId", requisitionRowId),
                    new SqlParameter("@ItemId", DbValue(line.ItemId)),
                    new SqlParameter("@Quantity", line.Quantity),
                    new SqlParameter("@UnitId", DbValue(line.UnitId)),
                    new SqlParameter("@UnitPriceEst", line.UnitPriceEst),
                    new SqlParameter("@TotalPriceEst", totalPrice),
                    new SqlParameter("@Remarks", DbValue(line.Remarks)),
                    new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        public static void DeleteRequisition(InvDepartmentRequisitionInfo requisition, string loggedInUser)
        {
            if (requisition == null || string.IsNullOrWhiteSpace(requisition.RowId))
                throw new Exception("Requisition ID is required.");

            using (var repo = new GMContext())
            {
                int affected = repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvDepartmentRequisition
                    SET
                         IsRemoved = 1,
                         UpdateUser = @UpdateUser,
                         UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                      AND DomainStatus = 1
                      AND Status = @Status
                ",
                    new SqlParameter("@UpdateUser", loggedInUser),
                    new SqlParameter("@RowId", requisition.RowId),
                    new SqlParameter("@Status", STATUS_NEW));

                if (affected == 0)
                    throw new Exception("Requisition is not in New status.");
            }
        }

        #endregion

        #region Forward

        //        public static bool ForwardRequisition(
        //            string rowId, string toDepartmentId, string loggedInUser,
        //            string remarks, out string message)
        //        {
        //            message = "";

        //            if (string.IsNullOrWhiteSpace(rowId))
        //            {
        //                message = "Requisition ID is required.";
        //                return false;
        //            }

        //            if (string.IsNullOrWhiteSpace(toDepartmentId))
        //            {
        //                message = "To Department is required.";
        //                return false;
        //            }

        //            if (string.IsNullOrWhiteSpace(loggedInUser))
        //            {
        //                message = "Logged-in user is required.";
        //                return false;
        //            }

        //            using (var repo = new GMContext())
        //            using (var transaction = repo.Database.BeginTransaction(IsolationLevel.Serializable))
        //            {
        //                try
        //                {
        //                    int currentStatus = repo.Database.SqlQuery<int>(@"
        //                        SELECT Status
        //                        FROM InvDepartmentRequisition
        //                        WHERE RowId = @RowId AND IsRemoved = 0 AND DomainStatus = 1
        //                    ", new SqlParameter("@RowId", rowId)).FirstOrDefault();

        //                    if (currentStatus != STATUS_NEW)
        //                    {
        //                        message = "Only New requisition can be forwarded.";
        //                        transaction.Rollback();
        //                        return false;
        //                    }

        //                    int departmentExists = repo.Database.SqlQuery<int>(@"
        //                        SELECT COUNT(1)
        //                        FROM HrDepartment
        //                        WHERE RowId = @DepartmentId AND DomainStatus = 1
        //                    ", new SqlParameter("@DepartmentId", toDepartmentId)).Single();

        //                    if (departmentExists <= 0)
        //                    {
        //                        message = "Selected To Department was not found.";
        //                        transaction.Rollback();
        //                        return false;
        //                    }

        //                    int affected = repo.Database.ExecuteSqlCommand(@"
        //                        UPDATE InvDepartmentRequisition
        //                        SET Status = @StatusForwarded,
        //                            UpdateUser = @UpdateUser,
        //                            UpdateDate = GETDATE()
        //                        WHERE RowId = @RowId
        //                          AND IsRemoved = 0
        //                          AND DomainStatus = 1
        //                          AND Status = @StatusNew
        //                    ",
        //                        new SqlParameter("@StatusForwarded", STATUS_FORWARDED),
        //new SqlParameter("@UpdateUser", loggedInUser),
        //new SqlParameter("@RowId", rowId),
        //new SqlParameter("@StatusNew", 0));

        //                    if (affected == 0)
        //                    {
        //                        message = "Requisition could not be forwarded.";
        //                        transaction.Rollback();
        //                        return false;
        //                    }

        //                    string initialTransactionRowId = repo.Database.SqlQuery<string>(@"
        //                        SELECT TOP 1 RowId
        //                        FROM InvDepartmentTransaction
        //                        WHERE RequisitionId = @RequisitionId
        //                          AND IsRemoved = 0
        //                          AND DomainStatus = 1
        //                        ORDER BY TransactionId ASC
        //                    ", new SqlParameter("@RequisitionId", rowId)).FirstOrDefault();

        //                    if (string.IsNullOrWhiteSpace(initialTransactionRowId))
        //                    {
        //                        message = "Initial transaction was not found.";
        //                        transaction.Rollback();
        //                        return false;
        //                    }

        //                    repo.Database.ExecuteSqlCommand(@"
        //                        UPDATE InvDepartmentTransaction
        //                        SET ToDepartment = @ToDepartment,
        //                            Remarks = @Remarks,
        //                            UpdateUser = @UpdateUser,
        //                            UpdateDate = GETDATE()
        //                        WHERE RowId = @TransactionRowId
        //                          AND RequisitionId = @RequisitionId
        //                          AND IsRemoved = 0
        //                          AND DomainStatus = 1
        //                    ",
        //                        new SqlParameter("@ToDepartment", toDepartmentId),
        //                        new SqlParameter("@Remarks", DbValue(remarks)),
        //                        new SqlParameter("@UpdateUser", loggedInUser),
        //                        new SqlParameter("@TransactionRowId", initialTransactionRowId),
        //                        new SqlParameter("@RequisitionId", rowId));

        //                    decimal totalAmount = repo.Database.SqlQuery<decimal>(@"
        //                        SELECT ISNULL(TotalAmount, 0)
        //                        FROM InvDepartmentTransaction
        //                        WHERE RowId = @RowId
        //                    ", new SqlParameter("@RowId", initialTransactionRowId)).FirstOrDefault();

        //                    int nextTransactionId = GetNextIntegerId(repo, "InvDepartmentTransaction", "TransactionId");
        //                    string transactionRowId = Guid.NewGuid().ToString();

        //                    repo.Database.ExecuteSqlCommand(@"
        //                        INSERT INTO InvDepartmentTransaction
        //                        (
        //                             RowId, TransactionId, TransactionCode, TransactionDate,
        //                             RequisitionId, MaterialReveiveId, MaterialIssueId,
        //                             TransactionType, FromDepartment, ToDepartment,
        //                             TotalAmount, Remarks, IsIssued, IsReceived,
        //                             IsRemoved, DomainStatus, CreateUser, CreateDate
        //                        )
        //                        VALUES
        //                        (
        //                             @RowId, @TransactionId, @TransactionCode, GETDATE(),
        //                             @RequisitionId, '', '',
        //                             'Forward', @FromDepartment, NULL,
        //                             @TotalAmount, @Remarks, 0, 0,
        //                             0, 1, @CreateUser, GETDATE()
        //                        )
        //                    ",
        //                        new SqlParameter("@RowId", transactionRowId),
        //                        new SqlParameter("@TransactionId", nextTransactionId),
        //                        new SqlParameter("@TransactionCode", "TRX-" + nextTransactionId.ToString("D6")),
        //                        new SqlParameter("@RequisitionId", rowId),
        //                        new SqlParameter("@FromDepartment", toDepartmentId),
        //                        new SqlParameter("@TotalAmount", totalAmount),
        //                        new SqlParameter("@Remarks", DbValue(remarks)),
        //                        new SqlParameter("@CreateUser", loggedInUser));

        //                    int nextDetailId = GetNextIntegerId(repo, "InvDepartmentTransactionDetail", "TransactionDetailId");

        //                    repo.Database.ExecuteSqlCommand(@"
        //                        INSERT INTO InvDepartmentTransactionDetail
        //                        (
        //                             RowId, TransactionDetailId, TransactionId,
        //                             ItemId, UnitId, Quantity, MRRQty,
        //                             UnitPrice, TotalPrice, DomainStatus,
        //                             CreateUser, CreateDate
        //                        )
        //                        SELECT
        //                             CAST(NEWID() AS VARCHAR(50)),
        //                             @StartDetailId + ROW_NUMBER() OVER (ORDER BY d.TransactionDetailId) - 1,
        //                             @NewTransactionId,
        //                             d.ItemId, d.UnitId,
        //                             ISNULL(d.Quantity, 0),
        //                             ISNULL(d.MRRQty, 0),
        //                             ISNULL(d.UnitPrice, 0),
        //                             ISNULL(d.TotalPrice, ISNULL(d.Quantity, 0) * ISNULL(d.UnitPrice, 0)),
        //                             1, @CreateUser, GETDATE()
        //                        FROM InvDepartmentTransactionDetail d
        //                        WHERE d.TransactionId = @OldTransactionId
        //                          AND d.DomainStatus = 1
        //                    ",
        //                        new SqlParameter("@StartDetailId", nextDetailId),
        //                        new SqlParameter("@NewTransactionId", transactionRowId),
        //                        new SqlParameter("@CreateUser", loggedInUser),
        //                        new SqlParameter("@OldTransactionId", initialTransactionRowId));

        //                    transaction.Commit();
        //                    message = "Requisition forwarded successfully.";
        //                    return true;
        //                }
        //                catch (Exception ex)
        //                {
        //                    try { transaction.Rollback(); } catch { }
        //                    message = "Forward failed: " + ex.GetBaseException().Message;
        //                    return false;
        //                }
        //            }
        //        }

        #endregion
        #region Forward

        /// <summary>
        /// Forward: From Department = selected (current holder), To Department = inputted.
        /// Unit remains the same.
        /// </summary>
        public static bool ForwardRequisition(
    string rowId,
    string fromDepartmentId,
    string toDepartmentId,
    string loggedInUser,
    string remarks,
    out string message)
        {
            message = "";

            if (string.IsNullOrWhiteSpace(rowId))
            {
                message = "Requisition ID is required.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(fromDepartmentId))
            {
                message = "From Department is required.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(toDepartmentId))
            {
                message = "To Department is required.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(loggedInUser))
            {
                message = "Logged-in user is required.";
                return false;
            }

            using (var repo = new GMContext())
            using (var transaction = repo.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    // 1. Check current status
                    int currentStatus = repo.Database.SqlQuery<int>(@"
                SELECT Status
                FROM InvDepartmentRequisition
                WHERE RowId = @RowId 
                  AND IsRemoved = 0 
                  AND DomainStatus = 1
            ", new SqlParameter("@RowId", rowId)).FirstOrDefault();

                    if (currentStatus != STATUS_NEW)
                    {
                        message = "Only New requisition can be forwarded.";
                        transaction.Rollback();
                        return false;
                    }

                    // 2. Validate From Department
                    int fromDeptExists = repo.Database.SqlQuery<int>(@"
                SELECT COUNT(1)
                FROM HrDepartment
                WHERE RowId = @DepartmentId AND DomainStatus = 1
            ", new SqlParameter("@DepartmentId", fromDepartmentId)).Single();

                    if (fromDeptExists <= 0)
                    {
                        message = "Selected From Department was not found.";
                        transaction.Rollback();
                        return false;
                    }

                    // 3. Validate To Department
                    int toDeptExists = repo.Database.SqlQuery<int>(@"
                SELECT COUNT(1)
                FROM HrDepartment
                WHERE RowId = @DepartmentId AND DomainStatus = 1
            ", new SqlParameter("@DepartmentId", toDepartmentId)).Single();

                    if (toDeptExists <= 0)
                    {
                        message = "Selected To Department was not found.";
                        transaction.Rollback();
                        return false;
                    }

                    // 4. Update Requisition Status → Forwarded
                    // Hard-coded status values to avoid EF6 parameter binding bug
                    int affected = repo.Database.ExecuteSqlCommand(@"
                UPDATE InvDepartmentRequisition
                SET Status = @StatusForwarded,
                    UpdateUser = @UpdateUser,
                    UpdateDate = GETDATE()
                WHERE RowId = @RowId
                  AND IsRemoved = 0
                  AND DomainStatus = 1
                  AND Status = " + STATUS_NEW + @"
            ",
                        new SqlParameter("@StatusForwarded", STATUS_FORWARDED),
                        new SqlParameter("@UpdateUser", loggedInUser),
                        new SqlParameter("@RowId", rowId));

                    if (affected == 0)
                    {
                        message = "Requisition could not be forwarded.";
                        transaction.Rollback();
                        return false;
                    }

                    // 5. Get the LATEST existing transaction (BEFORE inserting the new one)
                    string previousTransactionRowId = repo.Database.SqlQuery<string>(@"
                SELECT TOP 1 RowId
                FROM InvDepartmentTransaction
                WHERE RequisitionId = @RequisitionId
                  AND IsRemoved = 0
                  AND DomainStatus = 1
                ORDER BY TransactionId DESC
            ", new SqlParameter("@RequisitionId", rowId)).FirstOrDefault();

                    decimal totalAmount = 0;

                    if (!string.IsNullOrWhiteSpace(previousTransactionRowId))
                    {
                        totalAmount = repo.Database.SqlQuery<decimal>(@"
                    SELECT ISNULL(TotalAmount, 0)
                    FROM InvDepartmentTransaction
                    WHERE RowId = @RowId
                ", new SqlParameter("@RowId", previousTransactionRowId)).FirstOrDefault();
                    }

                    // 6. Create new Forward transaction
                    int nextTransactionId = GetNextIntegerId(repo, "InvDepartmentTransaction", "TransactionId");
                    string newTransactionRowId = Guid.NewGuid().ToString();

                    repo.Database.ExecuteSqlCommand(@"
                INSERT INTO InvDepartmentTransaction
                (
                     RowId, TransactionId, TransactionCode, TransactionDate,
                     RequisitionId, MaterialReveiveId, MaterialIssueId,
                     TransactionType, FromDepartment, ToDepartment,
                     TotalAmount, Remarks, IsIssued, IsReceived,
                     IsRemoved, DomainStatus, CreateUser, CreateDate
                )
                VALUES
                (
                     @RowId, @TransactionId, @TransactionCode, GETDATE(),
                     @RequisitionId, '', '',
                     'Forward', @FromDepartment, @ToDepartment,
                     @TotalAmount, @Remarks, 0, 0,
                     0, 1, @CreateUser, GETDATE()
                )
            ",
                        new SqlParameter("@RowId", newTransactionRowId),
                        new SqlParameter("@TransactionId", nextTransactionId),
                        new SqlParameter("@TransactionCode", "TRX-" + nextTransactionId.ToString("D6")),
                        new SqlParameter("@RequisitionId", rowId),
                        new SqlParameter("@FromDepartment", fromDepartmentId),
                        new SqlParameter("@ToDepartment", toDepartmentId),
                        new SqlParameter("@TotalAmount", totalAmount),
                        new SqlParameter("@Remarks", DbValue(remarks)),
                        new SqlParameter("@CreateUser", loggedInUser));

                    // 7. Copy details from the previous transaction
                    if (!string.IsNullOrWhiteSpace(previousTransactionRowId))
                    {
                        int nextDetailId = GetNextIntegerId(repo, "InvDepartmentTransactionDetail", "TransactionDetailId");

                        repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO InvDepartmentTransactionDetail
                    (
                         RowId, TransactionDetailId, TransactionId,
                         ItemId, UnitId, Quantity, MRRQty,
                         UnitPrice, TotalPrice, DomainStatus,
                         CreateUser, CreateDate
                    )
                    SELECT
                         CAST(NEWID() AS VARCHAR(50)),
                         @StartDetailId + ROW_NUMBER() OVER (ORDER BY d.TransactionDetailId) - 1,
                         @NewTransactionId,
                         d.ItemId,
                         d.UnitId,
                         ISNULL(d.Quantity, 0),
                         ISNULL(d.MRRQty, 0),
                         ISNULL(d.UnitPrice, 0),
                         ISNULL(d.TotalPrice, ISNULL(d.Quantity, 0) * ISNULL(d.UnitPrice, 0)),
                         1,
                         @CreateUser,
                         GETDATE()
                    FROM InvDepartmentTransactionDetail d
                    WHERE d.TransactionId = @OldTransactionId
                      AND d.DomainStatus = 1
                ",
                            new SqlParameter("@StartDetailId", nextDetailId),
                            new SqlParameter("@NewTransactionId", newTransactionRowId),
                            new SqlParameter("@CreateUser", loggedInUser),
                            new SqlParameter("@OldTransactionId", previousTransactionRowId));
                    }

                    transaction.Commit();
                    message = "Requisition forwarded successfully.";
                    return true;
                }
                catch (Exception ex)
                {
                    try { transaction.Rollback(); } catch { }
                    message = "Forward failed: " + ex.GetBaseException().Message;
                    return false;
                }
            }
        }

        #endregion


        #region Approve

        public static bool ApproveRequisition(
    string rowId,
    string toDepartmentId,
    string remarks,
    List<InvDepartmentRequisitionApprovedLine> approvedLines,
    string loggedInUser,
    out string message)
        {
            message = "";

            if (string.IsNullOrWhiteSpace(rowId))
            {
                message = "Requisition ID is required.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(toDepartmentId))
            {
                message = "To Department is required.";
                return false;
            }
            if (approvedLines == null || approvedLines.Count == 0)
            {
                message = "At least one approval item is required.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(loggedInUser))
            {
                message = "Logged-in user is required.";
                return false;
            }

            using (var repo = new GMContext())
            using (var transaction = repo.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    // 1. Check current status
                    int currentStatus = repo.Database.SqlQuery<int>(@"
                SELECT Status
                FROM InvDepartmentRequisition
                WHERE RowId = @RowId AND IsRemoved = 0 AND DomainStatus = 1
            ", new SqlParameter("@RowId", rowId)).FirstOrDefault();

                    if (currentStatus != STATUS_NEW && currentStatus != STATUS_FORWARDED)
                    {
                        message = "Only New or Forwarded requisition can be approved.";
                        transaction.Rollback();
                        return false;
                    }

                    // 2. Load all requisition details
                    var requestedDetails = repo.Database.SqlQuery<ApprovalDetailDbInfo>(@"
                SELECT
                     a.RowId, a.RequisitionDetailId, a.RequisitionId, a.ItemId,
                     CAST(ISNULL(a.Quantity, 0) AS DECIMAL(18,4)) AS Quantity,
                     a.UnitId,
                     CAST(ISNULL(a.UnitPriceEst, 0) AS DECIMAL(18,4)) AS UnitPriceEst,
                     CAST(ISNULL(a.TotalPriceEst, 0) AS DECIMAL(18,4)) AS TotalPriceEst,
                     CAST(ISNULL(a.ApprovedQuantity, 0) AS DECIMAL(18,4)) AS ApprovedQuantity
                FROM InvDepartmentRequisitionDetail a
                WHERE a.RequisitionId = @RequisitionId
                  AND a.DomainStatus = 1
                ORDER BY a.RequisitionDetailId
            ", new SqlParameter("@RequisitionId", rowId)).ToList();

                    if (requestedDetails.Count == 0)
                    {
                        message = "No requisition items were found.";
                        transaction.Rollback();
                        return false;
                    }

                    var detailDictionary = requestedDetails.ToDictionary(x => x.RowId, x => x);

                    // 3. Validation
                    if (approvedLines.Where(x => x != null).GroupBy(x => x.RowId).Any(g => g.Count() > 1))
                    {
                        message = "Duplicate approval item found.";
                        transaction.Rollback();
                        return false;
                    }

                    decimal totalApprovedQuantity = 0M;
                    decimal totalApprovedAmount = 0M;

                    foreach (var line in approvedLines)
                    {
                        if (line == null || string.IsNullOrWhiteSpace(line.RowId))
                        {
                            message = "Requisition detail RowId is missing.";
                            transaction.Rollback();
                            return false;
                        }

                        ApprovalDetailDbInfo detail;
                        if (!detailDictionary.TryGetValue(line.RowId, out detail))
                        {
                            message = "Invalid requisition detail RowId: " + line.RowId;
                            transaction.Rollback();
                            return false;
                        }

                        if (line.ApprovedQuantity < 0M)
                        {
                            message = "Approved quantity cannot be negative.";
                            transaction.Rollback();
                            return false;
                        }

                        if (line.ApprovedQuantity > detail.Quantity)
                        {
                            message = "Approved quantity cannot exceed requested quantity for item RowId " + line.RowId;
                            transaction.Rollback();
                            return false;
                        }

                        totalApprovedQuantity += line.ApprovedQuantity;
                        totalApprovedAmount += line.ApprovedQuantity * detail.UnitPriceEst;
                    }

                    if (totalApprovedQuantity <= 0M)
                    {
                        message = "At least one approved quantity must be greater than zero.";
                        transaction.Rollback();
                        return false;
                    }

                    // 4. Update ApprovedQuantity in detail table
                    foreach (var line in approvedLines)
                    {
                        int affected = repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvDepartmentRequisitionDetail
                    SET ApprovedQuantity = @ApprovedQuantity,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                      AND RequisitionId = @RequisitionId
                      AND DomainStatus = 1
                ",
                            new SqlParameter("@ApprovedQuantity", line.ApprovedQuantity),
                            new SqlParameter("@UpdateUser", loggedInUser),
                            new SqlParameter("@RowId", line.RowId),
                            new SqlParameter("@RequisitionId", rowId));

                        if (affected == 0)
                        {
                            message = "Unable to update approved quantity.";
                            transaction.Rollback();
                            return false;
                        }
                    }

                    // 5. Update Requisition Header → Approved
                    int headerAffected = repo.Database.ExecuteSqlCommand(@"
                UPDATE InvDepartmentRequisition
                SET Status = @StatusApproved,
                    IsApproved = 1,
                    UpdateUser = @UpdateUser,
                    UpdateDate = GETDATE()
                WHERE RowId = @RowId
                  AND IsRemoved = 0
                  AND DomainStatus = 1
                  AND Status IN (0, 1)
            ",
                        new SqlParameter("@StatusApproved", STATUS_APPROVED),
                        new SqlParameter("@UpdateUser", loggedInUser),
                        new SqlParameter("@RowId", rowId));

                    if (headerAffected == 0)
                    {
                        message = "Requisition could not be approved.";
                        transaction.Rollback();
                        return false;
                    }

                    // 6. Get FromDepartment from the latest Forward / previous transaction
                    string fromDepartmentId = repo.Database.SqlQuery<string>(@"
                SELECT TOP 1 ToDepartment
                FROM InvDepartmentTransaction
                WHERE RequisitionId = @RequisitionId
                  AND IsRemoved = 0
                  AND DomainStatus = 1
                  AND ToDepartment IS NOT NULL
                ORDER BY TransactionId DESC
            ", new SqlParameter("@RequisitionId", rowId)).FirstOrDefault();

                    // Fallback: if no previous transaction found, take original Department of Requisition
                    if (string.IsNullOrWhiteSpace(fromDepartmentId))
                    {
                        fromDepartmentId = repo.Database.SqlQuery<string>(@"
                    SELECT DepartmentId
                    FROM InvDepartmentRequisition
                    WHERE RowId = @RowId
                ", new SqlParameter("@RowId", rowId)).FirstOrDefault();
                    }

                    // 7. Create Approve Transaction
                    int transactionId = GetNextIntegerId(repo, "InvDepartmentTransaction", "TransactionId");
                    string transactionRowId = Guid.NewGuid().ToString();

                    repo.Database.ExecuteSqlCommand(@"
                INSERT INTO InvDepartmentTransaction
                (
                     RowId, TransactionId, TransactionCode, TransactionDate,
                     RequisitionId, MaterialReveiveId, MaterialIssueId,
                     TransactionType, FromDepartment, ToDepartment,
                     TotalAmount, Remarks, IsIssued, IsReceived,
                     IsRemoved, DomainStatus, CreateUser, CreateDate
                )
                VALUES
                (
                     @RowId, @TransactionId, @TransactionCode, GETDATE(),
                     @RequisitionId, '', '',
                     'Approve', @FromDepartment, @ToDepartment,
                     @TotalAmount, @Remarks, 0, 0,
                     0, 1, @CreateUser, GETDATE()
                )
            ",
                        new SqlParameter("@RowId", transactionRowId),
                        new SqlParameter("@TransactionId", transactionId),
                        new SqlParameter("@TransactionCode", "TRX-" + transactionId.ToString("D6")),
                        new SqlParameter("@RequisitionId", rowId),
                        new SqlParameter("@FromDepartment", fromDepartmentId),          // ← Fixed: Forward Department
                        new SqlParameter("@ToDepartment", toDepartmentId),
                        new SqlParameter("@TotalAmount", totalApprovedAmount),
                        new SqlParameter("@Remarks", DbValue(remarks)),
                        new SqlParameter("@CreateUser", loggedInUser));

                    // 8. Insert Transaction Details (only approved items)
                    int transactionDetailId = GetNextIntegerId(repo, "InvDepartmentTransactionDetail", "TransactionDetailId");

                    foreach (var detail in requestedDetails)
                    {
                        var line = approvedLines.FirstOrDefault(x => x != null && x.RowId == detail.RowId);
                        decimal approvedQty = line == null ? 0M : line.ApprovedQuantity;

                        if (approvedQty <= 0M)
                            continue;

                        decimal totalPrice = Math.Round(approvedQty * detail.UnitPriceEst, 2, MidpointRounding.AwayFromZero);

                        repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO InvDepartmentTransactionDetail
                    (
                         RowId, TransactionDetailId, TransactionId,
                         ItemId, UnitId, Quantity, MRRQty,
                         UnitPrice, TotalPrice, DomainStatus,
                         CreateUser, CreateDate
                    )
                    VALUES
                    (
                         @RowId, @TransactionDetailId, @TransactionId,
                         @ItemId, @UnitId, @Quantity, 0,
                         @UnitPrice, @TotalPrice, 1,
                         @CreateUser, GETDATE()
                    )
                ",
                            new SqlParameter("@RowId", Guid.NewGuid().ToString()),
                            new SqlParameter("@TransactionDetailId", transactionDetailId++),
                            new SqlParameter("@TransactionId", transactionRowId),
                            new SqlParameter("@ItemId", DbValue(detail.ItemId)),
                            new SqlParameter("@UnitId", DbValue(detail.UnitId)),
                            new SqlParameter("@Quantity", approvedQty),
                            new SqlParameter("@UnitPrice", detail.UnitPriceEst),
                            new SqlParameter("@TotalPrice", totalPrice),
                            new SqlParameter("@CreateUser", loggedInUser));
                    }

                    transaction.Commit();
                    message = "Requisition approved successfully.";
                    return true;
                }
                catch (Exception ex)
                {
                    try { transaction.Rollback(); } catch { }
                    message = "Approval failed: " + ex.GetBaseException().Message;
                    return false;
                }
            }
        }

        #endregion

        #region Reject

        public static bool RejectRequisition(
            string rowId, string loggedInUser, string remarks, out string message)
        {
            message = "";

            if (string.IsNullOrWhiteSpace(rowId))
            {
                message = "Requisition ID is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(loggedInUser))
            {
                message = "Logged-in user is required.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(remarks))
            {
                message = "Rejection reason is required.";
                return false;
            }

            using (var repo = new GMContext())
            using (var transaction = repo.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    int currentStatus = repo.Database.SqlQuery<int>(@"
                        SELECT Status
                        FROM InvDepartmentRequisition
                        WHERE RowId = @RowId AND IsRemoved = 0 AND DomainStatus = 1
                    ", new SqlParameter("@RowId", rowId)).FirstOrDefault();

                    if (currentStatus != STATUS_NEW && currentStatus != STATUS_FORWARDED)
                    {
                        message = "Only New or Forwarded requisition can be rejected.";
                        transaction.Rollback();
                        return false;
                    }

                    int affected = repo.Database.ExecuteSqlCommand(@"
                        UPDATE InvDepartmentRequisition
                        SET Status = @StatusRejected,
                            IsApproved = 0,
                            IsRemoved = 1,
                            UpdateUser = @UpdateUser,
                            UpdateDate = GETDATE()
                        WHERE RowId = @RowId
                          AND DomainStatus = 1
                          AND IsRemoved = 0
                          AND Status IN (0, 1)
                    ",
                        new SqlParameter("@StatusRejected", STATUS_REJECTED),
                        new SqlParameter("@UpdateUser", loggedInUser),
                        new SqlParameter("@RowId", rowId));

                    if (affected == 0)
                    {
                        message = "Requisition could not be rejected.";
                        transaction.Rollback();
                        return false;
                    }

                    int transactionId = GetNextIntegerId(repo, "InvDepartmentTransaction", "TransactionId");

                    repo.Database.ExecuteSqlCommand(@"
                        INSERT INTO InvDepartmentTransaction
                        (
                             RowId, TransactionId, TransactionCode, TransactionDate,
                             RequisitionId, MaterialReveiveId, MaterialIssueId,
                             TransactionType, FromDepartment, ToDepartment,
                             TotalAmount, Remarks, IsIssued, IsReceived,
                             IsRemoved, DomainStatus, CreateUser, CreateDate
                        )
                        VALUES
                        (
                             @RowId, @TransactionId, @TransactionCode, GETDATE(),
                             @RequisitionId, '', '',
                             'Reject', NULL, NULL,
                             0, @Remarks, 0, 0,
                             0, 1, @CreateUser, GETDATE()
                        )
                    ",
                        new SqlParameter("@RowId", Guid.NewGuid().ToString()),
                        new SqlParameter("@TransactionId", transactionId),
                        new SqlParameter("@TransactionCode", "TRX-" + transactionId.ToString("D6")),
                        new SqlParameter("@RequisitionId", rowId),
                        new SqlParameter("@Remarks", remarks),
                        new SqlParameter("@CreateUser", loggedInUser));

                    transaction.Commit();
                    message = "Requisition rejected successfully.";
                    return true;
                }
                catch (Exception ex)
                {
                    try { transaction.Rollback(); } catch { }
                    message = "Rejection failed: " + ex.GetBaseException().Message;
                    return false;
                }
            }
        }

        #endregion

        #region Helpers

        private static void InsertTransactionHeaderAndDetails(
            GMContext repo, string requisitionRowId, string transactionType,
            string remarks, decimal totalAmount,
            List<InvDepartmentRequisitionDetailInfo> details, string loggedInUser)
        {
            int transactionId = GetNextIntegerId(repo, "InvDepartmentTransaction", "TransactionId");
            string transactionRowId = Guid.NewGuid().ToString();

            repo.Database.ExecuteSqlCommand(@"
                INSERT INTO InvDepartmentTransaction
                (
                     RowId, TransactionId, TransactionCode, TransactionDate,
                     RequisitionId, MaterialReveiveId, MaterialIssueId,
                     TransactionType, FromDepartment, ToDepartment,
                     TotalAmount, Remarks, IsIssued, IsReceived,
                     IsRemoved, DomainStatus, CreateUser, CreateDate
                )
                VALUES
                (
                     @RowId, @TransactionId, @TransactionCode, GETDATE(),
                     @RequisitionId, '', '',
                     @TransactionType, NULL, NULL,
                     @TotalAmount, @Remarks, 0, 0,
                     0, 1, @CreateUser, GETDATE()
                )
            ",
                new SqlParameter("@RowId", transactionRowId),
                new SqlParameter("@TransactionId", transactionId),
                new SqlParameter("@TransactionCode", "TRX-" + transactionId.ToString("D6")),
                new SqlParameter("@RequisitionId", requisitionRowId),
                new SqlParameter("@TransactionType", transactionType),
                new SqlParameter("@TotalAmount", totalAmount),
                new SqlParameter("@Remarks", DbValue(remarks)),
                new SqlParameter("@CreateUser", loggedInUser));

            int transactionDetailId = GetNextIntegerId(repo, "InvDepartmentTransactionDetail", "TransactionDetailId");

            foreach (var line in details)
            {
                decimal quantity = line.Quantity;
                decimal unitPrice = line.UnitPriceEst;
                decimal totalPrice = Math.Round(quantity * unitPrice, 2, MidpointRounding.AwayFromZero);

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO InvDepartmentTransactionDetail
                    (
                         RowId, TransactionDetailId, TransactionId,
                         ItemId, UnitId, Quantity, MRRQty,
                         UnitPrice, TotalPrice, DomainStatus,
                         CreateUser, CreateDate
                    )
                    VALUES
                    (
                         @RowId, @TransactionDetailId, @TransactionId,
                         @ItemId, @UnitId, @Quantity, 0,
                         @UnitPrice, @TotalPrice, 1,
                         @CreateUser, GETDATE()
                    )
                ",
                    new SqlParameter("@RowId", Guid.NewGuid().ToString()),
                    new SqlParameter("@TransactionDetailId", transactionDetailId++),
                    new SqlParameter("@TransactionId", transactionRowId),
                    new SqlParameter("@ItemId", DbValue(line.ItemId)),
                    new SqlParameter("@UnitId", DbValue(line.UnitId)),
                    new SqlParameter("@Quantity", quantity),
                    new SqlParameter("@UnitPrice", unitPrice),
                    new SqlParameter("@TotalPrice", totalPrice),
                    new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        private static int GetNextIntegerId(GMContext repo, string tableName, string columnName)
        {
            string sql = "SELECT ISNULL(MAX(" + columnName + "), 0) + 1 FROM " + tableName + " WITH (UPDLOCK, HOLDLOCK)";
            return repo.Database.SqlQuery<int>(sql).Single();
        }

        private static void ValidateDetails(List<InvDepartmentRequisitionDetailInfo> details)
        {
            var duplicateCheck = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var line in details)
            {
                if (line == null)
                    throw new Exception("Invalid requisition detail.");

                if (string.IsNullOrWhiteSpace(line.ItemId))
                    throw new Exception("Item is required.");

                if (string.IsNullOrWhiteSpace(line.UnitId))
                    throw new Exception("Unit is required.");

                if (line.Quantity <= 0M)
                    throw new Exception("Quantity must be greater than zero.");

                if (line.UnitPriceEst < 0M)
                    throw new Exception("Unit price cannot be negative.");

                string key = line.ItemId.Trim() + "|" + line.UnitId.Trim();
                if (!duplicateCheck.Add(key))
                    throw new Exception("Duplicate Item and Unit are not allowed.");
            }
        }

        private static object DbValue(object value)
        {
            if (value == null)
                return DBNull.Value;

            if (value is string && string.IsNullOrWhiteSpace(Convert.ToString(value)))
                return DBNull.Value;

            return value;
        }

        #endregion
    }
}