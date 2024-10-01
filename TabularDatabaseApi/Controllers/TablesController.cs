using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using TabularDatabaseApi.Models;
using TabularDatabaseApi.Services;

namespace TabularDatabaseApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TablesController : ControllerBase
    {
        private readonly DatabaseService _dbService;

        public TablesController(DatabaseService dbService)
        {
            _dbService = dbService;
        }

        // GET: api/Tables
        [HttpGet]
        public ActionResult<List<Table>> GetTables()
        {
            return _dbService.GetTables();
        }

        // GET: api/Tables/{tableName}
        [HttpGet("{tableName}")]
        public ActionResult<Table> GetTable(string tableName)
        {
            var table = _dbService.GetTableByName(tableName);
            if (table == null)
                return NotFound();

            return table;
        }

        // POST: api/Tables
        [HttpPost]
        public ActionResult CreateTable([FromBody] Table table)
        {
            try
            {
                _dbService.CreateTable(table);
                return Ok();
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/Tables/{tableName}
        [HttpDelete("{tableName}")]
        public ActionResult DeleteTable(string tableName)
        {
            try
            {
                _dbService.DeleteTable(tableName);
                return Ok();
            }
            catch (System.Exception ex)
            {
                return NotFound(ex.Message);
            }
        }

        // GET: api/Tables/{tableName}/Rows
        [HttpGet("{tableName}/Rows")]
        public ActionResult<List<Row>> GetRows(string tableName)
        {
            var table = _dbService.GetTableByName(tableName);
            if (table == null)
                return NotFound();

            return table.Rows;
        }

        // POST: api/Tables/{tableName}/Rows
        [HttpPost("{tableName}/Rows")]
        public ActionResult AddRow(string tableName, [FromBody] Row row)
        {
            try
            {
                _dbService.AddRow(tableName, row);
                return Ok();
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // PUT: api/Tables/{tableName}/Rows/{index}
        [HttpPut("{tableName}/Rows/{index}")]
        public ActionResult UpdateRow(string tableName, int index, [FromBody] Row row)
        {
            try
            {
                _dbService.UpdateRow(tableName, index, row);
                return Ok();
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // DELETE: api/Tables/{tableName}/Rows/{index}
        [HttpDelete("{tableName}/Rows/{index}")]
        public ActionResult DeleteRow(string tableName, int index)
        {
            try
            {
                _dbService.DeleteRow(tableName, index);
                return Ok();
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // POST: api/Tables/Difference
        [HttpPost("Difference")]
        public ActionResult<Table> DifferenceTables([FromBody] DifferenceRequest request)
        {
            try
            {
                var resultTable = _dbService.DifferenceTables(request.TableName1, request.TableName2, request.ResultTableName);
                return Ok(resultTable);
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    public class DifferenceRequest
    {
        public string TableName1 { get; set; }
        public string TableName2 { get; set; }
        public string ResultTableName { get; set; }
    }
}
