using System.Collections.Generic;
using System.Data;
using easydev.Interfaces;
using easydev.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Mysqlx.Crud;

namespace easydev.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DatabaseController : ControllerBase
    {
        private readonly PostgresContext _context;

        public DatabaseController(PostgresContext context) 
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDbById(long id)
        {
            Database db = await _context.Databases.Where(d => d.Id == id).FirstAsync();
            if (db == null) return NotFound();
            return Ok(db);
        }

        [HttpPost]
        public async Task<IActionResult> CreateNewDatabase([FromBody] Database db)
        {
            string conn = db.GetConnectionString(db);
            //if(db.CheckDBConnection(db.Dbengine, conn))
            
            try
            {
                _context.Databases.Add(db);
                await _context.SaveChangesAsync();
                return Ok(db);
            }
            catch (Exception ex) {
                return BadRequest("Error");
            }
            
            
            
            
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDatabase(long id)
        {
            var db = await _context.Databases.FirstOrDefaultAsync(e => e.Id == id);
            if (db != null) _context.Databases.Remove(db);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDatabase(long id, [FromBody] Database db)
        {
            Database database = await _context.Databases.Where(d => d.Id == id).FirstAsync();
            if (db != null && database != null)
            {
                database.Id = id;
                database.Host = db.Host;
                database.Port = db.Port;
                database.Database1 = db.Database1;
                database.Dbengine = db.Dbengine;
                database.Password = db.Password;
                database.User = db.User;

                await _context.SaveChangesAsync();
            }
            else return BadRequest();
            return Ok();
        }

        [HttpGet("Schema/{projectID}")]
        public async Task<IActionResult> GetDatabaseSchema(long projectID)
        {
            SchemaResponse schemaResponse = new SchemaResponse();

            schemaResponse.Nodes = await GetSchema(projectID);

            schemaResponse.Edges = GetRelations(projectID);
            
            return Ok(schemaResponse);
        }

        private List<Edge> GetRelations(long projectID)
        {
            List<Edge> edges = new List<Edge>();
            try
            {
                

                // Obtener el proyecto y su base de datos
                Project proj = _context.Projects.First(x => x.Id == projectID);
                Database database = _context.Databases.First(x => x.Id == proj.Iddatabase);

                // Crear fábrica de conexión
                DatabaseFactory dbFactory = new DatabaseFactory();
                IDatabaseFactory db = dbFactory.CreateFactory(database);

                if (!db.CheckDBConnection(database))
                    return edges;

                // Obtener todas las tablas registradas en nuestro sistema
                List<TableDB> tables = _context.TableDB.Where(t => t.iddatabase == database.Id).ToList();

                // Por cada tabla, buscar las foreign keys donde actúa como tabla origen
                foreach (var table in tables)
                {
                    DataTable fkResults = db.GetForeignKeys(database, table.name); // table.name es el from_table

                    foreach (DataRow row in fkResults.Rows)
                    {
                        string fromTable = row["from_table"]?.ToString() ?? "";
                        string toTable = row["to_table"]?.ToString() ?? "";
                        string fromColumn = row["from_column"]?.ToString() ?? "";
                        string toColumn = row["to_column"]?.ToString() ?? "";

                        if (!string.IsNullOrWhiteSpace(fromTable) && !string.IsNullOrWhiteSpace(toTable))
                        {
                            edges.Add(new Edge
                            {
                                Id = $"{fromTable}-{toTable}-{fromColumn}-{toColumn}",
                                Source = fromTable,
                                Target = toTable,
                                SourceHandle = fromColumn,
                                TargetHandle = toColumn,
                                Animated = true,
                                Style = new EdgeStyle { Stroke = "#94a3b8" },
                                Label = $"FK: {fromColumn} → {toColumn}"
                            });
                        }
                    }
                }
            }
            catch(Exception ex)
            {

            }

            return edges;
        }


        private async Task<List<DatabaseNode>> GetSchema(long projectID)
        {
            List<DatabaseNode> nodes = new List<DatabaseNode>();
            
            int x = 0;
            int y = 0;


            Project project = _context.Projects.Where(x => x.Id == projectID).First();
            Database db = await _context.Databases.Where(d => d.Id == project.Iddatabase).FirstAsync();


            List<TableDB> tables = await _context.TableDB.Where(x => x.iddatabase == db.Id).ToListAsync();

            foreach (TableDB table in tables)
            {
                DatabaseNode node = new DatabaseNode();
                node.Type = "databaseNode";
                
                node.Data = new DatabaseNodeData();
                node.Data.Schema = new List<SchemaEntry>();
                node.Data.Label = table.name;

                node.Position = new Position();

                node.Id = table.name;

                node.Position.X = x;
                node.Position.Y = y;


                List<ColumnDB> columns = await _context.ColumnDB.Where(x => x.tableid == table.id).ToListAsync();

                foreach (ColumnDB column in columns)
                {

                    SchemaEntry schemaEntry = new SchemaEntry();

                    schemaEntry.Title = column.name;
                    schemaEntry.Type = column.type;

                    node.Data.Schema.Add(schemaEntry);
                }


                x += 320;
                if (x > 1000)
                {
                    x = 0;
                    y += 300;
                }

                nodes.Add(node);
            }

            return nodes;
        }
    }
}
