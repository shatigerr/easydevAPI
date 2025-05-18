using System.Data;
using easydev.Interfaces;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Npgsql;

namespace easydev.Models;

public class PostgreDatabaseFactory : IDatabaseFactory
{
    public string GetConnectionString(Database db)
    {
        return $"User Id={db.User};Password={db.Password};Server={db.Host};Port={db.Port};Database={db.Database1};Timeout=300;CommandTimeout=300;Pooling=false;";
    }

    public bool CheckDBConnection(Database db)
    {
        bool connected = false;
        try
        {
            using (var conn = new NpgsqlConnection(GetConnectionString(db)))
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
        NpgsqlConnection connection = new NpgsqlConnection(this.GetConnectionString(db));
        connection.Open();
        using (var command = new NpgsqlCommand(query, connection))
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
        NpgsqlConnection connection = new NpgsqlConnection(this.GetConnectionString(db));
        connection.Open();
        using (var command = new NpgsqlCommand(query, connection))
        {
            command.CommandTimeout = 120;
            // Agrega los parámetros de manera segura
            try
            {
                var affectedRows = command.ExecuteNonQuery();
                List<int> res = new List<int>();
                res.Add(affectedRows);
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
        DataTable dataTable = new DataTable();
        using (var conn = new Npgsql.NpgsqlConnection(db.GetConnectionString(db)))
        {
            conn.Open();

            string query = @"
                SELECT table_name 
                FROM information_schema.tables 
                WHERE table_schema = 'public' AND table_type = 'BASE TABLE';
            ";

            using (var cmd = new Npgsql.NpgsqlCommand(query, conn))
            using (var adapter = new Npgsql.NpgsqlDataAdapter(cmd))
            {
                adapter.Fill(dataTable);
            }
        }

        return dataTable;
    }

    public DataTable GetDBColumns(Database db, string table)
    {
        DataTable dataTable = new DataTable();
        using (var conn = new Npgsql.NpgsqlConnection(db.GetConnectionString(db)))
        {
            conn.Open();

            string query = @"
                SELECT 
                    c.column_name,
                    c.data_type,
                    c.character_maximum_length,
                    c.is_nullable,
                    c.column_default,

                    -- Es FK si existe una constraint FK
                    CASE WHEN tc.constraint_type = 'FOREIGN KEY' THEN true ELSE false END AS is_foreign_key,

                    -- Es PK si la columna está en una constraint PRIMARY KEY
                    CASE WHEN pk.column_name IS NOT NULL THEN true ELSE false END AS is_primary_key,

                    -- Referencia si es FK
                    ccu.table_name AS referenced_table,
                    ccu.column_name AS referenced_column

                FROM information_schema.columns c

                -- FK JOINs
                LEFT JOIN information_schema.key_column_usage kcu
                    ON c.table_name = kcu.table_name
                    AND c.column_name = kcu.column_name
                    AND c.table_schema = kcu.table_schema

                LEFT JOIN information_schema.table_constraints tc
                    ON tc.constraint_name = kcu.constraint_name
                    AND tc.constraint_schema = kcu.constraint_schema
                    AND tc.constraint_type = 'FOREIGN KEY'

                LEFT JOIN information_schema.constraint_column_usage ccu
                    ON ccu.constraint_name = tc.constraint_name
                    AND ccu.constraint_schema = tc.constraint_schema

                -- PK JOIN para verificar si esta columna forma parte de una PK
                LEFT JOIN (
                    SELECT kcu.table_name, kcu.column_name
                    FROM information_schema.table_constraints tc
                    JOIN information_schema.key_column_usage kcu 
                      ON tc.constraint_name = kcu.constraint_name
                     AND tc.constraint_schema = kcu.constraint_schema
                    WHERE tc.constraint_type = 'PRIMARY KEY'
                ) pk ON pk.table_name = c.table_name AND pk.column_name = c.column_name

                WHERE c.table_schema = 'public'
                  AND c.table_name = @tableName

                ORDER BY c.ordinal_position;
            ";

            using (var cmd = new Npgsql.NpgsqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@tableName", table);

                using (var adapter = new Npgsql.NpgsqlDataAdapter(cmd))
                {
                    adapter.Fill(dataTable);
                }
            }
        }

        return dataTable;
    }

    public DataTable GetForeignKeys(Database db, string fromTable)
    {
        DataTable dataTable = new DataTable();
        using (var conn = new NpgsqlConnection(db.GetConnectionString(db)))
        {
            conn.Open();

            string query = @"
            SELECT 
                tc.constraint_name,
                tc.table_name AS from_table,
                ccu.table_name AS to_table,
                kcu.column_name AS from_column,
                ccu.column_name AS to_column
            FROM 
                information_schema.table_constraints tc
            JOIN 
                information_schema.key_column_usage kcu 
                ON tc.constraint_name = kcu.constraint_name 
                AND tc.constraint_schema = kcu.constraint_schema
            JOIN 
                information_schema.constraint_column_usage ccu 
                ON ccu.constraint_name = tc.constraint_name 
                AND ccu.constraint_schema = tc.constraint_schema
            WHERE 
                tc.constraint_type = 'FOREIGN KEY'
                AND tc.table_name = @fromTable;
        ";

            using (var cmd = new NpgsqlCommand(query, conn))
            {
                cmd.Parameters.AddWithValue("@fromTable", fromTable);

                using (var adapter = new NpgsqlDataAdapter(cmd))
                {
                    adapter.Fill(dataTable);
                }
            }
        }

        return dataTable;
    }

    public List<string> GenerateAlterStatements(string tableName, ColumnDB current, ColumnDB updated)
    {
        var sqls = new List<string>();

        // Si es una columna nueva
        if (updated.isNew && updated.id == 0)
        {
            var nullable = updated.isnullable ? "" : "NOT NULL";
            var defaultValue = string.IsNullOrWhiteSpace(updated.defaultvalue) ? "" : $"DEFAULT {updated.defaultvalue}";

            sqls.Add(
                $"ALTER TABLE \"{tableName}\" ADD COLUMN \"{updated.name}\" {updated.type} {nullable} {defaultValue};".Trim()
            );

            // Si es PK
            if (updated.isprimarykey)
            {
                sqls.Add($"ALTER TABLE \"{tableName}\" ADD PRIMARY KEY (\"{updated.name}\");");
            }

            // Si es FK
            if (updated.isforeignkey && !string.IsNullOrWhiteSpace(updated.referencedTable))
            {
                string constraintName = $"fk_{tableName}_{updated.name}";
                sqls.Add($"ALTER TABLE \"{tableName}\" ADD CONSTRAINT \"{constraintName}\" FOREIGN KEY (\"{updated.name}\") REFERENCES \"{updated.referencedTable}\"(\"{updated.referencedColumn}\");");
            }

            return sqls; // Nada más que hacer
        }

        // Si es una columna a eliminar
        if (updated.isDelete)
        {
            sqls.Add($"ALTER TABLE \"{tableName}\" DROP COLUMN \"{current.name}\";");
            return sqls; // No continuamos con otros cambios
        }

        // 1. Eliminar FK si existía antes y va a cambiar
        bool willChangeFK =
            current.isforeignkey ||
            updated.isforeignkey ||
            current.referencedTable != updated.referencedTable ||
            current.referencedColumn != updated.referencedColumn;

        if (willChangeFK && current.isforeignkey && !string.IsNullOrWhiteSpace(current.referencedTable))
        {
            string constraintName = $"fk_{tableName}_{current.name}";
            sqls.Add($"ALTER TABLE \"{tableName}\" DROP CONSTRAINT IF EXISTS \"{constraintName}\";");
        }

        // 2. Renombrar columna
        if (current.name != updated.name)
        {
            sqls.Add($"ALTER TABLE \"{tableName}\" RENAME COLUMN \"{current.name}\" TO \"{updated.name}\";");
        }

        // 3. Cambiar tipo
        if (current.type != updated.type)
        {
            sqls.Add($"ALTER TABLE \"{tableName}\" ALTER COLUMN \"{updated.name}\" TYPE {updated.type};");
        }

        // 4. Cambiar nullabilidad
        if (current.isnullable != updated.isnullable)
        {
            if (updated.isnullable)
                sqls.Add($"ALTER TABLE \"{tableName}\" ALTER COLUMN \"{updated.name}\" DROP NOT NULL;");
            else
                sqls.Add($"ALTER TABLE \"{tableName}\" ALTER COLUMN \"{updated.name}\" SET NOT NULL;");
        }

        // 5. Cambiar default
        if (current.defaultvalue != updated.defaultvalue)
        {
            if (string.IsNullOrWhiteSpace(updated.defaultvalue))
                sqls.Add($"ALTER TABLE \"{tableName}\" ALTER COLUMN \"{updated.name}\" DROP DEFAULT;");
            else
                sqls.Add($"ALTER TABLE \"{tableName}\" ALTER COLUMN \"{updated.name}\" SET DEFAULT {updated.defaultvalue};");
        }

        // 6. Clave primaria
        if (current.isprimarykey != updated.isprimarykey)
        {
            if (updated.isprimarykey)
            {
                sqls.Add($"ALTER TABLE \"{tableName}\" ADD PRIMARY KEY (\"{updated.name}\");");
            }
            else
            {
                sqls.Add($"-- Manual action may be required to DROP PRIMARY KEY on column \"{updated.name}\"");
            }
        }

        // 7. Añadir FK de nuevo si es necesario
        if (updated.isforeignkey && !string.IsNullOrWhiteSpace(updated.referencedTable))
        {
            string newConstraint = $"fk_{tableName}_{updated.name}";
            sqls.Add($"ALTER TABLE \"{tableName}\" ADD CONSTRAINT \"{newConstraint}\" FOREIGN KEY (\"{updated.name}\") REFERENCES \"{updated.referencedTable}\"(\"{updated.referencedColumn}\");");
        }


        return sqls;
    }

    
    public bool ApplyAlterStatements(Database db, List<string> alterStatements,out string err)
    {
        err = "";
        if (alterStatements == null || alterStatements.Count == 0)
            return true;

        try
        {
            using (var conn = new NpgsqlConnection(db.GetConnectionString(db)))
            {
                conn.Open();

                foreach (var sql in alterStatements)
                {
                    if (!string.IsNullOrWhiteSpace(sql) && !sql.StartsWith("--"))
                    {
                        using (var cmd = new NpgsqlCommand(sql, conn))
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
            Console.WriteLine($"❌ Error applying ALTER statements: {ex.Message}");
            err = ex.Message;
            return false;
        }
    }


}