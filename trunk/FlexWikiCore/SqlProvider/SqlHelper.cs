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

namespace SqlProvider
{
	/// <summary>
	/// Summary description for SqlHelper.
	/// </summary>
	internal sealed class SqlHelper
	{
		private SqlHelper()
		{
		}

		/// <summary>
		/// Method to check if the NamespaceExists.
		/// </summary>
		/// <param name="ns">Namespace to check for existence.</param>
		/// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
		/// <returns></returns>
		public static bool NamespaceExists(string ns, string connectionString)
		{
			SqlParameter[] parameters = new SqlParameter[2];
			parameters[0] = new SqlParameter("@namespace", ns);
			parameters[1] = new SqlParameter("@namespaceExists", SqlDbType.Bit);
			parameters[1].Direction = ParameterDirection.Output;

			SqlDataAccessHelper.ExecuteScalar(connectionString, CommandType.StoredProcedure, "CheckNamespaceExists", parameters);

			return Boolean.Parse(parameters[1].Value.ToString());
		}

		/// <summary>
		/// Method to create Namespace.
		/// </summary>
		/// <param name="ns">Namespace to be created.</param>
		/// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
		public static void CreateNamespace(string ns, string connectionString)
		{
			SqlParameter[] parameters = new SqlParameter[1];
			parameters[0] = new SqlParameter("@namespace", ns);

			SqlDataAccessHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "CreateNamespace", parameters);
		}
		
