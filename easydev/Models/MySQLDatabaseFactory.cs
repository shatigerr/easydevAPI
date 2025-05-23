using System.Data;
using easydev.Interfaces;
using MySql.Data.MySqlClient;

namespace easydev.Models;

public class MySQLDatabaseFactory : IDatabaseFactory
{
    public string GetConnectionString(Database db)
    {
        return $"server={db.Host};port={db.Port};uid={db.User};pwd={db.Password};database={db.Database1}";
    }

    public bool CheckDBConnection(Database db)
    {
        bool connected = false;
        try
        {
            using (var conn = new MySqlConnection(db.GetConnectionString(db)))
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
        MySqlConnection connection = new MySqlConnection(GetConnectionString(db));
        connection.Open();
        using (var command = new MySqlCommand(query,connection))
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
        MySqlConnection connection = new MySqlConnection(GetConnectionString(db));
        connection.Open();
        using (var command = new MySqlCommand(query, connection))
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
        using (var conn = new MySqlConnection(db.GetConnectionString(db)))
        {
            conn.Open();
            string query = "SHOW TABLES";

            using (var cmd = new MySqlCommand(query, conn))
            using (var adapter = new MySqlDataAdapter(cmd))
            {
                adapter.Fill(dataTable);
            }

            if (dataTable.Columns.Count > 0)
            {
                dataTable.Columns[0].ColumnName = "table_name";
            }
        }
        return dataTable;
    }

    public DataTable GetDBColumns(Database db, string table)
    {
        var dataTable = new DataTable();
        using (var conn = new MySqlConnection(db.GetConnectionString(db)))
        {
            conn.Open();
            string query = $@"
                    SELECT 
                        c.COLUMN_NAME AS column_name,
                        c.DATA_TYPE AS data_type,
                        c.CHARACTER_MAXIMUM_LENGTH AS character_maximum_length,
                        c.IS_NULLABLE AS is_nullable,
                        c.COLUMN_DEFAULT AS column_default,
                        CASE WHEN c.COLUMN_KEY = 'PRI' THEN 'TRUE' ELSE 'FALSE' END AS is_primary_key,
                        CASE WHEN c.COLUMN_KEY = 'MUL' AND kcu.REFERENCED_TABLE_NAME IS NOT NULL THEN 'TRUE' ELSE 'FALSE' END AS is_foreign_key,
                        kcu.REFERENCED_TABLE_NAME AS referenced_table,
                        kcu.REFERENCED_COLUMN_NAME AS referenced_column
                    FROM INFORMATION_SCHEMA.COLUMNS c
                    LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
                        ON c.TABLE_NAME = kcu.TABLE_NAME 
                        AND c.COLUMN_NAME = kcu.COLUMN_NAME 
                        AND c.TABLE_SCHEMA = kcu.TABLE_SCHEMA
                    WHERE c.TABLE_SCHEMA = DATABASE() 
                      AND c.TABLE_NAME = @tableName
                    ORDER BY c.ORDINAL_POSITION;";


            using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@tableName", table);
                using (var adapter = new MySqlDataAdapter(cmd))
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
        using (var conn = new MySqlConnection(db.GetConnectionString(db)))
        {
            conn.Open();

            string query = @"
            SELECT 
                k.CONSTRAINT_NAME,
                k.TABLE_NAME AS from_table,
                k.COLUMN_NAME AS from_column,
                k.REFERENCED_TABLE_NAME AS to_table,
                k.REFERENCED_COLUMN_NAME AS to_column
            FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE k
            WHERE k.TABLE_SCHEMA = DATABASE() AND k.TABLE_NAME = @fromTable AND k.REFERENCED_TABLE_NAME IS NOT NULL";

            using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@fromTable", fromTable);
                using (var adapter = new MySqlDataAdapter(cmd))
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
            var nullable = updated.isnullable ? "" : "NOT NULL";
            var defaultValue = string.IsNullOrWhiteSpace(updated.defaultvalue) ? "" : $"DEFAULT '{updated.defaultvalue}'";

            sqls.Add($"ALTER TABLE `{tableName}` ADD COLUMN `{updated.name}` {updated.type} {nullable} {defaultValue};");

            if (updated.isprimarykey)
                sqls.Add($"ALTER TABLE `{tableName}` ADD PRIMARY KEY (`{updated.name}`);");

            if (updated.isforeignkey && !string.IsNullOrWhiteSpace(updated.referencedTable))
            {
                string constraintName = $"fk_{tableName}_{updated.name}";
                sqls.Add($"ALTER TABLE `{tableName}` ADD CONSTRAINT `{constraintName}` FOREIGN KEY (`{updated.name}`) REFERENCES `{updated.referencedTable}`(`{updated.referencedColumn}`);");
            }

            return sqls;
        }

        if (updated.isDelete)
        {
            if (current.isforeignkey)
            {
                string? constraintName = GetForeignKeyConstraintName(db, tableName, current.name);
                if (!string.IsNullOrEmpty(constraintName))
                    sqls.Add($"ALTER TABLE `{tableName}` DROP FOREIGN KEY `{constraintName}`;");
            }

            sqls.Add($"ALTER TABLE `{tableName}` DROP COLUMN `{current.name}`;");
            return sqls;
        }

        if (current.isforeignkey && (
            current.referencedTable != updated.referencedTable ||
            current.referencedColumn != updated.referencedColumn ||
            !updated.isforeignkey))
        {
            string? constraintName = GetForeignKeyConstraintName(db, tableName, current.name);
            if (!string.IsNullOrEmpty(constraintName))
                sqls.Add($"ALTER TABLE `{tableName}` DROP FOREIGN KEY `{constraintName}`;");
        }

        if (current.name != updated.name)
            sqls.Add($"ALTER TABLE `{tableName}` CHANGE `{current.name}` `{updated.name}` {updated.type};");

        if (current.type != updated.type || current.isnullable != updated.isnullable)
        {
            var nullable = updated.isnullable ? "" : "NOT NULL";
            sqls.Add($"ALTER TABLE `{tableName}` MODIFY `{updated.name}` {updated.type} {nullable};");
        }

        if (current.defaultvalue != updated.defaultvalue)
        {
            // No se puede eliminar un default sin saber el nombre del constraint (MySQL lo define de forma implícita)
            if (!string.IsNullOrWhiteSpace(updated.defaultvalue))
                sqls.Add($"ALTER TABLE `{tableName}` ALTER `{updated.name}` SET DEFAULT '{updated.defaultvalue}';");
        }

        if (!current.isprimarykey && updated.isprimarykey)
            sqls.Add($"ALTER TABLE `{tableName}` ADD PRIMARY KEY (`{updated.name}`);");

        if (updated.isforeignkey && !string.IsNullOrWhiteSpace(updated.referencedTable))
        {
            string constraintName = $"fk_{tableName}_{updated.name}";
            sqls.Add($"ALTER TABLE `{tableName}` ADD CONSTRAINT `{constraintName}` FOREIGN KEY (`{updated.name}`) REFERENCES `{updated.referencedTable}`(`{updated.referencedColumn}`);");
        }

        return sqls;
    }


    public bool ApplyAlterStatements(Database db, List<string> alterStatements, out string err)
    {
        err = "";
        if (alterStatements == null || alterStatements.Count == 0)
            return true;

        try
        {
            using (var conn = new MySqlConnection(db.GetConnectionString(db)))
            {
                conn.Open();

                foreach (var sql in alterStatements)
                {
                    if (!string.IsNullOrWhiteSpace(sql) && !sql.StartsWith("--"))
                    {
                        using (var cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }

                return true;
            }
        }
        catch (Exception ex)
        {
            err = ex.Message;
            return false;
        }
    }

    public string GetForeignKeyConstraintName(Database db, string tableName, string columnName)
    {
        using (var conn = new MySqlConnection(db.GetConnectionString(db)))
        {
            conn.Open();

            string query = @"
                SELECT CONSTRAINT_NAME
                FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                WHERE TABLE_SCHEMA = DATABASE()
                  AND TABLE_NAME = @tableName
                  AND COLUMN_NAME = @columnName
                  AND REFERENCED_TABLE_NAME IS NOT NULL
                LIMIT 1;";

            using (var cmd = new MySqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@tableName", tableName);
                cmd.Parameters.AddWithValue("@columnName", columnName);

                var result = cmd.ExecuteScalar();
                return result?.ToString();
            }
        }
    }

}