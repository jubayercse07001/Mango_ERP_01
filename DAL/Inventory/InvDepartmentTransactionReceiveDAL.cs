using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

using ERP.DAL;

using ERP.Models.Inventory.InvDepartmentRequisition;
using ERP.Models.Inventory.InvDepartmentRequisitionDetail;
using ERP.Models.Inventory.InvUnit;
using ERP.Models.Inventory.UnitConversion;

namespace ERP.DAL.Inventory
{
    // ============================================================
    // Receive Line DTO
    // ============================================================
    public class InvDepartmentRequisitionReceivedLine
    {
        public string RowId { get; set; }

        public decimal ReceivedQuantity { get; set; }

        public string Remarks { get; set; }
    }


    // ============================================================
    // Receive Response
    // ============================================================
    public class InvDepartmentTransactionReceiveResponse
    {
        public List<InvDepartmentRequisitionInfo> Requisitions
        {
            get;
            set;
        }

        public int TotalRecordCount { get; set; }

        public int FilteredRecordCount { get; set; }


        public InvDepartmentTransactionReceiveResponse()
        {
            Requisitions =
                new List<InvDepartmentRequisitionInfo>();
        }
    }


    // ============================================================
    // Internal Receive Detail DB Model
    // ============================================================
    internal class ReceiveDetailDbInfo
    {
        public string RowId { get; set; }

        public int RequisitionDetailId { get; set; }

        public string RequisitionId { get; set; }

        public string ItemId { get; set; }

        public string UnitId { get; set; }

        public decimal Quantity { get; set; }

        public decimal ApprovedQuantity { get; set; }

        public decimal IssuedQuantity { get; set; }

        public decimal ReceivedQuantity { get; set; }

        public decimal UnitPriceEst { get; set; }

        public string RequestedUnitId { get; set; }        
        public string IssueUnitId { get; set; }

        public decimal IssueQuantity { get; set; }          
        public decimal ConversionRate { get; set; }        
        public decimal ConversionPrice { get; set; }        
        public string UnitConversionId { get; set; }        
    
    }


    // ============================================================
    // MAIN DAL
    // ============================================================
    public class InvDepartmentTransactionReceiveDAL
    {
        // --------------------------------------------------------
        // Status
        // --------------------------------------------------------
        private const int STATUS_ISSUED = 4;

        private const int STATUS_RECEIVED = 5;


