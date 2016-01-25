Generates data access layer classes for a specified database object.

With the exception of -h [--help], all arguments can be specified either in the
configuration file (DalCodeGen.exe.config) or on the command line.  -h [--help]
can only be specified on the command line.  Command line arguments override
configuration file arguments.

ARGUMENTS

Command Line               Configuration file     Description

-a [--abstractns] <val>    AbtractClassnamespace  Abstract class namespace
-b [--publicdal] <val>     GenerateDalPublicClass Generate a public DAL class
                                                    Values: True / False
-c [--schema] <val>        ObjectSchema           Database object schema 
                                                    Example: dbo
-d [--database] <val>      DatabaseName           Database name
-f [--folder] <val>        OutputFolder           Output folder for generated 
                                                    files
-h [--help]                                       Show help
-i [--interfacens] <val>   InterfaceNamespace     Interface namespace
-k [--concreteclass] <val> GenerateConcreteClass  Generate a concrete data 
                                                    object class
                                                      Values: True / False
-l [--dalns] <val>         DalNamespace           DAL namespace
-n [--interface] <val>     GenerateInterface      Generate an interface
                                                    Values: True / False
-o [--objectname] <val>    ObjectName             Database object name
-p [--password] <val>      DatabasePassword       Database password
-s [--server] <val>        DatabaseServer         Database server name
-t [--type] <val>          ObjectType             Type of database object
                                                    Values: Table / View
-u [--user] <val>          DatabaseUsername       Database user
-y [--key] <val>           DalConnectionKey       Name of a connectionString 
                                                    entry in an application 
                                                    configuration file.  The
                                                    connectionString entry 
                                                    should be the one the 
                                                    generated classes will use.
                                                    Given the following 
                                                    connectionStrings section 
                                                    of a configuration file, 
                                                    the value for the command
                                                    line argument should be
                                                    'BHL':
                                                      <connectionStrings>
                                                        <add name="BHL" 
                                                         connectionString=""/>
                                                      </connectionStrings>
