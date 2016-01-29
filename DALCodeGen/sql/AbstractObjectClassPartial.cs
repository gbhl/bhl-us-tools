using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALCodeGen.sql
{
    public partial class AbstractObjectClass
    {
        private string _classNamespace = string.Empty;
        private string _objectSchema = string.Empty;
        private string _objectName = string.Empty;
        private string _className = string.Empty;
        private List<DBColumn> _columns = new List<DBColumn>();

        public AbstractObjectClass(string classNamespace, string objectSchema, string objectName, string className, List<sql.DBColumn> columns)
        {
            _classNamespace = classNamespace;
            _objectSchema = objectSchema;
            _objectName = objectName;
            _className = className;
            _columns = columns;
        }
    }
}
