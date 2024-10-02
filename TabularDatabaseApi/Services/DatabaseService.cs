using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using TabularDatabaseApi.Models;

namespace TabularDatabaseApi.Services
{
    public class DatabaseService
    {
        private readonly string _filePath = "database.json";
        private Database _database;

        public DatabaseService()
        {
            LoadFromFile();
        }

        public List<Table> GetTables()
        {
            return _database.Tables;
        }

        public Table GetTableByName(string name)
        {
            return _database.Tables.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public void CreateTable(Table table)
        {
            if (_database.Tables.Any(t => t.Name.Equals(table.Name, StringComparison.OrdinalIgnoreCase)))
                throw new Exception("Таблиця з такою назвою вже існує.");

            _database.Tables.Add(table);
            SaveToFile();
        }

        public void DeleteTable(string name)
        {
            var table = GetTableByName(name);
            if (table == null)
                throw new Exception("Таблиця не знайдена.");

            _database.Tables.Remove(table);
            SaveToFile();
        }

        public void AddRow(string tableName, Row row)
        {
            var table = GetTableByName(tableName);
            if (table == null)
                throw new Exception("Таблиця не знайдена.");

            // Валідація рядка
            ValidateRow(table, row);

            table.Rows.Add(row);
            SaveToFile();
        }

        public void UpdateRow(string tableName, int index, Row row)
        {
            var table = GetTableByName(tableName);
            if (table == null)
                throw new Exception("Таблиця не знайдена.");

            if (index < 0 || index >= table.Rows.Count)
                throw new Exception("Рядок не знайдено.");

            // Валідація рядка
            ValidateRow(table, row);

            table.Rows[index] = row;
            SaveToFile();
        }

        public void DeleteRow(string tableName, int index)
        {
            var table = GetTableByName(tableName);
            if (table == null)
                throw new Exception("Таблиця не знайдена.");

            if (index < 0 || index >= table.Rows.Count)
                throw new Exception("Рядок не знайдено.");

            table.Rows.RemoveAt(index);
            SaveToFile();
        }

        public Table DifferenceTables(string tableName1, string tableName2, string resultTableName)
        {
            var table1 = GetTableByName(tableName1);
            var table2 = GetTableByName(tableName2);

            if (table1 == null || table2 == null)
                throw new Exception("Одна з таблиць не знайдена.");

            var resultTable = new Table { Name = resultTableName, Fields = table1.Fields };

            foreach (var row1 in table1.Rows)
            {
                bool existsInTable2 = table2.Rows.Any(row2 => row2.Values.SequenceEqual(row1.Values));
                if (!existsInTable2)
                    resultTable.Rows.Add(row1);
            }

            _database.Tables.Add(resultTable);
            return resultTable; // Повернення результатної таблиці
        }

        private bool RowValuesAreEqual(Dictionary<string, object> values1, Dictionary<string, object> values2, List<Field> fields)
        {
            foreach (var field in fields)
            {
                var key = field.Name;
                if (values1.ContainsKey(key) && values2.ContainsKey(key))
                {
                    // Додаткова перевірка для типів DateInterval
                    if (field.Type == DataType.DateInterval)
                    {
                        var val1 = values1[key]?.ToString();
                        var val2 = values2[key]?.ToString();
                        if (!DateIntervalEqual(val1, val2))
                        {
                            return false;
                        }
                    }
                    else if (!values1[key].Equals(values2[key]))
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private bool DateIntervalEqual(string interval1, string interval2)
        {
            if (string.IsNullOrEmpty(interval1) || string.IsNullOrEmpty(interval2))
                return false;

            var dates1 = interval1.Split('-').Select(d => d.Trim()).ToArray();
            var dates2 = interval2.Split('-').Select(d => d.Trim()).ToArray();

            if (dates1.Length != 2 || dates2.Length != 2)
                return false;

            return dates1[0] == dates2[0] && dates1[1] == dates2[1];
        }
        private void ValidateRow(Table table, Row row)
        {
            foreach (var field in table.Fields)
            {
                if (!row.Values.ContainsKey(field.Name))
                    throw new Exception($"Поле {field.Name} відсутнє у рядку.");

                var value = row.Values[field.Name];

                if (!IsValidType(value, field.Type))
                    throw new Exception($"Значення поля {field.Name} має неправильний тип.");
            }
        }

        private bool IsValidType(object value, DataType type)
        {
            try
            {
                switch (type)
                {
                    case DataType.Integer:
                        Convert.ToInt32(value);
                        return true;
                    case DataType.Real:
                        Convert.ToDouble(value);
                        return true;
                    case DataType.Char:
                        return value is string s && s.Length == 1;
                    case DataType.String:
                        return value is string;
                    case DataType.Date:
                        DateTime.Parse(value.ToString());
                        return true;
                    case DataType.DateInterval:
                        var dates = value.ToString().Split('-');
                        if (dates.Length != 2)
                            return false;
                        DateTime.Parse(dates[0].Trim());
                        DateTime.Parse(dates[1].Trim());
                        return true;
                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool RowsEqual(Row row1, Row row2)
        {
            return row1.Values.OrderBy(kv => kv.Key).SequenceEqual(row2.Values.OrderBy(kv => kv.Key));
        }

        private bool TableStructuresEqual(Table table1, Table table2)
        {
            if (table1.Fields.Count != table2.Fields.Count)
                return false;

            for (int i = 0; i < table1.Fields.Count; i++)
            {
                if (table1.Fields[i].Name != table2.Fields[i].Name ||
                    table1.Fields[i].Type != table2.Fields[i].Type)
                    return false;
            }

            return true;
        }

        private void SaveToFile()
        {
            var jsonSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                Formatting = Formatting.Indented
            };
            var json = JsonConvert.SerializeObject(_database, jsonSettings);
            File.WriteAllText(_filePath, json);
        }

        private void LoadFromFile()
        {
            if (File.Exists(_filePath))
            {
                var jsonSettings = new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.Auto
                };
                var json = File.ReadAllText(_filePath);
                _database = JsonConvert.DeserializeObject<Database>(json, jsonSettings);
            }
            else
            {
                _database = new Database();
            }
        }
    }
}
