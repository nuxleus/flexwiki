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
	/// Summary description for SimplePermissionsDataProviderTests.
	/// </summary>
	[TestFixture]
	public class SimplePermissionsDataProviderTests
	{
		[SetUp]
		public void Setup()
		{
      TestUtilities.WriteConfigFile(); 
      TestUtilities.WritePermissionsFile(); 

#if false
			DataSet dataSet = new DataSet();
			// Get the test data and save it off since these tests overwrite the data
			dataSet.ReadXml("..\\..\\TestPermissions.xml",System.Data.XmlReadMode.ReadSchema);
			dataSet.WriteXml("PermissionsData.config",System.Data.XmlWriteMode.WriteSchema);
#endif

		}	
		[Test]
		public void GetSimplePermissionsDataProviderInstance()
		{
			// This tests to see if we can get an instance of the Permissions Data Provider
			IPermissionsDataProvider iPermissions = DataProviderFactory.GetPermissionsProvider("FlexWikiSecurity","FlexWikiSecurity.SimplePermissionsDataProvider","PermissionsData.config");
			iPermissions.SetConnection("PermissionsData.config");
			Assert.IsNotNull(iPermissions,"Object is null");
		}
		[Test]
		public void GetConfiguredPermissionsDataProviderInstance()
		{
			// This tests to see if we can get an instance of the Permissions Data Provider
			IPermissionsDataProvider iPermissions = DataProviderFactory.GetPermissionsProvider();
			Assert.IsNotNull(iPermissions,"Object is null");
		}
		[Test]
		public void SimplePermissionsDataProviderGetPermissionsTest()
		{
			IPermissionsDataProvider iPermissions = DataProviderFactory.GetPermissionsProvider("FlexWikiSecurity","FlexWikiSecurity.SimplePermissionsDataProvider","PermissionsData.config");
			iPermissions.SetConnection("PermissionsData.config");
			DataSet permissionsDataSet = iPermissions.GetPermissions("SecureNameSpace",1);
			Assert.IsTrue(permissionsDataSet.Tables["Permissions"].Rows.Count==4,"Number of rows found was incorrect");
		}
		[Test]
		public void SimplePermissionsDataProviderAddPermissionsTest()
		{
			IPermissionsDataProvider iPermissions = DataProviderFactory.GetPermissionsProvider("FlexWikiSecurity","FlexWikiSecurity.SimplePermissionsDataProvider","PermissionsData.config");
			iPermissions.SetConnection("PermissionsData.config");
			if (iPermissions.AddPermission("SecureNameSpace",2,2)!=1)
			{
				Assert.Fail("Record not added");
			}
		}
		[Test]
		public void SimplePermissionsDataProviderAddPermissionsThatAlreadyExistTest()
		{
			IPermissionsDataProvider iPermissions = DataProviderFactory.GetPermissionsProvider("FlexWikiSecurity","FlexWikiSecurity.SimplePermissionsDataProvider","PermissionsData.config");
			iPermissions.SetConnection("PermissionsData.config");
			if (iPermissions.AddPermission("SecureNameSpace",1,1)!=0)
			{
				Assert.Fail("Record was added and it shouldn't have been");
			}
		}
		[Test]
		public void SimplePermissionsDataProviderDeletePermissionsTest()
		{
			IPermissionsDataProvider iPermissions = DataProviderFactory.GetPermissionsProvider("FlexWikiSecurity","FlexWikiSecurity.SimplePermissionsDataProvider","PermissionsData.config");
			iPermissions.SetConnection("PermissionsData.config");
			if (iPermissions.DeletePermission("SecureNameSpace",4,1)!=1)
			{
				Assert.Fail("Record was not deleted");
			}
		}
		[Test]
		public void SimplePermissionsDataProviderDeletePermissionsThatDoesNotExistTest()
		{
			IPermissionsDataProvider iPermissions = DataProviderFactory.GetPermissionsProvider("FlexWikiSecurity","FlexWikiSecurity.SimplePermissionsDataProvider","PermissionsData.config");
			iPermissions.SetConnection("PermissionsData.config");
			if (iPermissions.DeletePermission("SecureNameSpace",1,50)!=0)
			{
				Assert.Fail("Record was deleted and it shouldn't have been");
			}
		}
	}
}
