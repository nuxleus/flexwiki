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

namespace FlexWikiSecurity
{
	/// <summary>
	/// Flexwiki SitePrincipal.
	/// </summary>
	public class SitePrincipal: System.Security.Principal.IPrincipal
	{
		/// <summary>
		/// Protected identity
		/// </summary>
		protected System.Security.Principal.IIdentity identity;
		/// <summary>
		/// Protected collection of available roles. A List of RoleIDs.
		/// </summary>
		protected ArrayList roleList;
		/// <summary>
		/// Protected namespace used to filter the roles
		/// </summary>
		protected string nameSpace="";
		/// <summary>
		/// Constructor based on email
		/// </summary>
		/// <param name="email">Email</param>
		public SitePrincipal(string email )
		{		
			identity = new SiteIdentity(email);
		}
		/// <summary>
		/// Constructor based on User ID
		/// </summary>
		/// <param name="userID">User ID</param>
		public SitePrincipal(int userID )
		{	
			identity = new SiteIdentity(userID);
		}
		/// <summary>
		/// Constructor based on Namespace and Email
		/// </summary>
		/// <param name="nameSpace">NameSpace</param>
		/// <param name="email">Email</param>
		public SitePrincipal( string nameSpace,string email )
		{		
			identity = new SiteIdentity(email);
			roleList = User.GetUserRoles(nameSpace, ((SiteIdentity)identity).UserID );
		}
		/// <summary>
		/// Constructor based on NameSpace User ID
		/// </summary>
		/// <param name="nameSpace">NameSpace</param>
		/// <param name="userID">ID of User</param>
		public SitePrincipal( string nameSpace,int userID )
		{	
			identity = new SiteIdentity(userID);
			roleList = User.GetUserRoles(nameSpace,((SiteIdentity)identity).UserID );
		}

		/// <summary>
		/// Validates the passed in email and password combination exists in the repository
		/// </summary>
		/// <param name="email">email address</param>
		/// <param name="password">password</param>
		/// <param name="nameSpace">Valid FlexWiki NameSpace</param>
		/// <returns>SitePrincipal object or null if the user does not validate</returns>
		public static SitePrincipal ValidateLogin(string nameSpace,string email, string password)
		{
			int userID=0;
			User user = new User(email);
			// validate password
			if (password == user.Password)
			{
				userID = user.UserID;
			}
			return (userID == 0 ? null : new SitePrincipal(nameSpace,userID));
		}
		/// <summary>
		/// Property access to User Identity
		/// </summary>
		public System.Security.Principal.IIdentity Identity
		{
			get { return identity; }
			set { identity = value; }
		}
		/// <summary>
		/// Property access to the Namespace to filter the roles by
		/// </summary>
		public string NameSpace
		{
			get{return nameSpace;}
			set
			{
				nameSpace=value;
				roleList = User.GetUserRoles(nameSpace,((SiteIdentity)identity).UserID );
			}
		}
		/// <summary>
		/// Read Only property of user roles
		/// </summary>
		public ArrayList Roles
		{
			get { return roleList; }
		}
		/// <summary>
		/// Determines if the user is in a given role
		/// </summary>
		/// <param name="role">Valid role</param>
		/// <returns>True if role is found</returns>
		public bool IsInRole(string role)
		{
			return roleList.Contains( Convert.ToInt32(role) );
		}


	}
}
