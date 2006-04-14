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
using System.Collections; 
using System.Data; 
using System.Data.SqlClient; 

namespace FlexWiki.SqlProvider
{
  /// <summary>
  /// SqlHelperParameterCache provides functions to leverage a static cache of procedure parameters, and the
  /// ability to discover parameters for stored procedures at run-time.
  /// </summary>
  public sealed class SqlHelperParameterCache
  {
    #region private methods, variables, and constructors

    //Since this class provides only static methods, make the default constructor private to prevent 
    //instances from being created with "new SqlHelperParameterCache()"
    private SqlHelperParameterCache() {}

    private static Hashtable paramCache = Hashtable.Synchronized(new Hashtable());

    /// <summary>
    /// Resolve at run time the appropriate set of SqlParameters for a stored procedure
    /// </summary>
    /// <param name="connection">A valid SqlConnection object</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="includeReturnValueParameter">Whether or not to include their return rawValue parameter</param>
    /// <returns>The parameter array discovered.</returns>
    private static SqlParameter[] DiscoverSpParameterSet(SqlConnection connection, string spName, bool includeReturnValueParameter)
    {
      if( connection == null ) throw new ArgumentNullException( "connection" );
      if( spName == null || spName.Length == 0 ) throw new ArgumentNullException( "spName" );

      SqlCommand cmd = new SqlCommand(spName, connection);
      cmd.CommandType = CommandType.StoredProcedure;

      connection.Open();
      SqlCommandBuilder.DeriveParameters(cmd);
      connection.Close();

      if (!includeReturnValueParameter) 
      {
        cmd.Parameters.RemoveAt(0);
      }
                
      SqlParameter[] discoveredParameters = new SqlParameter[cmd.Parameters.Count];

      cmd.Parameters.CopyTo(discoveredParameters, 0);

      // Init the parameters with a DBNull rawValue
      foreach (SqlParameter discoveredParameter in discoveredParameters)
      {
        discoveredParameter.Value = DBNull.Value;
      }
      return discoveredParameters;
    }

    /// <summary>
    /// Deep copy of cached SqlParameter array
    /// </summary>
    /// <param name="originalParameters"></param>
    /// <returns></returns>
    private static SqlParameter[] CloneParameters(SqlParameter[] originalParameters)
    {
      SqlParameter[] clonedParameters = new SqlParameter[originalParameters.Length];

      for (int i = 0, j = originalParameters.Length; i < j; i++)
      {
        clonedParameters[i] = (SqlParameter)((ICloneable)originalParameters[i]).Clone();
      }

      return clonedParameters;
    }

    #endregion private methods, variables, and constructors

    #region caching functions

    /// <summary>
    /// Add parameter array to the cache
    /// </summary>
    /// <param name="connectionString">A valid connection string for a SqlConnection</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <param name="commandParameters">An array of SqlParamters to be cached</param>
    public static void CacheParameterSet(string connectionString, string commandText, params SqlParameter[] commandParameters)
    {
      if( connectionString == null || connectionString.Length == 0 ) throw new ArgumentNullException( "connectionString" );
      if( commandText == null || commandText.Length == 0 ) throw new ArgumentNullException( "commandText" );

      string hashKey = connectionString + ":" + commandText;

      paramCache[hashKey] = commandParameters;
    }

    /// <summary>
    /// Retrieve a parameter array from the cache
    /// </summary>
    /// <param name="connectionString">A valid connection string for a SqlConnection</param>
    /// <param name="commandText">The stored procedure name or T-SQL command</param>
    /// <returns>An array of SqlParamters</returns>
    public static SqlParameter[] GetCachedParameterSet(string connectionString, string commandText)
    {
      if( connectionString == null || connectionString.Length == 0 ) throw new ArgumentNullException( "connectionString" );
      if( commandText == null || commandText.Length == 0 ) throw new ArgumentNullException( "commandText" );

      string hashKey = connectionString + ":" + commandText;

      SqlParameter[] cachedParameters = paramCache[hashKey] as SqlParameter[];
      if (cachedParameters == null)
      {			
        return null;
      }
      else
      {
        return CloneParameters(cachedParameters);
      }
    }

    #endregion caching functions

