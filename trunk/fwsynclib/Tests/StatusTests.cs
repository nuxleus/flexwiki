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

using NUnit.Framework; 

using FlexWiki.FwSyncLib; 

namespace FlexWiki.FwSyncLib.Tests
{
	[TestFixture]
	public class StatusTests
	{
		//		UpToDate,         // The file is identical with the latest revision in the repository.
		//		LocallyModified,  // You have edited the file, and not yet committed your changes.
		//		LocallyAdded,     // You have added the file with `add', and not yet committed your  changes.
		//		NoLocalFile,      // The local file does not exist - it needs to be retrieved from the repository
		//		LocallyOutOfDate, // Someone else has committed a newer revision to the repository.
		//		InConflict        // Someone else has committed a newer revision to the repository, and you have also made modifications to the file.

		private Synchronizer sync; 
		private IEditServiceProxy proxy; 
		private string dir = "test"; 

		[SetUp]
		public void StatusSetup()
		{
			if (Directory.Exists(dir))
			{
				Directory.Delete(dir, true); 
			}
			Directory.CreateDirectory(dir); 
			dir = Path.GetFullPath(dir); 

			proxy = UnitTests.GetMockServiceProxy(); 

			sync = new Synchronizer(dir, proxy); 

			Assert.AreEqual(0, Directory.GetFiles(sync.BaseDirectory).Length, "Checking that there are no local files"); 
			Assert.AreEqual(0, Directory.GetDirectories(sync.BaseDirectory).Length, "Checking that there are no local files"); 

			sync.Initialize(); 
			sync.Update(); 
		}

		[TearDown]
		public void StatusTeardown()
		{
			Directory.Delete(dir, true); 
		}

		[Test]
		public void UpToDateStatusTest()
		{
			foreach (LocalNamespace ns in sync.Namespaces)
			{
				foreach (LocalTopic topic in ns.Topics)
				{
					Assert.AreEqual(Status.UpToDate, topic.Status, "Checking that status is 'Up To Date'"); 
				}
			}
		}

		[Test]
		public void LocallyModifiedStatusTest()
		{
			LocalTopic theTopic = sync.Namespaces["A"].Topics["OneTopic"]; 

			foreach (LocalNamespace ns in sync.Namespaces)
			{
				foreach (LocalTopic topic in ns.Topics)
				{
					Assert.AreEqual(Status.UpToDate, topic.Status, "Checking that status of unmodified file is 'Up To Date'"); 
				}
			}

			File.SetLastWriteTime(theTopic.Path, DateTime.Now + TimeSpan.FromMinutes(1)); 


			foreach (LocalNamespace ns in sync.Namespaces)
			{
				foreach (LocalTopic topic in ns.Topics)
				{
					Assert.AreEqual(Status.UpToDate, topic.Status, "Checking that status of file is 'Up To Date' regardless of timestamp"); 
				}
			}

			UnitTests.AppendToTopic(theTopic, "A change"); 

			foreach (LocalNamespace ns in sync.Namespaces)
			{
				foreach (LocalTopic topic in ns.Topics)
				{
					if (topic == theTopic)
					{
						Assert.AreEqual(Status.LocallyModified, topic.Status, "Checking that status of changed file is 'LocallyModified'");
					}
					else
					{
						Assert.AreEqual(Status.UpToDate, topic.Status, "Checking that status of unmodified file is 'Up To Date'"); 
					}
				}

			}
		}

		[Test]
		public void NoLocalFileStatusTest()
		{
			LocalTopic theTopic = sync.Namespaces["B"].Topics["TwoTopic"];
			File.Delete(theTopic.Path); 

			foreach (LocalNamespace ns in sync.Namespaces)
			{
				foreach (LocalTopic topic in ns.Topics)
				{
					if (topic == theTopic)
					{
						Assert.AreEqual(Status.NoLocalFile, topic.Status, "Checking that status of changed file is 'NoLocalFile'");
					}
					else
					{
						Assert.AreEqual(Status.UpToDate, topic.Status, "Checking that status of unmodified file is 'Up To Date'"); 
					}
				}

			}

		}

		[Test]
		public void LocallyOutOfDateStatusTest()
		{
			LocalTopic theTopic = sync.Namespaces["B"].Topics["TwoTopic"];
			theTopic.RepositoryVersion = theTopic.RepositoryVersion + "New"; 

			foreach (LocalNamespace ns in sync.Namespaces)
			{
				foreach (LocalTopic topic in ns.Topics)
				{
					if (topic == theTopic)
					{
						Assert.AreEqual(Status.LocallyOutOfDate, topic.Status, "Checking that status of changed file is 'LocallyOutOfDate'");
					}
					else
					{
						Assert.AreEqual(Status.UpToDate, topic.Status, "Checking that status of unmodified file is 'Up To Date'"); 
					}
				}

			}

		}

		[Test]
		public void InConflictStatusTest()
		{
			LocalTopic theTopic = sync.Namespaces["B"].Topics["TwoTopic"];
			theTopic.RepositoryVersion = theTopic.RepositoryVersion + "New"; 

			UnitTests.AppendToTopic(theTopic, "A change"); 

			foreach (LocalNamespace ns in sync.Namespaces)
			{
				foreach (LocalTopic topic in ns.Topics)
				{
					if (topic == theTopic)
					{
						Assert.AreEqual(Status.InConflict, topic.Status, "Checking that status of changed file is 'InConflict'");
					}
					else
					{
						Assert.AreEqual(Status.UpToDate, topic.Status, "Checking that status of unmodified file is 'Up To Date'"); 
					}
				}

			}
		}
	}
}