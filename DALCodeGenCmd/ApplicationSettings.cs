using System.Collections.Generic;

namespace DALCodeGenCmd
{
    /// <summary>
    /// Class that contains the configuration settings for the application
    /// </summary>
    public class ApplicationSettings
    {
        [SettingInfo(
            Name: "ShowHelp",
            Description: "Show Help",
            ErrorMsg: "",
            ConfigKey: "")]
        public bool ShowHelp { get; set; }

        [SettingInfo(
            Name:"DatabaseServer", 
            Description:"Database Server", 
            ErrorMsg:"Invalid database server", 
            ConfigKey:"DatabaseServer")]
        public string DatabaseServer { get; set; }

        [SettingInfo(
            Name: "DatabaseName",
            Description: "Database Name",
            ErrorMsg: "Invalid database name",
            ConfigKey: "DatabaseName")]
        public string DatabaseName { get; set; }

        [SettingInfo(
            Name: "DatabaseUsername",
            Description: "Database Username",
            ErrorMsg: "Invalid database user",
            ConfigKey: "DatabaseUsername")]
        public string DatabaseUsername { get; set; }

        [SettingInfo(
            Name: "DatabasePassword",
            Description: "Database Password",
            ErrorMsg: "Invalid database password",
            ConfigKey: "DatabasePassword")]
        public string DatabasePassword { get; set; }

        [SettingInfo(
            Name: "OutputFolder",
            Description: "Output Folder",
            ErrorMsg: "Invalid output folder",
            ConfigKey: "OutputFolder")]
        public string OutputFolder { get; set; }

        [SettingInfo(
            Name: "ObjectType",
            Description: "Object Type - 'table' or 'view'",
            ErrorMsg: "Invalid object type - use 'table' or 'view'",
            ConfigKey: "ObjectType")]
        public ObjectType ObjectType { get; set; }

        [SettingInfo(
            Name: "ObjectSchema",
            Description: "Object Schema",
            ErrorMsg: "Invalid object schema - example value 'dbo'",
            ConfigKey: "ObjectSchema")]
        public string ObjectSchema { get; set; }

        [SettingInfo(
            Name: "ObjectName",
            Description: "Object Name",
            ErrorMsg: "Invalid object name",
            ConfigKey: "ObjectName")]
        public string ObjectName { get; set; }

        [SettingInfo(
            Name: "AbstractClassNamespace",
            Description: "Abstract Class Namespace",
            ErrorMsg: "Invalid abstract class namespace",
            ConfigKey: "AbstractClassNamespace")]
        public string AbstractClassNamespace { get; set; }

        [SettingInfo(
            Name: "GenerateConcreteClass",
            Description: "Generate Concrete Class - 'true' or 'false'",
            ErrorMsg: "Invalid concrete class generation flag - use 'true' or 'false'",
            ConfigKey: "GenerateConcreteClass")]
        public GenerateTrueFalse GenerateConcreteClass { get; set; }

        [SettingInfo(
            Name: "DalNamespace",
            Description: "DAL Namespace",
            ErrorMsg: "Invalid DAL namespace",
            ConfigKey: "DalNamespace")]
        public string DalNamespace { get; set; }

        [SettingInfo(
            Name: "GenerateDalPublicClass",
            Description: "Generate DAL Public Class - 'true' or 'false'",
            ErrorMsg: "Invalid DAL public class generation flag - use 'true' or 'false'",
            ConfigKey: "GenerateDalPublicClass")]
        public GenerateTrueFalse GenerateDalPublicClass { get; set; }

        [SettingInfo(
            Name: "DalConnectionKey",
            Description: "DAL Connection Key",
            ErrorMsg: "Invalid DAL connection key",
            ConfigKey: "DalConnectionKey")]
        public string DalConnectionKey { get; set; }

        [SettingInfo(
            Name: "GenerateInterfaces",
            Description: "Generate Interface - 'true' or 'false'",
            ErrorMsg: "Invalid interface generation flag - use 'true' or 'false'",
            ConfigKey: "GenerateInterface")]
        public GenerateTrueFalse GenerateInterfaces { get; set; }

        [SettingInfo(
            Name: "InterfaceNamespace",
            Description: "Interface Namespace",
            ErrorMsg: "Invalid interface namespace",
            ConfigKey: "InterfaceNamespace")]
        public string InterfaceNamespace { get; set; }

        public string ConnectionString
        {
            get
            {
                // NOTE:  To support a database other than SQL Server, this should be modified
                return string.Format("Data Source={0};Initial Catalog={1};user={2};password={3};Connection Timeout=180;",
                    this.DatabaseServer, this.DatabaseName, this.DatabaseUsername, this.DatabasePassword);
            }
        }

        #region Get Setting Attributes

        private string GetAttributeValue(string property, string attribute)
        {
            string value = string.Empty;

            System.Reflection.PropertyInfo[] props = this.GetType().GetProperties();
            foreach(System.Reflection.PropertyInfo prop in props)
            {
                if (prop.Name.ToLower() == property.ToLower())
                {
                    object[] attribs = prop.GetCustomAttributes(false);
                    foreach(object attrib in attribs)
                    {
                        if (attrib is SettingInfoAttribute)
                        {
                            SettingInfoAttribute a = (SettingInfoAttribute)attrib;
                            value = a.GetType().GetProperty(attribute).GetValue(a).ToString();
                        }
                    }
                }
            }
            
            return value;
        }

        public string GetName(string setting) { return GetAttributeValue(setting, "Name"); }
        public string GetDescription(string setting) { return GetAttributeValue(setting, "Description"); }
        public string GetErrorMsg(string setting) { return GetAttributeValue(setting, "ErrorMsg"); }
        public string GetConfigKey(string setting) { return GetAttributeValue(setting, "ConfigKey"); }
        public string GetShortParam(string setting) { return GetAttributeValue(setting, "ShortParam"); }
        public string GetLongParam(string setting) { return GetAttributeValue(setting, "LongParam"); }

        #endregion Get Setting Attributes

    }
}
