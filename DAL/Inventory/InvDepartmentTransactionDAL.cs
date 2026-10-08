using ERP.Models.Inventory.InvDepartmentTransaction;
using ERP.Models.Inventory.InvDepartmentTransactionDetail;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.Inventory
{
    public class InvDepartmentTransactionDAL
    {
        // ===============================================================
        // GET TRANSACTION
        // ===============================================================

        public static InvDepartmentTransactionInfo
            GetTransactionByID(int transactionId)
        {
            using (var repo = new GMContext())
            {
                var transaction =
                    repo.Database.SqlQuery<
                        InvDepartmentTransactionInfo>(
                    @"
                    SELECT
                         a.RowId
                        ,a.TransactionId
                        ,a.TransactionCode
                        ,a.TransactionDate
                        ,a.RequisitionId
                        ,a.MaterialReveiveId
                        ,a.MaterialIssueId
                        ,a.TransactionType
                        ,a.FromDepartment
                        ,a.ToDepartment
                        ,a.TotalAmount
                        ,a.CurrencyId
                        ,a.MIVNo
                        ,a.INCMIVNo
                        ,a.FinancialYearId
                        ,a.Remarks
                        ,a.IsIssued
                        ,a.IsReceived
                        ,a.IsRemoved

                        ,a.TransactionStatus
                        ,a.ActionBy
                        ,a.ActionDate
                        ,a.ActionRemarks

                        ,r.RequisitionCode AS RequisitionNo

                        ,fd.DepartmentName
                            AS FromDepartmentName

                        ,td.DepartmentName
                            AS ToDepartmentName

                        ,CASE
                            WHEN a.TransactionStatus = 1
                                THEN 'Pending'

                            WHEN a.TransactionStatus = 2
                                THEN 'Forwarded'

                            WHEN a.TransactionStatus = 3
                                THEN 'Approved'

                            WHEN a.TransactionStatus = 4
                                THEN 'Rejected'

                            WHEN a.TransactionStatus = 5
                                THEN 'Issued'

                            WHEN a.TransactionStatus = 6
                                THEN 'Received'

                            ELSE 'Unknown'
                         END AS TransactionStatusText

                    FROM InvDepartmentTransaction a

                    LEFT JOIN InvDepartmentRequisition r
                        ON r.RowId = a.RequisitionId

                    LEFT JOIN InvDepartment fd
                        ON fd.RowId = a.FromDepartment

                    LEFT JOIN InvDepartment td
                        ON td.RowId = a.ToDepartment

                    WHERE a.TransactionId =
                        @TransactionId

                    AND a.IsRemoved = 0
                    ",
                    new SqlParameter(
                        "@TransactionId",
                        transactionId))
                    .FirstOrDefault();

                return transaction;
            }
        }


        // ===============================================================
        // GET TRANSACTION DETAILS
        // ===============================================================

        public static List<
            InvDepartmentTransactionDetailInfo>
            GetTransactionDetails(
                int transactionId)
        {
            using (var repo = new GMContext())
            {
                return repo.Database.SqlQuery<
                    InvDepartmentTransactionDetailInfo>(
                    @"
                    SELECT
                         a.TransactionDetailId
                        ,a.TransactionId
                        ,a.ItemId
                        ,a.UnitId
                        ,a.Quantity
                        ,a.MRRQty
                        ,a.UnitPrice
                        ,a.TotalPrice
                        ,a.UnitConversionId
                        ,a.ConversionRate
                        ,a.ConversionPrice
                        ,a.ConversionTotal

                        ,b.ItemName
                        ,c.MinUnitName AS UnitName

                        ,t.TransactionCode

                    FROM InvDepartmentTransactionDetail a

                    INNER JOIN InvItem b
                        ON b.RowId = a.ItemId

                    INNER JOIN InvUnit c
                        ON c.RowId = a.UnitId

                    INNER JOIN InvDepartmentTransaction t
                        ON t.TransactionId =
                           a.TransactionId

                    WHERE a.TransactionId =
                        @TransactionId

                    AND a.IsRemoved = 0

                    ORDER BY a.TransactionDetailId
                    ",
                    new SqlParameter(
                        "@TransactionId",
                        transactionId))
                    .ToList();
            }
        }


        // ===============================================================
        // FORWARD
        // ===============================================================

        public static void ForwardTransaction(
            int transactionId,
            string loggedInUser,
            string remarks)
        {
            using (var repo = new GMContext())
            using (var transaction =
                   repo.Database.BeginTransaction())
            {
                try
                {
                    string requisitionId =
                        repo.Database.SqlQuery<string>(
                        @"
                        SELECT RequisitionId

                        FROM InvDepartmentTransaction

                        WHERE TransactionId =
                            @TransactionId

                        AND IsRemoved = 0
                        ",
                        new SqlParameter(
                            "@TransactionId",
                            transactionId))
                        .FirstOrDefault();

                    if (string.IsNullOrEmpty(
                        requisitionId))
                    {
                        throw new Exception(
                            "Transaction not found.");
                    }


                    // Transaction
                    repo.Database.ExecuteSqlCommand(
                    @"
                    UPDATE InvDepartmentTransaction
                    SET
                         TransactionStatus = 2
                        ,ActionBy = @ActionBy
                        ,ActionDate = GETDATE()
                        ,ActionRemarks = @ActionRemarks

                    WHERE TransactionId =
                        @TransactionId

                    AND IsRemoved = 0
                    ",
                    new SqlParameter(
                        "@TransactionId",
                        transactionId),

                    new SqlParameter(
                        "@ActionBy",
                        loggedInUser),

                    new SqlParameter(
                        "@ActionRemarks",
                        (object)remarks ??
                        ""));


                    // Requisition
                    repo.Database.ExecuteSqlCommand(
                    @"
                    UPDATE InvDepartmentRequisition
                    SET
                         Status = 2
                        ,UpdateUser = @UpdateUser
                        ,UpdateDate = GETDATE()

                    WHERE RowId = @RequisitionId
                    AND IsRemoved = 0
                    ",
                    new SqlParameter(
                        "@RequisitionId",
                        requisitionId),

                    new SqlParameter(
                        "@UpdateUser",
                        loggedInUser));


                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }


        // ===============================================================
        // APPROVE
        // ===============================================================

        public static void ApproveTransaction(
            int transactionId,
            string loggedInUser,
            string remarks)
        {
            using (var repo = new GMContext())
            using (var transaction =
                   repo.Database.BeginTransaction())
            {
                try
                {
                    string requisitionId =
                        repo.Database.SqlQuery<string>(
                        @"
                        SELECT RequisitionId

                        FROM InvDepartmentTransaction

                        WHERE TransactionId =
                            @TransactionId

                        AND IsRemoved = 0
                        ",
                        new SqlParameter(
                            "@TransactionId",
                            transactionId))
                        .FirstOrDefault();

                    if (string.IsNullOrEmpty(
                        requisitionId))
                    {
                        throw new Exception(
                            "Transaction not found.");
                    }


                    // -----------------------------------------------
                    // Approve transaction
                    // -----------------------------------------------

                    repo.Database.ExecuteSqlCommand(
                    @"
                    UPDATE InvDepartmentTransaction
                    SET
                         TransactionStatus = 3
                        ,ActionBy = @ActionBy
                        ,ActionDate = GETDATE()
                        ,ActionRemarks = @ActionRemarks

                    WHERE TransactionId =
                        @TransactionId

                    AND IsRemoved = 0
                    ",
                    new SqlParameter(
                        "@TransactionId",
                        transactionId),

                    new SqlParameter(
                        "@ActionBy",
                        loggedInUser),

                    new SqlParameter(
                        "@ActionRemarks",
                        (object)remarks ??
                        ""));


                    // -----------------------------------------------
                    // Approve requisition
                    // -----------------------------------------------

                    repo.Database.ExecuteSqlCommand(
                    @"
                    UPDATE InvDepartmentRequisition
                    SET
                         Status = 3
                        ,IsApproved = 1
                        ,UpdateUser = @UpdateUser
                        ,UpdateDate = GETDATE()

                    WHERE RowId =
                        @RequisitionId

                    AND IsRemoved = 0
                    ",
                    new SqlParameter(
                        "@RequisitionId",
                        requisitionId),

                    new SqlParameter(
                        "@UpdateUser",
                        loggedInUser));


                    // -----------------------------------------------
                    // Approved quantity
                    //
                    // Current default:
                    // Approved Quantity = requested Quantity
                    // -----------------------------------------------

                    repo.Database.ExecuteSqlCommand(
                    @"
                    UPDATE d
                    SET
                        d.ApprovedQuantity =
                            td.Quantity

                    FROM InvDepartmentRequisitionDetail d

                    INNER JOIN InvDepartmentTransaction t
                        ON t.RequisitionId =
                           d.RequisitionId

                    INNER JOIN
                        InvDepartmentTransactionDetail td
                        ON td.TransactionId =
                           t.TransactionId

                        AND td.ItemId =
                            d.ItemId

                    WHERE t.TransactionId =
                        @TransactionId

                    AND d.RequisitionId =
                        @RequisitionId
                    ",
                    new SqlParameter(
                        "@TransactionId",
                        transactionId),

                    new SqlParameter(
                        "@RequisitionId",
                        requisitionId));


                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }


        // ===============================================================
        // REJECT
        // ===============================================================

        public static void RejectTransaction(
            int transactionId,
            string loggedInUser,
            string remarks)
        {
            if (string.IsNullOrWhiteSpace(remarks))
            {
                throw new Exception(
                    "Reject reason is required.");
            }


            using (var repo = new GMContext())
            using (var transaction =
                   repo.Database.BeginTransaction())
            {
                try
                {
                    string requisitionId =
                        repo.Database.SqlQuery<string>(
                        @"
                        SELECT RequisitionId

                        FROM InvDepartmentTransaction

                        WHERE TransactionId =
                            @TransactionId

                        AND IsRemoved = 0
                        ",
                        new SqlParameter(
                            "@TransactionId",
                            transactionId))
                        .FirstOrDefault();


                    if (string.IsNullOrEmpty(
                        requisitionId))
                    {
                        throw new Exception(
                            "Transaction not found.");
                    }


                    // Transaction
                    repo.Database.ExecuteSqlCommand(
                    @"
                    UPDATE InvDepartmentTransaction
                    SET
                         TransactionStatus = 4
                        ,ActionBy = @ActionBy
                        ,ActionDate = GETDATE()
                        ,ActionRemarks = @ActionRemarks

                    WHERE TransactionId =
                        @TransactionId
                    ",
                    new SqlParameter(
                        "@TransactionId",
                        transactionId),

                    new SqlParameter(
                        "@ActionBy",
                        loggedInUser),

                    new SqlParameter(
                        "@ActionRemarks",
                        remarks));


                    // Requisition
                    repo.Database.ExecuteSqlCommand(
                    @"
                    UPDATE InvDepartmentRequisition
                    SET
                         Status = 4
                        ,IsApproved = 0
                        ,UpdateUser = @UpdateUser
                        ,UpdateDate = GETDATE()

                    WHERE RowId =
                        @RequisitionId
                    ",
                    new SqlParameter(
                        "@RequisitionId",
                        requisitionId),

                    new SqlParameter(
                        "@UpdateUser",
                        loggedInUser));


                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }


        // ===============================================================
        // EDIT TRANSACTION
        //
        // Only Pending transaction can be edited.
        // ===============================================================

        public static void UpdateTransaction(
            InvDepartmentTransactionInfo model,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            using (var transaction =
                   repo.Database.BeginTransaction())
            {
                try
                {
                    int status =
                        repo.Database.SqlQuery<int>(
                        @"
                        SELECT TransactionStatus
                        FROM InvDepartmentTransaction
                        WHERE TransactionId =
                            @TransactionId
                        AND IsRemoved = 0
                        ",
                        new SqlParameter(
                            "@TransactionId",
                            model.TransactionId))
                        .Single();

                    if (status != 1)
                    {
                        throw new Exception(
                            "Only Pending transactions can be edited.");
                    }


                    repo.Database.ExecuteSqlCommand(
                    @"
                    UPDATE InvDepartmentTransaction
                    SET
                         TransactionDate =
                            @TransactionDate
                        ,ToDepartment =
                            @ToDepartment
                        ,TotalAmount =
                            @TotalAmount
                        ,Remarks =
                            @Remarks

                    WHERE TransactionId =
                        @TransactionId
                    ",
                    new SqlParameter(
                        "@TransactionId",
                        model.TransactionId),

                    new SqlParameter(
                        "@TransactionDate",
                        model.TransactionDate),

                    new SqlParameter(
                        "@ToDepartment",
                        model.ToDepartment),

                    new SqlParameter(
                        "@TotalAmount",
                        model.TotalAmount),

                    new SqlParameter(
                        "@Remarks",
                        (object)model.Remarks ??
                        ""));


                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }


        // ===============================================================
        // DELETE TRANSACTION
        // ===============================================================

        public static void DeleteTransaction(
            int transactionId,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            using (var transaction =
                   repo.Database.BeginTransaction())
            {
                try
                {
                    int status =
                        repo.Database.SqlQuery<int>(
                        @"
                        SELECT TransactionStatus
                        FROM InvDepartmentTransaction

                        WHERE TransactionId =
                            @TransactionId

                        AND IsRemoved = 0
                        ",
                        new SqlParameter(
                            "@TransactionId",
                            transactionId))
                        .Single();


                    if (status != 1)
                    {
                        throw new Exception(
                            "Only Pending transactions can be deleted.");
                    }


                    repo.Database.ExecuteSqlCommand(
                    @"
                    UPDATE InvDepartmentTransaction
                    SET
                        IsRemoved = 1

                    WHERE TransactionId =
                        @TransactionId
                    ",
                    new SqlParameter(
                        "@TransactionId",
                        transactionId));


                    repo.Database.ExecuteSqlCommand(
                    @"
                    UPDATE InvDepartmentTransactionDetail
                    SET
                        IsRemoved = 1

                    WHERE TransactionId =
                        @TransactionId
                    ",
                    new SqlParameter(
                        "@TransactionId",
                        transactionId));


                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                    throw;
                }
            }
        }


        // ===============================================================
        // FILTERED TRANSACTION LIST
        // ===============================================================

        public static InvDepartmentTransactionResponse
    GetFilteredTransactions(HttpRequestBase Request)
        {
            InvDepartmentTransactionResponse resp =
                new InvDepartmentTransactionResponse();

            GMDataTableDAL gmdt =
                new GMDataTableDAL(Request);

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
            FROM InvDepartmentTransaction a

            LEFT JOIN InvDepartmentRequisition r
                ON r.RowId = a.RequisitionId

            LEFT JOIN InvDepartment fd
                ON fd.RowId = a.FromDepartment

            LEFT JOIN InvDepartment td
                ON td.RowId = a.ToDepartment

            WHERE a.IsRemoved = 0
        ");

                List<SqlParameter> parameters =
                    new List<SqlParameter>();


                // =========================================================
                // Transaction Code
                // =========================================================
                if (!string.IsNullOrWhiteSpace(
                    Request["TransactionCode"]))
                {
                    sb.Append(@"
                AND a.TransactionCode
                    LIKE '%' + @TransactionCode + '%'
            ");

                    parameters.Add(
                        new SqlParameter(
                            "@TransactionCode",
                            Request["TransactionCode"]));
                }


                // =========================================================
                // Requisition Code
                // =========================================================
                if (!string.IsNullOrWhiteSpace(
                    Request["RequisitionCode"]))
                {
                    sb.Append(@"
                AND r.RequisitionCode
                    LIKE '%' + @RequisitionCode + '%'
            ");

                    parameters.Add(
                        new SqlParameter(
                            "@RequisitionCode",
                            Request["RequisitionCode"]));
                }


                // =========================================================
                // Department
                // =========================================================
                if (!string.IsNullOrWhiteSpace(
                    Request["DepartmentId"]))
                {
                    sb.Append(@"
                AND a.ToDepartment = @DepartmentId
            ");

                    parameters.Add(
                        new SqlParameter(
                            "@DepartmentId",
                            Request["DepartmentId"]));
                }


                // =========================================================
                // Transaction Status
                // =========================================================
                if (!string.IsNullOrWhiteSpace(
                    Request["TransactionStatus"]))
                {
                    sb.Append(@"
                AND a.TransactionStatus =
                    @TransactionStatus
            ");

                    parameters.Add(
                        new SqlParameter(
                            "@TransactionStatus",
                            Request["TransactionStatus"]));
                }


                // =========================================================
                // Global DataTables Search
                // =========================================================
                if (!string.IsNullOrWhiteSpace(
                    gmdt.SearchValue))
                {
                    sb.Append(@"
                AND
                (
                       a.TransactionCode LIKE
                           '%' + @SearchValue + '%'

                    OR r.RequisitionCode LIKE
                           '%' + @SearchValue + '%'

                    OR fd.DepartmentName LIKE
                           '%' + @SearchValue + '%'

                    OR td.DepartmentName LIKE
                           '%' + @SearchValue + '%'

                    OR a.MIVNo LIKE
                           '%' + @SearchValue + '%'

                    OR a.INCMIVNo LIKE
                           '%' + @SearchValue + '%'

                    OR a.Remarks LIKE
                           '%' + @SearchValue + '%'
                )
            ");

                    parameters.Add(
                        new SqlParameter(
                            "@SearchValue",
                            gmdt.SearchValue));
                }


                // =========================================================
                // TOTAL RECORD COUNT
                // =========================================================
                string countSql =
                    "SELECT COUNT(*) " +
                    sb.ToString();

                int total =
                    repo.Database.SqlQuery<int>(
                        countSql,
                        parameters.ToArray())
                    .Single();

                resp.TotalRecordCount = total;
                resp.FilteredRecordCount = total;


                // =========================================================
                // DATATABLES SORT COLUMN
                //
                // Do NOT use:
                // gmdt.SortColumnIndex
                //
                // because GMDataTableDAL does not contain that property.
                // =========================================================

                int sortColumnIndex = 0;

                int.TryParse(
                    Request["order[0][column]"],
                    out sortColumnIndex);


                string sortColumn;

                switch (sortColumnIndex)
                {
                    case 0:
                        sortColumn = "a.TransactionCode";
                        break;

                    case 1:
                        sortColumn = "a.TransactionDate";
                        break;

                    case 2:
                        sortColumn = "r.RequisitionCode";
                        break;

                    case 3:
                        sortColumn = "fd.DepartmentName";
                        break;

                    case 4:
                        sortColumn = "td.DepartmentName";
                        break;

                    case 5:
                        sortColumn = "a.MIVNo";
                        break;

                    case 6:
                        sortColumn = "a.TotalAmount";
                        break;

                    case 7:
                        sortColumn = "a.TransactionStatus";
                        break;

                    default:
                        sortColumn = "a.TransactionId";
                        break;
                }


                // =========================================================
                // SORT DIRECTION
                // =========================================================
                string direction =
                    Request["order[0][dir]"];

                if (string.IsNullOrWhiteSpace(direction) ||
                    direction.ToLower() != "asc")
                {
                    direction = "DESC";
                }
                else
                {
                    direction = "ASC";
                }


                // =========================================================
                // MAIN SQL
                // =========================================================
                string sql = @"
            SELECT
                 a.RowId
                ,a.TransactionId
                ,a.TransactionCode
                ,a.TransactionDate
                ,a.RequisitionId
                ,a.MaterialReveiveId
                ,a.MaterialIssueId
                ,a.TransactionType
                ,a.FromDepartment
                ,a.ToDepartment
                ,a.TotalAmount
                ,a.CurrencyId
                ,a.MIVNo
                ,a.INCMIVNo
                ,a.FinancialYearId
                ,a.Remarks
                ,a.IsIssued
                ,a.IsReceived
                ,a.IsRemoved

                ,a.TransactionStatus
                ,a.ActionBy
                ,a.ActionDate
                ,a.ActionRemarks

                ,r.RequisitionCode
                    AS RequisitionNo

                ,fd.DepartmentName
                    AS FromDepartmentName

                ,td.DepartmentName
                    AS ToDepartmentName

                ,CASE
                    WHEN a.TransactionStatus = 1
                        THEN 'Pending'

                    WHEN a.TransactionStatus = 2
                        THEN 'Forwarded'

                    WHEN a.TransactionStatus = 3
                        THEN 'Approved'

                    WHEN a.TransactionStatus = 4
                        THEN 'Rejected'

                    WHEN a.TransactionStatus = 5
                        THEN 'Issued'

                    WHEN a.TransactionStatus = 6
                        THEN 'Received'

                    ELSE 'Unknown'
                 END AS TransactionStatusText

            " +
                    sb.ToString() +
                    @"

            ORDER BY " +
                    sortColumn + " " + direction +
                    @"

            OFFSET @Start ROWS
            FETCH NEXT @Length ROWS ONLY
        ";


                // =========================================================
                // PAGINATION PARAMETERS
                // =========================================================
                List<SqlParameter> p2 =
                    parameters.ToList();

                p2.Add(
                    new SqlParameter(
                        "@Start",
                        gmdt.Start));

                p2.Add(
                    new SqlParameter(
                        "@Length",
                        gmdt.Length));


                // =========================================================
                // EXECUTE
                // =========================================================
                resp.InvDepartmentTransactions =
                    repo.Database.SqlQuery<
                        InvDepartmentTransactionInfo>(
                            sql,
                            p2.ToArray())
                    .ToList();
            }

            return resp;
        }
    }
}