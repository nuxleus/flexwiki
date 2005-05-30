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
	/// Summary description for SimpleRoleDataProviderTests.
	/// </summary>
	[TestFixture]
	public class SimpleRoleDataProviderTests
	{
		[SetUp]
		public void Setup()
		{
      TestUtilities.WriteConfigFile(); 
		}
		[Test]
		public void SimpleRoleDataProviderGetRolesTest()
		{
			IRoleDataProvider iRole = DataProviderFactory.GetRoleProvider("FlexWikiSecurity","FlexWikiSecurity.SimpleRoleDataProvider");
			DataSet roleDataSet = iRole.GetRoles();
			Assert.IsTrue(roleDataSet.Tables["Roles"].Rows.Count==4,"Row Count is wrong");
		}
		[Test]
		public void SimpleRoleDataProviderGetConfiguredDataProvideTest()
		{
			IRoleDataProvider iRole = DataProviderFactory.GetRoleProvider();
			Assert.IsNotNull(iRole,"iRole is null");
		}
		[Test]
		public void SimpleRoleDataProviderGetRoleTest()
		{
			IRoleDataProvider iRole = DataProviderFactory.GetRoleProvider("FlexWikiSecurity","FlexWikiSecurity.SimpleRoleDataProvider");
			DataSet roleDataSet = iRole.GetRole(2);
			Assert.IsTrue((int)roleDataSet.Tables["Roles"].Rows[0]["RoleID"]==2,"Role Id not found");
		}
	}
}
