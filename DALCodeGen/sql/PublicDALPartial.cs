using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DALCodeGen.sql
{
    public partial class PublicDAL
    {
        private string _dalNamespace = string.Empty;
        private string _className = string.Empty;
        public PublicDAL(string dalNamespace, string className)
        {
            _dalNamespace = dalNamespace;
            _className = className;
        }
    }
}