		/// <summary>
		/// Method to delete a Namespace.
		/// </summary>
		/// <param name="ns">Namespace to delete.</param>
		/// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
		public static void DeleteNamespace(string ns, string connectionString)
		{
			SqlParameter[] parameters = new SqlParameter[1];
			parameters[0] = new SqlParameter("@namespace", ns);
		
			SqlDataAccessHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "DeleteNamespace", parameters);
		}

		/// <summary>
		/// Check if the Topic Exists in the namespace.
		/// </summary>
		/// <param name="ns">Namespace of the topic.</param>
		/// <param name="topicName">Name of the topic.</param>
		/// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
		/// <returns>Boolean value that indicates if the topic exists.</returns>
		public static bool TopicExists(string ns, string topicName, string connectionString)
		{
			SqlParameter[] parameters = new SqlParameter[3];
			parameters[0] = new SqlParameter("@namespace", ns);
			parameters[1] = new SqlParameter("@topicName", topicName);
			parameters[2] = new SqlParameter("@topicExists", SqlDbType.Bit);
			parameters[2].Direction = ParameterDirection.Output;

			SqlDataAccessHelper.ExecuteScalar(connectionString, CommandType.StoredProcedure, "CheckTopicExists", parameters);

			return Boolean.Parse(parameters[2].Value.ToString());
		}

		/// <summary>
		/// Check if the topic is writable. All topics are by default writable in the Sql store./
		/// We do not have support to set this property from the web interface.Database Connectionstring to use for this namespace.
		/// </summary>
		/// <param name="ns">Namespace of the topic.</param>
		/// <param name="topicName">Name of the topic.</param>
		/// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
		/// <returns>Boolean value indicating if the topic is writable.</returns>
		public static bool IsExistingTopicWritable(string ns, string topicName, string connectionString)
		{
			SqlParameter[] parameters = new SqlParameter[3];
			parameters[0] = new SqlParameter("@namespace", ns);
			parameters[1] = new SqlParameter("@topicName", topicName);
			parameters[2] = new SqlParameter("@topicWritable", SqlDbType.Bit);
			parameters[2].Direction = ParameterDirection.Output;

			SqlDataAccessHelper.ExecuteScalar(connectionString, CommandType.StoredProcedure, "IsExistingTopicWritable", parameters);

			return Boolean.Parse(parameters[2].Value.ToString());
		}

		/// <summary>
		/// Get the time when the topic was last updated.
		/// </summary>
		/// <param name="ns">Namespace of the topic.</param>
		/// <param name="topicName">Name of the Topic.</param>
		/// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
		/// <returns>Last Update Time for the topic.</returns>
		public static DateTime GetTopicLastWriteTime(string ns, string topicName, string connectionString)
		{
			SqlParameter[] parameters = new SqlParameter[3];
			parameters[0] = new SqlParameter("@namespace", ns);
			parameters[1] = new SqlParameter("@topicName", topicName);
			parameters[2] = new SqlParameter("@topicLastWriteTime", SqlDbType.DateTime);
			parameters[2].Direction = ParameterDirection.Output;

			SqlDataAccessHelper.ExecuteScalar(connectionString, CommandType.StoredProcedure, "GetTopicLastWriteTime", parameters);

			return (DateTime)parameters[2].Value;
		}

		/// <summary>
		/// Get the time when the topic was created.
		/// </summary>
		/// <param name="ns">Namespace of the topic.</param>
		/// <param name="topicName">Name of the topic</param>
		/// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
		/// <returns>Creation Time of the topic.</returns>
		public static DateTime GetTopicCreationTime(string ns, string topicName, string connectionString)
		{
			SqlParameter[] parameters = new SqlParameter[3];
			parameters[0] = new SqlParameter("@namespace", ns);
			parameters[1] = new SqlParameter("@topicName", topicName);
			parameters[2] = new SqlParameter("@topicCreationTime", SqlDbType.DateTime);
			parameters[2].Direction = ParameterDirection.Output;

			SqlDataAccessHelper.ExecuteScalar(connectionString, CommandType.StoredProcedure, "GetTopicCreationTime", parameters);

			return (DateTime)parameters[2].Value;		
		}

		/// <summary>
		/// Get the contents of the topic.
		/// </summary>
		/// <param name="ns">Namespace of the topic.</param>
		/// <param name="topicName">Name of the topic to fetch the topic body.</param>
		/// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
		/// <returns>Returns the topic contents</returns>
		public static string GetTopicBody(string ns, string topicName, string connectionString)
		{
			SqlParameter[] parameters = new SqlParameter[2];
			parameters[0] = new SqlParameter("@namespace", ns);
			parameters[1] = new SqlParameter("@topicName", topicName);
		
			return SqlDataAccessHelper.ExecuteScalar(connectionString, CommandType.StoredProcedure, "GetTopicBody", parameters) as string;
		}

		/// <summary>
		/// Gets all versions for the specified topic.
		/// </summary>
		/// <param name="ns">Namespace of the topic.</param>
		/// <param name="topicName">Name of the topic to fetch the archive topics.</param>
		/// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
		/// <returns>Array of SqlTopicInfo for all the archive versions of the specified topic.</returns>
		public static SqlInfoForTopic[] GetSqlTopicInfosForTopic(string ns, string topicName, string connectionString)
		{
			ArrayList topicInfos = new ArrayList();

			SqlParameter[] parameters = new SqlParameter[2];
			parameters[0] = new SqlParameter("@namespace", ns);
			parameters[1] = new SqlParameter("@topicName", topicName + "(%)"); // pattern for sql wild card search
		
			using(SqlDataReader reader = SqlDataAccessHelper.ExecuteReader(connectionString, CommandType.StoredProcedure, "GetSqlTopicArchiveInfos", parameters) )
			{
				while(reader.Read())
				{
					SqlInfoForTopic topicInfo = new SqlInfoForTopic(reader.GetString(0), reader.GetDateTime(1));
					topicInfos.Add(topicInfo);
				}
			}
			
			return (SqlInfoForTopic[])topicInfos.ToArray(typeof(SqlInfoForTopic));
		}

		/// <summary>
		/// Gets all versions for the specified topic since the specified date and including the 
		/// specified date.
		/// </summary>
		/// <param name="ns">Namespace of the topic.</param>
		/// <param name="topicName">Name of the topic to fetch the archive topics.</param>
		/// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
		/// <returns>Array of SqlTopicInfo for all the archive versions of the specified topic.</returns>
		
		/// <summary>
		/// Gets all versions for the specified topic since the specified date and including the 
		/// specified date.
		/// </summary>
		/// <param name="ns">Namespace of the topic.</param>
		/// <param name="topicName">Name of the topic to fetch the archive topics.</param>
		/// <param name="stamp">Datetime indicating the lower bound for topic last write time.</param>
		/// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
		/// <returns>Array of SqlTopicInfo for all the archive versions of the specified topic.</returns>
		public static SqlInfoForTopic[] GetSqlTopicInfosForTopicSince(string ns, string topicName, DateTime stamp ,string connectionString)
		{
			ArrayList topicInfos = new ArrayList();

			SqlParameter[] parameters = new SqlParameter[3];
			parameters[0] = new SqlParameter("@namespace", ns);
			parameters[1] = new SqlParameter("@topicName", topicName + "(%)"); // pattern for sql wild card search
			// Sql allow only dates between January 1, 1753 through December 31, 9999
			if( stamp.Year > 1800 )
			{
				parameters[2] = new SqlParameter("@stamp", stamp);
			}
			else
			{
				parameters[2] = new SqlParameter("@stamp", new DateTime(1800,1,1));
			}
		
			using(SqlDataReader reader = SqlDataAccessHelper.ExecuteReader(connectionString, CommandType.StoredProcedure, "GetSqlTopicArchiveInfosSince", parameters) )
			{
				while(reader.Read())
				{
					SqlInfoForTopic topicInfo = new SqlInfoForTopic(reader.GetString(0), reader.GetDateTime(1));
					topicInfos.Add(topicInfo);
				}
			}
			
			return (SqlInfoForTopic[])topicInfos.ToArray(typeof(SqlInfoForTopic));
		}

		/// <summary>
		/// Get all the non archive topics in the specified namespace.
		/// </summary>
		/// <param name="ns">Namespace of the topic.</param>
		/// <param name="sort">Boolean value to indicate if the topics need to be sorted in the 
		/// descending order based on last write time.</param>
		/// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
		/// <returns>Array of SqlTopicInfo containing the information for all non archive topics in the namespace.</returns>
		public static SqlInfoForTopic[] GetSqlTopicInfoForNonArchiveTopics(string ns, bool sort, string connectionString)
		{
			ArrayList topicInfos = new ArrayList();

			SqlParameter[] parameters = new SqlParameter[1];
			parameters[0] = new SqlParameter("@namespace", ns);
		
			string storedProcedureName = "GetSqlTopicInfoForNonArchiveTopics";
			if( sort )
			{
				// To benefit from Sql compiled stored procedure cache using 2 different
				// stored procedures instead of passing the sort value thru to Sql.
				storedProcedureName = "GetSqlTopicInfoForNonArchiveTopicsSortedDescending";	
			}
			using(SqlDataReader reader = SqlDataAccessHelper.ExecuteReader(connectionString, CommandType.StoredProcedure, storedProcedureName, parameters) )
			{
				while(reader.Read())
				{
					SqlInfoForTopic topicInfo = new SqlInfoForTopic(reader.GetString(0), reader.GetDateTime(1));
					topicInfos.Add(topicInfo);
				}
			}
			
			return (SqlInfoForTopic[])topicInfos.ToArray(typeof(SqlInfoForTopic));
		}

		/// <summary>
		/// Get the latest version for the specified topic.
		/// </summary>
		/// <param name="ns">Namespace of the topic.</param>
		/// <param name="topicName">Name of the topic.</param>
		/// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
		/// <returns>SqlTopicInfo containing the topic information.</returns>
		public static SqlInfoForTopic GetSqlTopicInfoForLatestTopicVersion(string ns, string topicName, string connectionString)
		{
			SqlInfoForTopic topicInfo = null;

			SqlParameter[] parameters = new SqlParameter[2];
			parameters[0] = new SqlParameter("@namespace", ns);
			parameters[1] = new SqlParameter("@topicName", topicName + "(%)"); // pattern for sql wild card search

			using(SqlDataReader reader = SqlDataAccessHelper.ExecuteReader(connectionString, CommandType.StoredProcedure, "GetSqlTopicInfoForLatestVersion", parameters) )
			{
				while(reader.Read())
				{
					topicInfo = new SqlInfoForTopic(reader.GetString(0), reader.GetDateTime(1));
				}
			}
			
			return topicInfo;
		}

		/// <summary>
		/// Delete a specified topic.
		/// </summary>
		/// <param name="ns">Namespace of the topic.</param>
		/// <param name="topicName">Name of the topic being deleted.</param>
		/// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
		public static void DeleteTopic(string ns, string topicName, string connectionString)
		{
			SqlParameter[] parameters = new SqlParameter[2];
			parameters[0] = new SqlParameter("@namespace", ns);
			parameters[1] = new SqlParameter("@topicName", topicName);
		
			SqlDataAccessHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "DeleteTopic", parameters);
		}

		/// <summary>
		/// Store the specified topic in the database.
		/// </summary>
		/// <param name="ns">Namespace of the topic.</param>
		/// <param name="topicName">Name of the topic being saved.</param>
		/// <param name="lastWriteTime">The write time for the topic.</param>
		/// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
		/// <param name="body">The contents of the topic being saved.</param>
		/// <param name="archive">Indicates if this is the archive record for the topic being saved.</param>
		public static void WriteTopic(string ns, string topicName, DateTime lastWriteTime, string connectionString, string body, bool archive)
		{
			SqlParameter[] parameters = new SqlParameter[5];
			parameters[0] = new SqlParameter("@namespace", ns);
			parameters[1] = new SqlParameter("@topicName", topicName);
			parameters[2] = new SqlParameter("@body", body);
			parameters[3] = new SqlParameter("@archive", archive);
			parameters[4] = new SqlParameter("@lastWriteTime", lastWriteTime);
		
			SqlDataAccessHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "WriteTopic", parameters);
		}


		/// <summary>
		/// Rename the specified topic with the new name.
		/// </summary>
		/// <param name="ns">Namespace of the topic.</param>
		/// <param name="topicCurrentName">Current Name of the topic</param>
		/// <param name="topicNewName">New Name for the topic.</param>
		/// <param name="connectionString">Database Connectionstring to use for this namespace.</param>
		public static void RenameTopic(string ns, string topicCurrentName, string topicNewName, string connectionString)
		{
			SqlParameter[] parameters = new SqlParameter[3];
			parameters[0] = new SqlParameter("@namespace", ns);
			parameters[1] = new SqlParameter("@topicCurrentName", topicCurrentName);
			parameters[2] = new SqlParameter("@topicNewName", topicNewName);
			
			SqlDataAccessHelper.ExecuteNonQuery(connectionString, CommandType.StoredProcedure, "RenameTopic", parameters);
		}

	}
}
