using System;
using DALCodeGen;
using System.Configuration;
using Fclp;
using System.IO;
using System.Text;

namespace DALCodeGenCmd
{
    class Program
    {
        static void Main(string[] args)
        {
            // Get the settings from the configuration file
            ApplicationSettings configSettings = ReadConfigFile();

            // Read any settings specified on the command line.  These will override any configuration file values
            bool commandlineOk = true;
            ApplicationSettings commandLineSettings = null;
            try
            {
                commandLineSettings = ReadCommandLine(args);
            }
            catch (Exception ex)
            {
                WriteOutput(ex.Message);
                commandlineOk = false;
            }

            // Combine the config and command line settings, giving preference to the command line
            ApplicationSettings settings = MergeSettings(configSettings, commandLineSettings);

            // Only proceed if the command line was parsed successfully
            bool writeConfig = false;
            if (commandlineOk)
            {
                if (settings.ShowHelp)
                {
                    WriteHelp();
                }
                else
                {
                    // Validate the values in the settings object
                    string validationErrors = ValidateSettings(settings);
                    if (string.IsNullOrWhiteSpace(validationErrors))
                    {
                        try
                        {
                            // Check for existence of output folder and create it if necessary
                            if (!Directory.Exists(settings.OutputFolder)) Directory.CreateDirectory(settings.OutputFolder);

                            // Generate the specified code
                            GenerateCode(settings);
                        }
                        catch (Exception ex)
                        {
                            WriteOutput(ex.Message);
                        }
                    }
                    else
                    {
                        WriteOutput(validationErrors);
                        writeConfig = true;
                    }
                }
            }
            else
            {
                writeConfig = true;
            }

            if (writeConfig) WriteConfiguration(settings);
        }

        static void GenerateCode(ApplicationSettings settings)
        {
            string connectionString = settings.ConnectionString;
            string outputFolder = settings.OutputFolder;
            ObjectType objectType = settings.ObjectType;
            string objectSchema = settings.ObjectSchema;
            string objectName = settings.ObjectName;
            string abstractClassNamespace = settings.AbstractClassNamespace;
            GenerateTrueFalse generateConcreteClass = settings.GenerateConcreteClass;
            string dalNamespace = settings.DalNamespace;
            GenerateTrueFalse generateDalPublicClass = settings.GenerateDalPublicClass;
            string dalConnectionKey = settings.DalConnectionKey;
            GenerateTrueFalse generateInterfaces = settings.GenerateInterfaces;
            string interfaceNamespace = settings.InterfaceNamespace;

            WriteOutput("Object generation starting");
            WriteOutput(DateTime.Now.ToString());
            WriteOutput();
            WriteOutput(string.Format("Generating objects for {0} {1}.{2}", objectType, objectSchema, objectName));
            WriteOutput();

            Generator generator = new Generator(connectionString, outputFolder);

            if (objectType == ObjectType.Table)
            {
                // Render SQL
                try
                {
                    WriteOutput("Generating Delete procedure");
                    generator.GenerateDeleteProcedure(objectSchema, objectName);
                }
                catch (NoPrimaryKeyException noKeyEx)
                {
                    WriteOutput(string.Format("Error: {0}", noKeyEx.Message));
                }
                try
                {
                    WriteOutput("Generating Insert procedure");
                    generator.GenerateInsertProcedure(objectSchema, objectName);
                }
                catch (NoPrimaryKeyException noKeyEx)
                {
                    WriteOutput(string.Format("Error: {0}", noKeyEx.Message));
                }
                try
                {
                    WriteOutput("Generating Update procedure");
                    generator.GenerateUpdateProcedure(objectSchema, objectName);
                }
                catch (NoPrimaryKeyException noKeyEx)
                {
                    WriteOutput(string.Format("Error: {0}", noKeyEx.Message));
                }
                try
                {
                    WriteOutput("Generating Select procedure");
                    generator.GenerateSelectProcedure(objectSchema, objectName);
                }
                catch (NoPrimaryKeyException noKeyEx)
                {
                    WriteOutput(string.Format("Error: {0}", noKeyEx.Message));
                }
            }

            // Render classes
            try
            {
                WriteOutput("Generating Abstract Class");
                generator.GenerateAbtractObjectClass(abstractClassNamespace, objectSchema, objectName);
            }
            catch (Exception ex)
            {
                WriteOutput(string.Format("Error: {0}", ex.Message));
            }

            if (generateConcreteClass == GenerateTrueFalse.True)
            {
                try
                {
                    WriteOutput("Generating Concrete Class");
                    generator.GenerateConcreteObjectClass(abstractClassNamespace, objectName);
                }
                catch (Exception ex)
                {
                    WriteOutput(string.Format("Error: {0}", ex.Message));
                }
            }

            if (objectType == ObjectType.Table)
            {
                // Render DAL
                try
                {
                    WriteOutput("Generating DAL");
                    generator.GenerateDal(objectSchema, objectName, abstractClassNamespace, dalNamespace, 
                        dalConnectionKey, (generateInterfaces == GenerateTrueFalse.True));
                }
                catch (Exception ex)
                {
                    WriteOutput(string.Format("Error: {0}", ex.Message));
                }
                
                if (generateDalPublicClass == GenerateTrueFalse.True)
                {
                    try
                    {
                        WriteOutput("Generating Public DAL");
                        generator.GeneratePublicDal(dalNamespace, objectName);
                    }
                    catch (Exception ex)
                    {
                        WriteOutput(string.Format("Error: {0}", ex.Message));
                    }
                }

                if (generateInterfaces == GenerateTrueFalse.True)
                {
                    try
                    {
                        WriteOutput("Generating DAL Interface");
                        generator.GenerateDalInterface(abstractClassNamespace, interfaceNamespace, objectSchema, objectName);
                    }
                    catch (Exception ex)
                    {
                        WriteOutput(string.Format("Error: {0}", ex.Message));
                    }
                }
            }

            WriteOutput();
            WriteOutput("Object generation complete");
            WriteOutput(DateTime.Now.ToString());
        }

