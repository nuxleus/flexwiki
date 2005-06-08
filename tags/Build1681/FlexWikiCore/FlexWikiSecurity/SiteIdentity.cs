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

namespace FlexWikiSecurity
{
	/// <summary>
	/// Summary description for SiteIdentity.
	/// </summary>
	public class SiteIdentity: System.Security.Principal.IIdentity
	{
		private int userID;
		private string fullName;
		private string email;
		private string password;
		
		/// <summary>
		/// Constructor based on email
		/// </summary>
		/// <param name="email">Email</param>
		public SiteIdentity( string email )
		{
			User user = new User(email);

			this.userID = user.UserID;
			this.fullName = user.FullName;
			this.email = user.Email;
			this.password = user.Password;
		}
		/// <summary>
		/// Constructor based on User ID
		/// </summary>
		/// <param name="userID">User ID</param>
		public SiteIdentity( int userID )
		{
			User user = new User(userID);

			this.userID = user.UserID;
			this.fullName = user.FullName;
			this.email = user.Email;
			this.password = user.Password;
		}

		/// <summary>
		/// Read only Property Authentication Type
		/// </summary>
		public string AuthenticationType
		{
			get { return "Custom Authentication"; }
		}

		/// <summary>
		/// Read Only Property
		/// </summary>
		public bool IsAuthenticated
		{
			get 
			{
				// assumption: all instances of a SiteIdentity have already
				// been authenticated.
				return true;
			}
		}
		/// <summary>
		/// Read Only Property User ID
		/// </summary>
		public int UserID
		{
			get { return userID; }
		}
		/// <summary>
		/// Read Only Property Name
		/// </summary>
		public string Name
		{
			get { return fullName; }
		}
		/// <summary>
		/// Read Only Property Email
		/// </summary>
		public string Email
		{
			get { return email; }
		}				
		/// <summary>
		/// Read Only Property Password
		/// </summary>
		public string Password
		{
			get { return password; }
		}
	}
}
