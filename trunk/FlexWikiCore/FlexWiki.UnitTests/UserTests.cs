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
	/// Summary description for UserTests.
	/// </summary>
	[TestFixture]
	public class UserTests
	{
		[SetUp]
		public void Setup()
		{
      TestUtilities.WriteConfigFile();
      TestUtilities.WriteUserFile();
      TestUtilities.WritePermissionsFile(); 

      //CA Removed because assumptions about file locations are incompatible with command-line build  
#if false
			// Copy the web.config over to the nunit directory so config seetings can be loaded
			if (File.Exists("FlexWiki.UnitTests.dll.config"))
			{
				File.SetAttributes("FlexWiki.UnitTests.dll.config",System.IO.FileAttributes.Normal);
			}
			File.Copy("..\\..\\..\\FlexWiki.Web\\web.config","FlexWiki.UnitTests.dll.config",true);

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
		public void GetUserByEmailTest()
		{
			User user = new User("test2@cox.net");
			Assert.AreEqual(2,user.UserID,"User was not found");
		}
		[Test]
		public void GetUserByIDTest()
		{
			User user = new User(2);
			Assert.AreEqual(2,user.UserID,"User was not found");
		}
		[Test]
		public void GetUserByIDUserNotFoundTest()
		{
			User user = new User(99);
			Assert.IsTrue(user.UserID==0,"User was found");
		}
		[Test]
		public void GetUserByEmailuserNotFoundTest()
		{
			User user = new User("testunknownuser@cox.net");
			Assert.IsTrue(user.UserID==0,"User was found");
		}
		[Test]
		public void DeleteUserTest()
		{
			User user = new User(2);
			user.Delete();
			
			// Verify the user was deleted
			user = new User(2);
			Assert.IsTrue(user.UserID==0,"User was found");
		}
		[Test]
		public void UpdateUserTest()
		{
			User user = new User(2);
			user.FullName = "New Name";
			user.Update();
			
			// Verify the user was updated
			user = new User(2);
			Assert.IsTrue(user.FullName=="New Name","User did not get updated");
		}
		[Test]
		public void AddRoleToUserTest()
		{
			User user = new User(2);
			Assert.IsTrue(user.AddToRole("SecureNameSpace",4),"Role was not granted to User");

			// Verify the role was really added
			user = new User(2);
			Assert.IsTrue(user.GetRoles("SecureNameSpace").Tables["Permissions"].Rows.Count==2,"Role was not persisted for the user");
		}
		[Test]
		public void RemoveRoleFromUserTest()
		{
			User user = new User(1);
			Assert.IsTrue(user.RemoveFromRole("SecureNameSpace",3),"Role was not removed from the User");

			// Verify the role was really removed
			user = new User(1);
			Assert.IsTrue(user.GetRoles("SecureNameSpace").Tables["Permissions"].Rows.Count==3,"Role was not persisted for the User");
		}
		[Test]
		public void GetAllUsersTest()
		{
			DataSet dataSet = User.GetUsers();
			Assert.IsTrue(dataSet.Tables["Users"].Rows.Count>5,"Multiple users where not retrieved");
		}
	}
}
