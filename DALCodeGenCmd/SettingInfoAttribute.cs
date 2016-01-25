namespace DALCodeGenCmd
{
    public class SettingInfoAttribute : System.Attribute
    {
        private string _name = string.Empty;
        private string _description = string.Empty;
        private string _errorMsg = string.Empty;
        private string _configKey = string.Empty;

        public string Name { get { return _name; } }
        public string Description { get { return _description; } }
        public string ErrorMsg { get { return _errorMsg; } }
        public string ConfigKey { get { return _configKey; } }

        public SettingInfoAttribute(string Name, string Description, string ErrorMsg, string ConfigKey)
        {
            this._name = Name;
            this._description = Description;
            this._errorMsg = ErrorMsg;
            this._configKey = ConfigKey;
        }
    }
}
