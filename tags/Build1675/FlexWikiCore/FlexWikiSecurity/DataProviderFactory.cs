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
using System.Reflection;

namespace FlexWikiSecurity
{
	/// <summary>
	/// Data Provider Factory allows a plug in architecture for security data providers
	/// </summary>
	public class DataProviderFactory
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public DataProviderFactory()
		{
		}
		/// <summary>
		/// Get the Role Provider that is currently configured
		/// </summary>
		/// <returns>Interface Role Data Provider</returns>
		public static IRoleDataProvider GetRoleProvider()
		{
			// Gets the currently configured provider and uses it
			string assyName = System.Configuration.ConfigurationSettings.AppSettings["RoleDataProvider"];
			string className = System.Configuration.ConfigurationSettings.AppSettings["RoleDataProviderClass"];

			//TODO: Change this to be config driven
			
			return GetRoleProvider(assyName,className);
		}
		/// <summary>
		/// Get a specific Role Provider
		/// </summary>
		/// <param name="assyName">The Assembly Name of the Role Provider</param>
		/// <param name="className">The Class Name of the Role Provider</param>
		/// <returns>Interface Role Data Provider</returns>
		public static IRoleDataProvider GetRoleProvider(string assyName,string className)
		{
			IRoleDataProvider iRole=null;
			Type ObjType = GetAssemblyType(assyName,className);
			if (ObjType != null)
			{
				iRole = (IRoleDataProvider)Activator.CreateInstance(ObjType);
			
			}
			return iRole;
		}
		/// <summary>
		/// Get the User Provider that is currently configured
		/// </summary>
		/// <returns>Interface User Data Provider</returns>
		public static IUserDataProvider GetUserProvider()
		{
			// Gets the currently configured provider and uses it
            string assyName = System.Configuration.ConfigurationSettings.AppSettings["UserDataProvider"];
			string className = System.Configuration.ConfigurationSettings.AppSettings["UserDataProviderClass"];
			string connection = System.Configuration.ConfigurationSettings.AppSettings["UserDataProviderConnection"];

			return GetUserProvider(assyName,className,connection);
		}
		/// <summary>
		/// Get a specific User Provider
		/// </summary>
		/// <param name="assyName">The Assembly Name of the User Provider</param>
		/// <param name="className">The Class Name of the User Provider</param>
		/// <param name="connection">The Connection string of the User Provider</param>
		/// <returns>Interface User Data Provider</returns>
		public static IUserDataProvider GetUserProvider(string assyName,string className,string connection)
		{
			IUserDataProvider iUser=null;
			Type ObjType = GetAssemblyType(assyName,className);
			if (ObjType != null)
			{
				iUser = (IUserDataProvider)Activator.CreateInstance(ObjType);
				iUser.SetConnection(connection);
			}
			return iUser;
		}
		/// <summary>
		/// Get the Permissions Provider that is currently configured
		/// </summary>
		/// <returns>Interface Permissions Data Provider</returns>
		public static IPermissionsDataProvider GetPermissionsProvider()
		{
			// Gets the currently configured provider and uses it
			string assyName = System.Configuration.ConfigurationSettings.AppSettings["PermissionsDataProvider"];
			string className = System.Configuration.ConfigurationSettings.AppSettings["PermissionsDataProviderClass"];
			string connection = System.Configuration.ConfigurationSettings.AppSettings["PermissionsDataProviderConnection"];

			return GetPermissionsProvider(assyName,className,connection);
		}
		/// <summary>
		/// Get a specific Provider that is currently configured
		/// </summary>
		/// <param name="assyName">Assembly Name of the Permissions Provider</param>
		/// <param name="className">Class Name of the Permissions Provider</param>
		/// <param name="connection">The connection string for the Permissions Provider</param>
		/// <returns>Interface Permissions Data Provider</returns>
		public static IPermissionsDataProvider GetPermissionsProvider(string assyName,string className,string connection)
		{
			IPermissionsDataProvider iPermissions=null;
			Type ObjType = GetAssemblyType(assyName,className);
			if (ObjType != null)
			{
				iPermissions = (IPermissionsDataProvider)Activator.CreateInstance(ObjType);
				iPermissions.SetConnection(connection);
			}
			return iPermissions;
		}
		private static Type GetAssemblyType(string assyName,string className)
		{
			Assembly assy=null;
			//Type ObjType = null;
			assy = Assembly.Load(assyName);
			if (assy != null)
			{
				return assy.GetType(className);
			}
			return null;
		}
	}
}
