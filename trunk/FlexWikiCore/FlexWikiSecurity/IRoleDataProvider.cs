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

namespace FlexWikiSecurity
{
	/// <summary>
	/// Interface to the Role Data Provider
	/// </summary>
	public interface IRoleDataProvider
	{
		/// <summary>
		/// Returns all Roles available for the DataProvider
		/// </summary>
		/// <returns>Role DataSet</returns>
		DataSet GetRoles();
		/// <summary>
		/// Gets a specific role
		/// </summary>
		/// <param name="roleID">ID of the Role</param>
		/// <returns>Role DataSet, if the Role is not found the DataSet is stillreturned but the number of rows is 0</returns>
		DataSet GetRole(int roleID);
		/// <summary>
		/// Add a new Role
		/// </summary>
		/// <param name="roleID">ID of the Role</param>
		/// <param name="roleName">Name of the Role</param>
		/// <returns>Number of Roles added, 0=no roles added, 1=a single role was added</returns>
		int AddRole(int roleID, string roleName);
		/// <summary>
		/// Deletes a specific Role
		/// </summary>
		/// <param name="roleID">ID of the Role</param>
		/// <returns>The number of Roles deleted</returns>
		int DeleteRole(int roleID);
	}
}
