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
	/// Interface to the User Data Provider.
	/// </summary>
	public interface IUserDataProvider
	{
		/// <summary>
		/// Get all Users stored in the Data Provider
		/// </summary>
		/// <returns>User Dataset</returns>
		DataSet GetUsers();
		/// <summary>
		/// Get a specific user by User ID.  If the user is not found the data set is still 
		/// returned but there will be no rows returned
		/// </summary>
		/// <param name="userID">ID of Specific User to load</param>
		/// <returns>User Dataset</returns>
		DataSet GetUser(long userID);
		/// <summary>
		/// Get a specific user by email.  If the user is not found the data set is still 
		/// returned but there will be no rows returned
		/// </summary>
		/// <param name="email">Email of Specific User to load</param>
		/// <returns>User DataSet</returns>
		DataSet GetUser(string email);
		/// <summary>
		/// Adds a user to the DataProvider repository
		/// </summary>
		/// <param name="name">Full Name of the user</param>
		/// <param name="email">Email address of the user</param>
		/// <param name="password">Password for the user</param>
		/// <returns>Number of users addedd. 0=no users added, 1=a single user added</returns>
		long AddUser(string name, string email, string password);
		/// <summary>
		/// Update the user to the DataProvider repository
		/// </summary>
		/// <param name="userID">User to update</param>
		/// <param name="name">Fule Name</param>
		/// <param name="email">Email address</param>
		/// <param name="password">Password</param>
		/// <returns>The unique ID of the new user. 0=if no user was added</returns>
		int UpdateUser(long userID, string name, string email, string password);
		/// <summary>
		/// Deletes a specific user from the DataProvider repository
		/// </summary>
		/// <param name="userID">ID of user to delete</param>
		/// <returns>Number of users deleted. 0=no users deleted, 1=a single user was deleted</returns>
		int DeleteUser(long userID);
		/// <summary>
		/// Sets the connection string for the DataProvider
		/// </summary>
		/// <param name="connection">connection string to set</param>
		void SetConnection(string connection);
	}
}
