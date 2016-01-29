using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALCodeGen.sql
{
    public partial class ConcreteObjectClass
    {
        private string _classNamespace = string.Empty;
        private string _objectName = string.Empty;
        private string _className = string.Empty;

        public ConcreteObjectClass(string classNamespace, string objectName, string className)
        {
            _classNamespace = classNamespace;
            _objectName = objectName;
            _className = className;
        }
    }
}
