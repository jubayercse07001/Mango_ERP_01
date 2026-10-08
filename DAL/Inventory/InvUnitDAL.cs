using ERP.Models.Inventory.InvUnit;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.Inventory
{
    public class InvUnitDAL
    {
        public static InvUnitInfo GetInvUnitByID(string RowId)
        {
            InvUnitInfo unit = null;

            using (var repo = new GMContext())
            {
                unit = repo.Database.SqlQuery<InvUnitInfo>(@"
                    SELECT
                        RowId,
                        UnitId,
                        MinUnitName,
                        UnitSymbol
                    FROM InvUnit
                    WHERE RowId = @RowId
                    AND DomainStatus = 1
                ",
                new SqlParameter("@RowId", RowId))
                .FirstOrDefault();
            }

            return unit;
        }

        public static InvUnitInfo GetInvUnitByName(
            string MinUnitName,
            string RowId)
        {
            InvUnitInfo unit = null;

            using (var repo = new GMContext())
            {
                unit = repo.Database.SqlQuery<InvUnitInfo>(@"
                    SELECT *
                    FROM InvUnit
                    WHERE MinUnitName = @MinUnitName
                    AND (@RowId IS NULL OR RowId <> @RowId)
                    AND DomainStatus = 1
                ",
                new SqlParameter("@MinUnitName", MinUnitName),
                new SqlParameter("@RowId", (object)RowId ?? DBNull.Value))
                .FirstOrDefault();
            }

            return unit;
        }

        public static List<InvUnitInfo> GetAllInvUnits()
        {
            List<InvUnitInfo> units = null;

            using (var repo = new GMContext())
            {
                units = repo.Database.SqlQuery<InvUnitInfo>(@"
                    SELECT
                        RowId,
                        UnitId,
                        MinUnitName,
                        UnitSymbol
                    FROM InvUnit
                    WHERE DomainStatus = 1
                    ORDER BY UnitId
                ").ToList();
            }

            return units;
        }

        public static void AddInvUnit(
            InvUnitInfo unit,
            string loggedInUser)
        {
            InvUnitInfo obj;

            using (var repo = new GMContext())
            {
                obj = repo.Database.SqlQuery<InvUnitInfo>(@"
                    SELECT
                        ISNULL(MAX(UnitId), 0) + 1 AS UnitId
                    FROM InvUnit
                    WHERE DomainStatus = 1
                ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO InvUnit
                    (
                        RowId,
                        UnitId,
                        MinUnitName,
                        UnitSymbol,
                        CreateUser,
                        CreateDate,
                        DomainStatus
                    )
                    VALUES
                    (
                        @RowId,
                        @UnitId,
                        @MinUnitName,
                        @UnitSymbol,
                        @CreateUser,
                        GETDATE(),
                        1
                    )
                ",
                new SqlParameter("@RowId", unit.RowId),
                new SqlParameter("@UnitId", obj.UnitId),
                new SqlParameter("@MinUnitName", unit.MinUnitName),
                new SqlParameter("@UnitSymbol", unit.UnitSymbol ?? ""),
                new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        public static void UpdateInvUnit(
            InvUnitInfo unit,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvUnit
                    SET
                        MinUnitName = @MinUnitName,
                        UnitSymbol = @UnitSymbol,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", unit.RowId),
                new SqlParameter("@MinUnitName", unit.MinUnitName),
                new SqlParameter("@UnitSymbol", unit.UnitSymbol ?? ""),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static void DeleteInvUnit(
            InvUnitInfo unit,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvUnit
                    SET
                        DomainStatus = 0,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", unit.RowId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static InvUnitResponse GetFilteredInvUnits(HttpRequestBase Request)
        {
            InvUnitResponse resp = new InvUnitResponse();

            GMDataTableDAL gmdt = new GMDataTableDAL(Request);

            resp.InvUnits = null;

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
                    FROM InvUnit
                    WHERE DomainStatus = 1
                ");

                resp.TotalRecordCount = repo.Database.SqlQuery<int>(string.Format(@"
                    SELECT COUNT(*) {0}
                ", sb.ToString())).Single();

                List<SqlParameter> sqlParameters = new List<SqlParameter>();
                List<SqlParameter> sqlParameters1 = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.AppendLine(@"
                        AND
                        (
                            MinUnitName LIKE '%' + @SearchValue + '%'
                            OR UnitSymbol LIKE '%' + @SearchValue + '%'
                        )");

                    sqlParameters.Add(
                        new SqlParameter("@SearchValue", gmdt.SearchValue));
                }

                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(string.Format(@"
                    SELECT COUNT(*) {0}
                ", sb.ToString()), sqlParameters.ToArray()).Single();

                sqlParameters1.AddRange(sqlParameters);

                sqlParameters1.Add(
                    new SqlParameter("@Skip", gmdt.Start));

                sqlParameters1.Add(
                    new SqlParameter("@Take", gmdt.Length));

                string sortColumn = gmdt.SortColum.ToString();

                resp.InvUnits = repo.Database.SqlQuery<InvUnitInfo>(string.Format(@"
                    SELECT
                         RowId
                        ,UnitId
                        ,MinUnitName
                        ,UnitSymbol

                    {0}

                    ORDER BY {1} {2}

                    OFFSET (@Skip) ROWS
                    FETCH NEXT (@Take) ROWS ONLY
                ",
                sb.ToString(),
                sortColumn,
                gmdt.SortDir),
                sqlParameters1.ToArray()).ToList();
            }

            return resp;
        }
    }
}