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
    public class InvDepartmentRequisition2ndDAL
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

       
        public static void AddRequisition(
    InvDepartmentRequisitionInfo requisition,
    string loggedInUser)
        {
            if (requisition == null)
                throw new ArgumentNullException(nameof(requisition));

            if (requisition.Details == null || !requisition.Details.Any())
                throw new Exception("At least one item is required.");

            using (var repo = new GMContext())
            using (var dbTransaction =
                repo.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    // 1. Allocate requisition ID
                    int requisitionId = repo.Database.SqlQuery<int>(@"
                SELECT ISNULL(MAX(RequisitionId), 0) + 1
                FROM InvDepartmentRequisition WITH (UPDLOCK, HOLDLOCK)
            ").Single();

                    // 2. Allocate transaction ID
                    int transactionId = repo.Database.SqlQuery<int>(@"
                SELECT ISNULL(MAX(TransactionId), 0) + 1
                FROM InvDepartmentTransaction WITH (UPDLOCK, HOLDLOCK)
            ").Single();

                    string requisitionRowId =
                        string.IsNullOrWhiteSpace(requisition.RowId)
                            ? Guid.NewGuid().ToString()
                            : requisition.RowId;

                    string transactionRowId = Guid.NewGuid().ToString();
                    string requisitionCode = "REQ-" + requisitionId.ToString("D4");
                    string transactionCode = "TRN-" + transactionId.ToString("D4");

                    // 3. Validate duplicate item/unit combinations
                    var duplicate = requisition.Details
                        .GroupBy(x => new { x.ItemId, x.UnitId })
                        .FirstOrDefault(g => g.Count() > 1);

                    if (duplicate != null)
                        throw new Exception(
                            "Duplicate item and unit combinations are not allowed.");

                    // 4. Validate lines and calculate estimated total
                    decimal totalEstimatedAmount = 0m;
                    foreach (var line in requisition.Details)
                    {
                        if (line == null)
                            throw new Exception("Invalid item line.");
                        if (string.IsNullOrWhiteSpace(line.ItemId))
                            throw new Exception("Item is required.");
                        if (string.IsNullOrWhiteSpace(line.UnitId))
                            throw new Exception("Unit is required.");
                        if (line.Quantity <= 0)
                            throw new Exception(
                                "Requested quantity must be greater than zero.");
                        if (line.UnitPriceEst < 0)
                            throw new Exception(
                                "Estimated unit price cannot be negative.");

                        line.TotalPriceEst = decimal.Round(
                            line.Quantity * line.UnitPriceEst,
                            2,
                            MidpointRounding.AwayFromZero);

                        totalEstimatedAmount += line.TotalPriceEst;
                    }

                    // ---------------------------------------------------------
                    // TABLE 1: InvDepartmentRequisition (header)
                    // ---------------------------------------------------------
                    // Use explicit int for Status (0 = STATUS_NEW)
                    // This avoids any constant / visibility issues
                    // const int statusNew = 0;

                    repo.Database.ExecuteSqlCommand(@"
                INSERT INTO InvDepartmentRequisition
                (
                    RowId, RequisitionId, RequisitionDate, RequisitionCode,
                    RequisitionName, DepartmentId, RequestedBy, JobId, JobNo,
                    RequiredFor, Status, IsApproved, IsIssued, IsRemoved,
                    DomainStatus, CreateUser, CreateDate
                )
                VALUES
                (
                    @RowId, @RequisitionId, @RequisitionDate, @RequisitionCode,
                    @RequisitionName, @DepartmentId, @RequestedBy, @JobId, @JobNo,
                    @RequiredFor, 0, 0, 0, 0,
                    1, @CreateUser, GETDATE()
                )",
                        new SqlParameter("@RowId", requisitionRowId),
                        new SqlParameter("@RequisitionId", requisitionId),
                        new SqlParameter("@RequisitionDate", requisition.RequisitionDate),
                        new SqlParameter("@RequisitionCode", requisitionCode),
                        new SqlParameter("@RequisitionName",
                            (object)requisition.RequisitionName ?? DBNull.Value),
                        new SqlParameter("@DepartmentId", requisition.DepartmentId),
                        new SqlParameter("@RequestedBy", requisition.RequestedBy),
                        new SqlParameter("@JobId",
                            (object)requisition.JobId ?? DBNull.Value),
                        new SqlParameter("@JobNo",
                            (object)requisition.JobNo ?? DBNull.Value),
                        new SqlParameter("@RequiredFor",
                            (object)requisition.RequiredFor ?? DBNull.Value),
                        //new SqlParameter("@Status", 0),          // ← FIXED
                        new SqlParameter("@CreateUser", loggedInUser));

                    // ---------------------------------------------------------
                    // TABLE 2: InvDepartmentRequisitionDetail
                    // ---------------------------------------------------------
                    foreach (var line in requisition.Details)
                    {
                        int detailId = repo.Database.SqlQuery<int>(@"
                    SELECT ISNULL(MAX(RequisitionDetailId), 0) + 1
                    FROM InvDepartmentRequisitionDetail
                        WITH (UPDLOCK, HOLDLOCK)
                ").Single();

                        repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO InvDepartmentRequisitionDetail
                    (
                        RowId, RequisitionDetailId, RequisitionId, ItemId,
                        Quantity, UnitId, UnitPriceEst, TotalPriceEst,
                        ApprovedQuantity, Remarks, DomainStatus, CreateUser,
                        CreateDate
                    )
                    VALUES
                    (
                        @RowId, @DetailId, @RequisitionId, @ItemId,
                        @Quantity, @UnitId, @UnitPriceEst, @TotalPriceEst,
                        0, @Remarks, 1, @CreateUser, GETDATE()
                    )",
                            new SqlParameter("@RowId", Guid.NewGuid().ToString()),
                            new SqlParameter("@DetailId", detailId),
                            new SqlParameter("@RequisitionId", requisitionRowId),
                            new SqlParameter("@ItemId", line.ItemId),
                            new SqlParameter("@Quantity", line.Quantity),
                            new SqlParameter("@UnitId", line.UnitId),
                            new SqlParameter("@UnitPriceEst", line.UnitPriceEst),
                            new SqlParameter("@TotalPriceEst", line.TotalPriceEst),
                            new SqlParameter("@Remarks",
                                (object)line.Remarks ?? DBNull.Value),
                            new SqlParameter("@CreateUser", loggedInUser));
                    }

                    // ---------------------------------------------------------
                    // TABLE 3: InvDepartmentTransaction (initial transaction)
                    // ---------------------------------------------------------
                    repo.Database.ExecuteSqlCommand(@"
                INSERT INTO InvDepartmentTransaction
                (
                    RowId, TransactionId, TransactionCode, TransactionDate,
                    RequisitionId, MaterialReveiveId, MaterialIssueId,
                    TransactionType, FromDepartment, ToDepartment, TotalAmount,
                    CurrencyId, MIVNo, INCMIVNo, FinancialYearId, Remarks,
                    IsIssued, IsReceived, IsRemoved, DomainStatus, CreateUser,
                    CreateDate
                )
                VALUES
                (
                    @RowId, @TransactionId, @TransactionCode, @TransactionDate,
                    @RequisitionId, '', '', @TransactionType,@FromDepartment, NULL, 
                    @TotalAmount, NULL, NULL, NULL, NULL, @Remarks,
                    0, 0, 0, 1, @CreateUser, GETDATE()
                )",
                        new SqlParameter("@RowId", transactionRowId),
                        new SqlParameter("@TransactionId", transactionId),
                        new SqlParameter("@TransactionCode", transactionCode),
                        new SqlParameter("@TransactionDate", requisition.RequisitionDate),
                        new SqlParameter("@RequisitionId", requisitionRowId),
                        new SqlParameter("@TransactionType", "Requisition New"),
                        new SqlParameter("@FromDepartment", requisition.DepartmentId),
                        new SqlParameter("@TotalAmount", totalEstimatedAmount),
                        new SqlParameter("@Remarks",
                            (object)requisition.RequiredFor ?? DBNull.Value),
                        new SqlParameter("@CreateUser", loggedInUser));

                    // ---------------------------------------------------------
                    // TABLE 4: InvDepartmentTransactionDetail
                    // Initial request is not a physical issue → Quantity = 0
                    // ---------------------------------------------------------
                    foreach (var line in requisition.Details)
                    {
                        int transactionDetailId =
                            repo.Database.SqlQuery<int>(@"
            SELECT ISNULL(MAX(TransactionDetailId), 0) + 1
            FROM InvDepartmentTransactionDetail
                WITH (UPDLOCK, HOLDLOCK)
        ").Single();

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
            UnitConversionId,
            ConversionRate,
            ConversionPrice,
            ConversionTotal,
            DomainStatus,
            CreateUser,
            CreateDate
        )
        VALUES
        (
            @RowId,
            @DetailId,
            @TransactionId,
            @ItemId,
            @UnitId,
            0,
            0,
            @UnitPrice,
            0,
            NULL,
            NULL,
            NULL,
            NULL,
            1,
            @CreateUser,
            GETDATE()
        )",
                            new SqlParameter(
                                "@RowId",
                                Guid.NewGuid().ToString()),

                            new SqlParameter(
                                "@DetailId",
                                transactionDetailId),

                            new SqlParameter(
                                "@TransactionId",
                                transactionRowId),

                            new SqlParameter(
                                "@ItemId",
                                line.ItemId),

                            new SqlParameter(
                                "@UnitId",
                                line.UnitId),

                            new SqlParameter(
                                "@UnitPrice",
                                line.UnitPriceEst),

                            new SqlParameter(
                                "@CreateUser",
                                loggedInUser)
                        );
                    }

                    // All four tables succeed or fail together
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


        //    public static void UpdateRequisition(
        //InvDepartmentRequisitionInfo requisition,
        //string loggedInUser)
        //    {
        //        if (requisition == null)
        //            throw new ArgumentNullException(nameof(requisition));

        //        if (string.IsNullOrWhiteSpace(requisition.RowId))
        //            throw new Exception("Requisition RowId is required.");

        //        if (requisition.Details == null || !requisition.Details.Any())
        //            throw new Exception("At least one item is required.");

        //        using (var repo = new GMContext())
        //        using (var dbTransaction =
        //            repo.Database.BeginTransaction(IsolationLevel.Serializable))
        //        {
        //            try
        //            {
        //                // ---------------------------------------------------------
        //                // 1. Get existing requisition
        //                // ---------------------------------------------------------

        //                var existingRequisition =
        //                    repo.Database.SqlQuery<InvDepartmentRequisitionInfo>(@"
        //                SELECT
        //                    RowId,
        //                    RequisitionId,
        //                    RequisitionDate,
        //                    RequisitionCode,
        //                    RequisitionName,
        //                    DepartmentId,
        //                    RequestedBy,
        //                    JobId,
        //                    JobNo,
        //                    RequiredFor,
        //                    Status,
        //                    IsApproved,
        //                    IsIssued,
        //                    IsRemoved
        //                FROM InvDepartmentRequisition WITH (UPDLOCK, HOLDLOCK)
        //                WHERE RowId = @RowId
        //                AND IsRemoved = 0
        //                AND DomainStatus = 1
        //            ",
        //                    new SqlParameter("@RowId", requisition.RowId))
        //                    .FirstOrDefault();

        //                if (existingRequisition == null)
        //                    throw new Exception("Requisition not found.");

        //                // ---------------------------------------------------------
        //                // 2. Only NEW requisitions can be edited
        //                // ---------------------------------------------------------

        //                if (existingRequisition.Status != STATUS_NEW)
        //                {
        //                    throw new Exception(
        //                        "Only a new/pending requisition can be edited. " +
        //                        "This requisition has already moved to workflow.");
        //                }

        //                if (existingRequisition.IsApproved != 0)
        //                {
        //                    throw new Exception(
        //                        "Approved requisition cannot be edited.");
        //                }

        //                if (existingRequisition.IsIssued != 0)
        //                {
        //                    throw new Exception(
        //                        "Issued requisition cannot be edited.");
        //                }

        //                // ---------------------------------------------------------
        //                // 3. Validate duplicate Item + Unit
        //                // ---------------------------------------------------------

        //                var duplicate = requisition.Details
        //                    .Where(x => x != null)
        //                    .GroupBy(x => new
        //                    {
        //                        x.ItemId,
        //                        x.UnitId
        //                    })
        //                    .FirstOrDefault(g => g.Count() > 1);

        //                if (duplicate != null)
        //                {
        //                    throw new Exception(
        //                        "Duplicate item and unit combinations are not allowed.");
        //                }

        //                // ---------------------------------------------------------
        //                // 4. Validate details and calculate estimated total
        //                // ---------------------------------------------------------

        //                decimal totalEstimatedAmount = 0m;

        //                foreach (var line in requisition.Details)
        //                {
        //                    if (line == null)
        //                        throw new Exception("Invalid item line.");

        //                    if (string.IsNullOrWhiteSpace(line.ItemId))
        //                        throw new Exception("Item is required.");

        //                    if (string.IsNullOrWhiteSpace(line.UnitId))
        //                        throw new Exception("Unit is required.");

        //                    if (line.Quantity <= 0)
        //                    {
        //                        throw new Exception(
        //                            "Requested quantity must be greater than zero.");
        //                    }

        //                    if (line.UnitPriceEst < 0)
        //                    {
        //                        throw new Exception(
        //                            "Estimated unit price cannot be negative.");
        //                    }

        //                    line.TotalPriceEst = decimal.Round(
        //                        line.Quantity * line.UnitPriceEst,
        //                        2,
        //                        MidpointRounding.AwayFromZero);

        //                    totalEstimatedAmount += line.TotalPriceEst;
        //                }

        //                // ---------------------------------------------------------
        //                // 5. Update requisition header
        //                // ---------------------------------------------------------

        //                repo.Database.ExecuteSqlCommand(@"
        //            UPDATE InvDepartmentRequisition
        //            SET
        //                RequisitionDate = @RequisitionDate,
        //                RequisitionName = @RequisitionName,
        //                DepartmentId = @DepartmentId,
        //                RequestedBy = @RequestedBy,
        //                JobId = @JobId,
        //                JobNo = @JobNo,
        //                RequiredFor = @RequiredFor,

        //                -- Keep workflow state as NEW
        //                Status = 0,
        //                IsApproved = 0,
        //                IsIssued = 0,

        //                UpdateUser = @UpdateUser,
        //                UpdateDate = GETDATE()

        //            WHERE RowId = @RowId
        //            AND IsRemoved = 0
        //            AND DomainStatus = 1
        //        ",
        //                new SqlParameter("@RowId", requisition.RowId),

        //                new SqlParameter("@RequisitionDate",
        //                    requisition.RequisitionDate),

        //                new SqlParameter("@RequisitionName",
        //                    (object)requisition.RequisitionName ?? DBNull.Value),

        //                new SqlParameter("@DepartmentId",
        //                    requisition.DepartmentId),

        //                new SqlParameter("@RequestedBy",
        //                    requisition.RequestedBy),

        //                new SqlParameter("@JobId",
        //                    (object)requisition.JobId ?? DBNull.Value),

        //                new SqlParameter("@JobNo",
        //                    (object)requisition.JobNo ?? DBNull.Value),

        //                new SqlParameter("@RequiredFor",
        //                    (object)requisition.RequiredFor ?? DBNull.Value),

        //                new SqlParameter("@UpdateUser",
        //                    loggedInUser));

        //                // ---------------------------------------------------------
        //                // 6. Delete old requisition details
        //                // ---------------------------------------------------------

        //                repo.Database.ExecuteSqlCommand(@"
        //            DELETE FROM InvDepartmentRequisitionDetail
        //            WHERE RequisitionId = @RequisitionId
        //        ",
        //                new SqlParameter(
        //                    "@RequisitionId",
        //                    requisition.RowId));

        //                // ---------------------------------------------------------
        //                // 7. Insert updated requisition details
        //                // ---------------------------------------------------------

        //                foreach (var line in requisition.Details)
        //                {
        //                    int detailId = repo.Database.SqlQuery<int>(@"
        //                SELECT ISNULL(MAX(RequisitionDetailId), 0) + 1
        //                FROM InvDepartmentRequisitionDetail
        //                WITH (UPDLOCK, HOLDLOCK)
        //            ").Single();

        //                    repo.Database.ExecuteSqlCommand(@"
        //                INSERT INTO InvDepartmentRequisitionDetail
        //                (
        //                    RowId,
        //                    RequisitionDetailId,
        //                    RequisitionId,
        //                    ItemId,
        //                    Quantity,
        //                    UnitId,
        //                    UnitPriceEst,
        //                    TotalPriceEst,
        //                    ApprovedQuantity,
        //                    Remarks,
        //                    DomainStatus,
        //                    CreateUser,
        //                    CreateDate
        //                )
        //                VALUES
        //                (
        //                    @RowId,
        //                    @DetailId,
        //                    @RequisitionId,
        //                    @ItemId,
        //                    @Quantity,
        //                    @UnitId,
        //                    @UnitPriceEst,
        //                    @TotalPriceEst,
        //                    0,
        //                    @Remarks,
        //                    1,
        //                    @CreateUser,
        //                    GETDATE()
        //                )
        //            ",
        //                    new SqlParameter(
        //                        "@RowId",
        //                        Guid.NewGuid().ToString()),

        //                    new SqlParameter(
        //                        "@DetailId",
        //                        detailId),

        //                    new SqlParameter(
        //                        "@RequisitionId",
        //                        requisition.RowId),

        //                    new SqlParameter(
        //                        "@ItemId",
        //                        line.ItemId),

        //                    new SqlParameter(
        //                        "@Quantity",
        //                        line.Quantity),

        //                    new SqlParameter(
        //                        "@UnitId",
        //                        line.UnitId),

        //                    new SqlParameter(
        //                        "@UnitPriceEst",
        //                        line.UnitPriceEst),

        //                    new SqlParameter(
        //                        "@TotalPriceEst",
        //                        line.TotalPriceEst),

        //                    new SqlParameter(
        //                        "@Remarks",
        //                        (object)line.Remarks ?? DBNull.Value),

        //                    new SqlParameter(
        //                        "@CreateUser",
        //                        loggedInUser));
        //                }

        //                // ---------------------------------------------------------
        //                // 8. Find the initial transaction
        //                //
        //                // TransactionType = 'Requisition New'
        //                // ---------------------------------------------------------

        //                var initialTransaction = repo.Database.SqlQuery<InitialTransactionInfo>(@"
        //                SELECT TOP 1
        //                    RowId,
        //                    TransactionId
        //                FROM InvDepartmentTransaction WITH (UPDLOCK, HOLDLOCK)
        //                WHERE RequisitionId = @RequisitionId
        //                AND TransactionType = 'Requisition New'
        //                AND IsRemoved = 0
        //                AND DomainStatus = 1
        //                ORDER BY TransactionId
        //            ",
        //                    new SqlParameter(
        //                        "@RequisitionId",
        //                        requisition.RowId))
        //                    .FirstOrDefault();

        //                // ---------------------------------------------------------
        //                // 9. Update initial transaction header
        //                // ---------------------------------------------------------

        //                if (initialTransaction != null)
        //                {
        //                    repo.Database.ExecuteSqlCommand(@"
        //                UPDATE InvDepartmentTransaction
        //                SET
        //                    TransactionDate = @TransactionDate,
        //                    FromDepartment = @FromDepartment,
        //                    ToDepartment = NULL,
        //                    TotalAmount = @TotalAmount,
        //                    Remarks = @Remarks,
        //                    UpdateUser = @UpdateUser,
        //                    UpdateDate = GETDATE()
        //                WHERE RowId = @RowId
        //                AND IsRemoved = 0
        //                AND DomainStatus = 1
        //            ",
        //                    new SqlParameter(
        //                        "@RowId",
        //                        initialTransaction.RowId),

        //                    new SqlParameter(
        //                        "@TransactionDate",
        //                        requisition.RequisitionDate),

        //                    new SqlParameter(
        //                        "@FromDepartment",
        //                        requisition.DepartmentId),

        //                    new SqlParameter(
        //                        "@TotalAmount",
        //                        totalEstimatedAmount),

        //                    new SqlParameter(
        //                        "@Remarks",
        //                        (object)requisition.RequiredFor ?? DBNull.Value),

        //                    new SqlParameter(
        //                        "@UpdateUser",
        //                        loggedInUser));

        //                    // -----------------------------------------------------
        //                    // 10. Delete old transaction detail snapshot
        //                    // -----------------------------------------------------

        //                    repo.Database.ExecuteSqlCommand(@"
        //                DELETE FROM InvDepartmentTransactionDetail
        //                WHERE TransactionId = @TransactionId
        //            ",
        //                    new SqlParameter(
        //                        "@TransactionId",
        //                        initialTransaction.RowId));

        //                    // -----------------------------------------------------
        //                    // 11. Insert updated transaction detail snapshot
        //                    //
        //                    // Requisition stage:
        //                    // Quantity = 0
        //                    // MRRQty = 0
        //                    // UnitPrice = estimated unit price
        //                    // TotalPrice = 0
        //                    // -----------------------------------------------------

        //                    foreach (var line in requisition.Details)
        //                    {
        //                        int transactionDetailId =
        //                            repo.Database.SqlQuery<int>(@"
        //                        SELECT
        //                            ISNULL(MAX(TransactionDetailId), 0) + 1
        //                        FROM InvDepartmentTransactionDetail
        //                        WITH (UPDLOCK, HOLDLOCK)
        //                    ").Single();

        //                        repo.Database.ExecuteSqlCommand(@"
        //                    INSERT INTO InvDepartmentTransactionDetail
        //                    (
        //                        RowId,
        //                        TransactionDetailId,
        //                        TransactionId,
        //                        ItemId,
        //                        UnitId,
        //                        Quantity,
        //                        MRRQty,
        //                        UnitPrice,
        //                        TotalPrice,
        //                        UnitConversionId,
        //                        ConversionRate,
        //                        ConversionPrice,
        //                        ConversionTotal,
        //                        DomainStatus,
        //                        CreateUser,
        //                        CreateDate
        //                    )
        //                    VALUES
        //                    (
        //                        @RowId,
        //                        @DetailId,
        //                        @TransactionId,
        //                        @ItemId,
        //                        @UnitId,
        //                        0,
        //                        0,
        //                        @UnitPrice,
        //                        0,
        //                        NULL,
        //                        NULL,
        //                        NULL,
        //                        NULL,
        //                        1,
        //                        @CreateUser,
        //                        GETDATE()
        //                    )
        //                ",
        //                        new SqlParameter(
        //                            "@RowId",
        //                            Guid.NewGuid().ToString()),

        //                        new SqlParameter(
        //                            "@DetailId",
        //                            transactionDetailId),

        //                        new SqlParameter(
        //                            "@TransactionId",
        //                            initialTransaction.RowId),

        //                        new SqlParameter(
        //                            "@ItemId",
        //                            line.ItemId),

        //                        new SqlParameter(
        //                            "@UnitId",
        //                            line.UnitId),

        //                        new SqlParameter(
        //                            "@UnitPrice",
        //                            line.UnitPriceEst),

        //                        new SqlParameter(
        //                            "@CreateUser",
        //                            loggedInUser));
        //                    }
        //                }

        //                // ---------------------------------------------------------
        //                // 12. Commit everything
        //                // ---------------------------------------------------------

        //                dbTransaction.Commit();
        //            }
        //            catch
        //            {
        //                dbTransaction.Rollback();
        //                throw;
        //            }
        //        }
        //    }
        public static void UpdateRequisition(
        InvDepartmentRequisitionInfo requisition,
        string loggedInUser)
        {
            if (requisition == null)
                throw new ArgumentNullException(nameof(requisition));

            if (string.IsNullOrWhiteSpace(requisition.RowId))
                throw new Exception("Requisition RowId is required.");

            if (requisition.Details == null || !requisition.Details.Any())
                throw new Exception("At least one item is required.");

            using (var repo = new GMContext())
            using (var dbTransaction =
                repo.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    // ---------------------------------------------------------
                    // 1. Get existing requisition
                    // ---------------------------------------------------------
                    var existingRequisition =
                        repo.Database.SqlQuery<InvDepartmentRequisitionInfo>(@"
                    SELECT
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
                        IsRemoved
                    FROM InvDepartmentRequisition WITH (UPDLOCK, HOLDLOCK)
                    WHERE RowId = @RowId
                    AND IsRemoved = 0
                    AND DomainStatus = 1
                ",
                        new SqlParameter("@RowId", requisition.RowId))
                        .FirstOrDefault();

                    if (existingRequisition == null)
                        throw new Exception("Requisition not found.");

                    // ---------------------------------------------------------
                    // 2. Only NEW requisitions can be edited
                    // ---------------------------------------------------------
                    if (existingRequisition.Status != STATUS_NEW)
                    {
                        throw new Exception(
                            "Only a new/pending requisition can be edited. " +
                            "This requisition has already moved to workflow.");
                    }

                    if (existingRequisition.IsApproved != 0)
                        throw new Exception("Approved requisition cannot be edited.");

                    if (existingRequisition.IsIssued != 0)
                        throw new Exception("Issued requisition cannot be edited.");

                    // ---------------------------------------------------------
                    // 3. Validate duplicate Item + Unit
                    // ---------------------------------------------------------
                    var duplicate = requisition.Details
                        .Where(x => x != null)
                        .GroupBy(x => new { x.ItemId, x.UnitId })
                        .FirstOrDefault(g => g.Count() > 1);

                    if (duplicate != null)
                        throw new Exception("Duplicate item and unit combinations are not allowed.");

                    // ---------------------------------------------------------
                    // 4. Validate details and calculate estimated total
                    // ---------------------------------------------------------
                    decimal totalEstimatedAmount = 0m;

                    foreach (var line in requisition.Details)
                    {
                        if (line == null)
                            throw new Exception("Invalid item line.");

                        if (string.IsNullOrWhiteSpace(line.ItemId))
                            throw new Exception("Item is required.");

                        if (string.IsNullOrWhiteSpace(line.UnitId))
                            throw new Exception("Unit is required.");

                        if (line.Quantity <= 0)
                            throw new Exception("Requested quantity must be greater than zero.");

                        if (line.UnitPriceEst < 0)
                            throw new Exception("Estimated unit price cannot be negative.");

                        line.TotalPriceEst = decimal.Round(
                            line.Quantity * line.UnitPriceEst,
                            2,
                            MidpointRounding.AwayFromZero);

                        totalEstimatedAmount += line.TotalPriceEst;
                    }

                    // ---------------------------------------------------------
                    // 5. Update requisition header
                    // ---------------------------------------------------------
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
                    Status = 0,
                    IsApproved = 0,
                    IsIssued = 0,
                    UpdateUser = @UpdateUser,
                    UpdateDate = GETDATE()
                WHERE RowId = @RowId
                AND IsRemoved = 0
                AND DomainStatus = 1
            ",
                    new SqlParameter("@RowId", requisition.RowId),
                    new SqlParameter("@RequisitionDate", requisition.RequisitionDate),
                    new SqlParameter("@RequisitionName", (object)requisition.RequisitionName ?? DBNull.Value),
                    new SqlParameter("@DepartmentId", requisition.DepartmentId),
                    new SqlParameter("@RequestedBy", requisition.RequestedBy),
                    new SqlParameter("@JobId", (object)requisition.JobId ?? DBNull.Value),
                    new SqlParameter("@JobNo", (object)requisition.JobNo ?? DBNull.Value),
                    new SqlParameter("@RequiredFor", (object)requisition.RequiredFor ?? DBNull.Value),
                    new SqlParameter("@UpdateUser", loggedInUser));

                    // ---------------------------------------------------------
                    // 6. Soft-delete old requisition details (DomainStatus = 0)
                    // ---------------------------------------------------------
                    repo.Database.ExecuteSqlCommand(@"
                UPDATE InvDepartmentRequisitionDetail
                SET
                    DomainStatus = 0,
                    UpdateUser = @UpdateUser,
                    UpdateDate = GETDATE()
                WHERE RequisitionId = @RequisitionId
                AND DomainStatus = 1
            ",
                    new SqlParameter("@RequisitionId", requisition.RowId),
                    new SqlParameter("@UpdateUser", loggedInUser));

                    // ---------------------------------------------------------
                    // 7. Insert updated requisition details (DomainStatus = 1)
                    // ---------------------------------------------------------
                    foreach (var line in requisition.Details)
                    {
                        int detailId = repo.Database.SqlQuery<int>(@"
                    SELECT ISNULL(MAX(RequisitionDetailId), 0) + 1
                    FROM InvDepartmentRequisitionDetail
                    WITH (UPDLOCK, HOLDLOCK)
                ").Single();

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
                        ApprovedQuantity,
                        Remarks,
                        DomainStatus,
                        CreateUser,
                        CreateDate
                    )
                    VALUES
                    (
                        @RowId,
                        @DetailId,
                        @RequisitionId,
                        @ItemId,
                        @Quantity,
                        @UnitId,
                        @UnitPriceEst,
                        @TotalPriceEst,
                        0,
                        @Remarks,
                        1,
                        @CreateUser,
                        GETDATE()
                    )
                ",
                        new SqlParameter("@RowId", Guid.NewGuid().ToString()),
                        new SqlParameter("@DetailId", detailId),
                        new SqlParameter("@RequisitionId", requisition.RowId),
                        new SqlParameter("@ItemId", line.ItemId),
                        new SqlParameter("@Quantity", line.Quantity),
                        new SqlParameter("@UnitId", line.UnitId),
                        new SqlParameter("@UnitPriceEst", line.UnitPriceEst),
                        new SqlParameter("@TotalPriceEst", line.TotalPriceEst),
                        new SqlParameter("@Remarks", (object)line.Remarks ?? DBNull.Value),
                        new SqlParameter("@CreateUser", loggedInUser));
                    }

                    // ---------------------------------------------------------
                    // 8. Find the initial transaction (TransactionType = 'Requisition New')
                    // ---------------------------------------------------------
                    var initialTransaction = repo.Database.SqlQuery<InitialTransactionInfo>(@"
                SELECT TOP 1
                    RowId,
                    TransactionId
                FROM InvDepartmentTransaction WITH (UPDLOCK, HOLDLOCK)
                WHERE RequisitionId = @RequisitionId
                AND TransactionType = 'Requisition New'
                AND IsRemoved = 0
                AND DomainStatus = 1
                ORDER BY TransactionId
            ",
                    new SqlParameter("@RequisitionId", requisition.RowId))
                    .FirstOrDefault();

                    // ---------------------------------------------------------
                    // 9. Update initial transaction header
                    // ---------------------------------------------------------
                    if (initialTransaction != null)
                    {
                        repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvDepartmentTransaction
                    SET
                        TransactionDate = @TransactionDate,
                        FromDepartment = @FromDepartment,
                        ToDepartment = NULL,
                        TotalAmount = @TotalAmount,
                        Remarks = @Remarks,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                    AND IsRemoved = 0
                    AND DomainStatus = 1
                ",
                        new SqlParameter("@RowId", initialTransaction.RowId),
                        new SqlParameter("@TransactionDate", requisition.RequisitionDate),
                        new SqlParameter("@FromDepartment", requisition.DepartmentId),
                        new SqlParameter("@TotalAmount", totalEstimatedAmount),
                        new SqlParameter("@Remarks", (object)requisition.RequiredFor ?? DBNull.Value),
                        new SqlParameter("@UpdateUser", loggedInUser));

                        // -----------------------------------------------------
                        // 10. Soft-delete old transaction detail snapshot (DomainStatus = 0)
                        // -----------------------------------------------------
                        repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvDepartmentTransactionDetail
                    SET
                        DomainStatus = 0,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE TransactionId = @TransactionId
                    AND DomainStatus = 1
                ",
                        new SqlParameter("@TransactionId", initialTransaction.RowId),
                        new SqlParameter("@UpdateUser", loggedInUser));

                        // -----------------------------------------------------
                        // 11. Insert updated transaction detail snapshot (DomainStatus = 1)
                        //     Quantity = 0, MRRQty = 0, TotalPrice = 0
                        // -----------------------------------------------------
                        foreach (var line in requisition.Details)
                        {
                            int transactionDetailId = repo.Database.SqlQuery<int>(@"
                        SELECT ISNULL(MAX(TransactionDetailId), 0) + 1
                        FROM InvDepartmentTransactionDetail
                        WITH (UPDLOCK, HOLDLOCK)
                    ").Single();

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
                            UnitConversionId,
                            ConversionRate,
                            ConversionPrice,
                            ConversionTotal,
                            DomainStatus,
                            CreateUser,
                            CreateDate
                        )
                        VALUES
                        (
                            @RowId,
                            @DetailId,
                            @TransactionId,
                            @ItemId,
                            @UnitId,
                            0,
                            0,
                            @UnitPrice,
                            0,
                            NULL,
                            NULL,
                            NULL,
                            NULL,
                            1,
                            @CreateUser,
                            GETDATE()
                        )
                    ",
                            new SqlParameter("@RowId", Guid.NewGuid().ToString()),
                            new SqlParameter("@DetailId", transactionDetailId),
                            new SqlParameter("@TransactionId", initialTransaction.RowId),
                            new SqlParameter("@ItemId", line.ItemId),
                            new SqlParameter("@UnitId", line.UnitId),
                            new SqlParameter("@UnitPrice", line.UnitPriceEst),
                            new SqlParameter("@CreateUser", loggedInUser));
                        }
                    }

                    // ---------------------------------------------------------
                    // 12. Commit everything
                    // ---------------------------------------------------------
                    dbTransaction.Commit();
                }
                catch
                {
                    dbTransaction.Rollback();
                    throw;
                }
            }
        }
        private class InitialTransactionInfo
        {
            public string RowId { get; set; }

            public int TransactionId { get; set; }
        }
        
        public static void DeleteRequisition(string requisitionRowId, string loggedInUser)

        {
            // ---------------------------------------------------------
            // 1. Basic validation
            // ---------------------------------------------------------

            if (string.IsNullOrWhiteSpace(requisitionRowId))
                throw new Exception("Requisition RowId is required.");

            if (string.IsNullOrWhiteSpace(loggedInUser))
                throw new Exception("Logged in user is required.");


            using (var repo = new GMContext())
            using (var dbTransaction =
                repo.Database.BeginTransaction(IsolationLevel.Serializable))
            {
                try
                {
                    // ---------------------------------------------------------
                    // 2. Get existing requisition
                    // ---------------------------------------------------------

                    var requisition =
                        repo.Database.SqlQuery<InvDepartmentRequisitionInfo>(@"
                    SELECT
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
                        IsRemoved
                    FROM InvDepartmentRequisition WITH (UPDLOCK, HOLDLOCK)
                    WHERE RowId = @RowId
                    AND IsRemoved = 0
                    AND DomainStatus = 1
                ",
                        new SqlParameter(
                            "@RowId",
                            requisitionRowId))
                        .FirstOrDefault();


                    // ---------------------------------------------------------
                    // 3. Check requisition exists
                    // ---------------------------------------------------------

                    if (requisition == null)
                        throw new Exception(
                            "Requisition not found or already deleted.");


                    // ---------------------------------------------------------
                    // 4. Only NEW requisition can be deleted
                    // ---------------------------------------------------------

                    if (requisition.Status != STATUS_NEW)
                    {
                        throw new Exception(
                            "Only a new/pending requisition can be deleted. " +
                            "This requisition has already moved to workflow.");
                    }


                    // ---------------------------------------------------------
                    // 5. Approved requisition cannot be deleted
                    // ---------------------------------------------------------

                    if (requisition.IsApproved != 0)
                    {
                        throw new Exception(
                            "Approved requisition cannot be deleted.");
                    }


                    // ---------------------------------------------------------
                    // 6. Issued requisition cannot be deleted
                    // ---------------------------------------------------------

                    if (requisition.IsIssued != 0)
                    {
                        throw new Exception(
                            "Issued requisition cannot be deleted.");
                    }


                    // ---------------------------------------------------------
                    // 7. Get initial transaction
                    //
                    // This is the transaction created during Add Requisition.
                    //
                    // TransactionType = 'Requisition New'
                    // ---------------------------------------------------------

                    var initialTransaction =
                        repo.Database.SqlQuery<InitialTransactionInfo>(@"
                    SELECT TOP 1
                        RowId,
                        TransactionId
                    FROM InvDepartmentTransaction WITH (UPDLOCK, HOLDLOCK)
                    WHERE RequisitionId = @RequisitionId
                    AND TransactionType = 'Requisition New'
                    AND IsRemoved = 0
                    AND DomainStatus = 1
                    ORDER BY TransactionId
                ",
                        new SqlParameter(
                            "@RequisitionId",
                            requisitionRowId))
                        .FirstOrDefault();


                    // ---------------------------------------------------------
                    // 8. Soft delete transaction details
                    // ---------------------------------------------------------

                    if (initialTransaction != null)
                    {
                        repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvDepartmentTransactionDetail
                    SET
                        DomainStatus = 0,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE TransactionId = @TransactionId
                    AND DomainStatus = 1
                ",
                        new SqlParameter(
                            "@TransactionId",
                            initialTransaction.RowId),

                        new SqlParameter(
                            "@UpdateUser",
                            loggedInUser));


                        // -----------------------------------------------------
                        // 9. Soft delete initial transaction
                        // -----------------------------------------------------

                        repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvDepartmentTransaction
                    SET
                        IsRemoved = 1,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                    AND IsRemoved = 0
                    AND DomainStatus = 1
                ",
                        new SqlParameter(
                            "@RowId",
                            initialTransaction.RowId),

                        new SqlParameter(
                            "@UpdateUser",
                            loggedInUser));
                    }


                    // ---------------------------------------------------------
                    // 10. Soft delete requisition details
                    // ---------------------------------------------------------

                    repo.Database.ExecuteSqlCommand(@"
                UPDATE InvDepartmentRequisitionDetail
                SET
                    DomainStatus = 0,
                    UpdateUser = @UpdateUser,
                    UpdateDate = GETDATE()
                WHERE RequisitionId = @RequisitionId
                AND DomainStatus = 1
            ",
                    new SqlParameter(
                        "@RequisitionId",
                        requisitionRowId),

                    new SqlParameter(
                        "@UpdateUser",
                        loggedInUser));


                    // ---------------------------------------------------------
                    // 11. Soft delete requisition header
                    // ---------------------------------------------------------

                    repo.Database.ExecuteSqlCommand(@"
                UPDATE InvDepartmentRequisition
                SET
                    IsRemoved = 1,
                    UpdateUser = @UpdateUser,
                    UpdateDate = GETDATE()
                WHERE RowId = @RowId
                AND IsRemoved = 0
                AND DomainStatus = 1
            ",
                    new SqlParameter(
                        "@RowId",
                        requisitionRowId),

                    new SqlParameter(
                        "@UpdateUser",
                        loggedInUser));


                    // ---------------------------------------------------------
                    // 12. Commit all changes
                    // ---------------------------------------------------------

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
        
        public static InvDepartmentRequisitionResponse GetFilteredRequisitions(HttpRequestBase Request, string loggedInUserID)
        {
            InvDepartmentRequisitionResponse resp = new InvDepartmentRequisitionResponse();

            GMDataTableDAL gmdt = new GMDataTableDAL(Request);
            string requestEmpId = loggedInUserID;
            resp.Requisitions = null;

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
                    FROM InvDepartmentRequisition a
                    LEFT JOIN InvDepartment b ON b.RowId = a.DepartmentId
                    LEFT JOIN HrEmployee c ON c.RowId = a.RequestedBy
                    WHERE a.DomainStatus = 1 and a.Status =0 -- and a.RequestedBy = requestEmpId
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
