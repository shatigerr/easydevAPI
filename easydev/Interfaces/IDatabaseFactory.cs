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
}