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
using System.Data;
using System.IO;

namespace FlexWikiSecurity
{
	/// <summary>
	/// Summary description for SimplePermissionsDataProvider.
	/// </summary>
	public class SimplePermissionsDataProvider : IPermissionsDataProvider
	{
		string xmlFileName = "Permissions.Config";
		/// <summary>
		/// Default constructor
		/// </summary>
		public SimplePermissionsDataProvider()
		{
		}
		/// <summary>
		/// Set the Connection String which ends up being the path including the file of the Permissions DataSet
		/// </summary>
		/// <param name="connection"></param>
		public void SetConnection(string connection)
		{
			xmlFileName = connection;
		}
		/// <summary>
		/// Get the permissions for a given NameSpace and User ID combination
		/// </summary>
		/// <param name="nameSpace">NameSpace</param>
		/// <param name="userID">ID of the User</param>
		/// <returns>Permissions DataSet</returns>
		public DataSet GetPermissions(string nameSpace,int userID)
		{
			DataSet dataSet = LoadPermissionsDataSet();
			// Filter data by Namespace and User
			DataRow[] rows = dataSet.Tables["Permissions"].Select("NameSpace<>'" + nameSpace + "'");
			foreach(DataRow row in rows)
			{
				// Permissions with Empty Namespace apply to all namespaces so do not delete them.
				if ((string)row["NameSpace"]!="")
				{
					row.Delete();
				}
			}
			rows = dataSet.Tables["Permissions"].Select("UserId<>" + userID);
			foreach(DataRow row in rows)
			{
				row.Delete();
			}
			dataSet.AcceptChanges();
			return dataSet;
		}
		/// <summary>
		/// Get all permissions for a given user
		/// </summary>
		/// <param name="userID">ID of User</param>
		/// <returns>Permissions DataSet</returns>
		public DataSet GetPermissions(int userID)
		{
			DataSet dataSet = LoadPermissionsDataSet();
			// Filter data by Namespace and User
			DataRow[] rows = dataSet.Tables["Permissions"].Select("UserId<>" + userID);
			foreach(DataRow row in rows)
			{
				row.Delete();
			}
			dataSet.AcceptChanges();
			return dataSet;
		}
		/// <summary>
		/// Add a specific Permission to a given User
		/// </summary>
		/// <param name="nameSpace">NameSpace</param>
		/// <param name="roleID">ID of Role</param>
		/// <param name="userID">ID of User</param>
		/// <returns>The number of rows affected</returns>
		public int AddPermission(string nameSpace,int roleID,int userID)
		{
			DataSet dataSet = LoadPermissionsDataSet();

			// Verify the permission does not already exist
			DataRow[] rows = dataSet.Tables["Permissions"].Select("Namespace='" + nameSpace + "' and RoleID=" + roleID + " and UserID=" + userID);
			if (rows.Length==0)
			{
				// Add the Permissions to the DataSet
				DataRow row;
				row = dataSet.Tables["Permissions"].NewRow();
				row["Namespace"] = nameSpace;
				row["RoleID"] = roleID;
				row["UserID"] = userID;
				dataSet.Tables["Permissions"].Rows.Add(row);

				// Save the DataSet
				dataSet.WriteXml(xmlFileName,System.Data.XmlWriteMode.WriteSchema);
			}
			else
			{
				return 0;
			}

			return 1;
		}
		/// <summary>
		/// Delete a specifc Permission for a given user
		/// </summary>
		/// <param name="nameSpace">NameSpace</param>
		/// <param name="roleID">ID of Role</param>
		/// <param name="userID">ID of User</param>
		/// <returns>Permissions DataSet</returns>
		public int DeletePermission(string nameSpace, int roleID, int userID)
		{
			DataSet dataSet = LoadPermissionsDataSet();
			// Verify the Permission does exist
			DataRow[] rows = dataSet.Tables["Permissions"].Select("Namespace='" + nameSpace + "' and RoleID=" + roleID + " and UserID=" + userID);
			if (rows.Length==1)
			{
				// Delete the Permission from the DataSet
				rows[0].Delete();

				// Save the DataSet
				dataSet.WriteXml(xmlFileName,System.Data.XmlWriteMode.WriteSchema);
				return 1;
			}

			return 0;
		}
		private DataSet LoadPermissionsDataSet()
		{
			DataSet dataSet = new DataSet("Permissions");
			// If the dataset won't load then create a new one
			if (File.Exists(xmlFileName))
			{
				dataSet.ReadXml(xmlFileName,System.Data.XmlReadMode.ReadSchema);
			}
			else
			{
				DataTable dataTable = new DataTable("Permissions");
				DataColumn namespaceColumn = new DataColumn("NameSpace",System.Type.GetType("System.String"));
				DataColumn userIDColumn = new DataColumn("UserID",System.Type.GetType("System.Int32"));
				DataColumn roleIDColumn = new DataColumn("RoleID",System.Type.GetType("System.Int32"));
				dataTable.Columns.Add(namespaceColumn);
				dataTable.Columns.Add(userIDColumn);
				dataTable.Columns.Add(roleIDColumn);
				dataSet.Tables.Add(dataTable);
			}
			return dataSet;
		}

	}
}
