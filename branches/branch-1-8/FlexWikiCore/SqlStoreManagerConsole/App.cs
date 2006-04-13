#region License Statement
// Copyright (c) Microsoft Corporation.  All rights reserved.
//
// The use and distribution terms for this software are covered by the 
// Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file CPL.TXT at the root of this distribution.
// By using this software in any fashion, you are agreeing to be bound by 
// the terms of this license.
//
// You must not remove this notice, or any other, from this software.
#endregion

using System;

using FlexWiki.SqlStoreManagerLib; 

namespace FlexWiki.SqlStoreManagerConsole
{
	public class App
	{
    private const string USAGE = 
@"Usage: 

ssmc create <instance> <database> <datadir>
  Creates a new SqlStore database at the specified location.

ssmc grant <instance> <database> <user>
  Grants access to the database for the specified user.

ssmc help
ssmc /help
ssmc /?
ssmc
  Displays this message.

<instance> - Name of a SQL Server instance to use. E.g. ""."".
<database> - Name of the SQL Server database to use. E.g. FlexWikiSqlStore.
<datadir>  - Directory in which to create the data files. E.g. C:\temp
<user>     - Fully qualified NT user to grant access to. E.g. ISENGARD\ASPNET.
";

    public static int Main(string[] args)
    {
      try
      {
        Options options = null; 
        try
        {
          options = ParseOptions(args); 
        }
        catch (ParseOptionsException poe)
        {
          Console.WriteLine(poe.Message); 
          Console.WriteLine(USAGE);
          return (int) Result.ErrorUsage; 
        }

        if (options.Command == Commands.Help)
        {
          PrintHelp();
        }
        else if (options.Command == Commands.Create)
        {
          DatabaseHelper.CreateDatabase(options.Instance, options.Database, options.DataDirectory); 
        }
        else if (options.Command == Commands.Grant)
        {
          DatabaseHelper.GrantUser(options.Instance, options.Database, options.User); 
        }

        return (int) Result.Success; 
      }
      catch (Exception e)
      {
        Console.WriteLine(e);
        return (int) Result.ErrorException; 
      }
    }

    public static Options ParseOptions(string[] args)
    {
      Options options = new Options(); 
      if (args.Length == 0)
      {
        options.Command = Commands.Help; 
        return options; 
      }

      string command = args[0].ToLower(); 

      if (command.Equals("help") || command.Equals("/help") || command.Equals("/?"))
      {
        options.Command = Commands.Help; 
      }
      else if (command.Equals("grant"))
      {
        if (args.Length != 4)
        {
          throw new ParseOptionsException("Wrong number of arguments");
        }
        
        options.Command = Commands.Grant; 
        options.Instance = args[1];
        options.Database = args[2];
        options.User = args[3]; 

      }
      else if (command.Equals("create"))
      {
        if (args.Length != 4)
        {
          throw new ParseOptionsException("Wrong number of arguments");
        }

        options.Command = Commands.Create; 
        options.Instance = args[1];
        options.Database = args[2];
        options.DataDirectory = args[3]; 
  
      }
      else
      {
        throw new ParseOptionsException("Unrecognized command " + command);
      }

      return options; 

    }

    private static void PrintHelp()
    {
      Console.WriteLine(USAGE); 
    }
	}
}
