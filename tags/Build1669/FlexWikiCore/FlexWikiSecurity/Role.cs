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

namespace FlexWikiSecurity
{
	/// <summary>
	/// Summary description for Role.
	/// </summary>
	public class Role
	{
		private int roleID;
		private string roleName;
		/// <summary>
		/// Default Constructor
		/// </summary>
		public Role()
		{
		}
		/// <summary>
		/// Constructor based on Role ID
		/// </summary>
		/// <param name="roleID"></param>
		public Role(int roleID)
		{
			// Load the data
			IRoleDataProvider iRole = DataProviderFactory.GetRoleProvider();
			DataSet dataSet = iRole.GetRole(roleID);
			if (dataSet.Tables["Roles"].Rows.Count!=0)
			{
				DataRow row = dataSet.Tables["Roles"].Rows[0];
				roleID = roleID;
				roleName = (string)row["RoleName"];
			}
			else
			{
				throw new Exception("Invalid Role Id");
			}
		}
		/// <summary>
		/// Property Role ID
		/// </summary>
		public int RoleID
		{
			get { return roleID; }
			set { roleID = value; }
		}
		/// <summary>
		/// Property Role Name
		/// </summary>
		public string RoleName
		{
			get { return roleName; }
			set { roleName = value; }
		}
		/// <summary>
		/// Get all roles
		/// </summary>
		/// <returns>DataSet of roles</returns>
		public static DataSet GetRoles()
		{

			IRoleDataProvider iRole = DataProviderFactory.GetRoleProvider();
			return iRole.GetRoles();
		}
	}
}