        /// <summary>
        /// Read configuration file
        /// </summary>
        static ApplicationSettings ReadConfigFile()
        {
            ApplicationSettings settings = new ApplicationSettings();

            settings.DatabaseServer = ConfigurationManager.AppSettings[settings.GetConfigKey("DatabaseServer")] as string;
            settings.DatabaseName = ConfigurationManager.AppSettings[settings.GetConfigKey("DatabaseName")] as string;
            settings.DatabaseUsername = ConfigurationManager.AppSettings[settings.GetConfigKey("DatabaseUsername")] as string;
            settings.DatabasePassword = ConfigurationManager.AppSettings[settings.GetConfigKey("DatabasePassword")] as string;
            settings.OutputFolder = ConfigurationManager.AppSettings[settings.GetConfigKey("OutputFolder")] as string;
            string objectType = ConfigurationManager.AppSettings[settings.GetConfigKey("ObjectType")] as string;   // Table | View
            if (objectType != null) settings.ObjectType = (objectType == "View" ? ObjectType.View : ObjectType.Table);
            settings.ObjectSchema = ConfigurationManager.AppSettings[settings.GetConfigKey("ObjectSchema")] as string;
            settings.ObjectName = ConfigurationManager.AppSettings[settings.GetConfigKey("ObjectName")] as string;
            settings.AbstractClassNamespace = ConfigurationManager.AppSettings[settings.GetConfigKey("AbstractClassnamespace")] as string;
            string generateConcreteClass = ConfigurationManager.AppSettings[settings.GetConfigKey("GenerateConcreteClass")] as string;
            if (generateConcreteClass != null) settings.GenerateConcreteClass = (generateConcreteClass == "True" ? GenerateTrueFalse.True: GenerateTrueFalse.False);
            settings.DalNamespace = ConfigurationManager.AppSettings[settings.GetConfigKey("DalNamespace")] as string;
            string generateDalPublicClass = ConfigurationManager.AppSettings[settings.GetConfigKey("GenerateDalPublicClass")] as string;
            if (generateDalPublicClass != null) settings.GenerateDalPublicClass = (generateDalPublicClass == "True" ? GenerateTrueFalse.True: GenerateTrueFalse.False);
            settings.DalConnectionKey = ConfigurationManager.AppSettings[settings.GetConfigKey("DalConnectionKey")] as string;
            string generateInterfaces = ConfigurationManager.AppSettings[settings.GetConfigKey("GenerateInterfaces")] as string;
            if (generateInterfaces != null) settings.GenerateInterfaces = (generateInterfaces == "True" ? GenerateTrueFalse.True: GenerateTrueFalse.False);
            settings.InterfaceNamespace = ConfigurationManager.AppSettings[settings.GetConfigKey("InterfaceNamespace")] as string;

            return settings;
        }

