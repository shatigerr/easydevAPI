using System.Data;
using easydev.Interfaces;
using easydev.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace easydev.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class SqlController : Controller
    {
        private readonly PostgresContext _context;
        public SqlController(PostgresContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDBModels(long id)
        {
            try
            {
                // Obtener el proyecto
                Project project = await _context.Projects.Where(p => p.Id == id).FirstAsync();

                // Obtener las tablas asociadas a la base de datos
                List<TableDB> tables = _context.TableDB
                    .Where(x => x.iddatabase == project.Iddatabase)
                    .ToList();

                // Si no hay tablas, devolver BadRequest con indicador de primera vez
                if (tables.Count == 0)
                    return Ok(new { firstTime = true });

                // Para cada tabla, obtener sus columnas asociadas y asignarlas a la propiedad columnsDB
                foreach (var table in tables)
                {
                    table.columnsDB = _context.ColumnDB
                        .Where(c => c.tableid == table.id)
                        .ToList();
                }

                return Ok(tables);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> CreateUpdateDBModels(long id)
        {
            try
            {
                // Obtener el proyecto
                Project project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
                if (project == null) return NotFound(new { importedModels = false, msg = "Project not found" });

                // Obtener la base de datos asociada al proyecto
                Database database = await _context.Databases.FirstOrDefaultAsync(d => d.Id == project.Iddatabase);
                if (database == null) return NotFound(new { importedModels = false, msg = "Database not found" });

                // Crear el factory para la base de datos
                DatabaseFactory dbFactory = new DatabaseFactory();
                IDatabaseFactory db = dbFactory.CreateFactory(database);

                if (!db.CheckDBConnection(database))
                    return BadRequest(new { importedModels = false, msg = "No conection to database" });

                // Obtener tablas
                DataTable tables = db.GetDBTables(database);

                foreach (DataRow tableRow in tables.Rows)
                {
                    string tableName = tableRow["table_name"].ToString();

                    // Insertar la tabla en TableDB
                    var tableDB = new TableDB
                    {
                        iddatabase = database.Id,
                        name = tableName,
                        description = ""
                    };

                    _context.TableDB.Add(tableDB);
                    await _context.SaveChangesAsync();

                    // Obtener las columnas de la tabla
                    DataTable columns = db.GetDBColumns(database, tableName);

                    foreach (DataRow columnRow in columns.Rows)
                    {
                        string columnName = columnRow["column_name"].ToString();
                        string dataType = columnRow["data_type"].ToString();
                        object lengthObj = columnRow["character_maximum_length"];
                        int length = lengthObj != DBNull.Value ? Convert.ToInt32(lengthObj) : 0;
                        string isNullableStr = columnRow["is_nullable"].ToString();
                        bool isNullable = isNullableStr == "YES";
                        string defaultValue = columnRow["column_default"] != DBNull.Value ? columnRow["column_default"].ToString() : null;

                        // Insertar columna en ColumnDB
                        var columnDB = new ColumnDB
                        {
                            tableid = tableDB.id,
                            name = columnName,
                            type = dataType,
                            length = length,
                            isnullable = isNullable,
                            isprimarykey = false, // Puedes mejorarlo después para detectar PK
                        };

                        _context.ColumnDB.Add(columnDB);
                    }

                    await _context.SaveChangesAsync();
                }

                return Ok(new { importedModels = true, msg = "Models imported succesfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { importedModels = false, msg = "Error importing models" });
            }
        }

    }
}
