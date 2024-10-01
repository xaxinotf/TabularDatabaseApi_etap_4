using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TabularDatabaseApi.Models
{
    public class Table
    {
        [Required]
        public string Name { get; set; }
        public List<Field> Fields { get; set; } = new List<Field>();
        public List<Row> Rows { get; set; } = new List<Row>();
    }
}