        /// <summary>
        /// Read application settings from the command line
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        static ApplicationSettings ReadCommandLine(string[] args)
        {
            // Create a generic parser for the ApplicationArguments type
            var p = new FluentCommandLineParser<ApplicationSettings>();
            ApplicationSettings settings = new ApplicationSettings();

            // Read the command line arguments
            p.Setup(arg => arg.ShowHelp).As('h', "help").WithDescription(settings.GetDescription("ShowHelp"));
            p.Setup(arg => arg.DatabaseServer).As('s', "server").WithDescription(settings.GetDescription("DatabaseServer"));
            p.Setup(arg => arg.DatabaseName).As('d', "database").WithDescription(settings.GetDescription("DatabaseName"));
            p.Setup(arg => arg.DatabaseUsername).As('u', "user").WithDescription(settings.GetDescription("DatabaseUsername"));
            p.Setup(arg => arg.DatabasePassword).As('p', "password").WithDescription(settings.GetDescription("DatabasePassword"));
            p.Setup(arg => arg.OutputFolder).As('f', "folder").WithDescription(settings.GetDescription("OutputFolder"));
            p.Setup(arg => arg.ObjectType).As('t', "type").WithDescription(settings.GetDescription("ObjectType"));
            p.Setup(arg => arg.ObjectSchema).As('c', "schema").WithDescription(settings.GetDescription("ObjectSchema"));
            p.Setup(arg => arg.ObjectName).As('o', "objectname").WithDescription(settings.GetDescription("ObjectName"));
            p.Setup(arg => arg.AbstractClassNamespace).As('a', "abstractns").WithDescription(settings.GetDescription("AbstractClassNamespace"));
            p.Setup(arg => arg.GenerateConcreteClass).As('k', "concreteclass").WithDescription(settings.GetDescription("GenerateConcreteClass"));
            p.Setup(arg => arg.DalNamespace).As('l', "dalns").WithDescription(settings.GetDescription("DalNamespace"));
            p.Setup(arg => arg.GenerateDalPublicClass).As('b', "publicdal").WithDescription(settings.GetDescription("GenerateDalPublicClass"));
            p.Setup(arg => arg.DalConnectionKey).As('y', "key").WithDescription(settings.GetDescription("DalConnectionKey"));
            p.Setup(arg => arg.GenerateInterfaces).As('n', "interface").WithDescription(settings.GetDescription("GenerateInterfaces"));
            p.Setup(arg => arg.InterfaceNamespace).As('i', "interfacens").WithDescription(settings.GetDescription("InterfaceNamespace"));

            var result = p.Parse(args);
            if (result.HasErrors) throw new Exception(result.ErrorText);
            settings = p.Object;

            return settings;
        }

        /// <summary>
        /// Combine config and command line settings, giving preference to the command line when a value is 
        /// provided in both places.
        /// </summary>
        /// <param name="configSettings"></param>
        /// <param name="commandLineSettings"></param>
        /// <returns></returns>
        static ApplicationSettings MergeSettings(ApplicationSettings configSettings, ApplicationSettings commandLineSettings)
        {
            ApplicationSettings settings = configSettings;

            if (commandLineSettings != null)
            {
                settings.ShowHelp = commandLineSettings.ShowHelp;   // No config setting for ShowHelp; always use command line
                settings.DatabaseServer = commandLineSettings.DatabaseServer ?? configSettings.DatabaseServer;
                settings.DatabaseName = commandLineSettings.DatabaseName ?? configSettings.DatabaseName;
                settings.DatabaseUsername = commandLineSettings.DatabaseUsername ?? configSettings.DatabaseUsername;
                settings.DatabasePassword = commandLineSettings.DatabasePassword ?? configSettings.DatabasePassword;
                settings.OutputFolder = commandLineSettings.OutputFolder ?? configSettings.OutputFolder;
                settings.ObjectType = commandLineSettings.ObjectType == 0 ? configSettings.ObjectType : commandLineSettings.ObjectType;
                settings.ObjectSchema = commandLineSettings.ObjectSchema ?? configSettings.ObjectSchema;
                settings.ObjectName = commandLineSettings.ObjectName ?? configSettings.ObjectName;
                settings.AbstractClassNamespace = commandLineSettings.AbstractClassNamespace ?? configSettings.AbstractClassNamespace;
                settings.GenerateConcreteClass = commandLineSettings.GenerateConcreteClass == 0 ? configSettings.GenerateConcreteClass : commandLineSettings.GenerateConcreteClass;
                settings.DalNamespace = commandLineSettings.DalNamespace ?? configSettings.DalNamespace;
                settings.GenerateDalPublicClass = commandLineSettings.GenerateDalPublicClass == 0 ? configSettings.GenerateDalPublicClass : commandLineSettings.GenerateDalPublicClass;
                settings.DalConnectionKey = commandLineSettings.DalConnectionKey ?? configSettings.DalConnectionKey;
                settings.GenerateInterfaces = commandLineSettings.GenerateInterfaces == 0 ? configSettings.GenerateInterfaces : commandLineSettings.GenerateInterfaces;
                settings.InterfaceNamespace = commandLineSettings.InterfaceNamespace ?? configSettings.InterfaceNamespace;
            }
            
            return settings;
        }

