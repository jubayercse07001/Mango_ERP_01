using ERP.Models.Inventory.InvUnitConversion;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.Inventory
{
    public class InvUnitConversionDAL
    {
        // ============================================================
        // GET UNIT CONVERSION BY ROW ID
        // ============================================================
        public static InvUnitConversionInfo GetInvUnitConversionByID(
            string RowId)
        {
            InvUnitConversionInfo conversion = null;

            using (var repo = new GMContext())
            {
                conversion =
                    repo.Database.SqlQuery<InvUnitConversionInfo>(@"
                        SELECT
                            a.RowId,
                            a.InvUnitConversionId,

                            a.FirstUnitId,
                            u1.MinUnitName AS FirstUnitName,

                            a.SecondUnitId,
                            u2.MinUnitName AS SecondUnitName,

                            a.ConversionRate,

                            a.DomainStatus,

                            a.CreateUser,
                            a.UpdateUser,

                            a.CreateDate,
                            a.UpdateDate

                        FROM InvUnitConversion a

                        LEFT JOIN InvUnit u1
                            ON u1.RowId = a.FirstUnitId

                        LEFT JOIN InvUnit u2
                            ON u2.RowId = a.SecondUnitId

                        WHERE a.RowId = @RowId
                        AND a.DomainStatus = 1
                    ",
                    new SqlParameter(
                        "@RowId",
                        RowId
                    ))
                    .FirstOrDefault();
            }

            return conversion;
        }


        // ============================================================
        // CHECK DUPLICATE UNIT CONVERSION
        //
        // Add:
        // RowId = null
        //
        // Edit:
        // RowId = current RowId
        //
        // Example:
        // Box -> Piece
        // ============================================================
        public static InvUnitConversionInfo
            GetInvUnitConversionByUnit(
                string FirstUnitId,
                string SecondUnitId,
                string RowId)
        {
            InvUnitConversionInfo conversion = null;

            using (var repo = new GMContext())
            {
                conversion =
                    repo.Database.SqlQuery<InvUnitConversionInfo>(@"
                        SELECT
                            RowId,
                            InvUnitConversionId,

                            FirstUnitId,
                            SecondUnitId,

                            ConversionRate,

                            DomainStatus,

                            CreateUser,
                            UpdateUser,

                            CreateDate,
                            UpdateDate

                        FROM InvUnitConversion

                        WHERE FirstUnitId = @FirstUnitId

                        AND SecondUnitId = @SecondUnitId

                        AND
                        (
                            @RowId IS NULL
                            OR RowId <> @RowId
                        )

                        AND DomainStatus = 1
                    ",
                    new SqlParameter(
                        "@FirstUnitId",
                        FirstUnitId
                    ),
                    new SqlParameter(
                        "@SecondUnitId",
                        SecondUnitId
                    ),
                    new SqlParameter(
                        "@RowId",
                        (object)RowId ?? DBNull.Value
                    ))
                    .FirstOrDefault();
            }

            return conversion;
        }


        // ============================================================
        // GET ALL UNIT CONVERSIONS
        // ============================================================
        public static List<InvUnitConversionInfo>
            GetAllInvUnitConversions()
        {
            List<InvUnitConversionInfo> conversions = null;

            using (var repo = new GMContext())
            {
                conversions =
                    repo.Database.SqlQuery<InvUnitConversionInfo>(@"
                        SELECT
                            a.RowId,
                            a.InvUnitConversionId,

                            a.FirstUnitId,
                            u1.MinUnitName AS FirstUnitName,

                            a.SecondUnitId,
                            u2.MinUnitName AS SecondUnitName,

                            a.ConversionRate,

                            a.DomainStatus,

                            a.CreateUser,
                            a.UpdateUser,

                            a.CreateDate,
                            a.UpdateDate

                        FROM InvUnitConversion a

                        LEFT JOIN InvUnit u1
                            ON u1.RowId = a.FirstUnitId

                        LEFT JOIN InvUnit u2
                            ON u2.RowId = a.SecondUnitId

                        WHERE a.DomainStatus = 1

                        ORDER BY
                            a.InvUnitConversionId
                    ")
                    .ToList();
            }

            return conversions;
        }


        // ============================================================
        // ADD UNIT CONVERSION
        // ============================================================
        public static void AddInvUnitConversion(
    InvUnitConversionInfo conversion,
    string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                // ----------------------------------------------------
                // Generate next InvUnitConversionId
                // ----------------------------------------------------
                int nextInvUnitConversionId =
                    repo.Database.SqlQuery<int>(@"
                SELECT
                    ISNULL(
                        MAX(InvUnitConversionId),
                        0
                    ) + 1
                FROM InvUnitConversion
            ")
                    .FirstOrDefault();


                // ----------------------------------------------------
                // INSERT
                // ----------------------------------------------------
                repo.Database.ExecuteSqlCommand(@"
            INSERT INTO InvUnitConversion
            (
                 RowId
                ,InvUnitConversionId
                ,FirstUnitId
                ,SecondUnitId
                ,ConversionRate
                ,CreateUser
                ,CreateDate
                ,DomainStatus
            )
            VALUES
            (
                 @RowId
                ,@InvUnitConversionId
                ,@FirstUnitId
                ,@SecondUnitId
                ,@ConversionRate
                ,@CreateUser
                ,GETDATE()
                ,1
            )
        ",
                    new SqlParameter(
                        "@RowId",
                        conversion.RowId
                    ),

                    new SqlParameter(
                        "@InvUnitConversionId",
                        nextInvUnitConversionId
                    ),

                    new SqlParameter(
                        "@FirstUnitId",
                        conversion.FirstUnitId
                    ),

                    new SqlParameter(
                        "@SecondUnitId",
                        conversion.SecondUnitId
                    ),

                    new SqlParameter(
                        "@ConversionRate",
                        conversion.ConversionRate
                    ),

                    new SqlParameter(
                        "@CreateUser",
                        loggedInUser
                    )
                );
            }
        }


        // ============================================================
        // UPDATE UNIT CONVERSION
        // ============================================================
        public static void UpdateInvUnitConversion(
            InvUnitConversionInfo conversion,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvUnitConversion

                    SET
                        FirstUnitId = @FirstUnitId,

                        SecondUnitId = @SecondUnitId,

                        ConversionRate = @ConversionRate,

                        UpdateUser = @UpdateUser,

                        UpdateDate = GETDATE()

                    WHERE RowId = @RowId

                    AND DomainStatus = 1
                ",
                new SqlParameter(
                    "@RowId",
                    conversion.RowId
                ),

                new SqlParameter(
                    "@FirstUnitId",
                    conversion.FirstUnitId
                ),

                new SqlParameter(
                    "@SecondUnitId",
                    conversion.SecondUnitId
                ),

                new SqlParameter(
                    "@ConversionRate",
                    conversion.ConversionRate
                ),

                new SqlParameter(
                    "@UpdateUser",
                    loggedInUser
                ));
            }
        }


        // ============================================================
        // DELETE UNIT CONVERSION
        // ============================================================
        public static void DeleteInvUnitConversion(
            InvUnitConversionInfo conversion,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvUnitConversion

                    SET
                        DomainStatus = 0,

                        UpdateUser = @UpdateUser,

                        UpdateDate = GETDATE()

                    WHERE RowId = @RowId

                    AND DomainStatus = 1
                ",
                new SqlParameter(
                    "@RowId",
                    conversion.RowId
                ),

                new SqlParameter(
                    "@UpdateUser",
                    loggedInUser
                ));
            }
        }


        // ============================================================
        // GET FILTERED UNIT CONVERSIONS
        //
        // DATATABLES SERVER-SIDE PROCESSING
        // ============================================================
        public static InvUnitConversionResponse GetFilteredInvUnitConversions(HttpRequestBase Request)
        {
            InvUnitConversionResponse resp = new InvUnitConversionResponse();

            GMDataTableDAL gmdt = new GMDataTableDAL(Request);

            resp.InvUnitConversions = null;

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
            FROM InvUnitConversion a

            LEFT JOIN InvUnit u1
                ON u1.RowId = a.FirstUnitId

            LEFT JOIN InvUnit u2
                ON u2.RowId = a.SecondUnitId

            WHERE a.DomainStatus = 1
        ");

                // =========================================================
                // TOTAL RECORD COUNT
                // =========================================================

                resp.TotalRecordCount = repo.Database.SqlQuery<int>(string.Format(@"
            SELECT COUNT(*)
            {0}
        ", sb.ToString())).Single();


                // =========================================================
                // SEARCH
                // =========================================================

                List<SqlParameter> sqlParameters =
                    new List<SqlParameter>();

                List<SqlParameter> sqlParameters1 =
                    new List<SqlParameter>();


                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.AppendLine(@"
                AND
                (
                       u1.MinUnitName LIKE '%' + @SearchValue + '%'
                    OR u2.MinUnitName LIKE '%' + @SearchValue + '%'
                    OR CAST(a.ConversionRate AS VARCHAR(50))
                       LIKE '%' + @SearchValue + '%'
                )
            ");

                    sqlParameters.Add(
                        new SqlParameter(
                            "@SearchValue",
                            gmdt.SearchValue
                        )
                    );
                }


                // =========================================================
                // FILTERED RECORD COUNT
                // =========================================================

                resp.FilteredRecordCount =
                    repo.Database.SqlQuery<int>(string.Format(@"
                SELECT COUNT(*)
                {0}
            ", sb.ToString()),
                    sqlParameters.ToArray())
                    .Single();


                // =========================================================
                // PAGING PARAMETERS
                // =========================================================

                sqlParameters1.AddRange(sqlParameters);

                sqlParameters1.Add(
                    new SqlParameter("@Skip", gmdt.Start)
                );

                sqlParameters1.Add(
                    new SqlParameter("@Take", gmdt.Length)
                );


                // =========================================================
                // SORT COLUMN
                // =========================================================

                string sortColumn;

                switch (gmdt.SortColum.ToString())
                {
                    case "FirstUnitName":
                        sortColumn = "u1.MinUnitName";
                        break;

                    case "SecondUnitName":
                        sortColumn = "u2.MinUnitName";
                        break;

                    case "ConversionRate":
                        sortColumn = "a.ConversionRate";
                        break;

                    case "InvUnitConversionId":
                        sortColumn = "a.InvUnitConversionId";
                        break;

                    case "FirstUnitId":
                        sortColumn = "a.FirstUnitId";
                        break;

                    case "SecondUnitId":
                        sortColumn = "a.SecondUnitId";
                        break;

                    default:
                        sortColumn = "a.InvUnitConversionId";
                        break;
                }


                // =========================================================
                // SORT DIRECTION
                // =========================================================

                string sortDirection =
                    gmdt.SortDir != null &&
                    gmdt.SortDir.ToLower() == "desc"
                        ? "DESC"
                        : "ASC";


                // =========================================================
                // GET DATA
                // =========================================================

                resp.InvUnitConversions =
                    repo.Database.SqlQuery<InvUnitConversionInfo>(string.Format(@"
                SELECT
                     a.RowId
                    ,a.InvUnitConversionId
                  --  ,a.FirstUnitId
                    ,u1.MinUnitName AS FirstUnitName
                  --  ,a.SecondUnitId
                    ,u2.MinUnitName AS SecondUnitName
                    ,a.ConversionRate

                {0}

                ORDER BY {1} {2}

                OFFSET (@Skip) ROWS
                FETCH NEXT (@Take) ROWS ONLY
            ",
                    sb.ToString(),
                    sortColumn,
                    sortDirection),
                    sqlParameters1.ToArray())
                    .ToList();
            }

            return resp;
        }


        // ============================================================
        // RESPONSE CLASS
        // ============================================================
        public class InvUnitConversionResponse
        {
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


            public List<InvUnitConversionInfo>
                InvUnitConversions
            {
                get;
                set;
            }
        }
    }
}