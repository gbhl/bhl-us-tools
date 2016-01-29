using System.Collections.Generic;
using System.Text;

namespace DALCodeGen.sql
{
    public partial class DALInterface
    {
        private string _objectSchema = string.Empty;
        private string _objectName = string.Empty;
        private string _classNamespace = string.Empty;
        private string _interfaceNamespace = string.Empty;
        private string _interfaceName = string.Empty;
        private StringBuilder _interfaceBody = null;
        public DALInterface(string objectSchema, string objectName, List<DBColumn> columns, string classNamespace, string interfaceNamespace, string interfaceName)
        {
            _objectSchema = objectSchema;
            _objectName = objectName;
            _classNamespace = classNamespace;
            _interfaceNamespace = interfaceNamespace;
            _interfaceName = interfaceName;
            _interfaceBody = new StringBuilder();

            string functionName = string.Empty;
            string functionParmsString = string.Empty;

            // Select methods
            functionName = string.Format("{0}{1}", objectName, "SelectAuto");
            functionParmsString = GetFunctionParms(columns, IsColumnValidForSelect);

            _interfaceBody.AppendFormat("\t\t{0} {1}(SqlConnection sqlConnection, SqlTransaction sqlTransaction,\r\n\t\t\t{2});\r\n\r\n",
                objectName, functionName, functionParmsString);
            _interfaceBody.AppendFormat("\t\t{0} {1}(SqlConnection sqlConnection, SqlTransaction sqlTransaction, string connectionKeyName,\r\n\t\t\t{2});\r\n\r\n",
                objectName, functionName, functionParmsString);
            _interfaceBody.AppendFormat("\t\tCustomGenericList<CustomDataRow> {0}Raw(SqlConnection sqlConnection, SqlTransaction sqlTransaction,\r\n\t\t\t{1});\r\n\r\n",
                functionName, functionParmsString);
            _interfaceBody.AppendFormat("\t\tCustomGenericList<CustomDataRow> {0}Raw(SqlConnection sqlConnection, SqlTransaction sqlTransaction, string connectionKeyName,\r\n\t\t\t{1});\r\n\r\n",
                functionName, functionParmsString);

            // Insert methods
            functionName = string.Format("{0}{1}", objectName, "InsertAuto");
            functionParmsString = GetFunctionParms(columns, IsColumnValidForInsert);

            _interfaceBody.AppendFormat("\t\t{0} {1}(SqlConnection sqlConnection, SqlTransaction sqlTransaction,\r\n\t\t\t{2});\r\n\r\n",
                objectName, functionName, functionParmsString);
            _interfaceBody.AppendFormat("\t\t{0} {1}(SqlConnection sqlConnection, SqlTransaction sqlTransaction, string connectionKeyName,\r\n\t\t\t{2});\r\n\r\n",
                objectName, functionName, functionParmsString);
            _interfaceBody.AppendFormat("\t\t{0} {1}(SqlConnection sqlConnection, SqlTransaction sqlTransaction, {0} value);\r\n\r\n",
                objectName, functionName);
            _interfaceBody.AppendFormat("\t\t{0} {1}(SqlConnection sqlConnection, SqlTransaction sqlTransaction, string connectionKeyName, {0} value);\r\n\r\n",
                objectName, functionName);

            string insertParms = functionParmsString.Replace("\r\n", "\r\n\t\t");

            // Delete methods
            functionName = string.Format("{0}{1}", objectName, "DeleteAuto");
            functionParmsString = GetFunctionParms(columns, IsColumnValidForDelete);

            _interfaceBody.AppendFormat("\t\tbool {0}(SqlConnection sqlConnection, SqlTransaction sqlTransaction,\r\n\t\t\t{1});\r\n\r\n",
                functionName, functionParmsString);
            _interfaceBody.AppendFormat("\t\tbool {0}(SqlConnection sqlConnection, SqlTransaction sqlTransaction, string connectionKeyName,\r\n\t\t\t{1});\r\n\r\n",
                functionName, functionParmsString);

            // Update methods
            functionName = string.Format("{0}{1}", objectName, "UpdateAuto");
            functionParmsString = GetFunctionParms(columns, IsColumnValidForUpdate);

            _interfaceBody.AppendFormat("\t\t{0} {1}(SqlConnection sqlConnection, SqlTransaction sqlTransaction,\r\n\t\t\t{2});\r\n\r\n",
                objectName, functionName, functionParmsString);
            _interfaceBody.AppendFormat("\t\t{0} {1}(SqlConnection sqlConnection, SqlTransaction sqlTransaction, string connectionKeyName,\r\n\t\t\t{2});\r\n\r\n",
                objectName, functionName, functionParmsString);
            _interfaceBody.AppendFormat("\t\t{0} {1}(SqlConnection sqlConnection, SqlTransaction sqlTransaction, {0} value);\r\n\r\n",
                objectName, functionName);
            _interfaceBody.AppendFormat("\t\t{0} {1}(SqlConnection sqlConnection, SqlTransaction sqlTransaction, string connectionKeyName, {0} value);\r\n\r\n",
                objectName, functionName);

            // Manage methods
            functionName = string.Format("{0}{1}", objectName, "ManageAuto");
            string userIdString = PrintUserId(insertParms);

            _interfaceBody.AppendFormat("\t\tCustomDataAccessStatus<{0}> {1}(SqlConnection sqlConnection, SqlTransaction sqlTransaction, {0} value{2});\r\n\r\n",
                objectName, functionName, userIdString);
            _interfaceBody.AppendFormat("\t\tCustomDataAccessStatus<{0}> {1}(SqlConnection sqlConnection, SqlTransaction sqlTransaction, string connectionKeyName, {0} value{2});\r\n\r\n",
                objectName, functionName, userIdString);
        }

