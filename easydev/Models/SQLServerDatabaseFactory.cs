using System.Data;
using easydev.Interfaces;
using Microsoft.Data.SqlClient;

namespace easydev.Models;

public class SQLServerDatabaseFactory : IDatabaseFactory
{
    public string GetConnectionString(Database db)
    {
        return $"Server={db.Host};Database={db.Database1};User Id={db.User};Password={db.Password};TrustServerCertificate=True;Connection Timeout=30;";
    }

    public bool CheckDBConnection(Database db)
    {
        bool connected = false;
        try
        {
            using (var conn = new SqlConnection(GetConnectionString(db)))
            {
                conn.Open();
                if (conn.State == System.Data.ConnectionState.Open)
                {
                    connected = true;
                }
            }
        }
        catch (Exception ex)
        {
            connected = false;
        }


        return connected;
    }

    public async Task<List<Dictionary<string, object>>> Get(Database db, string query)
    {
        SqlConnection connection = new SqlConnection(this.GetConnectionString(db));
        connection.Open();
        using (var command = new SqlCommand(query, connection))
        {
            using (var reader = command.ExecuteReader())
            {

                var result = new List<Dictionary<string, object>>();

                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.GetValue(i);
                    }
                    result.Add(row);
                }

                return result;
            }
        }
    }

    public List<int> Post(Database db, string query)
    {
        SqlConnection connection = new SqlConnection(this.GetConnectionString(db));
        connection.Open();
        using (var command = new SqlCommand(query, connection))
        {
            command.CommandTimeout = 120;
            // Agrega los parámetros de manera segura
            try
            {
                var affectedRows = command.ExecuteNonQuery();
                List<int> res = new List<int>();
                return res;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing query: {ex.Message}");
                throw;
            }

                
        }
    }

    public DataTable GetDBTables(Database db)
    {
        var dataTable = new DataTable();
        using (var conn = new SqlConnection(GetConnectionString(db)))
        {
            conn.Open();
            string query = "SELECT TABLE_NAME AS table_name FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE'";
            using (var cmd = new SqlCommand(query, conn))
            using (var adapter = new SqlDataAdapter(cmd))
            {
                adapter.Fill(dataTable);
            }
        }
        return dataTable;
    }


    public DataTable GetDBColumns(Database db, string table)
    {
        var dataTable = new DataTable();
        using (var conn = new SqlConnection(GetConnectionString(db)))
        {
            conn.Open();

            string query = @"
            SELECT 
                c.COLUMN_NAME AS column_name,
                c.DATA_TYPE AS data_type,
                c.CHARACTER_MAXIMUM_LENGTH AS character_maximum_length,
                c.IS_NULLABLE AS is_nullable,
                c.COLUMN_DEFAULT AS column_default,
                CASE WHEN k.CONSTRAINT_TYPE = 'PRIMARY KEY' THEN 'TRUE' ELSE 'FALSE' END AS is_primary_key,
                CASE WHEN fk.CONSTRAINT_NAME IS NOT NULL THEN 'TRUE' ELSE 'FALSE' END AS is_foreign_key,
                fk.REFERENCED_TABLE_NAME AS referenced_table,
                fk.REFERENCED_COLUMN_NAME AS referenced_column
            FROM INFORMATION_SCHEMA.COLUMNS c
            LEFT JOIN (
                SELECT ku.TABLE_NAME, ku.COLUMN_NAME, tc.CONSTRAINT_TYPE
                FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku
                JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                    ON ku.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
                WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
            ) k ON c.TABLE_NAME = k.TABLE_NAME AND c.COLUMN_NAME = k.COLUMN_NAME
            LEFT JOIN (
                SELECT 
                    ku.TABLE_NAME, ku.COLUMN_NAME,
                    ku2.TABLE_NAME AS REFERENCED_TABLE_NAME,
                    ku2.COLUMN_NAME AS REFERENCED_COLUMN_NAME,
                    ku.CONSTRAINT_NAME
                FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS rc
                JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku
                    ON rc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
                JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE ku2
                    ON rc.UNIQUE_CONSTRAINT_NAME = ku2.CONSTRAINT_NAME
            ) fk ON c.TABLE_NAME = fk.TABLE_NAME AND c.COLUMN_NAME = fk.COLUMN_NAME
            WHERE c.TABLE_NAME = @table
            ORDER BY c.ORDINAL_POSITION";

            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@table", table);
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(dataTable);
                }
            }
        }
        return dataTable;
    }


    public DataTable GetForeignKeys(Database db, string fromTable)
    {
        var dataTable = new DataTable();
        using (var conn = new SqlConnection(GetConnectionString(db)))
        {
            conn.Open();

            string query = @"
        SELECT 
            cu.TABLE_NAME AS from_table,
            cu.COLUMN_NAME AS from_column,
            pt.TABLE_NAME AS to_table,
            pt.COLUMN_NAME AS to_column
        FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS fk
        JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE cu ON fk.CONSTRAINT_NAME = cu.CONSTRAINT_NAME
        JOIN (
            SELECT 
                i1.CONSTRAINT_NAME,
                i2.TABLE_NAME,
                i2.COLUMN_NAME
            FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS i1
            JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE i2 ON i1.CONSTRAINT_NAME = i2.CONSTRAINT_NAME
            WHERE i1.CONSTRAINT_TYPE = 'PRIMARY KEY'
        ) pt ON fk.UNIQUE_CONSTRAINT_NAME = pt.CONSTRAINT_NAME
        WHERE cu.TABLE_NAME = @table";

            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@table", fromTable);
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    adapter.Fill(dataTable);
                }
            }
        }
        return dataTable;
    }


    public List<string> GenerateAlterStatements(string tableName, ColumnDB current, ColumnDB updated, Database db)
    {
        var sqls = new List<string>();

        if (updated.isNew && updated.id == 0)
        {
            var nullable = updated.isnullable ? "NULL" : "NOT NULL";
            var defaultValue = string.IsNullOrWhiteSpace(updated.defaultvalue) ? "" : $"DEFAULT '{updated.defaultvalue}'";

            sqls.Add($"ALTER TABLE [{tableName}] ADD [{updated.name}] {updated.type} {nullable} {defaultValue};");

            if (updated.isprimarykey)
                sqls.Add($"ALTER TABLE [{tableName}] ADD CONSTRAINT PK_{tableName}_{updated.name} PRIMARY KEY ([{updated.name}]);");

            if (updated.isforeignkey && !string.IsNullOrWhiteSpace(updated.referencedTable))
            {
                string constraintName = $"FK_{tableName}_{updated.name}";
                sqls.Add($"ALTER TABLE [{tableName}] ADD CONSTRAINT [{constraintName}] FOREIGN KEY ([{updated.name}]) REFERENCES [{updated.referencedTable}]([{updated.referencedColumn}]);");
            }

            return sqls;
        }

        if (updated.isDelete)
        {
            if (current.isforeignkey)
            {
                string? constraintName = GetForeignKeyConstraintName(db, tableName, current.name);
                if (!string.IsNullOrEmpty(constraintName))
                    sqls.Add($"ALTER TABLE [{tableName}] DROP CONSTRAINT [{constraintName}];");
            }

            sqls.Add($"ALTER TABLE [{tableName}] DROP COLUMN [{current.name}];");
            return sqls;
        }

        if (current.isforeignkey && (
            current.referencedTable != updated.referencedTable ||
            current.referencedColumn != updated.referencedColumn ||
            !updated.isforeignkey))
        {
            string? constraintName = GetForeignKeyConstraintName(db, tableName, current.name);
            if (!string.IsNullOrEmpty(constraintName))
                sqls.Add($"ALTER TABLE [{tableName}] DROP CONSTRAINT [{constraintName}];");
        }

        if (current.name != updated.name)
            sqls.Add($"EXEC sp_rename '{tableName}.{current.name}', '{updated.name}', 'COLUMN';");

        if (current.type != updated.type || current.isnullable != updated.isnullable)
        {
            var nullable = updated.isnullable ? "NULL" : "NOT NULL";
            sqls.Add($"ALTER TABLE [{tableName}] ALTER COLUMN [{updated.name}] {updated.type} {nullable};");
        }

        if (current.defaultvalue != updated.defaultvalue)
        {
            string constraintName = $"DF_{tableName}_{updated.name}";
            sqls.Add($"ALTER TABLE [{tableName}] DROP CONSTRAINT [{constraintName}];");

            if (!string.IsNullOrWhiteSpace(updated.defaultvalue))
                sqls.Add($"ALTER TABLE [{tableName}] ADD CONSTRAINT [{constraintName}] DEFAULT '{updated.defaultvalue}' FOR [{updated.name}];");
        }

        if (!current.isprimarykey && updated.isprimarykey)
            sqls.Add($"ALTER TABLE [{tableName}] ADD CONSTRAINT PK_{tableName}_{updated.name} PRIMARY KEY ([{updated.name}]);");

        if (updated.isforeignkey && !string.IsNullOrWhiteSpace(updated.referencedTable))
        {
            string constraintName = $"FK_{tableName}_{updated.name}";
            sqls.Add($"ALTER TABLE [{tableName}] ADD CONSTRAINT [{constraintName}] FOREIGN KEY ([{updated.name}]) REFERENCES [{updated.referencedTable}]([{updated.referencedColumn}]);");
        }

        return sqls;
    }



    public bool ApplyAlterStatements(Database db, List<string> alterStatements, out string err)
    {
        err = "";
        try
        {
            using (var conn = new SqlConnection(GetConnectionString(db)))
            {
                conn.Open();
                foreach (var sql in alterStatements)
                {
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            err = ex.Message;
            return false;
        }
    }

    public string GetForeignKeyConstraintName(Database db, string tableName, string columnName)
    {
        object result = "";
        using (var conn = new SqlConnection(GetConnectionString(db)))
        {
            conn.Open();

            string query = @"
        SELECT tc.CONSTRAINT_NAME
        FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
        INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu 
            ON tc.CONSTRAINT_NAME = kcu.CONSTRAINT_NAME
        WHERE tc.CONSTRAINT_TYPE = 'FOREIGN KEY'
          AND tc.TABLE_NAME = @tableName
          AND kcu.COLUMN_NAME = @columnName";

            using (var cmd = new SqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@tableName", tableName);
                cmd.Parameters.AddWithValue("@columnName", columnName);

                result = cmd.ExecuteScalar();
                return result?.ToString();
            }
        }
    }

}