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
	/// Interface for Permissions Data Provider.
	/// </summary>
	public interface IPermissionsDataProvider
	{
		/// <summary>
		/// Returns the permissions for a given nameSpace and UserID combination
		/// </summary>
		/// <param name="nameSpace">Namespace</param>
		/// <param name="userID">ID of User permissions requested</param>
		/// <returns>Permissions DataSet</returns>
		DataSet GetPermissions(string nameSpace,int userID);
		/// <summary>
		/// Gets the permissions for the given User 
		/// </summary>
		/// <param name="userID">ID of User</param>
		/// <returns>Permissions DataSet</returns>
		DataSet GetPermissions(int userID);
		/// <summary>
		/// Adds a permission to a specified user
		/// </summary>
		/// <param name="nameSpace">Namespace of the permission</param>
		/// <param name="roleID">ID of the Role</param>
		/// <param name="userID">ID of the User</param>
		/// <returns>Number of rows affected, 0=no rows, 1=a single row</returns>
		int AddPermission(string nameSpace,int roleID,int userID);
		/// <summary>
		/// Deletes a permission assigned to aspecified user
		/// </summary>
		/// <param name="nameSpace">Namespace of the permission</param>
		/// <param name="roleID">ID of the Role</param>
		/// <param name="userID">ID of the User</param>
		/// <returns>Number of rows affected, 0=no rows affected, 1=a single row affected</returns>
		int DeletePermission(string nameSpace, int roleID, int userID);
		/// <summary>
		/// Sets the connection string for the DataProvider
		/// </summary>
		/// <param name="connection">connection string</param>
		void SetConnection(string connection);
	}
}
