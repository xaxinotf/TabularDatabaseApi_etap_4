using System.ComponentModel.DataAnnotations;

namespace TabularDatabaseApi.Models
{
    public class Field
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public DataType Type { get; set; }
    }
}