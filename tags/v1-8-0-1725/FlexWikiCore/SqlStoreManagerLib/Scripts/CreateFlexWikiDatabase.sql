/*License Statement
 Copyright (c) Microsoft Corporation.  All rights reserved.

The use and distribution terms for this software are covered by the 
Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
which can be found in the file CPL.TXT at the root of this distribution.
By using this software in any fashion, you are agreeing to be bound by 
the terms of this license.

You must not remove this notice, or any other, from this software.
*/

IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'${database}')  DROP DATABASE [${database}] 
GO 
CREATE DATABASE [${database}]  ON (NAME = N'${database}_Data', FILENAME = N'${datadir}\${database}.mdf' , SIZE = 1, FILEGROWTH = 10%) LOG ON (NAME = N'${database}_Log', FILENAME = N'${datadir}\${database}_log.ldf' , FILEGROWTH = 10%) 
COLLATE SQL_Latin1_General_CP1_CI_AS 
GO
exec sp_dboption N'${database}', N'autoclose', N'false'
GO
exec sp_dboption N'${database}', N'bulkcopy', N'false'
GO
exec sp_dboption N'${database}', N'trunc. log', N'true'
GO
exec sp_dboption N'${database}', N'torn page detection', N'true'
GO
exec sp_dboption N'${database}', N'read only', N'false'
GO
exec sp_dboption N'${database}', N'dbo use', N'false'
GO
exec sp_dboption N'${database}', N'single', N'false'
GO
exec sp_dboption N'${database}', N'autoshrink', N'true'
GO
exec sp_dboption N'${database}', N'ANSI null default', N'false'
GO
exec sp_dboption N'${database}', N'recursive triggers', N'false'
GO
exec sp_dboption N'${database}', N'ANSI nulls', N'false'
GO
exec sp_dboption N'${database}', N'concat null yields null', N'false'
GO
exec sp_dboption N'${database}', N'cursor close on commit', N'false'
GO
exec sp_dboption N'${database}', N'default to local cursor', N'false'
GO
exec sp_dboption N'${database}', N'quoted identifier', N'false'
GO
exec sp_dboption N'${database}', N'ANSI warnings', N'false'
GO
exec sp_dboption N'${database}', N'auto create statistics', N'true'
GO
exec sp_dboption N'${database}', N'auto update statistics', N'true'
GO
