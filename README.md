# bhl-us-tools
Development tools for bhl-us projects.

This repository contains tools used to aid development of the Biodiversity Heritage Library (BHL).

DALCodeGen produces a .NET assembly which can be used to create Data Access Layer (DAL) classes and SQL Server stored procedures for basic CRUD operations.

DALCodeGenCmd produces a command line interface for the DALCodeGen assembly.  Building this project creates a DALCodeGen.exe application which should be used to produce DAL classes and stored procedures for use in BHL.  For details of the arguments accepted by the DALCodeGen.exe application, see the ReadMe.txt in the DALCodeGenCmd folder.
