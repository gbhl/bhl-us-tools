using System;
using System.Text;

namespace DALCodeGen.sql
{
    public class DBColumn
    {
        public string Name { get; set; }
        public string Alias { get; set; }
        public int Ordinal { get; set; }
        public bool IsInPrimaryKey { get; set; }
        public bool IsInForeignKey { get; set; }
        public string DataTypeName { get; set; }
        public string DataTypeNameComplete { get; set; }
        public string LanguageType { get; set; }
        public string DataProviderType { get; set; }
        public int CharacterMaxLength { get; set; }
        public int NumericPrecision { get; set; }
        public int NumericScale { get; set; }
        public bool IsComputed { get; set; }
        public bool IsNullable { get; set; }
        public bool IsAutoKey { get; set; }

        /// <summary>
        /// Parameter name to associate with the column name.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Parameter name as a string.</returns>
        public string ParamName()
        {
            string paramName = string.Empty;

            if (!string.IsNullOrWhiteSpace(this.Alias))
            {
                char[] c = this.Alias.ToCharArray();
                // lowercase first character of name
                c[0] = c[0].ToString().ToLower().ToCharArray()[0];
                paramName = new string(c);
            }

            return paramName;
        }

        /// <summary>
        /// Type of column based upon language type and if is nullable.
        /// </summary>
        /// <param name="column"></param>
        /// <returns>Column type.</returns>
        public string Type()
        {
            string type = this.LanguageType;

            if (this.IsNullable)
            {
                switch (this.LanguageType)
                {
                    case "string":
                        break;
                    default:
                        if (this.LanguageType.EndsWith("[]"))
                        {
                            string s = this.LanguageType.Replace("[]", "");
                            type = string.Format("{0}?[]", s);
                        }
                        else
                        {
                            type = string.Format("{0}?", this.LanguageType);
                        }
                        break;
                }
            }

            return type;
        }

        /// <summary>
        /// Property name for the column.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Property name as a string.</returns>
        public string PropertyName()
        {
            string propertyName = string.Empty;

            if (!string.IsNullOrWhiteSpace(this.Alias))
            {
                char[] c = this.Alias.ToCharArray();
                // uppercase first character of column name
                c[0] = c[0].ToString().ToUpper().ToCharArray()[0];
                propertyName = new string(c);
            }

            return propertyName;
        }

        /// <summary>
        /// Variable name to associate with the column.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Variable name as a string.</returns>
        public string VariableName()
        {
            string variableName = string.Empty;
            if (!string.IsNullOrWhiteSpace(this.Alias)) variableName = "_" + PropertyName();
            return variableName;
        }

        /// <summary>
        /// Name to use for setting values.
        /// </summary>
        /// <param name="column"></param>
        /// <returns>Name as a string.</returns>
        public string SetName()
        {
            if (this.IsComputed || this.IsAutoKey)
            {
                return VariableName();
            }
            else
            {
                return PropertyName();
            }
        }

        /// <summary>
        /// The column's attribute definition. 
        /// </summary>
        /// <param name="column"></param>
        /// <returns>Column's attribute definition as a string.</returns>
        public string ColumnDefinition()
        {
            StringBuilder csb = new StringBuilder();

            StringBuilder sb = new StringBuilder();
            sb.Append(string.Format("\"{0}\"", this.Name));
            sb.Append(string.Format(", DbTargetType={0}", this.DataProviderType));
            sb.Append(string.Format(", Ordinal={0}", this.Ordinal.ToString()));
            if (this.CharacterMaxLength > 0)
            {
                sb.Append(string.Format(", CharacterMaxLength={0}", this.CharacterMaxLength.ToString()));
            }
            if (this.NumericPrecision > 0)
            {
                sb.Append(string.Format(", NumericPrecision={0}", this.NumericPrecision.ToString()));
            }
            if (this.NumericScale > 0)
            {
                sb.Append(string.Format(", NumericScale={0}", this.NumericScale.ToString()));
            }
            if (this.IsAutoKey)
            {
                sb.Append(string.Format(", IsAutoKey={0}", this.IsAutoKey.ToString().ToLower()));
            }
            if (this.IsComputed)
            {
                sb.Append(string.Format(", IsComputed={0}", this.IsComputed.ToString().ToLower()));
            }
            if (this.IsInForeignKey)
            {
                sb.Append(string.Format(", IsInForeignKey={0}", this.IsInForeignKey.ToString().ToLower()));
            }
            if (this.IsInPrimaryKey)
            {
                sb.Append(string.Format(", IsInPrimaryKey={0}", this.IsInPrimaryKey.ToString().ToLower()));
            }
            if (this.IsNullable)
            {
                sb.Append(string.Format(", IsNullable={0}", this.IsNullable.ToString().ToLower()));
            }

            return string.Format("[ColumnDefinition({0})]", sb.ToString());
        }

        /// <summary>
        /// Default action to use when assigning value to a property associated with the column.
        /// </summary>
        /// <param name="column"></param>
        /// <returns>Any default action to apply as a string.</returns>
        public string DefaultAction()
        {
            string defaultAction = string.Empty;

            switch (this.LanguageType)
            {
                case "string":
                    int maxLen = this.CharacterMaxLength;
                    defaultAction = "if (value != null) value = CalibrateValue(value, " + maxLen + ");";
                    break;
                default:
                    break;
            }

            return defaultAction;
        }

        /// <summary>
        /// Default value to use when initializing a private variable associated with the column.
        /// </summary>
        /// <param name="column"></param>
        /// <returns>Any default value to use as variable initialization as a string.</returns>
        public string DefaultValue()
        {
            string defaultValue = string.Empty;

            if (this.IsNullable)
            {
                defaultValue = " = null";
            }
            else
            {
                switch (this.LanguageType)
                {
                    case "string":
                        defaultValue = " = string.Empty";
                        break;
                    case "long":
                        defaultValue = " = default(long)";
                        break;
                    case "short":
                        defaultValue = " = default(short)";
                        break;
                    case "int":
                        defaultValue = " = default(int)";
                        break;
                    case "byte":
                        defaultValue = " = default(byte)";
                        break;
                    case "decimal":
                        defaultValue = " = default(decimal)";
                        break;
                    case "bool":
                        defaultValue = " = false";
                        break;
                    case "byte[]":
                        defaultValue = " = new byte[0]";
                        break;
                    case "DateTime":
                        defaultValue = string.Empty;
                        break;
                    case "Guid":
                        defaultValue = " = Guid.Empty";
                        break;
                    case "":
                        defaultValue = "";
                        break;
                    default:
                        defaultValue = " = null";
                        break;
                }
            }

            return defaultValue;
        }
    }
}
