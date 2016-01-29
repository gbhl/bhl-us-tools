using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALCodeGen.sql
{
    public partial class DAL
    {
        private List<DBColumn> _columns = new List<DBColumn>();
        private string _objectSchema = string.Empty;
        private string _objectName = string.Empty;
        private string _objectNameClean = string.Empty;
        private string _className = string.Empty;
        private string _classNamespace = string.Empty;
        private string _dalNamespace = string.Empty;
        private string _interfaceName = string.Empty;
        private string _connectionKey = string.Empty;

        public DAL(string objectSchema, string objectName, string objectNameClean, string className, string classNamespace, 
            string dalNamespace, string interfaceName, string connectionKey, List<sql.DBColumn> columns)
        {
            _objectSchema = objectSchema;
            _objectName = objectName;
            _objectNameClean = objectNameClean;
            _className = className;
            _classNamespace = classNamespace;
            _dalNamespace = dalNamespace;
            _interfaceName = interfaceName;
            _connectionKey = connectionKey;
            _columns = columns;
        }
    }
}