        // ========================================================
        // 1. GET FILTERED ISSUED REQUISITIONS
        // ========================================================
        public static InvDepartmentTransactionReceiveResponse
            GetFilteredIssuedRequisitions(
                HttpRequestBase Request,
                string loggedInUserID)
        {
            var response =
                new InvDepartmentTransactionReceiveResponse();

            GMDataTableDAL gmdt =
                new GMDataTableDAL(Request);


            using (var repo = new GMContext())
            {
                // ------------------------------------------------
                // Base WHERE
                // ------------------------------------------------
                var fromWhere =
                    new StringBuilder(@"
                        FROM InvDepartmentRequisition a

                        LEFT JOIN HrDepartment b
                            ON b.RowId = a.DepartmentId

                        LEFT JOIN HrEmployee c
                            ON c.RowId = a.RequestedBy

                        WHERE a.DomainStatus = 1

                          AND a.IsRemoved = 0

                          AND a.Status = @StatusIssued

                          AND a.IsIssued = 1

                          AND ISNULL(a.IsReceived, 0) = 0
                    ");


                // ------------------------------------------------
                // Parameters
                // ------------------------------------------------
                var baseParameters =
                    new List<SqlParameter>
                    {
                        new SqlParameter(
                            "@StatusIssued",
                            STATUS_ISSUED)
                    };


                // ------------------------------------------------
                // Search fields
                // ------------------------------------------------
                string requisitionCode =
                    Request["RequisitionCode"];

                string requisitionName =
                    Request["RequisitionName"];

                string departmentId =
                    Request["DepartmentId"];

                string jobNo =
                    Request["JobNo"];

                string fromDate =
                    Request["FromDate"];

                string toDate =
                    Request["ToDate"];


                // ------------------------------------------------
                // Requisition Code
                // ------------------------------------------------
                if (!string.IsNullOrWhiteSpace(
                    requisitionCode))
                {
                    fromWhere.AppendLine(@"
                        AND a.RequisitionCode
                            LIKE '%' + @RequisitionCode + '%'");

                    baseParameters.Add(
                        new SqlParameter(
                            "@RequisitionCode",
                            requisitionCode));
                }


                // ------------------------------------------------
                // Requisition Name
                // ------------------------------------------------
                //if (!string.IsNullOrWhiteSpace(
                //    requisitionName))
                //{
                //    fromWhere.AppendLine(@"
                //        AND a.RequisitionName
                //            LIKE '%' + @RequisitionName + '%'");

                //    baseParameters.Add(
                //        new SqlParameter(
                //            "@RequisitionName,
                //            requisitionName));
                //}


                // ------------------------------------------------
                // Department
                // ------------------------------------------------
                if (!string.IsNullOrWhiteSpace(
                    departmentId))
                {
                    fromWhere.AppendLine(@"
                        AND a.DepartmentId = @DepartmentId");

                    baseParameters.Add(
                        new SqlParameter(
                            "@DepartmentId",
                            departmentId));
                }


                // ------------------------------------------------
                // Job No
                // ------------------------------------------------
                if (!string.IsNullOrWhiteSpace(jobNo))
                {
                    fromWhere.AppendLine(@"
                        AND a.JobNo
                            LIKE '%' + @JobNo + '%'");

                    baseParameters.Add(
                        new SqlParameter(
                            "@JobNo",
                            jobNo));
                }


                // ------------------------------------------------
                // Date variables
                // ------------------------------------------------
                DateTime parsedDate;


                // ------------------------------------------------
                // From Date
                // ------------------------------------------------
                if (!string.IsNullOrWhiteSpace(fromDate)
                    &&
                    DateTime.TryParse(
                        fromDate,
                        out parsedDate))
                {
                    fromWhere.AppendLine(@"
                        AND a.RequisitionDate >= @FromDate");

                    baseParameters.Add(
                        new SqlParameter(
                            "@FromDate",
                            parsedDate.Date));
                }


                // ------------------------------------------------
                // To Date
                // ------------------------------------------------
                if (!string.IsNullOrWhiteSpace(toDate)
                    &&
                    DateTime.TryParse(
                        toDate,
                        out parsedDate))
                {
                    fromWhere.AppendLine(@"
                        AND a.RequisitionDate < @ToDateExclusive");

                    baseParameters.Add(
                        new SqlParameter(
                            "@ToDateExclusive",
                            parsedDate.Date.AddDays(1)));
                }


                // ------------------------------------------------
                // Total Record Count
                // ------------------------------------------------
                response.TotalRecordCount =
                    repo.Database
                        .SqlQuery<int>(
                            "SELECT COUNT(1) "
                            + fromWhere,
                            CloneParameters(
                                baseParameters)
                            .ToArray())
                        .Single();


                // ------------------------------------------------
                // DataTable Search
                // ------------------------------------------------
                var filteredWhere =
                    new StringBuilder(
                        fromWhere.ToString());

                var filteredParameters =
                    CloneParameters(
                        baseParameters);


                if (!string.IsNullOrWhiteSpace(
                    gmdt.SearchValue))
                {
                    filteredWhere.AppendLine(@"
                        AND
                        (
                               a.RequisitionCode
                                   LIKE '%' + @SearchValue + '%'

                            OR a.RequisitionName
                                   LIKE '%' + @SearchValue + '%'

                            OR b.DepartmentName
                                   LIKE '%' + @SearchValue + '%'

                            OR c.LastName
                                   LIKE '%' + @SearchValue + '%'

                            OR a.JobNo
                                   LIKE '%' + @SearchValue + '%'
                        )");


                    filteredParameters.Add(
                        new SqlParameter(
                            "@SearchValue",
                            gmdt.SearchValue));
                }


                // ------------------------------------------------
                // Filtered Record Count
                // ------------------------------------------------
                response.FilteredRecordCount =
                    repo.Database
                        .SqlQuery<int>(
                            "SELECT COUNT(1) "
                            + filteredWhere,
                            CloneParameters(
                                filteredParameters)
                            .ToArray())
                        .Single();


                // ------------------------------------------------
                // Sorting
                // ------------------------------------------------
                string sortColumn =
                    Convert.ToString(
                        gmdt.SortColum);


                if (string.IsNullOrWhiteSpace(
                    sortColumn))
                {
                    sortColumn =
                        "a.RequisitionDate";
                }


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
                    case "IsReceived":

                        sortColumn =
                            "a." + sortColumn;

                        break;


                    case "DepartmentName":

                        sortColumn =
                            "b.DepartmentName";

                        break;


                    case "RequestedByName":

                        sortColumn =
                            "c.LastName";

                        break;


                    default:

                        sortColumn =
                            "a.RequisitionDate";

                        break;
                }


                string sortDirection =
                    string.Equals(
                        gmdt.SortDir,
                        "desc",
                        StringComparison.OrdinalIgnoreCase)
                        ? "DESC"
                        : "ASC";


                // ------------------------------------------------
                // Paging
                // ------------------------------------------------
                var listParameters =
                    CloneParameters(
                        filteredParameters);


                listParameters.Add(
                    new SqlParameter(
                        "@Skip",
                        gmdt.Start));


                listParameters.Add(
                    new SqlParameter(
                        "@Take",
                        gmdt.Length > 0
                            ? gmdt.Length
                            : 10));


                // ------------------------------------------------
                // SELECT
                // ------------------------------------------------
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
                        ,ISNULL(
                            a.IsReceived,
                            0)
                            AS IsReceived
                    "
                    + filteredWhere
                    + @"
                    ORDER BY "
                    + sortColumn
                    + " "
                    + sortDirection
                    + @"
                    OFFSET @Skip ROWS
                    FETCH NEXT @Take ROWS ONLY";


                response.Requisitions =
                    repo.Database
                        .SqlQuery<
                            InvDepartmentRequisitionInfo>(
                                sql,
                                listParameters.ToArray())
                        .ToList();
            }


            return response;
        }


        // ========================================================
        // 2. GET REQUISITION BY ROW ID
        // ========================================================
        public static InvDepartmentRequisitionInfo
            GetRequisitionByID(
                string rowId)
        {
            if (string.IsNullOrWhiteSpace(rowId))
                return null;


            using (var repo = new GMContext())
            {
                var requisition =
                    repo.Database
                        .SqlQuery<
                            InvDepartmentRequisitionInfo>(
                            @"
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
                                ,a.RequiredFor
                                ,a.Status
                                ,a.IsApproved
                                ,a.IsIssued
                                ,ISNULL(
                                    a.IsReceived,
                                    0)
                                    AS IsReceived
                                ,a.IsRemoved

                            FROM InvDepartmentRequisition a

                            LEFT JOIN HrDepartment b
                                ON b.RowId =
                                   a.DepartmentId

                            LEFT JOIN HrEmployee c
                                ON c.RowId =
                                   a.RequestedBy

                            WHERE a.RowId = @RowId

                              AND a.DomainStatus = 1

                              AND a.IsRemoved = 0

                              AND a.Status = @StatusIssued

                              AND a.IsIssued = 1

                              AND ISNULL(
                                    a.IsReceived,
                                    0) = 0
                            ",
                            new SqlParameter(
                                "@RowId",
                                rowId),

                            new SqlParameter(
                                "@StatusIssued",
                                STATUS_ISSUED))
                        .FirstOrDefault();


                if (requisition == null)
                    return null;


                requisition.Details =
                    GetRequisitionDetailsByRequisitionId(
                        requisition.RowId);


                return requisition;
            }
        }


        // ========================================================
        // 3. GET REQUISITION DETAILS
        // ========================================================
        //public static List<
        //    InvDepartmentRequisitionDetailInfo>
        //    GetRequisitionDetailsByRequisitionId(
        //        string requisitionId)
        //{
        //    if (string.IsNullOrWhiteSpace(
        //        requisitionId))
        //    {
        //        return new List<
        //            InvDepartmentRequisitionDetailInfo>();
        //    }


        //    using (var repo = new GMContext())
        //    {
        //        return repo.Database
        //            .SqlQuery<
        //                InvDepartmentRequisitionDetailInfo>(
        //                @"
        //                SELECT
        //                     a.RowId
        //                    ,a.RequisitionDetailId
        //                    ,a.RequisitionId
        //                    ,a.ItemId

        //                    ,ISNULL(
        //                        a.Quantity,
        //                        0)
        //                        AS Quantity

        //                    ,a.UnitId

        //                    ,ISNULL(
        //                        a.UnitPriceEst,
        //                        0)
        //                        AS UnitPriceEst

        //                    ,ISNULL(
        //                        a.TotalPriceEst,
        //                        0)
        //                        AS TotalPriceEst

        //                    ,ISNULL(
        //                        a.ApprovedQuantity,
        //                        0)
        //                        AS ApprovedQuantity

        //                    ,ISNULL(
        //                        a.IssuedQuantity,
        //                        0)
        //                        AS IssuedQuantity

        //                    ,ISNULL(
        //                        a.ReceivedQuantity,
        //                        0)
        //                        AS ReceivedQuantity

        //                    ,a.Remarks

        //                    ,b.ItemName

        //                    ,c.MinUnitName

        //                FROM InvDepartmentRequisitionDetail a

        //                LEFT JOIN InvItem b
        //                    ON b.RowId =
        //                       a.ItemId

        //                LEFT JOIN InvUnit c
        //                    ON c.RowId =
        //                       a.UnitId

        //                WHERE a.RequisitionId =
        //                      @RequisitionId

        //                  AND a.DomainStatus = 1

        //                  AND ISNULL(
        //                        a.IssuedQuantity,
        //                        0) > 0

        //                ORDER BY
        //                    a.RequisitionDetailId
        //                ",
        //                new SqlParameter(
        //                    "@RequisitionId",
        //                    requisitionId))
        //            .ToList();
        //    }
        //}
        public static List<InvDepartmentRequisitionDetailInfo>
    GetRequisitionDetailsByRequisitionId(string requisitionId)
        {
            if (string.IsNullOrWhiteSpace(requisitionId))
            {
                return new List<InvDepartmentRequisitionDetailInfo>();
            }

            using (var repo = new GMContext())
            {
                // ১. সর্বশেষ Issue Transaction-এর RowId বের করা
                string latestIssueTransactionRowId = repo.Database.SqlQuery<string>(@"
            SELECT TOP 1 RowId
            FROM InvDepartmentTransaction
            WHERE RequisitionId = @RequisitionId
              AND TransactionType = 'Issue'
              AND IsRemoved = 0
              AND DomainStatus = 1
            ORDER BY TransactionId DESC
        ", new SqlParameter("@RequisitionId", requisitionId)).FirstOrDefault();

                if (string.IsNullOrWhiteSpace(latestIssueTransactionRowId))
                {
                    // যদি Issue Transaction না পাওয়া যায়, তাহলে পুরনো পদ্ধতিতে Requested Unit দেখাবে
                    return GetRequisitionDetailsFallback(requisitionId);
                }

                // ২. Issue Transaction Detail + Requisition Detail জয়েন করে ডাটা আনা
                return repo.Database.SqlQuery<InvDepartmentRequisitionDetailInfo>(@"
            SELECT
                 rd.RowId
                ,rd.RequisitionDetailId
                ,rd.RequisitionId
                ,rd.ItemId
                ,ISNULL(rd.Quantity, 0)              AS Quantity
                ,td.UnitId                           AS UnitId              -- ← Issue Unit
                ,ISNULL(rd.UnitPriceEst, 0)          AS UnitPriceEst
                ,ISNULL(rd.TotalPriceEst, 0)         AS TotalPriceEst
                ,ISNULL(rd.ApprovedQuantity, 0)      AS ApprovedQuantity
                ,ISNULL(td.Quantity, 0)              AS IssuedQuantity      -- ← আসল Issue Qty
                ,ISNULL(rd.ReceivedQuantity, 0)      AS ReceivedQuantity
                ,rd.Remarks
                ,i.ItemName
                ,u.MinUnitName                       AS MinUnitName         -- ← Issue Unit Name
                ,ISNULL(td.ConversionRate, 1)        AS ConversionRate
                ,ISNULL(td.ConversionPrice, 0)       AS ConversionPrice
                ,ISNULL(td.ConversionTotal, 0)       AS ConversionTotal
                ,td.UnitConversionId
            FROM InvDepartmentTransactionDetail td
            INNER JOIN InvDepartmentRequisitionDetail rd
                ON rd.ItemId = td.ItemId
               AND rd.RequisitionId = @RequisitionId
               AND rd.DomainStatus = 1
            LEFT JOIN InvItem i
                ON i.RowId = rd.ItemId
            LEFT JOIN InvUnit u
                ON u.RowId = td.UnitId
            WHERE td.TransactionId = @TransactionRowId
              AND td.DomainStatus = 1
              AND ISNULL(td.Quantity, 0) > 0
            ORDER BY rd.RequisitionDetailId
        ",
                new SqlParameter("@RequisitionId", requisitionId),
                new SqlParameter("@TransactionRowId", latestIssueTransactionRowId))
                .ToList();
            }
        }

        // Fallback method (Issue Transaction না পাওয়া গেলে)
        private static List<InvDepartmentRequisitionDetailInfo>
            GetRequisitionDetailsFallback(string requisitionId)
        {
            using (var repo = new GMContext())
            {
                return repo.Database.SqlQuery<InvDepartmentRequisitionDetailInfo>(@"
            SELECT
                 a.RowId
                ,a.RequisitionDetailId
                ,a.RequisitionId
                ,a.ItemId
                ,ISNULL(a.Quantity, 0)           AS Quantity
                ,a.UnitId
                ,ISNULL(a.UnitPriceEst, 0)       AS UnitPriceEst
                ,ISNULL(a.TotalPriceEst, 0)      AS TotalPriceEst
                ,ISNULL(a.ApprovedQuantity, 0)   AS ApprovedQuantity
                ,ISNULL(a.IssuedQuantity, 0)     AS IssuedQuantity
                ,ISNULL(a.ReceivedQuantity, 0)   AS ReceivedQuantity
                ,a.Remarks
                ,b.ItemName
                ,c.MinUnitName
            FROM InvDepartmentRequisitionDetail a
            LEFT JOIN InvItem b ON b.RowId = a.ItemId
            LEFT JOIN InvUnit c ON c.RowId = a.UnitId
            WHERE a.RequisitionId = @RequisitionId
              AND a.DomainStatus = 1
              AND ISNULL(a.IssuedQuantity, 0) > 0
            ORDER BY a.RequisitionDetailId
        ", new SqlParameter("@RequisitionId", requisitionId)).ToList();
            }
        }



        // ========================================================
        // 4. RECEIVE REQUISITION
        // ========================================================
        public static bool ReceiveRequisition(
    string rowId,
    string remarks,
    List<InvDepartmentRequisitionReceivedLine> receivedLines,
    string loggedInUser,
    out string message)
        {
            message = "";

            if (string.IsNullOrWhiteSpace(rowId))
            {
                message = "Requisition ID is required.";
                return false;
            }
            if (receivedLines == null || receivedLines.Count == 0)
            {
                message = "At least one receive item is required.";
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
                    // 1. Lock Header
                    var header = repo.Database.SqlQuery<InvDepartmentRequisitionInfo>(@"
                SELECT Status, IsApproved, IsIssued,
                       ISNULL(IsReceived, 0) AS IsReceived,
                       DepartmentId
                FROM InvDepartmentRequisition WITH (UPDLOCK, HOLDLOCK)
                WHERE RowId = @RowId
                  AND IsRemoved = 0
                  AND DomainStatus = 1
            ", new SqlParameter("@RowId", rowId)).FirstOrDefault();

                    if (header == null)
                    {
                        message = "Requisition was not found.";
                        transaction.Rollback();
                        return false;
                    }

                    if (header.Status != STATUS_ISSUED ||
                        Convert.ToInt32(header.IsIssued) != 1 ||
                        Convert.ToInt32(header.IsReceived) == 1)
                    {
                        message = "Only Issued and not-yet-received requisitions can be received.";
                        transaction.Rollback();
                        return false;
                    }

                    // 2. Get latest Issue Transaction
                    string latestIssueTransactionRowId = repo.Database.SqlQuery<string>(@"
                SELECT TOP 1 RowId
                FROM InvDepartmentTransaction
                WHERE RequisitionId = @RequisitionId
                  AND TransactionType = 'Issue'
                  AND IsRemoved = 0
                  AND DomainStatus = 1
                ORDER BY TransactionId DESC
            ", new SqlParameter("@RequisitionId", rowId)).FirstOrDefault();

                    if (string.IsNullOrWhiteSpace(latestIssueTransactionRowId))
                    {
                        message = "No Issue transaction found for this requisition.";
                        transaction.Rollback();
                        return false;
                    }

                    // 3. Load Details with Issue info
                    var details = repo.Database.SqlQuery<ReceiveDetailDbInfo>(@"
                SELECT
                     rd.RowId
                    ,rd.RequisitionDetailId
                    ,rd.RequisitionId
                    ,rd.ItemId
                    ,rd.UnitId                          AS RequestedUnitId
                    ,td.UnitId                          AS IssueUnitId
                    ,ISNULL(rd.Quantity, 0)             AS Quantity
                    ,ISNULL(rd.ApprovedQuantity, 0)     AS ApprovedQuantity
                    ,ISNULL(rd.IssuedQuantity, 0)       AS IssuedQuantity      -- Requested Unit equivalent
                    ,ISNULL(rd.ReceivedQuantity, 0)     AS ReceivedQuantity
                    ,ISNULL(rd.UnitPriceEst, 0)         AS UnitPriceEst
                    ,ISNULL(td.Quantity, 0)             AS IssueQuantity       -- Actual issued qty in Issue Unit
                    ,ISNULL(td.ConversionRate, 1)       AS ConversionRate
                    ,ISNULL(td.ConversionPrice, 0)      AS ConversionPrice
                    ,ISNULL(td.UnitConversionId, '')    AS UnitConversionId
                FROM InvDepartmentRequisitionDetail rd WITH (UPDLOCK, HOLDLOCK)
                INNER JOIN InvDepartmentTransactionDetail td
                    ON td.ItemId = rd.ItemId
                   AND td.TransactionId = @IssueTransactionId
                   AND td.DomainStatus = 1
                WHERE rd.RequisitionId = @RequisitionId
                  AND rd.DomainStatus = 1
                  AND ISNULL(rd.IssuedQuantity, 0) > 0
                ORDER BY rd.RequisitionDetailId
            ",
                    new SqlParameter("@RequisitionId", rowId),
                    new SqlParameter("@IssueTransactionId", latestIssueTransactionRowId)).ToList();

                    if (details.Count == 0)
                    {
                        message = "No issued items found.";
                        transaction.Rollback();
                        return false;
                    }

                    var detailDictionary = details.ToDictionary(x => x.RowId, x => x);

                    // 4. Duplicate check
                    if (receivedLines.Where(x => x != null).GroupBy(x => x.RowId).Any(g => g.Count() > 1))
                    {
                        message = "Duplicate receive item found.";
                        transaction.Rollback();
                        return false;
                    }

                    // 5. Validate lines
                    decimal totalAmount = 0M;
                    var qtyMap = new Dictionary<string, decimal>();          // key = detail.RowId, value = received qty in Requested Unit
                    var validDetails = new List<ReceiveDetailDbInfo>();

                    foreach (var line in receivedLines)
                    {
                        if (line == null || string.IsNullOrWhiteSpace(line.RowId))
                        {
                            message = "Requisition detail RowId is missing.";
                            transaction.Rollback();
                            return false;
                        }

                        ReceiveDetailDbInfo detail;
                        if (!detailDictionary.TryGetValue(line.RowId, out detail))
                        {
                            message = "Invalid requisition detail RowId: " + line.RowId;
                            transaction.Rollback();
                            return false;
                        }

                        if (line.ReceivedQuantity < 0)
                        {
                            message = "Received quantity cannot be negative.";
                            transaction.Rollback();
                            return false;
                        }

                        if (line.ReceivedQuantity == 0)
                            continue;

                        // Remaining in Requested Unit
                        decimal remaining = detail.IssuedQuantity - detail.ReceivedQuantity;
                        if (remaining < 0) remaining = 0;

                        // Convert received qty (if user entered in Issue Unit) to Requested Unit equivalent
                        // Assuming frontend sends quantity in the same unit that is shown (Issue Unit)
                        decimal receivedInRequestedUnit = line.ReceivedQuantity;

                        if (detail.IssueUnitId != detail.RequestedUnitId && detail.ConversionRate > 0)
                        {
                            // User entered in Issue Unit → convert to Requested Unit
                            receivedInRequestedUnit = line.ReceivedQuantity / detail.ConversionRate;
                        }

                        if (receivedInRequestedUnit > remaining + 0.0001M)
                        {
                            message = "Received quantity exceeds remaining issued quantity. Remaining: " +
                                      remaining.ToString("0.####");
                            transaction.Rollback();
                            return false;
                        }

                        totalAmount += receivedInRequestedUnit * detail.UnitPriceEst;
                        qtyMap[line.RowId] = receivedInRequestedUnit;
                        validDetails.Add(detail);
                    }

                    if (qtyMap.Count == 0)
                    {
                        message = "At least one received quantity must be greater than zero.";
                        transaction.Rollback();
                        return false;
                    }

                    // 6. Update ReceivedQuantity (always in Requested Unit)
                    foreach (var kv in qtyMap)
                    {
                        int affected = repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvDepartmentRequisitionDetail
                    SET ReceivedQuantity = ISNULL(ReceivedQuantity, 0) + @ReceivedQuantity,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                      AND RequisitionId = @RequisitionId
                      AND DomainStatus = 1
                ",
                        new SqlParameter("@ReceivedQuantity", kv.Value),
                        new SqlParameter("@UpdateUser", loggedInUser),
                        new SqlParameter("@RowId", kv.Key),
                        new SqlParameter("@RequisitionId", rowId));

                        if (affected == 0)
                        {
                            message = "Unable to update received quantity.";
                            transaction.Rollback();
                            return false;
                        }
                    }

                    // 7. Create Receive Transaction
                    int transactionId = GetNextIntegerId(repo, "InvDepartmentTransaction", "TransactionId");
                    string transactionRowId = Guid.NewGuid().ToString();
                    string transactionCode = "TRX-" + transactionId.ToString("D6");

                    // FromDepartment = Issue Transaction-এর ToDepartment বা Header Department
                    string fromDepartmentId = repo.Database.SqlQuery<string>(@"
                SELECT TOP 1 ToDepartment
                FROM InvDepartmentTransaction
                WHERE RowId = @TransactionId
            ", new SqlParameter("@TransactionId", latestIssueTransactionRowId)).FirstOrDefault()
                    ?? header.DepartmentId;

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
                     'Receive', @FromDepartment, @ToDepartment,
                     @TotalAmount, @Remarks, 0, 1,
                     0, 1, @CreateUser, GETDATE()
                )
            ",
                    new SqlParameter("@RowId", transactionRowId),
                    new SqlParameter("@TransactionId", transactionId),
                    new SqlParameter("@TransactionCode", transactionCode),
                    new SqlParameter("@RequisitionId", rowId),
                    new SqlParameter("@FromDepartment", fromDepartmentId),
                    new SqlParameter("@ToDepartment", header.DepartmentId),
                    new SqlParameter("@TotalAmount", Math.Round(totalAmount, 2, MidpointRounding.AwayFromZero)),
                    new SqlParameter("@Remarks", DbValue(remarks)),
                    new SqlParameter("@CreateUser", loggedInUser));

                    // 8. Insert Transaction Details
                    int transactionDetailId = GetNextIntegerId(repo, "InvDepartmentTransactionDetail", "TransactionDetailId");

                    foreach (var detail in validDetails)
                    {
                        decimal qtyInRequestedUnit = qtyMap[detail.RowId];
                        decimal price = detail.UnitPriceEst;
                        decimal total = Math.Round(qtyInRequestedUnit * price, 2, MidpointRounding.AwayFromZero);

                        // Store in Requested Unit for consistency
                        repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO InvDepartmentTransactionDetail
                    (
                         RowId, TransactionDetailId, TransactionId,
                         ItemId, UnitId, Quantity, MRRQty,
                         UnitPrice, TotalPrice,
                         UnitConversionId, ConversionRate, ConversionPrice, ConversionTotal,
                         DomainStatus, CreateUser, CreateDate
                    )
                    VALUES
                    (
                         @RowId, @TransactionDetailId, @TransactionId,
                         @ItemId, @UnitId, @Quantity, 0,
                         @UnitPrice, @TotalPrice,
                         @UnitConversionId, @ConversionRate, @ConversionPrice, @ConversionTotal,
                         1, @CreateUser, GETDATE()
                    )
                ",
                        new SqlParameter("@RowId", Guid.NewGuid().ToString()),
                        new SqlParameter("@TransactionDetailId", transactionDetailId++),
                        new SqlParameter("@TransactionId", transactionRowId),
                        new SqlParameter("@ItemId", DbValue(detail.ItemId)),
                        new SqlParameter("@UnitId", DbValue(detail.RequestedUnitId)),   // Requested Unit
                        new SqlParameter("@Quantity", qtyInRequestedUnit),
                        new SqlParameter("@UnitPrice", price),
                        new SqlParameter("@TotalPrice", total),
                        new SqlParameter("@UnitConversionId", DbValue(detail.UnitConversionId)),
                        new SqlParameter("@ConversionRate", detail.ConversionRate),
                        new SqlParameter("@ConversionPrice", detail.ConversionPrice),
                        new SqlParameter("@ConversionTotal", total),
                        new SqlParameter("@CreateUser", loggedInUser));
                    }

                    // 9. Check if fully received
                    int remainingCount = repo.Database.SqlQuery<int>(@"
                SELECT COUNT(1)
                FROM InvDepartmentRequisitionDetail
                WHERE RequisitionId = @RequisitionId
                  AND DomainStatus = 1
                  AND ISNULL(IssuedQuantity, 0) > 0
                  AND ISNULL(ReceivedQuantity, 0) < ISNULL(IssuedQuantity, 0)
            ", new SqlParameter("@RequisitionId", rowId)).Single();

                    if (remainingCount == 0)
                    {
                        // Fully Received
                        int headerAffected = repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvDepartmentRequisition
                    SET Status = @StatusReceived,
                        IsReceived = 1,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                      AND IsRemoved = 0
                      AND DomainStatus = 1
                      AND Status = @StatusIssued
                      AND IsIssued = 1
                      AND ISNULL(IsReceived, 0) = 0
                ",
                        new SqlParameter("@StatusReceived", STATUS_RECEIVED),
                        new SqlParameter("@UpdateUser", loggedInUser),
                        new SqlParameter("@RowId", rowId),
                        new SqlParameter("@StatusIssued", STATUS_ISSUED));

                        if (headerAffected == 0)
                        {
                            message = "Requisition could not be marked as received.";
                            transaction.Rollback();
                            return false;
                        }

                        transaction.Commit();
                        message = "Requisition received successfully.";
                        return true;
                    }
                    else
                    {
                        // Partial Receive
                        repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvDepartmentRequisition
                    SET UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                        new SqlParameter("@UpdateUser", loggedInUser),
                        new SqlParameter("@RowId", rowId));

                        transaction.Commit();
                        message = "Requisition partially received successfully.";
                        return true;
                    }
                }
                catch (Exception ex)
                {
                    try { transaction.Rollback(); } catch { }
                    message = "Receive failed: " + ex.GetBaseException().Message;
                    return false;
                }
            }
        }


        // ========================================================
        // 5. GET NEXT INTEGER ID
        // ========================================================
        private static int GetNextIntegerId(
            GMContext repo,
            string tableName,
            string columnName)
        {
            string sql =
                "SELECT ISNULL(MAX("
                +
                columnName
                +
                "), 0) + 1 FROM "
                +
                tableName
                +
                " WITH (UPDLOCK, HOLDLOCK)";


            return repo.Database
                .SqlQuery<int>(sql)
                .Single();
        }


        // ========================================================
        // 6. DB VALUE
        // ========================================================
        private static object DbValue(
            object value)
        {
            if (value == null)
                return DBNull.Value;


            if (
                value is string
                &&
                string.IsNullOrWhiteSpace(
                    Convert.ToString(value))
               )
            {
                return DBNull.Value;
            }


            return value;
        }


        // ========================================================
        // 7. CLONE SQL PARAMETERS
        // ========================================================
        private static List<SqlParameter>
            CloneParameters(
                IEnumerable<SqlParameter> source)
        {
            var result =
                new List<SqlParameter>();


            foreach (SqlParameter p in source)
            {
                result.Add(
                    new SqlParameter
                    {
                        ParameterName =
                            p.ParameterName,

                        SqlDbType =
                            p.SqlDbType,

                        Direction =
                            p.Direction,

                        Size =
                            p.Size,

                        Precision =
                            p.Precision,

                        Scale =
                            p.Scale,

                        Value =
                            p.Value ?? DBNull.Value
                    });
            }


            return result;
        }


        // ========================================================
        // 8. GET UNIT LIST
        // ========================================================
        public static List<InvUnitInfo>
            GetUnitList()
        {
            using (var repo = new GMContext())
            {
                return repo.Database
                    .SqlQuery<InvUnitInfo>(
                        @"
                        SELECT
                             RowId
                            ,MinUnitName

                        FROM InvUnit

                        WHERE ISNULL(
                            DomainStatus,
                            1) = 1

                        ORDER BY
                            MinUnitName
                        ")
                    .ToList();
            }
        }


        // ========================================================
        // 9. INTERNAL UNIT CONVERSION
        //
        // IMPORTANT:
        // Using existing model:
        // ERP.Models.Inventory.UnitConversion.UnitConversionInfo
        //
        // No UnitConversionDbInfo class is required.
        // ========================================================
        private static UnitConversionInfo
            GetUnitConversion(
                GMContext repo,
                string requestedUnitId,
                string receiveUnitId)
        {
            if (
                string.IsNullOrWhiteSpace(
                    requestedUnitId)
                ||
                string.IsNullOrWhiteSpace(
                    receiveUnitId)
               )
            {
                return null;
            }


            return repo.Database
                .SqlQuery<UnitConversionInfo>(
                    @"
                    SELECT TOP 1
                         RowId
                        ,FirstUnitId
                        ,SecondUnitId

                        ,ISNULL(
                            ConversionRate,
                            0)
                            AS ConversionRate

                    FROM InvUnitConversion

                    WHERE ISNULL(
                            DomainStatus,
                            1) = 1

                      AND ISNULL(
                            IsRemoved,
                            0) = 0

                      AND
                      (
                            (
                                FirstUnitId =
                                    @RequestedUnitId

                                AND

                                SecondUnitId =
                                    @ReceiveUnitId
                            )

                            OR

                            (
                                FirstUnitId =
                                    @ReceiveUnitId

                                AND

                                SecondUnitId =
                                    @RequestedUnitId
                            )
                      )

                    ORDER BY
                        RowId
                    ",
                    new SqlParameter(
                        "@RequestedUnitId",
                        requestedUnitId),

                    new SqlParameter(
                        "@ReceiveUnitId",
                        receiveUnitId))
                .FirstOrDefault();
        }


        // ========================================================
        // 10. PUBLIC UNIT CONVERSION
        // ========================================================
        public static UnitConversionInfo
            GetUnitConversionPublic(
                string requestedUnitId,
                string receiveUnitId)
        {
            using (var repo = new GMContext())
            {
                return GetUnitConversion(
                    repo,
                    requestedUnitId,
                    receiveUnitId);
            }
        }
    }
}