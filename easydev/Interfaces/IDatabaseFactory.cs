using System.Data;
using easydev.Models;

namespace easydev.Interfaces;

public interface IDatabaseFactory
{
    string GetConnectionString(Database db);
    bool CheckDBConnection(Database db);
    
    Task<List<Dictionary<string, object>>> Get(Database db,string query);
    List<int> Post(Database db,string query);

    DataTable GetDBTables(Database db);
    DataTable GetDBColumns(Database db, string table);

    DataTable GetForeignKeys(Database db, string fromTable);
    List<string> GenerateAlterStatements(string tableName, ColumnDB current, ColumnDB updated, Database db);
    bool ApplyAlterStatements(Database db, List<string> alterStatements,out string err);

    public string GetForeignKeyConstraintName(Database db, string tableName, string columnName);
}