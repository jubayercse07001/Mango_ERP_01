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

namespace ERP.DAL.Inventory
{
    // ================================================================
    // ISSUE LINE DTO
    // ================================================================

    public class InvDepartmentRequisitionIssuedLine
    {
        public string RowId { get; set; }

        public decimal IssuedQuantity { get; set; }

        public string IssueUnitId { get; set; }

        // These values are received from UI for display only.
        // SERVER recalculates them and does NOT trust them.
        public string UnitConversionId { get; set; }

        public decimal ConversionRate { get; set; }

        public decimal ConversionPrice { get; set; }

        public decimal ConversionTotal { get; set; }

        public string Remarks { get; set; }
    }


    // ================================================================
    // UNIT LIST DTO
    // ================================================================

    public class InvDepartmentTransactionIssueUnitInfo
    {
        public string RowId { get; set; }

        public string MinUnitName { get; set; }
    }


    // ================================================================
    // RESPONSE
    // ================================================================

    public class InvDepartmentTransactionIssueResponse
    {
        public List<InvDepartmentRequisitionInfo>
            Requisitions
        {
            get;
            set;
        }

        public int TotalRecordCount
        {
            get;
            set;
        }

        public int FilteredRecordCount
        {
            get;
            set;
        }


        public InvDepartmentTransactionIssueResponse()
        {
            Requisitions =
                new List<InvDepartmentRequisitionInfo>();

            TotalRecordCount = 0;

            FilteredRecordCount = 0;
        }
    }


    // ================================================================
    // INTERNAL REQUISITION DETAIL
    // ================================================================

    internal class IssueDetailDbInfo
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

