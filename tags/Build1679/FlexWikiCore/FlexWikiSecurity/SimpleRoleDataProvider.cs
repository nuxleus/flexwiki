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
using FlexWikiSecurity;

namespace FlexWikiSecurity
{
	/// <summary>
	/// This Role Provider has a fixed list of roles and does not persist the roles anywhere.
	/// Therefore the Add and Delete method does nothing to the list of available roles.
	/// </summary>
	public class SimpleRoleDataProvider : IRoleDataProvider
	{
		/// <summary>
		/// Default Constructor
		/// </summary>
		public SimpleRoleDataProvider() 
		{
		}
		/// <summary>
		/// Returns the fixed set of available roles
		/// </summary>
		/// <returns>DataSet of available roles in the Roles table</returns>
		public DataSet GetRoles()
		{
			DataSet dataSet = GetRoleDataSet();
			dataSet = PopulateData(dataSet);

			return dataSet;
		}
		/// <summary>
		/// Get a specific Role
		/// </summary>
		/// <param name="roleID">ID of Role</param>
		/// <returns>Role DataSet</returns>
		public DataSet GetRole(int roleID)
		{
			DataSet sourceDataSet = PopulateData(GetRoleDataSet());
			DataSet returnDataSet = GetRoleDataSet();
			DataRow[] rows = sourceDataSet.Tables["Roles"].Select("RoleID=" + roleID);
			DataRow row = returnDataSet.Tables["Roles"].NewRow();
			row["RoleID"] = rows[0]["RoleID"];
			row["RoleName"] = rows[0]["RoleName"];
			returnDataSet.Tables["Roles"].Rows.Add(row);
			returnDataSet.AcceptChanges();
			return returnDataSet;

		}
		/// <summary>
		/// Is not implimented and will always return -1
		/// </summary>
		/// <param name="id"></param>
		/// <param name="name"></param>
		/// <returns></returns>
		public int AddRole(int id,string name)
		{
			return -1;
		}
		/// <summary>
		/// Is not implimented and will always return -1
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public int DeleteRole(int id)
		{
			return -1;
		}
		private DataSet GetRoleDataSet()
		{
			// Set up the Role DataSet
			DataSet dataSet = new DataSet("RoleDataSet");
			DataTable dataTable = new DataTable("Roles");
			DataColumn roleIdColumn = new DataColumn("RoleID",System.Type.GetType("System.Int32"));
			DataColumn roleNameColumn = new DataColumn("RoleName",System.Type.GetType("System.String"));
			dataTable.Columns.Add(roleIdColumn);
			dataTable.Columns.Add(roleNameColumn);
			dataSet.Tables.Add(dataTable);
			return dataSet;
		}
		private DataSet PopulateData(DataSet dataSet)
		{
			// Get the data
			// Roles are not persisted anywhere in this case so dynamically create them on the fly
			DataRow row = dataSet.Tables["Roles"].NewRow();
			row["RoleID"] = 1;
			row["RoleName"] = "Read";
			dataSet.Tables["Roles"].Rows.Add(row);

			row = dataSet.Tables["Roles"].NewRow();
			row["RoleID"] = 2;
			row["RoleName"] = "Write";
			dataSet.Tables["Roles"].Rows.Add(row);

			row = dataSet.Tables["Roles"].NewRow();
			row["RoleID"] = 3;
			row["RoleName"] = "Delete";
			dataSet.Tables["Roles"].Rows.Add(row);

			row = dataSet.Tables["Roles"].NewRow();
			row["RoleID"] = 4;
			row["RoleName"] = "Administrator";
			dataSet.Tables["Roles"].Rows.Add(row);

			dataSet.AcceptChanges();
			
			return dataSet;
		}
	}
}
