using System.Collections.Generic;

namespace TabularDatabaseApi.Models
{
    public class Database
    {
        public List<Table> Tables { get; set; } = new List<Table>();
    }
}
