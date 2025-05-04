using easydev.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace easydev.Models;

public class DatabaseFactory
{
    public IDatabaseFactory CreateFactory(Database db)
    {
        if (db.Dbengine.ToUpper() == "POSTGRESQL")
        {
            return new PostgreDatabaseFactory();
        }
        else if (db.Dbengine.ToUpper() == "MYSQL")
        {
            //return new MySQLConnectionFactory();
            return new MySQLDatabaseFactory();
        }
        else
        {
            return new SQLServerDatabaseFactory();
        }
    }
}