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
using System.Collections.Specialized; 
using System.Data; 
using System.Data.SqlClient; 
using System.IO; 
using System.Reflection; 
using System.Text; 

namespace FlexWiki.SqlStoreManagerLib
{
  public sealed class DatabaseHelper
  {
    /// <summary>
    /// Creates a new FlexWiki SQL Server database using the credentials of the current user.
    /// </summary>
    /// <param name="instance">The SQL Server instance to use (e.g. "localhost").</param>
    /// <param name="database">The database name within the instance to create.</param>
    /// <param name="dataFileDirectory">The directory in which to put the SQL Server data and log files.</param>
    public static void CreateDatabase(string instance, string database, string dataFileDirectory)
    {
      string connectionString = string.Format("server={0};Integrated Security=true", instance);
      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        connection.Open();
        CreateDatabase(connection, database, dataFileDirectory);
        CreateDatabaseObjects(connection, database);
      }
    }

    /// <summary>
    /// Grants access to an existing FlexWiki SQL Server database for a given user
    /// </summary>
    /// <param name="instance">The SQL Server instance to use (e.g. "localhost").</param>
    /// <param name="database">The database name within the instance to use.</param>
    /// <param name="user">The SQL Server user that will be granted access to the newly created database.</param>
    public static void GrantUser(string instance, string database, string user)
    {
      string connectionString = string.Format("server={0};Integrated Security=true", instance);
      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        connection.Open();
        GrantUserPermissions(connection, database, user);
      }
    }

    /// <summary>
    /// Sets up a new FlexWiki SQL Server database using the credentials of the currently logged-in user.
    /// </summary>
    /// <param name="instance">The SQL Server instance to use (e.g. "localhost").</param>
    /// <param name="database">The database name within the instance to create.</param>
    /// <param name="dataFileDirectory">The directory in which to put the SQL Server data and log files.</param>
    /// <param name="user">The SQL Server user that will be granted access to the newly created database.</param>
    /// <remarks>Note: this will overwrite any existing database, destroying all data. Use with caution.</remarks>
    public static void SetUpDatabase(string instance, string database, string dataFileDirectory, string user)
    {
      string connectionString = string.Format("server={0};Integrated Security=true", instance);
      SetUpDatabaseInternal(connectionString, instance, database, dataFileDirectory, user);
    }

    /// <summary>
    /// Sets up a new FlexWiki SQL Server database using the specified credentials.
    /// </summary>
    /// <param name="instance">The SQL Server instance to use (e.g. "localhost").</param>
    /// <param name="database">The database name within the instance to create.</param>
    /// <param name="dataFileDirectory">The directory in which to put the SQL Server data and log files.</param>
    /// <param name="user">The SQL Server user that will be granted access to the newly created database.</param>
    /// <param name="saUser">The username of a SQL Server account with sufficient permissions to create the new database.</param>
    /// <param name="saPassword">The password of a SQL Server account with sufficient permissions to create the new database.</param>
    /// <remarks>Note: this will overwrite any existing database, destroying all data. Use with caution.</remarks>
    public static void SetUpDatabase(string instance, string database, string dataFileDirectory, string user, string saUser, string saPassword)
    {
      string connectionString = string.Format("server={0};uid={1};pwd={2}", instance, saUser, saPassword);
      SetUpDatabaseInternal(connectionString, instance, database, dataFileDirectory, user);
    }

    private static void SetUpDatabaseInternal(string connectionString, string instance, string database, string dataFileDirectory, string user)
    {
      using (SqlConnection connection = new SqlConnection(connectionString))
      {
        connection.Open();
        CreateDatabase(connection, database, dataFileDirectory);
        CreateDatabaseObjects(connection, database);
        GrantUserPermissions(connection, database, user);
      }
    }

    /// <summary>
    /// Creates the database by running the script. 
    /// The name of the database is a constant, but could be parameterized.
    /// </summary>
    private static void CreateDatabase(SqlConnection connection, string database, string dataFileDirectory)
    {
      if (!Directory.Exists(dataFileDirectory))
      {
        Directory.CreateDirectory(dataFileDirectory); 
      }

      NameValueCollection args = new NameValueCollection();
      args.Add("database", database);
      args.Add("datadir", dataFileDirectory);
      ExecuteCommand(connection, "CreateFlexWikiDatabase.sql", args);
    }

    /// <summary>
    /// Creates the database objects including the Tables and Stored Procedures.
    /// This is thru a .sql script. The script is run using OSql.
    /// </summary>
    private static void CreateDatabaseObjects(SqlConnection connection, string database)
    {
      NameValueCollection args = new NameValueCollection();
      args.Add("database", database);
      ExecuteCommand(connection, "FlexWikiSqlStoreObjectCreationScript.sql", args);
    }

    /// <summary>
    /// Grants the specified user Database Owner (DBO) access.
    /// </summary>
    private static void GrantUserPermissions(SqlConnection connection, string database, string user)
    {
      NameValueCollection args = new NameValueCollection();
      args.Add("database", database);
      args.Add("user", user);                                       
      ExecuteCommand(connection, "GrantUserPermisions.sql", args);
    }

    private static void ExecuteCommand(SqlConnection connection, string script, NameValueCollection args)
    {
      string scriptResource = "FlexWiki.SqlStoreManagerLib.Scripts." + script;
      Stream scriptStream = Assembly.GetExecutingAssembly().GetManifestResourceStream(scriptResource);
      try
      {
        StreamReader scriptReader = new StreamReader(scriptStream); 
        try
        {
          string line = string.Empty; 
          StringBuilder commandBuilder = new StringBuilder(); 
          while ((line = scriptReader.ReadLine()) != null)
          {
            if (line.Trim().ToUpper() == "GO")
            {
              string commandText = ExpandMacros(commandBuilder, args);
              SqlCommand command = new SqlCommand(commandText, connection);
              command.ExecuteNonQuery();
              commandBuilder.Remove(0, commandBuilder.Length); 
            }
            else
            {
              commandBuilder.Append(line);
              commandBuilder.Append('\n');
            }
          }
        }
        finally
        {
          scriptReader.Close();
        }
      }
      finally
      {
        scriptStream.Close(); 
      }

    }

    private static string ExpandMacros(StringBuilder text, NameValueCollection args)
    {
      foreach (string macro in args.AllKeys)
      {
        text = text.Replace("${" + macro + "}", args[macro]);
      }

      return text.ToString();
    }


  }
}