    #region Parameter Discovery Functions

    /// <summary>
    /// Retrieves the set of SqlParameters appropriate for the stored procedure
    /// </summary>
    /// <remarks>
    /// This method will query the database for this information, and then store it in a cache for future requests.
    /// </remarks>
    /// <param name="connectionString">A valid connection string for a SqlConnection</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <returns>An array of SqlParameters</returns>
    public static SqlParameter[] GetSpParameterSet(string connectionString, string spName)
    {
      return GetSpParameterSet(connectionString, spName, false);
    }

    /// <summary>
    /// Retrieves the set of SqlParameters appropriate for the stored procedure
    /// </summary>
    /// <remarks>
    /// This method will query the database for this information, and then store it in a cache for future requests.
    /// </remarks>
    /// <param name="connectionString">A valid connection string for a SqlConnection</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="includeReturnValueParameter">A bool rawValue indicating whether the return rawValue parameter should be included in the details</param>
    /// <returns>An array of SqlParameters</returns>
    public static SqlParameter[] GetSpParameterSet(string connectionString, string spName, bool includeReturnValueParameter)
    {
      if( connectionString == null || connectionString.Length == 0 ) throw new ArgumentNullException( "connectionString" );
      if( spName == null || spName.Length == 0 ) throw new ArgumentNullException( "spName" );

      using(SqlConnection connection = new SqlConnection(connectionString))
      {
        return GetSpParameterSetInternal(connection, spName, includeReturnValueParameter);
      }
    }

    /// <summary>
    /// Retrieves the set of SqlParameters appropriate for the stored procedure
    /// </summary>
    /// <remarks>
    /// This method will query the database for this information, and then store it in a cache for future requests.
    /// </remarks>
    /// <param name="connection">A valid SqlConnection object</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <returns>An array of SqlParameters</returns>
    internal static SqlParameter[] GetSpParameterSet(SqlConnection connection, string spName)
    {
      return GetSpParameterSet(connection, spName, false);
    }

    /// <summary>
    /// Retrieves the set of SqlParameters appropriate for the stored procedure
    /// </summary>
    /// <remarks>
    /// This method will query the database for this information, and then store it in a cache for future requests.
    /// </remarks>
    /// <param name="connection">A valid SqlConnection object</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="includeReturnValueParameter">A bool rawValue indicating whether the return rawValue parameter should be included in the details</param>
    /// <returns>An array of SqlParameters</returns>
    internal static SqlParameter[] GetSpParameterSet(SqlConnection connection, string spName, bool includeReturnValueParameter)
    {
      if( connection == null ) throw new ArgumentNullException( "connection" );
      using (SqlConnection clonedConnection = (SqlConnection)((ICloneable)connection).Clone())
      {
        return GetSpParameterSetInternal(clonedConnection, spName, includeReturnValueParameter);
      }
    }

    /// <summary>
    /// Retrieves the set of SqlParameters appropriate for the stored procedure
    /// </summary>
    /// <param name="connection">A valid SqlConnection object</param>
    /// <param name="spName">The name of the stored procedure</param>
    /// <param name="includeReturnValueParameter">A bool rawValue indicating whether the return rawValue parameter should be included in the details</param>
    /// <returns>An array of SqlParameters</returns>
    private static SqlParameter[] GetSpParameterSetInternal(SqlConnection connection, string spName, bool includeReturnValueParameter)
    {
      if( connection == null ) throw new ArgumentNullException( "connection" );
      if( spName == null || spName.Length == 0 ) throw new ArgumentNullException( "spName" );

      string hashKey = connection.ConnectionString + ":" + spName + (includeReturnValueParameter ? ":include ReturnValue Parameter":"");

      SqlParameter[] cachedParameters;
        	
      cachedParameters = paramCache[hashKey] as SqlParameter[];
      if (cachedParameters == null)
      {	
        SqlParameter[] spParameters = DiscoverSpParameterSet(connection, spName, includeReturnValueParameter);
        paramCache[hashKey] = spParameters;
        cachedParameters = spParameters;
      }
        	
      return CloneParameters(cachedParameters);
    }
        
    #endregion Parameter Discovery Functions

  }
}
