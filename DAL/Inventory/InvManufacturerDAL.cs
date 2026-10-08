using ERP.Models.Inventory.InvManufacturer;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;

namespace ERP.DAL.Inventory
{
    public class InvManufacturerDAL
    {
        public static InvManufacturerInfo GetManufacturerByID(string RowId)
        {
            InvManufacturerInfo manufacturer = null;

            using (var repo = new GMContext())
            {
                manufacturer = repo.Database.SqlQuery<InvManufacturerInfo>(@"
                    SELECT
                        RowId,
                        ManufacturerId,
                        ManufacturerName,
                        ManufacturerAddress
                    FROM InvManufacturer
                    WHERE RowId = @RowId
                    AND DomainStatus = 1
                ",
                new SqlParameter("@RowId", RowId))
                .FirstOrDefault();
            }

            return manufacturer;
        }

        public static InvManufacturerInfo GetInvManufacturerByName(
            string ManufacturerName,
            string RowId)
        {
            InvManufacturerInfo manufacturer = null;

            using (var repo = new GMContext())
            {
                manufacturer = repo.Database.SqlQuery<InvManufacturerInfo>(@"
                    SELECT *
                    FROM InvManufacturer
                    WHERE ManufacturerName = @ManufacturerName
                    AND (@RowId IS NULL OR RowId <> @RowId)
                    AND DomainStatus = 1
                ",
                new SqlParameter("@ManufacturerName", ManufacturerName),
                new SqlParameter("@RowId", (object)RowId ?? DBNull.Value))
                .FirstOrDefault();
            }

            return manufacturer;
        }

        public static List<InvManufacturerInfo> GetAllInvManufacturers()
        {
            List<InvManufacturerInfo> manufacturers = null;

            using (var repo = new GMContext())
            {
                manufacturers = repo.Database.SqlQuery<InvManufacturerInfo>(@"
                    SELECT
                        RowId,
                        ManufacturerId,
                        ManufacturerName,
                        ManufacturerAddress
                    FROM InvManufacturer
                    WHERE DomainStatus = 1
                    ORDER BY ManufacturerId
                ").ToList();
            }

            return manufacturers;
        }

        public static void AddInvManufacturer(
            InvManufacturerInfo manufacturer,
            string loggedInUser)
        {
            InvManufacturerInfo obj;

            using (var repo = new GMContext())
            {
                obj = repo.Database.SqlQuery<InvManufacturerInfo>(@"
                    SELECT
                        ISNULL(MAX(ManufacturerId),0)+1 AS ManufacturerId
                    FROM InvManufacturer
                    WHERE DomainStatus = 1
                ").FirstOrDefault();

                repo.Database.ExecuteSqlCommand(@"
                    INSERT INTO InvManufacturer
                    (
                        RowId,
                        ManufacturerId,
                        ManufacturerName,
                        ManufacturerAddress,
                        CreateUser,
                        CreateDate,
                        DomainStatus
                    )
                    VALUES
                    (
                        @RowId,
                        @ManufacturerId,
                        @ManufacturerName,
                        @ManufacturerAddress,
                        @CreateUser,
                        GETDATE(),
                        1
                    )
                ",
                new SqlParameter("@RowId", manufacturer.RowId),
                new SqlParameter("@ManufacturerId", obj.ManufacturerId),
                new SqlParameter("@ManufacturerName", manufacturer.ManufacturerName),
                new SqlParameter("@ManufacturerAddress", manufacturer.ManufacturerAddress ?? ""),
                new SqlParameter("@CreateUser", loggedInUser));
            }
        }

        public static void UpdateInvManufacturer(
            InvManufacturerInfo manufacturer,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvManufacturer
                    SET
                        ManufacturerName = @ManufacturerName,
                        ManufacturerAddress = @ManufacturerAddress,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", manufacturer.RowId),
                new SqlParameter("@ManufacturerName", manufacturer.ManufacturerName),
                new SqlParameter("@ManufacturerAddress", manufacturer.ManufacturerAddress ?? ""),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static void DeleteInvManufacturer(
            InvManufacturerInfo manufacturer,
            string loggedInUser)
        {
            using (var repo = new GMContext())
            {
                repo.Database.ExecuteSqlCommand(@"
                    UPDATE InvManufacturer
                    SET
                        DomainStatus = 0,
                        UpdateUser = @UpdateUser,
                        UpdateDate = GETDATE()
                    WHERE RowId = @RowId
                ",
                new SqlParameter("@RowId", manufacturer.RowId),
                new SqlParameter("@UpdateUser", loggedInUser));
            }
        }

        public static InvManufacturerResponse GetFilteredInvManufacturers(HttpRequestBase Request)
        {
            InvManufacturerResponse resp = new InvManufacturerResponse();

            GMDataTableDAL gmdt = new GMDataTableDAL(Request);

            resp.InvManufacturers = null;

            using (var repo = new GMContext())
            {
                StringBuilder sb = new StringBuilder(@"
                    FROM InvManufacturer
                    WHERE DomainStatus = 1
                ");

                resp.TotalRecordCount = repo.Database.SqlQuery<int>(string.Format(@"
                    SELECT COUNT(*) {0}
                ", sb.ToString())).Single();

                List<SqlParameter> sqlParameters = new List<SqlParameter>();

                if (!string.IsNullOrEmpty(gmdt.SearchValue))
                {
                    sb.AppendLine(@"
                        AND
                        (
                            ManufacturerName LIKE '%' + @SearchValue + '%'
                            OR ManufacturerAddress LIKE '%' + @SearchValue + '%'
                        )");

                    sqlParameters.Add(
                        new SqlParameter("@SearchValue", gmdt.SearchValue));
                }

                resp.FilteredRecordCount = repo.Database.SqlQuery<int>(string.Format(@"
                    SELECT COUNT(*) {0}
                ", sb.ToString()), sqlParameters.Select(p => ((ICloneable)p).Clone()).ToArray()).Single();

                List<SqlParameter> sqlParameters1 = sqlParameters.Select(p => ((ICloneable)p).Clone()).Cast<SqlParameter>().ToList();

                sqlParameters1.Add(
                    new SqlParameter("@Skip", gmdt.Start));

                sqlParameters1.Add(
                    new SqlParameter("@Take", gmdt.Length));

                string sortColumn = gmdt.SortColum.ToString();

                resp.InvManufacturers = repo.Database.SqlQuery<InvManufacturerInfo>(string.Format(@"
                    SELECT
                         RowId
                        ,ManufacturerId
                        ,ManufacturerName
                        ,ManufacturerAddress

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

        public class InvManufacturerResponse
        {
            public int TotalRecordCount { get; set; }
            public int FilteredRecordCount { get; set; }
            public List<InvManufacturerInfo> InvManufacturers { get; set; }
        }
    }
}