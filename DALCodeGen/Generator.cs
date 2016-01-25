using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace DALCodeGen
{
    public class Generator
    {
        private string _connectionString = string.Empty;
        private string _outputFolder = string.Empty;
        private DALType _dalType;
        private ColumnLanguageType _columnLanguageType;
        private ColumnDataProviderType _columnDataProviderType;
        private Dictionary<string, string> _columnLanguageTypeValues;
        private Dictionary<string, string> _columnDataProviderTypeValues;
        private Dictionary<string, string> _languageReservedWordValues;

        public Generator(string connectionString)
        {
            _connectionString = connectionString;
            _outputFolder = ".";
            _dalType = DALType.SQL;
            _columnLanguageType = ColumnLanguageType.CSHARP;
            _columnDataProviderType = ColumnDataProviderType.SQLCLIENT;

            _columnLanguageTypeValues = this.LoadColumnLanguageTypeValues(_dalType.ToString(), _columnLanguageType.ToString());
            _columnDataProviderTypeValues = this.LoadColumnDataProviderTypeValues(_dalType.ToString(), _columnDataProviderType.ToString());
            _languageReservedWordValues = this.LoadReservedWordValues(_columnLanguageType.ToString());
        }

        public Generator(string connectionString, string outputFolder)
        {
            _connectionString = connectionString;
            _outputFolder = outputFolder;
            _dalType = DALType.SQL;
            _columnLanguageType = ColumnLanguageType.CSHARP;
            _columnDataProviderType = ColumnDataProviderType.SQLCLIENT;

            _columnLanguageTypeValues = this.LoadColumnLanguageTypeValues(_dalType.ToString(), _columnLanguageType.ToString());
            _columnDataProviderTypeValues = this.LoadColumnDataProviderTypeValues(_dalType.ToString(), _columnDataProviderType.ToString());
            _languageReservedWordValues = this.LoadReservedWordValues(_columnLanguageType.ToString());
        }

        public Generator(string connectionString, string outputFolder, DALType dalType, 
            ColumnLanguageType columnLanguageType, ColumnDataProviderType columnDataProviderType)
        {
            _connectionString = connectionString;
            _outputFolder = outputFolder;
            _dalType = dalType;
            _columnLanguageType = columnLanguageType;
            _columnDataProviderType = columnDataProviderType;

            _columnLanguageTypeValues = this.LoadColumnLanguageTypeValues(_dalType.ToString(), _columnLanguageType.ToString());
            _columnDataProviderTypeValues = this.LoadColumnDataProviderTypeValues(_dalType.ToString(), _columnDataProviderType.ToString());
            _languageReservedWordValues = this.LoadReservedWordValues(_columnLanguageType.ToString());
        }

        public sealed class DALType
        {
            public static readonly DALType SQL = new DALType(1, "SQL");
            public static readonly DALType MYSQL2 = new DALType(2, "MYSQL2");
            public static readonly DALType POSTGRESQL = new DALType(3, "POSTGRESQL");
            public static readonly DALType POSTGRESQL8 = new DALType(4, "POSTGRESQL8");
            public static readonly DALType ORACLE = new DALType(5, "ORACLE");

            private readonly String name;
            private readonly int value;

            private DALType(int value, String name) { this.name = name; this.value = value; }

            public int Value() { return value; }
            public override String ToString() { return name; }
        }
        
        public sealed class ColumnLanguageType
        {
            public static readonly ColumnLanguageType CSHARP = new ColumnLanguageType(1, "C#");
            public static readonly ColumnLanguageType CSHARPSYSTEMTYPES = new ColumnLanguageType(2, "C# System Types");
            public static readonly ColumnLanguageType VBNET = new ColumnLanguageType(3, "VB.NET");
            public static readonly ColumnLanguageType VBNETSYSTEMTYPES = new ColumnLanguageType(4, "VB.NET System Types");
            public static readonly ColumnLanguageType VB60 = new ColumnLanguageType(5, "VB60");
            public static readonly ColumnLanguageType CSHARPNPGSQL= new ColumnLanguageType(6, "C# (Npgsql)");
            public static readonly ColumnLanguageType VBNETNPGSQL = new ColumnLanguageType(7, "VB.NET (Npgsql)");
            public static readonly ColumnLanguageType CSHARPCORELAB = new ColumnLanguageType(8, "C# (CoreLab)");
            public static readonly ColumnLanguageType VBNETCORELAB = new ColumnLanguageType(9, "VB.NET (CoreLab)");
            public static readonly ColumnLanguageType CSHARPMYSQLCONNECTOR = new ColumnLanguageType(10, "MySQL Connector/Net (C#)");
            public static readonly ColumnLanguageType VBNETMYSQLCONNECTOR = new ColumnLanguageType(11, "MySQL Connector/Net (VB.NET)");

            private readonly String name;
            private readonly int value;

            private ColumnLanguageType(int value, String name) { this.name = name; this.value = value; }

            public override String ToString() { return name; }
        }

        public sealed class ColumnDataProviderType
        {
            public static readonly ColumnDataProviderType SQLCLIENT = new ColumnDataProviderType(1, "SqlClient");
            public static readonly ColumnDataProviderType OLEDB= new ColumnDataProviderType(2, "OleDb");
            public static readonly ColumnDataProviderType ALLDBCLIENT = new ColumnDataProviderType(3, "AllDbClient");
            public static readonly ColumnDataProviderType ADODB = new ColumnDataProviderType(4, "ADODB");
            public static readonly ColumnDataProviderType ORACLECLIENT = new ColumnDataProviderType(5, "OracleClient");
            public static readonly ColumnDataProviderType NPGSQL = new ColumnDataProviderType(6, "Npgsql");
            public static readonly ColumnDataProviderType MYSQLCONNECTOR= new ColumnDataProviderType(7, "MySQL Connector/Net");
            public static readonly ColumnDataProviderType DBTYPE = new ColumnDataProviderType(8, "DbType");

            private readonly String name;
            private readonly int value;

            private ColumnDataProviderType(int value, String name) { this.name = name; this.value = value; }

            public override String ToString() { return name; }
        }

        /// <summary>
        /// Get the Column Language Types for the specified DAL type and programming language type
        /// </summary>
        /// <param name="dalType"></param>
        /// <param name="languageType"></param>
        /// <returns></returns>
        private Dictionary<string, string> LoadColumnLanguageTypeValues(string dalType, string languageType)
        {
            Dictionary<string, string> languageTypeValues = new Dictionary<string, string>();

            XElement root = XElement.Load(@"LanguageTypes.xml");
            IEnumerable<XElement> language =
                from lang in root.Elements("Language")
                where (string)lang.Attribute("From") == dalType && (string)lang.Attribute("To") == languageType
                select lang;
            foreach(XElement type in language.Elements<XElement>())
            {
                languageTypeValues.Add(type.Attribute("From").Value, type.Attribute("To").Value);
            }

            return languageTypeValues;
        }

        /// <summary>
        /// Get the Column Data Provider Types for the specified DAL type and programming language type
        /// </summary>
        /// <param name="dalType"></param>
        /// <param name="dataProviderType"></param>
        /// <returns></returns>
        private Dictionary<string, string> LoadColumnDataProviderTypeValues(string dalType, string dataProviderType)
        {
            Dictionary<string, string> dataProviderTypes = new Dictionary<string, string>();

            XElement root = XElement.Load(@"DataProviderTypes.xml");
            IEnumerable<XElement> dbTarget =
                from db in root.Elements("DbTarget")
                where (string)db.Attribute("From") == dalType && (string)db.Attribute("To") == dataProviderType
                select db;
            foreach (XElement type in dbTarget.Elements<XElement>())
            {
                dataProviderTypes.Add(type.Attribute("From").Value, type.Attribute("To").Value);
            }

            return dataProviderTypes;
        }

        /// <summary>
        /// Get the Reserved Words for the specified programming language
        /// </summary>
        /// <param name="languageType"></param>
        /// <returns></returns>
        private Dictionary<string, string> LoadReservedWordValues(string languageType)
        {
            Dictionary<string, string> reservedWords = new Dictionary<string, string>();

            XElement root = XElement.Load(@"LanguageReservedWords.xml");
            IEnumerable<XElement> words =
                from language in root.Elements("Language")
                where (string)language.Attribute("Type") == languageType
                select language;
            foreach (XElement word in words.Elements<XElement>())
            {
                reservedWords.Add(word.Attribute("From").Value, word.Attribute("To").Value);
            }

            return reservedWords;
        }

        public void GenerateDeleteProcedure(string objectSchema, string objectName)
        {
            switch (_dalType.Value())
            {
                case 1: // Sql Server
                    GenerateSqlServerProcedure(SqlAction.Delete, objectSchema, objectName, GetDeleteProcedureTemplate);
                    break;
                case 2: // MySql
                case 3: // PostgreSql
                case 4: // PostgreSql
                case 5: // Oracle
                default:
                    throw new NotImplementedException();
            }
        }

        public void GenerateInsertProcedure(string objectSchema, string objectName)
        {
            switch (_dalType.Value())
            {
                case 1: // Sql Server
                    GenerateSqlServerProcedure(SqlAction.Insert, objectSchema, objectName, GetInsertProcedureTemplate);
                    break;
                case 2: // MySql
                case 3: // PostgreSql
                case 4: // PostgreSql
                case 5: // Oracle
                default:
                    throw new NotImplementedException();
            }
        }

        public void GenerateUpdateProcedure(string objectSchema, string objectName)
        {
            switch (_dalType.Value())
            {
                case 1: // Sql Server
                    GenerateSqlServerProcedure(SqlAction.Update, objectSchema, objectName, GetUpdateProcedureTemplate);
                    break;
                case 2: // MySql
                case 3: // PostgreSql
                case 4: // PostgreSql
                case 5: // Oracle
                default:
                    throw new NotImplementedException();
            }
        }

        public void GenerateSelectProcedure(string objectSchema, string objectName)
        {
            switch (_dalType.Value())
            {
                case 1: // Sql Server
                    GenerateSqlServerProcedure(SqlAction.Select, objectSchema, objectName, GetSelectProcedureTemplate);
                    break;
                case 2: // MySql
                case 3: // PostgreSql
                case 4: // PostgreSql
                case 5: // Oracle
                default:
                    throw new NotImplementedException();
            }
        }
        
        public void GenerateAbtractObjectClass(string classNamespace, string objectSchema, string objectName)
        {
            string className = string.Format("__{0}", renameDotNetReservedNames(objectName));
            List<sql.DBColumn> columns = new sql.DataAccess(_connectionString).GetColumns(
                objectSchema, objectName, _columnLanguageTypeValues, _columnDataProviderTypeValues);

            var abstractClassTemplate = new sql.AbstractObjectClass(classNamespace, objectSchema, objectName, className, columns);
            string classText = abstractClassTemplate.TransformText();
            File.WriteAllText(string.Format("{0}\\{1}.cs", _outputFolder, className), classText);
        }

        public void GenerateConcreteObjectClass(string classNamespace, string objectName)
        {
            string className = string.Format("__{0}", renameDotNetReservedNames(objectName));
            var concreteClassTemplate = new sql.ConcreteObjectClass(classNamespace, this.renameDotNetReservedNames(objectName), className);
            string classText = concreteClassTemplate.TransformText();
            File.WriteAllText(string.Format("{0}\\{1}.cs", _outputFolder, renameDotNetReservedNames(objectName)), classText);
        }

        public void GeneratePublicDal(string dalNamespace, string objectName)
        {
            string className = string.Format("{0}DAL", renameDotNetReservedNames(objectName));
            var publicDALTemplate = new sql.PublicDAL(dalNamespace, className);
            string classText = publicDALTemplate.TransformText();
            File.WriteAllText(string.Format("{0}\\{1}.cs", _outputFolder, className), classText);
        }

        public void GenerateDalInterface(string classNamespace, string interfaceNamespace, string objectSchema, string objectName)
        {
            string className = string.Format("{0}DAL", renameDotNetReservedNames(objectName));
            string interfaceName = string.Format("I{0}", className);
            List<sql.DBColumn> columns = new sql.DataAccess(_connectionString).GetColumns(
                objectSchema, objectName, _columnLanguageTypeValues, _columnDataProviderTypeValues);
            if (!HasPrimaryKey(columns)) throw new NoPrimaryKeyException(string.Format("{0}.{1} has no primary key.  No DAL interface created.", objectSchema, objectName));
            var interfaceTemplate = new sql.DALInterface(objectSchema, renameDotNetReservedNames(objectName), columns, classNamespace, interfaceNamespace, interfaceName);
            string interfaceText = interfaceTemplate.TransformText();
            File.WriteAllText(string.Format("{0}\\{1}.Auto.cs", _outputFolder, interfaceName), interfaceText);
        }

        public void GenerateDal(string objectSchema, string objectName, string classNamespace, string dalNamespace, string dalConnectionKey, bool includeInterface)
        {
            string className = string.Format("{0}DAL", renameDotNetReservedNames(objectName));
            string interfaceName = string.Empty;
            if (includeInterface) interfaceName = string.Format("I{0}", className);
            List<sql.DBColumn> columns = new sql.DataAccess(_connectionString).GetColumns(
                objectSchema, objectName, _columnLanguageTypeValues, _columnDataProviderTypeValues);
            if (!HasPrimaryKey(columns)) throw new NoPrimaryKeyException(string.Format("{0}.{1} has no primary key.  No DAL created.", objectSchema, objectName));
            var dalTemplate = new sql.DAL(objectSchema, objectName, renameDotNetReservedNames(objectName), className, classNamespace, dalNamespace, interfaceName, dalConnectionKey, columns);
            string classText = dalTemplate.TransformText();
            File.WriteAllText(string.Format("{0}\\{1}.Auto.cs", _outputFolder, className), classText);
        }

        #region SQL Procedure template delegates

        private delegate string GetProcedureTemplate(string objectSchema, string objectName, string procedureName, List<sql.DBColumn> columns);

        private string GetDeleteProcedureTemplate(string objectSchema, string objectName, string procedureName, List<sql.DBColumn> columns)
        {
            return new sql.DeleteProc(objectSchema, objectName, procedureName, columns).TransformText();
        }

        private string GetInsertProcedureTemplate(string objectSchema, string objectName, string procedureName, List<sql.DBColumn> columns)
        {
            return new sql.InsertProc(objectSchema, objectName, procedureName, columns).TransformText();
        }

        private string GetUpdateProcedureTemplate(string objectSchema, string objectName, string procedureName, List<sql.DBColumn> columns)
        {
            return new sql.UpdateProc(objectSchema, objectName, procedureName, columns).TransformText();
        }

        private string GetSelectProcedureTemplate(string objectSchema, string objectName, string procedureName, List<sql.DBColumn> columns)
        {
            return new sql.SelectProc(objectSchema, objectName, procedureName, columns).TransformText();
        }

        #endregion SQL Procedure template delegates

        /// <summary>
        /// Generate a SQL Server stored procedure for the specified SQL Action (Select, Insert, Update, Delete)
        /// </summary>
        /// <param name="sqlAction"></param>
        /// <param name="objectSchema"></param>
        /// <param name="objectName"></param>
        /// <param name="getProcedureTemplate"></param>
        private void GenerateSqlServerProcedure(SqlAction sqlAction, string objectSchema, string objectName, GetProcedureTemplate getProcedureTemplate)
        {
            string procedureName = string.Format("{0}{1}Auto", objectName, sqlAction.ToString());
            List<sql.DBColumn> columns = new sql.DataAccess(_connectionString).GetColumns(
                objectSchema, objectName, _columnLanguageTypeValues, _columnDataProviderTypeValues);
            if (!HasPrimaryKey(columns)) throw new NoPrimaryKeyException(string.Format("{0}.{1} has no primary key.  No Sql Server {2} procedure created.", objectSchema, objectName, sqlAction.ToString()));
            string procText = getProcedureTemplate(objectSchema, objectName, procedureName, columns);
            File.WriteAllText(string.Format("{0}\\{1}.{2}.sql", _outputFolder, objectSchema, procedureName), procText);
        }

        /// <summary>
        /// Examine the specified list of column for any that are in the primary key
        /// </summary>
        /// <param name="columns"></param>
        /// <returns></returns>
        private bool HasPrimaryKey(List<sql.DBColumn> columns)
        {
            bool hasKey = false;
            foreach(sql.DBColumn column in columns)
            {
                if (column.IsInPrimaryKey) { hasKey = true; break; }
            }
            return hasKey;
        }

        /// <summary>
        /// If the specified string includes a reserved word, return its replacement.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private string renameDotNetReservedNames(string s)
        {
            if (_languageReservedWordValues.ContainsKey(s.ToLower())) s = _languageReservedWordValues[s.ToLower()];
            return s;
        }

        private enum SqlAction
        {
            Select,
            Insert,
            Update,
            Delete
        }
    }
}