        #region Functions for evaluating columns for inclusion in interface

        private delegate bool IsColumnValid(DBColumn column);

        private bool IsColumnValidForSelect(DBColumn column)
        {
            return column.IsInPrimaryKey;
        }

        private bool IsColumnValidForInsert(DBColumn column)
        {
            bool isValid = false;
            if (!DataAccess.isModifiedDate(column) && !DataAccess.isCreatedDate(column) && !DataAccess.isAuthorityId(column))
            {
                isValid = !column.IsAutoKey && !column.IsComputed;
            }
            return isValid;
        }

        private bool IsColumnValidForDelete(DBColumn column)
        {
            return column.IsInPrimaryKey;
        }

        private bool IsColumnValidForUpdate(DBColumn column)
        {
            return !DataAccess.isModifiedDate(column) && !DataAccess.isCreatedDate(column) &&
                    !DataAccess.isAuthorityId(column) && !DataAccess.isCreationUserId(column);
        }

        #endregion Functions for evaluating columns for inclusion in interface

        /// <summary>
        /// Build the parameter list for a function, given a list of columns and a function 
        /// to evaluate the validity of a column.
        /// </summary>
        /// <param name="columns"></param>
        /// <param name="isColumnValid"></param>
        /// <returns></returns>
        private string GetFunctionParms(List<DBColumn> columns, IsColumnValid isColumnValid)
        {
            StringBuilder functionParmsSb = new StringBuilder();
            int x = 0;
            foreach (DBColumn column in columns)
            {
                if (isColumnValid(column))
                {
                    if (x > 0) functionParmsSb.AppendFormat(",\r\n\t\t\t");
                    functionParmsSb.AppendFormat("{0} {1}", column.Type(), column.ParamName());
                    x++;
                }
            }
            string functionParmsString = functionParmsSb.ToString();
            functionParmsSb = null;

            return functionParmsString;
        }

        private string PrintUserId(string insertParms)
        {
            if (insertParms.IndexOf("CreationUserID", System.StringComparison.CurrentCultureIgnoreCase) > 0 || 
                insertParms.IndexOf("LastModifiedUserID", System.StringComparison.CurrentCultureIgnoreCase) > 0)
            {
                return ", int userId";
            }
            else
            {
                return "";
            }
        }
    }
}
