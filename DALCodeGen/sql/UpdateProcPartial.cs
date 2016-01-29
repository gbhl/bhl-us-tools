using System.Collections.Generic;

namespace DALCodeGen.sql
{
    public partial class UpdateProc
    {
        private string _objectSchema = string.Empty;
        private string _objectName = string.Empty;
        private string _procedureName = string.Empty;
        private List<DBColumn> _columns = new List<DBColumn>();

        public UpdateProc(string objectSchema, string objectName, string procedureName, List<sql.DBColumn> columns)
        {
            _objectSchema = objectSchema;
            _objectName = objectName;
            _procedureName = procedureName;
            _columns = columns;
        }
    }
}
