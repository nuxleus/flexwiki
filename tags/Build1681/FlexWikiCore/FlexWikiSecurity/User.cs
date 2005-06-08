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
using System.Collections;

namespace FlexWikiSecurity
{
	/// <summary>
	/// Manage users
	/// </summary>
	public class User
	{
		private int userID=0;
		private string fullName="";
		private string email="";
		private string password="";
		private DateTime dateAdded;

		/// <summary>
		/// Empty constructor
		/// </summary>
		public User()
		{
		}
		/// <summary>
		/// Constructor based on User ID
		/// </summary>
		/// <param name="userID"></param>
		public User(int userID)
		{
			DataSet dataSet = new DataSet();
			// Using configuration determine where to get user information
			IUserDataProvider iUser = DataProviderFactory.GetUserProvider();

			dataSet = iUser.GetUser(userID);
			if (dataSet.Tables["Users"].Rows.Count>0)
			{
				LoadDetails(dataSet.Tables["Users"].Rows[0]);
			}
			dataSet.Dispose();
		}

		/// <summary>
		/// Constructor based on email
		/// </summary>
		/// <param name="email"></param>
		public User(string email)
		{
			DataSet dataSet = new DataSet();
			// Using configuration determine where to get user information
			IUserDataProvider iUser = DataProviderFactory.GetUserProvider();
			dataSet = iUser.GetUser(email);
			if (dataSet.Tables["Users"].Rows.Count>0)
			{
				LoadDetails(dataSet.Tables["Users"].Rows[0]);
			}
			dataSet.Dispose();
		}
		/// <summary>
		/// Property User ID
		/// </summary>
		public int UserID
		{
			get { return userID; }
			set { userID = value; }
		}
		/// <summary>
		/// Property Full Name
		/// </summary>
		public string FullName
		{
			get { return fullName; }
			set { fullName = value; }
		}
		/// <summary>
		/// Property Email
		/// </summary>
		public string Email
		{
			get { return email; }
			set { email = value; }
		}
		/// <summary>
		/// Property password
		/// </summary>
		public string Password
		{
			get { return password; }
			set { password = value; }
		}
		/// <summary>
		/// Property DateTime added
		/// </summary>
		public DateTime DateAdded
		{
			get { return dateAdded; }
			set { dateAdded = value; }
		}
		private void LoadDetails (DataRow user)
		{
			userID = (int)user["UserID"];
			fullName = (string)user["UserName"];
			email = (string)user["UserEmail"];
			password = (string)user["UserPassword"];
			//dateAdded = (DateTime)user["DateAdded"];
		}

		/// <summary>
		/// Static method to get All available users
		/// </summary>
		/// <returns>User DataSet</returns>
		public static DataSet GetUsers()
		{
			DataSet dataSet = new DataSet();

			IUserDataProvider iUser = DataProviderFactory.GetUserProvider();
			dataSet = iUser.GetUsers();

			return dataSet;
		}
		/// <summary>
		/// Static method to get available Roles
		/// </summary>
		/// <param name="nameSpace">NameSpace</param>
		/// <param name="userID">ID of User</param>
		/// <returns>ArrayList of roles</returns>
		public static ArrayList GetUserRoles(string nameSpace,int userID)
		{
			ArrayList roles = new ArrayList();

			IPermissionsDataProvider iPermissions = DataProviderFactory.GetPermissionsProvider();
			DataSet dataSet = iPermissions.GetPermissions(nameSpace,userID);

			// Iterate through the list and add the Role names to an ArrayList
			for(int x=0;x<dataSet.Tables["Permissions"].Rows.Count;x++)
			{
				roles.Add(dataSet.Tables["Permissions"].Rows[x]["RoleID"]);
			}
			return roles;
		}
		/// <summary>
		/// Get permissions for the current user
		/// </summary>
		/// <returns>Permissions DataSet</returns>
		public DataSet GetRoles()
		{
			DataSet dataSet = new DataSet();

			IPermissionsDataProvider iPermissions = DataProviderFactory.GetPermissionsProvider();
			dataSet = iPermissions.GetPermissions(userID);

			return dataSet;
		}
		/// <summary>
		/// Get permissions for a the current user and NameSpace combination
		/// </summary>
		/// <param name="nameSpace">NameSpace</param>
		/// <returns>Permissions DataSet</returns>
		public DataSet GetRoles(string nameSpace)
		{
			DataSet dataSet = new DataSet();

			IPermissionsDataProvider iPermissions = DataProviderFactory.GetPermissionsProvider();
			dataSet = iPermissions.GetPermissions(nameSpace,userID);

			return dataSet;
		}
        /// <summary>
        /// Add the specific User
        /// </summary>
        /// <returns>The unique ID of the new user</returns>
		public long Add()
		{
			IUserDataProvider iUser = DataProviderFactory.GetUserProvider();
			long id = iUser.AddUser(this.fullName,this.email,this.password);
			return id;
		}
		/// <summary>
		/// Persist the current user 
		/// </summary>
		/// <returns>Update was successfull</returns>
		public bool Update()
		{
			int rowsAffected=-1;
			IUserDataProvider iUser = DataProviderFactory.GetUserProvider();
			rowsAffected = iUser.UpdateUser(this.userID,this.fullName,this.email,this.password);
			return (rowsAffected == 1);
		}
		/// <summary>
		/// Delete the current user 
		/// </summary>
		/// <returns>Delete was successfull</returns>
		public bool Delete()
		{
			int rowsAffected=-1;
			IUserDataProvider iUser = DataProviderFactory.GetUserProvider();

			rowsAffected = iUser.DeleteUser(userID);

			return (rowsAffected==1);
		}
		/// <summary>
		/// Add a Permission to the current user
		/// </summary>
		/// <param name="nameSpace">NameSpace</param>
		/// <param name="roleID">ID of Role</param>
		/// <returns>Successfull</returns>
		public bool AddToRole(string nameSpace,int roleID)
		{
			int rowsAffected=-1;
			// Using configuration determine where to get user information
			IPermissionsDataProvider iPermissions = DataProviderFactory.GetPermissionsProvider();

			rowsAffected = iPermissions.AddPermission(nameSpace,roleID, this.userID);

			return (rowsAffected == 1);
		}
		/// <summary>
		///	Remove a Permission from the current user
		/// </summary>
		/// <param name="nameSpace">nameSpace</param>
		/// <param name="roleID">ID of Role</param>
		/// <returns>Successfull</returns>
		public bool RemoveFromRole(string nameSpace,int roleID)
		{
			int rowsAffected=-1;
			// Using configuration determine where to get user information
			IPermissionsDataProvider iPermissions = DataProviderFactory.GetPermissionsProvider();

			rowsAffected = iPermissions.DeletePermission(nameSpace,roleID, this.userID);

			return (rowsAffected == 1);	
		}

	}
}
