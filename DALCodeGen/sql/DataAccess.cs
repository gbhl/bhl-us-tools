using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DALCodeGen.sql
{
    public class DataAccess
    {
        private string _connectionString = string.Empty;

        public DataAccess(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<DBColumn> GetColumns(string objectSchema, string objectName, Dictionary<string, string> languageTypes, 
            Dictionary<string, string> dataProviders)
        {
            SqlConnection connection = new SqlConnection(_connectionString);
            connection.Open();

            SqlCommand command = new SqlCommand();
            command.Connection = connection;
            command.CommandType = System.Data.CommandType.Text;
            command.CommandText = "SELECT c.COLUMN_NAME, " +
                                        "c.ORDINAL_POSITION, " +
                                        "c.DATA_TYPE, " +
                                        "c.CHARACTER_MAXIMUM_LENGTH, " +
                                        "c.NUMERIC_PRECISION, " +
		                                "c.NUMERIC_SCALE, " +
                                        "CASE WHEN tc.CONSTRAINT_TYPE = 'PRIMARY KEY' THEN 1 ELSE 0 END AS IS_IN_PRIMARY_KEY, " +
                                        "CASE WHEN fk.COLUMN_NAME IS NULL THEN 0 ELSE 1 END AS IS_IN_FOREIGN_KEY, " +
                                        "COLUMNPROPERTY(OBJECT_ID(c.TABLE_SCHEMA + '.' + c.TABLE_NAME), c.COLUMN_NAME,'IsComputed') AS IS_COMPUTED, " +
                                        "COLUMNPROPERTY(OBJECT_ID(c.TABLE_SCHEMA + '.' + c.TABLE_NAME), c.COLUMN_NAME,'IsIdentity') AS IS_IDENTITY, " +
                                        "c.IS_NULLABLE " +
                                "FROM    INFORMATION_SCHEMA.COLUMNS c " +
                                        "LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE cu " +
                                            "ON c.TABLE_SCHEMA = cu.TABLE_SCHEMA " +
                                            "AND c.TABLE_NAME = cu.TABLE_NAME " +
                                            "AND c.COLUMN_NAME = cu.COLUMN_NAME " +
                                        "LEFT JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc " +
                                            "ON tc.CONSTRAINT_TYPE IN ('PRIMARY KEY', 'FOREIGN KEY') " +
                                            "AND tc.TABLE_SCHEMA = cu.TABLE_SCHEMA " +
                                            "AND tc.TABLE_NAME = cu.TABLE_NAME " +
                                            "AND tc.CONSTRAINT_NAME = cu.CONSTRAINT_NAME " +

                                        "LEFT JOIN ( " +
                                            //"--Foreign keys defined on the target table " +
                                            "SELECT DISTINCT s1.name AS TABLE_SCHEMA, o1.name AS TABLE_NAME, c1.name AS COLUMN_NAME " +
                                            "FROM    sys.objects o1 " +
                                                    "INNER JOIN sys.foreign_keys fk ON o1.object_id = fk.parent_object_id " +
                                                    "INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id " +
                                                    "INNER JOIN sys.columns c1 ON fkc.parent_object_id = c1.object_id AND fkc.parent_column_id = c1.column_id " +
                                                    "INNER JOIN sys.schemas s1 ON s1.schema_id = o1.schema_id " +
                                            "UNION " +
                                            //"-- Foreign keys defined on other tables that reference the target table " +
                                            "SELECT DISTINCT s2.name, o2.name, c2.name " +
                                            "FROM    sys.objects o1 " +
                                                    "INNER JOIN sys.foreign_keys fk ON o1.object_id = fk.parent_object_id " +
                                                    "INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id " +
                                                    "INNER JOIN sys.columns c2 ON fkc.referenced_object_id = c2.object_id AND fkc.referenced_column_id = c2.column_id " +
                                                    "INNER JOIN sys.objects o2 ON fk.referenced_object_id = o2.object_id " +
                                                    "INNER JOIN sys.schemas s2 ON s2.schema_id = o2.schema_id " +
                                        ") AS fk " +
                                            "ON c.TABLE_SCHEMA = fk.TABLE_SCHEMA " +
                                            "AND c.TABLE_NAME = fk.TABLE_NAME " +
                                            "AND c.COLUMN_NAME = fk.COLUMN_NAME " +

                                "WHERE c.TABLE_SCHEMA = '" + objectSchema + "' " +
                                "AND c.TABLE_NAME = '" + objectName + "' " +
                                "ORDER BY c.ORDINAL_POSITION";

            SqlDataReader reader = command.ExecuteReader();

            List<DBColumn> columns = new List<DBColumn>();

            while (reader.Read())
            {
                DBColumn column = new DBColumn();
                column.Name = reader.GetString(reader.GetOrdinal("COLUMN_NAME"));
                column.Alias = createAlias(column.Name);
                column.Ordinal = reader.GetInt32(reader.GetOrdinal("ORDINAL_POSITION"));
                column.IsInPrimaryKey = (reader.GetInt32(reader.GetOrdinal("IS_IN_PRIMARY_KEY")) == 1);
                column.IsInForeignKey = (reader.GetInt32(reader.GetOrdinal("IS_IN_FOREIGN_KEY")) == 1);
                string dataType = reader.GetString(reader.GetOrdinal("DATA_TYPE"));
                string characterMaximum = string.Empty;
                if (!reader.IsDBNull(reader.GetOrdinal("CHARACTER_MAXIMUM_LENGTH"))) characterMaximum = reader.GetInt32(reader.GetOrdinal("CHARACTER_MAXIMUM_LENGTH")).ToString();
                column.NumericPrecision = (reader.IsDBNull(reader.GetOrdinal("NUMERIC_PRECISION"))) ? 0 : Convert.ToInt32(reader.GetByte(reader.GetOrdinal("NUMERIC_PRECISION")));
                column.NumericScale = (reader.IsDBNull(reader.GetOrdinal("NUMERIC_SCALE"))) ? 0 : reader.GetInt32(reader.GetOrdinal("NUMERIC_SCALE"));
                column.DataTypeName = dataType;
                string dataTypeNameComplete = string.IsNullOrWhiteSpace(characterMaximum) ? dataType : string.Format("{0}({1})", dataType, characterMaximum);
                column.DataTypeNameComplete = dataTypeNameComplete;
                column.LanguageType = languageTypes[dataType];
                column.DataProviderType = dataProviders[dataType];
                column.CharacterMaxLength = (string.IsNullOrWhiteSpace(characterMaximum)) ? column.CharacterMaxLength : Convert.ToInt32(characterMaximum);
                column.IsComputed = (reader.GetInt32(reader.GetOrdinal("IS_COMPUTED")) == 1);
                column.IsAutoKey = (reader.GetInt32(reader.GetOrdinal("IS_IDENTITY")) == 1);
                column.IsNullable = (reader.GetString(reader.GetOrdinal("IS_NULLABLE")) == "YES");
                columns.Add(column);
            }

            return columns;
        }

        private string createAlias(string value)
        {
            string s = value.Trim();
            ArrayList arrayList = new ArrayList();
            for (int i = 0; i < s.Length; i++)
            {
                if (!Char.IsLetterOrDigit(s[i])) arrayList.Add(s[i]);
            }
            for (int i = 0; i < arrayList.Count; i++)
            {
                s = s.Replace((Char)arrayList[i], '_');
            }

            return s;
        }

        public static bool isModifiedDate(DBColumn column)
        {
            if ((column.Name.ToLower() == "modifieddate" &&
                column.DataTypeName.ToLower() == "datetime") ||
                (column.Name.ToLower() == "lastmodifieddate" &&
                column.DataTypeName.ToLower() == "datetime"))
                /* -- Could use this as well, but need to extend DBColumn and DataAccess.GetColumns() to read column description metadata
                ||
                (column.Description.ToLower().StartsWith("modifieddate") &&
                column.DataTypeName.ToLower() == "datetime") ||
                (column.Description.ToLower().StartsWith("lastmodifieddate") &&
                column.DataTypeName.ToLower() == "datetime"))
                */
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool isCreatedDate(DBColumn column)
        {
            if ((column.Name.ToLower() == "createddate" &&
                column.DataTypeName.ToLower() == "datetime") ||
                (column.Name.ToLower() == "creationdate" &&
                column.DataTypeName.ToLower() == "datetime"))
                /* -- Could use this as well, but need to extend DBColumn and DataAccess.GetColumns() to read column description metadata
                ||
                (column.Description.ToLower().StartsWith("createddate") &&
                column.DataTypeName.ToLower() == "datetime") ||
                (column.Description.ToLower().StartsWith("creationdate") &&
                column.DataTypeName.ToLower() == "datetime"))
                */
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool isAuthorityId(DBColumn column)
        {
            return column.Name.ToLower() == "authorityid" && column.DataTypeName.ToLower() == "int";
        }

        public static bool isCreationUserId(DBColumn column)
        {
            return column.Name.ToLower() == "creationuserid" && column.DataTypeName.ToLower() == "int";
        }
    }
}
