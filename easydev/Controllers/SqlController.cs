using System.Collections.Generic;
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
                // Obtener proyecto y base de datos
                var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
                if (project == null) return NotFound(new { importedModels = false, msg = "Project not found" });

                var database = await _context.Databases.FirstOrDefaultAsync(d => d.Id == project.Iddatabase);
                if (database == null) return NotFound(new { importedModels = false, msg = "Database not found" });

                var dbFactory = new DatabaseFactory();
                var db = dbFactory.CreateFactory(database);
                if (!db.CheckDBConnection(database))
                    return BadRequest(new { importedModels = false, msg = "No connection to database" });

                // Obtener todas las tablas de la base de datos externa
                var tables = db.GetDBTables(database);
                var existingTables = _context.TableDB.Where(t => t.iddatabase == database.Id).ToList();

                foreach (DataRow tableRow in tables.Rows)
                {
                    string tableName = tableRow["table_name"].ToString();

                    var tableDB = existingTables.FirstOrDefault(t => t.name == tableName);
                    if (tableDB == null)
                    {
                        // Nueva tabla
                        tableDB = new TableDB
                        {
                            iddatabase = database.Id,
                            name = tableName,
                            description = ""
                        };
                        _context.TableDB.Add(tableDB);
                        await _context.SaveChangesAsync(); // necesitamos el ID para insertar columnas
                    }

                    // Actualizar columnas
                    var dbColumns = db.GetDBColumns(database, tableName);
                    var existingColumns = _context.ColumnDB.Where(c => c.tableid == tableDB.id).ToList();

                    var columnsToKeep = new HashSet<string>();

                    foreach (DataRow columnRow in dbColumns.Rows)
                    {
                        string columnName = columnRow["column_name"].ToString();
                        string dataType = columnRow["data_type"].ToString();
                        int length = columnRow["character_maximum_length"] != DBNull.Value
                            ? Convert.ToInt32(columnRow["character_maximum_length"])
                            : 0;
                        bool isNullable = columnRow["is_nullable"].ToString() == "YES";
                        string defaultValue = columnRow["column_default"] != DBNull.Value
                            ? columnRow["column_default"].ToString()
                            : null;

                        columnsToKeep.Add(columnName);

                        var existingColumn = existingColumns.FirstOrDefault(c => c.name == columnName);
                        if (existingColumn == null)
                        {
                            var newColumn = new ColumnDB
                            {
                                tableid = tableDB.id,
                                name = columnName,
                                type = dataType,
                                length = length,
                                isnullable = isNullable,
                                isprimarykey = false
                            };
                            _context.ColumnDB.Add(newColumn);
                        }
                        else
                        {
                            existingColumn.type = dataType;
                            existingColumn.length = length;
                            existingColumn.isnullable = isNullable;
                            existingColumn.isprimarykey = false;
                        }
                    }

                    // Eliminar columnas que ya no existen
                    var columnsToRemove = existingColumns
                        .Where(c => !columnsToKeep.Contains(c.name))
                        .ToList();

                    _context.ColumnDB.RemoveRange(columnsToRemove);

                    await _context.SaveChangesAsync();
                }

                return Ok(new { importedModels = true, msg = "Models imported successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { importedModels = false, msg = "Error importing models", error = ex.Message });
            }
        }


        [HttpPost("query/{id}")]
        public async Task<IActionResult> ExecQuery(long id, [FromBody] string query)
        {
            try
            {
                // Obtener el proyecto
                var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == id);
                if (project == null)
                    return NotFound(new { msg = "Project not found" });

                // Obtener la base de datos asociada
                var database = await _context.Databases.FirstOrDefaultAsync(d => d.Id == project.Iddatabase);
                if (database == null)
                    return NotFound(new { msg = "Database not found" });

                // Instanciar tu lógica de ejecución si usas una factory
                var dbFactory = new DatabaseFactory();
                var db = dbFactory.CreateFactory(database);

                if (!db.CheckDBConnection(database))
                    return BadRequest(new { msg = "Database connection failed" });

                // Ejecutar la consulta (ajústalo si devuelves resultados)
                List<Dictionary<string, object>> result = new List<Dictionary<string, object>>();
                List<int> resultPost = new List<int>();
                if (query.ToUpper().StartsWith("SELECT"))
                {
                    result = await db.Get(database, query);
                    return Ok(new
                    {
                        success = true,
                        result,
                        msg = "Query executed successfully"
                    });
                }
                // Esto debería devolver un DataTable, List<Dictionary<string, object>>, etc.
                else
                {
                    resultPost = db.Post(database, query);
                        return Ok(new
                    {
                        success = true,
                        resultPost,
                        msg = "Query executed successfully"
                    });
                }
                
            
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, msg = ex.Message });
            }
        }

        [HttpGet("tables/{id}")]
        public async Task<IActionResult> GetTables(long id)
        {
            try
            {
                // Obtener el proyecto
                Project project = await _context.Projects.Where(p => p.Id == id).FirstAsync();

                // Obtener las tablas asociadas a la base de datos
                List<TableDB> tables = _context.TableDB
                    .Where(x => x.iddatabase == project.Iddatabase)
                    .ToList();
                

                return Ok(tables);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        [HttpGet("columns/{idTable}")]
        public async Task<IActionResult> GetColumnsByTable(long idTable)
        {
            try
            {
                // Obtener el proyecto
                

                // Obtener las tablas asociadas a la base de datos
                List<ColumnDB> columns = _context.ColumnDB
                    .Where(x => x.tableid == idTable)
                    .ToList();
                

                return Ok(columns);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