        public decimal IssuedQuantity { get; set; }
    }


    // ================================================================
    // DATABASE UNIT CONVERSION
    //
    // EXACT InvUnitConversion STRUCTURE
    // ================================================================

    internal class UnitConversionDbInfo
    {
        public string RowId { get; set; }

        public string FirstUnitId { get; set; }

        public string SecondUnitId { get; set; }

        public decimal ConversionRate { get; set; }
    }


    // ================================================================
    // CALCULATED ISSUE LINE
    // ================================================================

    internal class CalculatedIssueLine
    {
        public string DetailRowId { get; set; }

        public string ItemId { get; set; }

        public string RequestedUnitId { get; set; }

        public string IssueUnitId { get; set; }

        public decimal IssueQuantity { get; set; }

        // Quantity converted back to requisition unit.
        public decimal RequestedEquivalentQuantity { get; set; }

        public string UnitConversionId { get; set; }

        public decimal ConversionRate { get; set; }

        public decimal ConversionPrice { get; set; }

        public decimal ConversionTotal { get; set; }

        public string Remarks { get; set; }
    }


    // ================================================================
    // CONVERSION RESULT
    // ================================================================

    public class InvDepartmentTransactionIssueConversionInfo
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public bool SameUnit { get; set; }

        public string UnitConversionId { get; set; }

        public decimal ConversionRate { get; set; }

        public decimal ConversionPrice { get; set; }

        public decimal ConversionTotal { get; set; }

        public decimal RequestedEquivalentQuantity { get; set; }
    }


    // ================================================================
    // DAL
    // ================================================================

    public class InvDepartmentTransactionIssueDAL
    {
        private const int STATUS_APPROVED = 2;

        private const int STATUS_ISSUED = 4;


        // ============================================================
        // INDEX
        // ============================================================

        public static InvDepartmentTransactionIssueResponse
            GetFilteredApprovedRequisitions(
                HttpRequestBase Request,
                string loggedInUserID)
        {
            InvDepartmentTransactionIssueResponse response =
                new InvDepartmentTransactionIssueResponse();


            GMDataTableDAL gmdt =
                new GMDataTableDAL(Request);


            using (var repo = new GMContext())
            {
                StringBuilder fromWhere =
                    new StringBuilder(@"
                        FROM InvDepartmentRequisition a

                        LEFT JOIN HrDepartment b
                            ON b.RowId = a.DepartmentId

                        LEFT JOIN HrEmployee c
                            ON c.RowId = a.RequestedBy

                        WHERE a.DomainStatus = 1
                         AND a.IsRemoved = 0
                          AND a.Status = @StatusApproved
                          AND a.IsApproved = 1
                          AND ISNULL(a.IsIssued, 0) = 0
                    ");


                List<SqlParameter> baseParameters =
                    new List<SqlParameter>
                    {
                        new SqlParameter(
                            "@StatusApproved",
                            STATUS_APPROVED)
                    };


                // ----------------------------------------------------
                // SEARCH
                // ----------------------------------------------------

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


                if (!string.IsNullOrWhiteSpace(
                    requisitionCode))
                {
                    fromWhere.AppendLine(
                        @"AND a.RequisitionCode
                          LIKE '%' + @RequisitionCode + '%'");

                    baseParameters.Add(
                        new SqlParameter(
                            "@RequisitionCode",
                            requisitionCode));
                }


                if (!string.IsNullOrWhiteSpace(
                    requisitionName))
                {
                    fromWhere.AppendLine(
                        @"AND a.RequisitionName
                          LIKE '%' + @RequisitionName + '%'");

                    baseParameters.Add(
                        new SqlParameter(
                            "@RequisitionName",
                            requisitionName));
                }


                if (!string.IsNullOrWhiteSpace(
                    departmentId))
                {
                    fromWhere.AppendLine(
                        @"AND a.DepartmentId =
                          @DepartmentId");

                    baseParameters.Add(
                        new SqlParameter(
                            "@DepartmentId",
                            departmentId));
                }


                if (!string.IsNullOrWhiteSpace(jobNo))
                {
                    fromWhere.AppendLine(
                        @"AND a.JobNo
                          LIKE '%' + @JobNo + '%'");

                    baseParameters.Add(
                        new SqlParameter(
                            "@JobNo",
                            jobNo));
                }


                DateTime parsedDate;


                if (!string.IsNullOrWhiteSpace(fromDate) &&
                    DateTime.TryParse(
                        fromDate,
                        out parsedDate))
                {
                    fromWhere.AppendLine(
                        @"AND a.RequisitionDate >= @FromDate");

                    baseParameters.Add(
                        new SqlParameter(
                            "@FromDate",
                            parsedDate.Date));
                }


                if (!string.IsNullOrWhiteSpace(toDate) &&
                    DateTime.TryParse(
                        toDate,
                        out parsedDate))
                {
                    DateTime nextDate =
                        parsedDate.Date.AddDays(1);

                    fromWhere.AppendLine(
                        @"AND a.RequisitionDate <
                          @ToDateExclusive");

                    baseParameters.Add(
                        new SqlParameter(
                            "@ToDateExclusive",
                            nextDate));
                }


                // ----------------------------------------------------
                // TOTAL
                // ----------------------------------------------------

                response.TotalRecordCount =
                    repo.Database.SqlQuery<int>(
                        "SELECT COUNT(1) " +
                        fromWhere,
                        CloneParameters(
                            baseParameters).ToArray())
                    .Single();


                // ----------------------------------------------------
                // GLOBAL SEARCH
                // ----------------------------------------------------

                StringBuilder filteredWhere =
                    new StringBuilder(
                        fromWhere.ToString());


                List<SqlParameter> filteredParameters =
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
                        )
                    ");


                    filteredParameters.Add(
                        new SqlParameter(
                            "@SearchValue",
                            gmdt.SearchValue));
                }


                // ----------------------------------------------------
                // FILTERED COUNT
                // ----------------------------------------------------

                response.FilteredRecordCount =
                    repo.Database.SqlQuery<int>(
                        "SELECT COUNT(1) " +
                        filteredWhere,
                        CloneParameters(
                            filteredParameters).ToArray())
                    .Single();


                // ----------------------------------------------------
                // SORT
                // ----------------------------------------------------

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


                // ----------------------------------------------------
                // PAGINATION
                // ----------------------------------------------------

                List<SqlParameter> listParameters =
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
                        ,a.RequiredFor
                        ,a.Status
                        ,a.IsApproved
                        ,a.IsIssued
                        ,a.IsRemoved

                    " + filteredWhere + @"

                    ORDER BY "
                    + sortColumn + " "
                    + sortDirection + @"

                    OFFSET @Skip ROWS

                    FETCH NEXT @Take ROWS ONLY
                ";


                response.Requisitions =
                    repo.Database.SqlQuery<
                        InvDepartmentRequisitionInfo>(
                            sql,
                            listParameters.ToArray())
                    .ToList();
            }


            return response;
        }


        // ============================================================
        // GET UNIT LIST
        // ============================================================

        public static List<
            InvDepartmentTransactionIssueUnitInfo>
            GetUnitList()
        {
            using (var repo = new GMContext())
            {
                return repo.Database.SqlQuery<
                    InvDepartmentTransactionIssueUnitInfo>(
                    @"
                    SELECT
                         RowId
                        ,MinUnitName

                    FROM InvUnit

                    WHERE ISNULL(DomainStatus, 1) = 1
                     -- AND ISNULL(IsRemoved, 0) = 0

                    ORDER BY MinUnitName
                    ")
                    .ToList();
            }
        }


        // ============================================================
        // GET REQUISITION
        // ============================================================

        public static InvDepartmentRequisitionInfo
            GetRequisitionByID(
                string rowId)
        {
            if (string.IsNullOrWhiteSpace(rowId))
                return null;


            using (var repo = new GMContext())
            {
                InvDepartmentRequisitionInfo requisition =
                    repo.Database.SqlQuery<
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
                            ,a.IsRemoved

                        FROM InvDepartmentRequisition a

                        LEFT JOIN HrDepartment b
                            ON b.RowId = a.DepartmentId

                        LEFT JOIN HrEmployee c
                            ON c.RowId = a.RequestedBy

                        WHERE a.RowId = @RowId

                          AND a.DomainStatus = 1
                          AND a.IsRemoved = 0

                          AND a.Status = @StatusApproved
                          AND a.IsApproved = 1
                          AND ISNULL(a.IsIssued, 0) = 0
                        ",
                        new SqlParameter(
                            "@RowId",
                            rowId),

                        new SqlParameter(
                            "@StatusApproved",
                            STATUS_APPROVED))
                    .FirstOrDefault();


                if (requisition == null)
                    return null;


                requisition.Details =
                    GetRequisitionDetailsByRequisitionId(
                        requisition.RowId);


                return requisition;
            }
        }


        // ============================================================
        // GET REQUISITION DETAILS
        // ============================================================

        public static List<
            InvDepartmentRequisitionDetailInfo>
            GetRequisitionDetailsByRequisitionId(
                string requisitionId)
        {
            if (string.IsNullOrWhiteSpace(
                requisitionId))
            {
                return new List<
                    InvDepartmentRequisitionDetailInfo>();
            }


            using (var repo = new GMContext())
            {
                return repo.Database.SqlQuery<
                    InvDepartmentRequisitionDetailInfo>(
                    @"
                    SELECT
                         a.RowId
                        ,a.RequisitionDetailId
                        ,a.RequisitionId
                        ,a.ItemId

                        ,ISNULL(a.Quantity, 0)
                            AS Quantity

                        ,a.UnitId

                        ,ISNULL(a.UnitPriceEst, 0)
                            AS UnitPriceEst

                        ,ISNULL(a.TotalPriceEst, 0)
                            AS TotalPriceEst

                        ,ISNULL(a.ApprovedQuantity, 0)
                            AS ApprovedQuantity

                        ,ISNULL(a.IssuedQuantity, 0)
                            AS IssuedQuantity

                        ,a.Remarks

                        ,b.ItemName

                        ,c.MinUnitName

                    FROM InvDepartmentRequisitionDetail a

                    LEFT JOIN InvItem b
                        ON b.RowId = a.ItemId

                    LEFT JOIN InvUnit c
                        ON c.RowId = a.UnitId

                    WHERE a.RequisitionId = @RequisitionId

                      AND a.DomainStatus = 1

                      AND ISNULL(a.ApprovedQuantity, 0) > 0

                    ORDER BY a.RequisitionDetailId
                    ",
                    new SqlParameter(
                        "@RequisitionId",
                        requisitionId))
                .ToList();
            }
        }


        // ============================================================
        // CALCULATE UNIT CONVERSION - PUBLIC
        // ============================================================

        public static
            InvDepartmentTransactionIssueConversionInfo
            CalculateUnitConversion(
                string requestedUnitId,
                string issueUnitId,
                decimal unitPriceEst,
                decimal issueQuantity)
        {
            using (var repo = new GMContext())
            {
                return CalculateUnitConversion(
                    repo,
                    requestedUnitId,
                    issueUnitId,
                    unitPriceEst,
                    issueQuantity);
            }
        }


        // ============================================================
        // CALCULATE UNIT CONVERSION
        //
        // Example:
        //
        // FirstUnitId  = Feet
        // SecondUnitId = Inch
        // ConversionRate = 12
        //
        // Requested = Feet
        // Issue     = Inch
        //
        // Effective Rate = 12
        //
        // 12 Inch = 1 Feet
        //
        // Requested Equivalent =
        //      IssueQuantity / Rate
        //
        // Conversion Price =
        //      Requested Price / Rate
        // ============================================================

        private static
            InvDepartmentTransactionIssueConversionInfo
            CalculateUnitConversion(
                GMContext repo,
                string requestedUnitId,
                string issueUnitId,
                decimal unitPriceEst,
                decimal issueQuantity)
        {
            InvDepartmentTransactionIssueConversionInfo result =
                new InvDepartmentTransactionIssueConversionInfo();


            result.Success = false;


            if (string.IsNullOrWhiteSpace(
                requestedUnitId))
            {
                result.Message =
                    "Requested Unit is required.";

                return result;
            }


            if (string.IsNullOrWhiteSpace(
                issueUnitId))
            {
                result.Message =
                    "Issue Unit is required.";

                return result;
            }


            if (unitPriceEst < 0)
            {
                result.Message =
                    "Unit price cannot be negative.";

                return result;
            }


            if (issueQuantity < 0)
                issueQuantity = 0;


            // ========================================================
            // SAME UNIT
            // ========================================================

            if (string.Equals(
                requestedUnitId,
                issueUnitId,
                StringComparison.OrdinalIgnoreCase))
            {
                result.Success = true;

                result.SameUnit = true;

                result.UnitConversionId = null;

                result.ConversionRate = 1M;

                result.ConversionPrice =
                    Math.Round(
                        unitPriceEst,
                        2,
                        MidpointRounding.AwayFromZero);

                result.RequestedEquivalentQuantity =
                    Math.Round(
                        issueQuantity,
                        4,
                        MidpointRounding.AwayFromZero);

                result.ConversionTotal =
                    Math.Round(
                        issueQuantity *
                        result.ConversionPrice,
                        2,
                        MidpointRounding.AwayFromZero);

                return result;
            }


            // ========================================================
            // FIND CONVERSION
            // ========================================================

            UnitConversionDbInfo conversion =
                repo.Database.SqlQuery<
                    UnitConversionDbInfo>(
                    @"
                    SELECT TOP 1
                         RowId
                        ,FirstUnitId
                        ,SecondUnitId
                        ,ConversionRate

                    FROM InvUnitConversion

                    WHERE ISNULL(DomainStatus, 1) = 1
                    --  AND ISNULL(IsRemoved, 0) = 0

                      AND
                      (
                            (
                                FirstUnitId =
                                    @RequestedUnitId

                                AND

                                SecondUnitId =
                                    @IssueUnitId
                            )

                            OR

                            (
                                FirstUnitId =
                                    @IssueUnitId

                                AND

                                SecondUnitId =
                                    @RequestedUnitId
                            )
                      )

                    ORDER BY RowId
                    ",
                    new SqlParameter(
                        "@RequestedUnitId",
                        requestedUnitId),

                    new SqlParameter(
                        "@IssueUnitId",
                        issueUnitId))
                .FirstOrDefault();


            if (conversion == null)
            {
                result.Message =
                    "No unit conversion exists between " +
                    "the selected units.";

                return result;
            }


            if (conversion.ConversionRate <= 0)
            {
                result.Message =
                    "Invalid conversion rate.";

                return result;
            }


            decimal effectiveRate;


            // ========================================================
            // NORMAL DIRECTION
            //
            // First = Requested
            // Second = Issue
            // ========================================================

            if (
                string.Equals(
                    conversion.FirstUnitId,
                    requestedUnitId,
                    StringComparison.OrdinalIgnoreCase)
                &&
                string.Equals(
                    conversion.SecondUnitId,
                    issueUnitId,
                    StringComparison.OrdinalIgnoreCase)
            )
            {
                effectiveRate = conversion.ConversionRate;
            }
            else
            {
                // ====================================================
                // REVERSE DIRECTION
                //
                // First = Issue
                // Second = Requested
                // ====================================================

                effectiveRate =
                    1M /
                    conversion.ConversionRate;
            }


            if (effectiveRate <= 0)
            {
                result.Message =
                    "Invalid calculated conversion rate.";

                return result;
            }


            // ========================================================
            // REQUESTED UNIT EQUIVALENT
            //
            // Example:
            //
            // 24 Inch / 12 = 2 Feet
            // ========================================================

            decimal requestedEquivalentQuantity =
                issueQuantity /
                effectiveRate;


            // ========================================================
            // CONVERSION PRICE
            //
            // Example:
            //
            // Feet Price = 1200
            // Rate = 12
            //
            // Inch Price = 1200 / 12 = 100
            // ========================================================

            decimal conversionPrice =
                unitPriceEst /
                effectiveRate;


            // ========================================================
            // CONVERSION TOTAL
            // ========================================================

            decimal conversionTotal =
                issueQuantity *
                conversionPrice;


            result.Success = true;

            result.SameUnit = false;

            result.UnitConversionId =
                conversion.RowId;

            result.ConversionRate =
                Math.Round(
                    effectiveRate,
                    6,
                    MidpointRounding.AwayFromZero);

            result.RequestedEquivalentQuantity =
                Math.Round(
                    requestedEquivalentQuantity,
                    4,
                    MidpointRounding.AwayFromZero);

            result.ConversionPrice =
                Math.Round(
                    conversionPrice,
                    2,
                    MidpointRounding.AwayFromZero);

            result.ConversionTotal =
                Math.Round(
                    conversionTotal,
                    2,
                    MidpointRounding.AwayFromZero);

            return result;
        }


        // ============================================================
        // ISSUE REQUISITION
        // ============================================================

        public static bool IssueRequisition(
            string rowId,
            string fromDepartmentId,
            string toDepartmentId,
            string remarks,
            List<InvDepartmentRequisitionIssuedLine>
                issuedLines,
            string loggedInUser,
            out string message)
        {
            message = "";


            if (string.IsNullOrWhiteSpace(rowId))
            {
                message =
                    "Requisition ID is required.";

                return false;
            }


            if (string.IsNullOrWhiteSpace(
                fromDepartmentId))
            {
                message =
                    "From Department is required.";

                return false;
            }


            if (string.IsNullOrWhiteSpace(
                toDepartmentId))
            {
                message =
                    "To Department is required.";

                return false;
            }


            if (issuedLines == null ||
                issuedLines.Count == 0)
            {
                message =
                    "At least one issue item is required.";

                return false;
            }


            if (string.IsNullOrWhiteSpace(
                loggedInUser))
            {
                message =
                    "Logged-in user is required.";

                return false;
            }


            using (var repo = new GMContext())
            using (var transaction =
                repo.Database.BeginTransaction(
                    IsolationLevel.Serializable))
            {
                try
                {
                    // =================================================
                    // 1. LOCK REQUISITION
                    // =================================================

                    InvDepartmentRequisitionInfo header =
                        repo.Database.SqlQuery<
                            InvDepartmentRequisitionInfo>(
                            @"
                            SELECT
                                 Status
                                ,IsApproved
                                ,IsIssued
                                ,DepartmentId

                            FROM InvDepartmentRequisition
                                WITH (UPDLOCK, HOLDLOCK)

                            WHERE RowId = @RowId

                          --    AND IsRemoved = 0
                              AND DomainStatus = 1
                            ",
                            new SqlParameter(
                                "@RowId",
                                rowId))
                        .FirstOrDefault();


                    if (header == null)
                    {
                        message =
                            "Requisition was not found.";

                        transaction.Rollback();

                        return false;
                    }


                    // =================================================
                    // 2. STATUS
                    // =================================================

                    if (header.Status != STATUS_APPROVED ||
                        header.IsApproved != 1 ||
                        header.IsIssued == 1)
                    {
                        message =
                            "Only Approved and not-yet-issued " +
                            "requisitions can be issued.";

                        transaction.Rollback();

                        return false;
                    }


                    // =================================================
                    // 3. FROM DEPARTMENT
                    // =================================================

                    int fromDepartmentExists =
                        repo.Database.SqlQuery<int>(
                            @"
                            SELECT COUNT(1)

                            FROM HrDepartment

                            WHERE RowId = @DepartmentId
                              AND DomainStatus = 1
                            ",
                            new SqlParameter(
                                "@DepartmentId",
                                fromDepartmentId))
                        .Single();


                    if (fromDepartmentExists <= 0)
                    {
                        message =
                            "From Department was not found.";

                        transaction.Rollback();

                        return false;
                    }


                    // =================================================
                    // 4. TO DEPARTMENT
                    // =================================================

                    int toDepartmentExists =
                        repo.Database.SqlQuery<int>(
                            @"
                            SELECT COUNT(1)

                            FROM HrDepartment

                            WHERE RowId = @DepartmentId
                              AND DomainStatus = 1
                            ",
                            new SqlParameter(
                                "@DepartmentId",
                                toDepartmentId))
                        .Single();


                    if (toDepartmentExists <= 0)
                    {
                        message =
                            "To Department was not found.";

                        transaction.Rollback();

                        return false;
                    }


                    // =================================================
                    // 5. LOAD DETAILS WITH LOCK
                    // =================================================

                    List<IssueDetailDbInfo>
                        requestedDetails =
                            repo.Database.SqlQuery<
                                IssueDetailDbInfo>(
                                @"
                                SELECT
                                     a.RowId
                                    ,a.RequisitionDetailId
                                    ,a.RequisitionId
                                    ,a.ItemId

                                    ,ISNULL(a.Quantity, 0)
                                        AS Quantity

                                    ,a.UnitId

                                    ,ISNULL(a.UnitPriceEst, 0)
                                        AS UnitPriceEst

                                    ,ISNULL(a.TotalPriceEst, 0)
                                        AS TotalPriceEst

                                    ,ISNULL(
                                        a.ApprovedQuantity,
                                        0)
                                        AS ApprovedQuantity

                                    ,ISNULL(
                                        a.IssuedQuantity,
                                        0)
                                        AS IssuedQuantity

                                FROM
                                    InvDepartmentRequisitionDetail a
                                    WITH (UPDLOCK, HOLDLOCK)

                                WHERE a.RequisitionId =
                                      @RequisitionId

                                  AND a.DomainStatus = 1

                                ORDER BY
                                    a.RequisitionDetailId
                                ",
                                new SqlParameter(
                                    "@RequisitionId",
                                    rowId))
                            .ToList();


                    if (requestedDetails.Count == 0)
                    {
                        message =
                            "No requisition items were found.";

                        transaction.Rollback();

                        return false;
                    }


                    Dictionary<string, IssueDetailDbInfo>
                        detailDictionary =
                            requestedDetails.ToDictionary(
                                x => x.RowId,
                                x => x);


                    // =================================================
                    // 6. DUPLICATE CHECK
                    // =================================================

                    if (issuedLines
                        .Where(x => x != null)
                        .GroupBy(x => x.RowId)
                        .Any(g => g.Count() > 1))
                    {
                        message =
                            "Duplicate issue item found.";

                        transaction.Rollback();

                        return false;
                    }


                    // =================================================
                    // 7. CALCULATE
                    // =================================================

                    List<CalculatedIssueLine>
                        calculatedLines =
                            new List<CalculatedIssueLine>();


                    foreach (
                        InvDepartmentRequisitionIssuedLine line
                        in issuedLines)
                    {
                        if (line == null)
                            continue;


                        IssueDetailDbInfo detail;


                        if (!detailDictionary.TryGetValue(
                            line.RowId,
                            out detail))
                        {
                            message =
                                "Invalid requisition detail RowId: " +
                                line.RowId;

                            transaction.Rollback();

                            return false;
                        }


                        if (line.IssuedQuantity < 0)
                        {
                            message =
                                "Issued quantity cannot be negative.";

                            transaction.Rollback();

                            return false;
                        }


                        if (line.IssuedQuantity == 0)
                            continue;


                        if (string.IsNullOrWhiteSpace(
                            line.IssueUnitId))
                        {
                            message =
                                "Issue Unit is required.";

                            transaction.Rollback();

                            return false;
                        }


                        // =================================================
                        // VALIDATE ISSUE UNIT
                        // =================================================

                        int unitExists =
                            repo.Database.SqlQuery<int>(
                                @"
                                SELECT COUNT(1)

                                FROM InvUnit

                                WHERE RowId = @UnitId
                             --     AND ISNULL(IsRemoved, 0) = 0
                                  AND ISNULL(DomainStatus, 1) = 1
                                ",
                                new SqlParameter(
                                    "@UnitId",
                                    line.IssueUnitId))
                            .Single();


                        if (unitExists <= 0)
                        {
                            message =
                                "Selected Issue Unit was not found.";

                            transaction.Rollback();

                            return false;
                        }


                        // =================================================
                        // CONVERSION
                        // =================================================

                        InvDepartmentTransactionIssueConversionInfo
                            conversionResult =
                                CalculateUnitConversion(
                                    repo,
                                    detail.UnitId,
                                    line.IssueUnitId,
                                    detail.UnitPriceEst,
                                    line.IssuedQuantity);


                        if (conversionResult == null ||
                            !conversionResult.Success)
                        {
                            message =
                                conversionResult == null
                                    ? "Unit conversion failed."
                                    : conversionResult.Message;

                            transaction.Rollback();

                            return false;
                        }


                        // =================================================
                        // REMAINING APPROVED
                        //
                        // IMPORTANT:
                        //
                        // Compare REQUESTED UNIT equivalent.
                        //
                        // Example:
                        //
                        // Approved = 2 Feet
                        // Already = 0 Feet
                        // Issue = 24 Inch
                        // Equivalent = 2 Feet
                        //
                        // Valid.
                        // =================================================

                        decimal remaining =
                            detail.ApprovedQuantity -
                            detail.IssuedQuantity;


                        if (remaining < 0)
                            remaining = 0;


                        if (conversionResult
                                .RequestedEquivalentQuantity
                            > remaining + 0.0001M)
                        {
                            message =
                                "Issued quantity exceeds remaining " +
                                "approved quantity for item RowId " +
                                line.RowId +
                                ". Remaining approved quantity: " +
                                remaining.ToString("0.####") +
                                " " +
                                detail.UnitId;

                            transaction.Rollback();

                            return false;
                        }


                        // =================================================
                        // ADD CALCULATED LINE
                        // =================================================

                        calculatedLines.Add(
                            new CalculatedIssueLine
                            {
                                DetailRowId =
                                    detail.RowId,

                                ItemId =
                                    detail.ItemId,

                                RequestedUnitId =
                                    detail.UnitId,

                                IssueUnitId =
                                    line.IssueUnitId,

                                IssueQuantity =
                                    line.IssuedQuantity,

                                RequestedEquivalentQuantity =
                                    conversionResult
                                        .RequestedEquivalentQuantity,

                                UnitConversionId =
                                    conversionResult
                                        .UnitConversionId,

                                ConversionRate =
                                    conversionResult
                                        .ConversionRate,

                                ConversionPrice =
                                    conversionResult
                                        .ConversionPrice,

                                ConversionTotal =
                                    conversionResult
                                        .ConversionTotal,

                                Remarks =
                                    line.Remarks
                            });
                    }


                    // =================================================
                    // 8. AT LEAST ONE LINE
                    // =================================================

                    if (calculatedLines.Count == 0)
                    {
                        message =
                            "At least one issued quantity " +
                            "must be greater than zero.";

                        transaction.Rollback();

                        return false;
                    }


                    // =================================================
                    // 9. TOTAL
                    // =================================================

                    decimal totalIssueAmount =
                        calculatedLines.Sum(
                            x => x.ConversionTotal);


                    totalIssueAmount =
                        Math.Round(
                            totalIssueAmount,
                            2,
                            MidpointRounding.AwayFromZero);


                    // =================================================
                    // 10. CREATE NEW ISSUE TRANSACTION
                    // =================================================

                    int transactionId =
                        GetNextIntegerId(
                            repo,
                            "InvDepartmentTransaction",
                            "TransactionId");


                    string transactionRowId =
                        Guid.NewGuid().ToString();


                    string transactionCode =
                        "TRX-" +
                        transactionId.ToString("D6");


                    repo.Database.ExecuteSqlCommand(
                        @"
                        INSERT INTO InvDepartmentTransaction
                        (
                             RowId
                            ,TransactionId
                            ,TransactionCode
                            ,TransactionDate

                            ,RequisitionId
                            ,MaterialReveiveId
                            ,MaterialIssueId

                            ,TransactionType

                            ,FromDepartment
                            ,ToDepartment

                            ,TotalAmount
                            ,Remarks

                            ,IsIssued
                            ,IsReceived

                            ,IsRemoved
                            ,DomainStatus

                            ,CreateUser
                            ,CreateDate
                        )
                        VALUES
                        (
                             @RowId
                            ,@TransactionId
                            ,@TransactionCode
                            ,GETDATE()

                            ,@RequisitionId
                            ,''
                            ,''

                            ,'Issue'

                            ,@FromDepartment
                            ,@ToDepartment

                            ,@TotalAmount
                            ,@Remarks

                            ,1
                            ,0

                            ,0
                            ,1

                            ,@CreateUser
                            ,GETDATE()
                        )
                        ",
                        new SqlParameter(
                            "@RowId",
                            transactionRowId),

                        new SqlParameter(
                            "@TransactionId",
                            transactionId),

                        new SqlParameter(
                            "@TransactionCode",
                            transactionCode),

                        new SqlParameter(
                            "@RequisitionId",
                            rowId),

                        new SqlParameter(
                            "@FromDepartment",
                            fromDepartmentId),

                        new SqlParameter(
                            "@ToDepartment",
                            toDepartmentId),

                        new SqlParameter(
                            "@TotalAmount",
                            totalIssueAmount),

                        new SqlParameter(
                            "@Remarks",
                            DbValue(remarks)),

                        new SqlParameter(
                            "@CreateUser",
                            loggedInUser)
                    );


                    // =================================================
                    // 11. TRANSACTION DETAILS
                    // =================================================

                    int transactionDetailId =
                        GetNextIntegerId(
                            repo,
                            "InvDepartmentTransactionDetail",
                            "TransactionDetailId");


                    foreach (
                        CalculatedIssueLine line
                        in calculatedLines)
                    {
                        repo.Database.ExecuteSqlCommand(
                            @"
                            INSERT INTO
                                InvDepartmentTransactionDetail
                            (
                                 RowId
                                ,TransactionDetailId
                                ,TransactionId

                                ,ItemId
                                ,UnitId

                                ,Quantity
                                ,MRRQty

                                ,UnitPrice
                                ,TotalPrice

                                ,UnitConversionId
                                ,ConversionRate
                                ,ConversionPrice
                                ,ConversionTotal

                                --,Remarks

                                ,DomainStatus
                                ,CreateUser
                                ,CreateDate
                            )
                            VALUES
                            (
                                 @RowId
                                ,@TransactionDetailId
                                ,@TransactionId

                                ,@ItemId
                                ,@UnitId

                                ,@Quantity
                                ,0

                                ,@UnitPrice
                                ,@TotalPrice

                                ,@UnitConversionId
                                ,@ConversionRate
                                ,@ConversionPrice
                                ,@ConversionTotal

                             --   ,@Remarks

                                ,1
                                ,@CreateUser
                                ,GETDATE()
                            )
                            ",
                            new SqlParameter(
                                "@RowId",
                                Guid.NewGuid().ToString()),

                            new SqlParameter(
                                "@TransactionDetailId",
                                transactionDetailId++),

                            new SqlParameter(
                                "@TransactionId",
                                transactionRowId),

                            new SqlParameter(
                                "@ItemId",
                                DbValue(line.ItemId)),

                            // Actual issued unit
                            new SqlParameter(
                                "@UnitId",
                                DbValue(line.IssueUnitId)),

                            new SqlParameter(
                                "@Quantity",
                                line.IssueQuantity),

                            new SqlParameter(
                                "@UnitPrice",
                                line.ConversionPrice),

                            new SqlParameter(
                                "@TotalPrice",
                                line.ConversionTotal),

                            // InvUnitConversion.RowId
                            new SqlParameter(
                                "@UnitConversionId",
                                DbValue(
                                    line.UnitConversionId)),

                            new SqlParameter(
                                "@ConversionRate",
                                line.ConversionRate),

                            new SqlParameter(
                                "@ConversionPrice",
                                line.ConversionPrice),

                            new SqlParameter(
                                "@ConversionTotal",
                                line.ConversionTotal),

                           //-- new SqlParameter("@Remarks",DbValue(line.Remarks)),

                            new SqlParameter(
                                "@CreateUser",
                                loggedInUser)
                        );
                    }


                    // =================================================
                    // 12. UPDATE REQUISITION DETAIL
                    //
                    // IMPORTANT:
                    //
                    // Store IssuedQuantity in the ORIGINAL/
                    // REQUESTED UNIT.
                    // =================================================

                    foreach (
                        CalculatedIssueLine line
                        in calculatedLines)
                    {
                        int affected =
                            repo.Database.ExecuteSqlCommand(
                                @"
                                UPDATE
                                    InvDepartmentRequisitionDetail

                                SET
                                     IssuedQuantity =
                                        ISNULL(
                                            IssuedQuantity,
                                            0)
                                        + @IssuedQuantity

                                    ,UpdateUser =
                                        @UpdateUser

                                    ,UpdateDate =
                                        GETDATE()

                                WHERE RowId =
                                      @RowId

                                  AND RequisitionId =
                                      @RequisitionId

                                  AND DomainStatus = 1
                                ",
                                new SqlParameter(
                                    "@IssuedQuantity",
                                    line.RequestedEquivalentQuantity),

                                new SqlParameter(
                                    "@UpdateUser",
                                    loggedInUser),

                                new SqlParameter(
                                    "@RowId",
                                    line.DetailRowId),

                                new SqlParameter(
                                    "@RequisitionId",
                                    rowId));


                        if (affected == 0)
                        {
                            message =
                                "Unable to update requisition detail.";

                            transaction.Rollback();

                            return false;
                        }
                    }


                    // =================================================
                    // 13. UPDATE REQUISITION HEADER
                    //
                    // APPROVAL TRANSACTION IS NOT MODIFIED.
                    // =================================================

                    int headerAffected =
                        repo.Database.ExecuteSqlCommand(
                            @"
                            UPDATE
                                InvDepartmentRequisition

                            SET
                                 Status =
                                    @StatusIssued

                                ,IsApproved = 1

                                ,IsIssued = 1

                                ,UpdateUser =
                                    @UpdateUser

                                ,UpdateDate =
                                    GETDATE()

                            WHERE RowId =
                                  @RowId

                              AND IsRemoved = 0
                              AND DomainStatus = 1

                              AND Status =
                                  @StatusApproved

                              AND IsApproved = 1

                              AND ISNULL(IsIssued, 0) = 0
                            ",
                            new SqlParameter(
                                "@StatusIssued",
                                STATUS_ISSUED),

                            new SqlParameter(
                                "@UpdateUser",
                                loggedInUser),

                            new SqlParameter(
                                "@RowId",
                                rowId),

                            new SqlParameter(
                                "@StatusApproved",
                                STATUS_APPROVED));


                    if (headerAffected == 0)
                    {
                        message =
                            "Requisition could not be issued.";

                        transaction.Rollback();

                        return false;
                    }


                    // =================================================
                    // 14. COMMIT
                    // =================================================

                    transaction.Commit();


                    message =
                        "Requisition issued successfully.";

                    return true;
                }
                catch (Exception ex)
                {
                    try
                    {
                        transaction.Rollback();
                    }
                    catch
                    {
                    }


                    message =
                        "Issue failed: " +
                        ex.GetBaseException().Message;

                    return false;
                }
            }
        }


        // ============================================================
        // NEXT INTEGER ID
        // ============================================================

        private static int GetNextIntegerId(
            GMContext repo,
            string tableName,
            string columnName)
        {
            string sql =
                "SELECT ISNULL(MAX(" +
                columnName +
                "), 0) + 1 " +
                "FROM " +
                tableName +
                " WITH (UPDLOCK, HOLDLOCK)";


            return repo.Database
                .SqlQuery<int>(sql)
                .Single();
        }


        // ============================================================
        // DB VALUE
        // ============================================================

        private static object DbValue(
            object value)
        {
            if (value == null)
                return DBNull.Value;


            if (value is string &&
                string.IsNullOrWhiteSpace(
                    Convert.ToString(value)))
            {
                return DBNull.Value;
            }


            return value;
        }


        // ============================================================
        // CLONE PARAMETERS
        // ============================================================

        private static List<SqlParameter>
            CloneParameters(
                IEnumerable<SqlParameter> source)
        {
            List<SqlParameter> result =
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
                            p.Value ??
                            DBNull.Value
                    });
            }


            return result;
        }
    }
}