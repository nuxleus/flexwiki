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
using System.Data;
using NUnit.Framework;
using FlexWikiSecurity;


namespace FlexWiki.UnitTests
{
	/// <summary>
	/// Summary description for RoleTests.
	/// </summary>
	[TestFixture]
	public class RoleTests
	{
		[SetUp]
		public void Setup()
		{
      TestUtilities.WriteConfigFile();
			// Do any initializing of the test data here
		}
		[Test]
		public void GetRoleByIDTest()
		{
			Role role = new Role(2);
			Assert.AreSame("Write",role.RoleName);
		}
		[Test]
		public void GetRolesTest()
		{
			DataSet dataSet = Role.GetRoles();
			Assert.AreEqual(4,dataSet.Tables["Roles"].Rows.Count,"Number of roles found is wrong");
		}
	}
}