        /// <summary>
        /// Validate the application settings
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        static string ValidateSettings(ApplicationSettings settings)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(settings.DatabaseServer)) errors.AppendLine(settings.GetErrorMsg("DatabaseServer"));
            if (string.IsNullOrWhiteSpace(settings.DatabaseName)) errors.AppendLine(settings.GetErrorMsg("DatabaseName"));
            if (string.IsNullOrWhiteSpace(settings.DatabaseUsername)) errors.AppendLine(settings.GetErrorMsg("DatabaseUsername"));
            if (string.IsNullOrWhiteSpace(settings.OutputFolder)) errors.AppendLine(settings.GetErrorMsg("OutputFolder"));
            if (settings.ObjectType == 0) errors.AppendLine(settings.GetErrorMsg("ObjectType"));
            if (string.IsNullOrWhiteSpace(settings.ObjectSchema)) errors.AppendLine(settings.GetErrorMsg("ObjectSchema"));
            if (string.IsNullOrWhiteSpace(settings.ObjectName)) errors.AppendLine(settings.GetErrorMsg("ObjectName"));
            if (string.IsNullOrWhiteSpace(settings.AbstractClassNamespace)) errors.AppendLine(settings.GetErrorMsg("AbstractClassNamespace"));
            if (settings.GenerateConcreteClass == 0) errors.AppendLine(settings.GetErrorMsg("GenerateConcreteClass"));
            if (string.IsNullOrWhiteSpace(settings.DalNamespace)) errors.AppendLine(settings.GetErrorMsg("DalNamespace"));
            if (settings.GenerateDalPublicClass == 0) errors.AppendLine(settings.GetErrorMsg("GenerateDalPublicClass"));
            if (string.IsNullOrWhiteSpace(settings.DalConnectionKey)) errors.AppendLine(settings.GetErrorMsg("DalConnectionKey"));
            if (settings.GenerateInterfaces == 0) errors.AppendLine(settings.GetErrorMsg("GenerateInterfaces"));
            if (string.IsNullOrWhiteSpace(settings.InterfaceNamespace)) errors.AppendLine(settings.GetErrorMsg("InterfaceNamespace"));

            return errors.ToString();
        }

        /// <summary>
        /// Output the specified message to this application's standard output stream
        /// </summary>
        /// <param name="message"></param>
        static void WriteOutput(string message = "")
        {
            Console.WriteLine(message);
        }

        /// <summary>
        /// Write all of the configuration settings to the standard output stream
        /// </summary>
        /// <param name="settings"></param>
        static void WriteConfiguration(ApplicationSettings settings)
        {
            WriteOutput();

            WriteOutput(string.Format("{0}: {1}", settings.GetName("DatabaseServer"), settings.DatabaseServer));
            WriteOutput(string.Format("{0}: {1}", settings.GetName("DatabaseName"), settings.DatabaseName));
            WriteOutput(string.Format("{0}: {1}", settings.GetName("DatabaseUsername"), settings.DatabaseUsername));
            WriteOutput(string.Format("{0}: {1}", settings.GetName("DatabasePassword"), settings.DatabasePassword));
            WriteOutput(string.Format("{0}: {1}", settings.GetName("OutputFolder"), settings.OutputFolder));
            WriteOutput(string.Format("{0}: {1}", settings.GetName("ObjectType"), settings.ObjectType));
            WriteOutput(string.Format("{0}: {1}", settings.GetName("ObjectSchema"), settings.ObjectSchema));
            WriteOutput(string.Format("{0}: {1}", settings.GetName("ObjectName"), settings.ObjectName));
            WriteOutput(string.Format("{0}: {1}", settings.GetName("AbstractClassNamespace"), settings.AbstractClassNamespace));
            WriteOutput(string.Format("{0}: {1}", settings.GetName("GenerateConcreteClass"), settings.GenerateConcreteClass));
            WriteOutput(string.Format("{0}: {1}", settings.GetName("DalNamespace"), settings.DalNamespace));
            WriteOutput(string.Format("{0}: {1}", settings.GetName("GenerateDalPublicClass"), settings.GenerateDalPublicClass));
            WriteOutput(string.Format("{0}: {1}", settings.GetName("DalConnectionKey"), settings.DalConnectionKey));
            WriteOutput(string.Format("{0}: {1}", settings.GetName("GenerateInterfaces"), settings.GenerateInterfaces));
            WriteOutput(string.Format("{0}: {1}", settings.GetName("InterfaceNamespace"), settings.InterfaceNamespace));

            WriteOutput();
            WriteOutput("For help, specify -h or --help");
        }

        /// <summary>
        /// Write the contents of the application's README file to the standard output stream
        /// </summary>
        static void WriteHelp()
        {
            string helpText = "No help available.  Is the README.txt file missing?";
            if (File.Exists(@"README.txt")) helpText = File.ReadAllText(@"README.txt");
            WriteOutput(helpText);
        }
    }
}
