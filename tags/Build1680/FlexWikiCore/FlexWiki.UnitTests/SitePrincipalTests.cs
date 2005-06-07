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
using System.IO;
using System.Collections;
using System.Data;
using NUnit.Framework;
using FlexWikiSecurity;

namespace FlexWiki.UnitTests
{
	/// <summary>
	/// Summary description for SiteIdentityTests.
	/// </summary>
	[TestFixture]
	public class SitePrincipalTests
	{
		[SetUp]
		public void Setup()
		{
      TestUtilities.WriteConfigFile(); 
      TestUtilities.WriteUserFile();
      TestUtilities.WritePermissionsFile(); 

#if false
			// Do any initializing of the test data here
			DataSet dataSet = new DataSet();
			// Get the test data and save it off since these tests overwrite the data
			dataSet.ReadXml("..\\..\\TestUser.xml",System.Data.XmlReadMode.ReadSchema);
			dataSet.WriteXml("UserData.config",System.Data.XmlWriteMode.WriteSchema);

			dataSet = new DataSet();
			// Get the test data and save it off since these tests overwrite the data
			dataSet.ReadXml("..\\..\\TestPermissions.xml",System.Data.XmlReadMode.ReadSchema);
			dataSet.WriteXml("PermissionsData.config",System.Data.XmlWriteMode.WriteSchema);
#endif

		}
		[Test]
		public void LoadUserTest()
		{
			// Using the SitePricipal load a user and verify the roles are loaded
			SitePrincipal principal = new SitePrincipal("SecureNameSpace",1);
			Assert.IsTrue(principal.Roles.Count==4,"The number of roles for user 1 is wrong");
		}
		[Test]
		public void ValidateUserTest()
		{
			SitePrincipal principal = SitePrincipal.ValidateLogin("SecureNameSpace","test@cox.net","password");
			Assert.IsNotNull(principal,"User not validated");
		}
		[Test]
		public void ValidateUserUserDoesNotExistTest()
		{
			SitePrincipal principal = SitePrincipal.ValidateLogin("SecureNameSpace","bogususer@cox.net","password");
			Assert.IsNull(principal,"User validated and it shouldn't have");
		}
		[Test]
		public void ValidateUserWrongPasswordTest()
		{
			SitePrincipal principal = SitePrincipal.ValidateLogin("SecureNameSpace","test@cox.net","WrongPassword");
			Assert.IsNull(principal,"User validated and it shouldn't have");
		}
		[Test]
		public void IsInRoleTest()
		{
			// Test to see if user is in a given role
			SitePrincipal principal = new SitePrincipal("SecureNameSpace",1);
			Assert.IsTrue(principal.IsInRole("1"),"Role not found");
		}
		[Test]
		public void RolesTest()
		{
			// Test to see if we can get the roles for a user
			SitePrincipal principal = new SitePrincipal("SecureNameSpace",1);
			ArrayList roles = principal.Roles;
			Assert.IsTrue(roles.Count==4,"Role count is wrong");
		}
	}
}
