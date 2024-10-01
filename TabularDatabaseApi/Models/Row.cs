using System.Collections.Generic;

namespace TabularDatabaseApi.Models
{
    public class Row
    {
        public Dictionary<string, object> Values { get; set; } = new Dictionary<string, object>();
    }
}