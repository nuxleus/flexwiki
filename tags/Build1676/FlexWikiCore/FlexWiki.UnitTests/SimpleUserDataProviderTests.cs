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
using NUnit.Framework;
using FlexWikiSecurity;
using System.IO;


namespace FlexWiki.UnitTests
{
	/// <summary>
	/// Summary description for SimpleUserDataProviderTests.
	/// </summary>
	[TestFixture]
	public class SimpleUserDataProviderTests
	{
		[SetUp]
		public void Setup()
		{
      TestUtilities.WriteConfigFile(); 
      TestUtilities.WriteUserFile(); 

#if false
			DataSet dataSet = new DataSet();
			// Get the test data and save it off since these tests overwrite the data
			dataSet.ReadXml("..\\..\\TestUser.xml",System.Data.XmlReadMode.ReadSchema);
			dataSet.WriteXml("UserData.config",System.Data.XmlWriteMode.WriteSchema);
#endif
		}
		[Test]
		public void GetSimpleUserDataProviderInstance()
		{
			// This tests to see if we can get an instance of the User Data Provider
			IUserDataProvider iUser = DataProviderFactory.GetUserProvider("FlexWikiSecurity","FlexWikiSecurity.SimpleUserDataProvider","UserData.config");
			Assert.IsNotNull(iUser,"Object is null");
		}
		[Test]
		public void GetConfiguredUserDataProviderInstance()
		{
			// This tests to see if we can get an instance of the User Data Provider
			IUserDataProvider iUser = DataProviderFactory.GetUserProvider();
			Assert.IsNotNull(iUser,"Object is null");
		}
		[Test]
		public void SimpleUserDataProviderGetUsersTest()
		{
			IUserDataProvider iUser = DataProviderFactory.GetUserProvider("FlexWikiSecurity","FlexWikiSecurity.SimpleUserDataProvider","UserData.config");
			iUser.SetConnection("UserData.config");
			DataSet userDataSet = iUser.GetUsers();
			Assert.IsTrue(userDataSet.Tables["Users"].Rows.Count>5,"Row Count is wrong");
		}
		[Test]
		public void SimpleUserDataProviderGetAUserByEmailTest()
		{
			IUserDataProvider iUser = DataProviderFactory.GetUserProvider("FlexWikiSecurity","FlexWikiSecurity.SimpleUserDataProvider","UserData.config");
			iUser.SetConnection("UserData.config");
			DataSet userDataSet = iUser.GetUser("test@cox.net");
			Assert.IsTrue(userDataSet.Tables["Users"].Rows.Count==1,"User was not found");
		}
		[Test]
		public void SimpleUserDataProviderGetAUserByIDTest()
		{
			IUserDataProvider iUser = DataProviderFactory.GetUserProvider("FlexWikiSecurity","FlexWikiSecurity.SimpleUserDataProvider","UserData.config");
			iUser.SetConnection("UserData.config");
			DataSet userDataSet = iUser.GetUser(1);
			Assert.IsTrue(userDataSet.Tables["Users"].Rows.Count==1,"User was not found");
		}
		[Test]
		public void SimpleUserDataProviderAddUserTest()
		{
			IUserDataProvider iUser = DataProviderFactory.GetUserProvider("FlexWikiSecurity","FlexWikiSecurity.SimpleUserDataProvider","UserData.config");
			iUser.SetConnection("UserData.config");
			long id = iUser.AddUser("test user","test@cox.net","password");
		}
		[Test]
		public void SimpleUserDataProviderAddUserThatAlreadyExistTest()
		{
			IUserDataProvider iUser = DataProviderFactory.GetUserProvider("FlexWikiSecurity","FlexWikiSecurity.SimpleUserDataProvider","UserData.config");
			iUser.SetConnection("UserData.config");
			if (iUser.AddUser("test user","test@cox.net","password")!=0)
			{
				Assert.Fail("Failed to recognize the user was not added");
			}
		}
		[Test]
		public void SimpleUserDataProviderDeleteUserTest()
		{
			IUserDataProvider iUser = DataProviderFactory.GetUserProvider("FlexWikiSecurity","FlexWikiSecurity.SimpleUserDataProvider","UserData.config");
			iUser.SetConnection("UserData.config");
			iUser.DeleteUser(1);
		}
		[Test]
		public void SimpleUserDataProviderDeleteUserThatDoesNotExistTest()
		{
			IUserDataProvider iUser = DataProviderFactory.GetUserProvider("FlexWikiSecurity","FlexWikiSecurity.SimpleUserDataProvider","UserData.config");
			iUser.SetConnection("UserData.config");
			if (iUser.DeleteUser(99)!=0)
			{
				Assert.Fail("Failed to recognize the user does not exist");
			}
		}
		[Test]
		public void SimpleUserDataProviderUpdateUserTest()
		{
			IUserDataProvider iUser = DataProviderFactory.GetUserProvider("FlexWikiSecurity","FlexWikiSecurity.SimpleUserDataProvider","UserData.config");
			iUser.SetConnection("UserData.config");
			if (iUser.UpdateUser(1,"test user","test@cox.com","test2")!=1)
			{
				Assert.Fail("Failed to recognize the user was updated");
			}
		}
		[Test]
		public void SimpleUserDataProviderUpdateUserThatDoesNotExistTest()
		{
			IUserDataProvider iUser = DataProviderFactory.GetUserProvider("FlexWikiSecurity","FlexWikiSecurity.SimpleUserDataProvider","UserData.config");
			iUser.SetConnection("UserData.config");
			if (iUser.UpdateUser(99,"test user","test@cox.com","test2")!=0)
			{
				Assert.Fail("Failed to recognize the User does not exist");
			}
		}

	}
}
