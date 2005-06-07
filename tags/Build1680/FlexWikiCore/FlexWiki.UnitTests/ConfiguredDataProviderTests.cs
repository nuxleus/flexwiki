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
using System.IO;
using NUnit.Framework;
using FlexWikiSecurity;

namespace FlexWiki.UnitTests
{
	/// <summary>
	/// This test fixture verifies the configuration is correct for all DataProviders.
	/// </summary>
	/// 
	[TestFixture]
	public class ConfiguredDataProviderTests
	{
		[SetUp]
		public void Setup()
		{
      TestUtilities.WriteConfigFile(); 
    }
		[Test]
		public void GetRoleDataProviderInstance()
		{
			// This tests to see if we can get an instance of the Role Data Provider
			IRoleDataProvider iRole = DataProviderFactory.GetRoleProvider();
			Assert.IsNotNull(iRole,"Object is null");
		}
		[Test]
		public void GetUserDataProviderInstance()
		{
			// This tests to see if we can get an instance of the User Data Provider
			IUserDataProvider iUser = DataProviderFactory.GetUserProvider();
			Assert.IsNotNull(iUser,"Object is null");
		}
		[Test]
		public void GetPermissionsDataProviderInstance()
		{
			// This tests to see if we can get an instance of the Permissions Data Provider
			IPermissionsDataProvider iPermissions = DataProviderFactory.GetPermissionsProvider();
			Assert.IsNotNull(iPermissions,"Object is null");
		}
	}
}
