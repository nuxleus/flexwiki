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

namespace FlexWiki.FwSyncLib.Tests
{
  public abstract class SynchronizerTestBase
  {
    protected Synchronizer sync; 
    protected MockEditServiceProxy proxy; 
    protected string dir; 

    protected abstract string TestDirectory { get; }

    [SetUp]
    public virtual void SetUp()
    {
      dir = this.TestDirectory; 
      dir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dir)); 
  
      if (Directory.Exists(dir))
      {
        Directory.Delete(dir, true); 
      }
      Directory.CreateDirectory(dir); 

      Directory.SetCurrentDirectory(dir); 

      proxy = UnitTests.GetMockServiceProxy(); 

      sync = new Synchronizer(dir, proxy); 

      Assert.AreEqual(0, Directory.GetFiles(sync.BaseDirectory).Length, 
        "Checking that there are no local files"); 
      Assert.AreEqual(0, Directory.GetDirectories(sync.BaseDirectory).Length, 
        "Checking that there are no local files"); 
    }

    [TearDown]
    public virtual void TearDown()
    {
      // This is so we don't hold on to the directory we created, preventing
      // others from deleting it
      Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory); 
    }

    
    [Test]
    public virtual void CommitWithConflictIgnored()
    {
      sync.Initialize();
      sync.Update(); 

      string topicname = "TwoTopic";
      LocalTopic theTopic = sync.Namespaces["A"].Topics[topicname]; 

      SetUpConflict(theTopic); 

      SetupAddMultipleLocally(); 
     
      sync.SyncToLocal(); 

      LocalTopic a1 = sync.Namespaces["A"].Topics["FooBar"];
      LocalTopic a2 = sync.Namespaces["A"].Topics["BarFoo"];
      LocalTopic b1 = sync.Namespaces["B"].Topics["FooBar"];

      sync.SyncToLocal(); 
      sync.SyncToRemote(); 

      Assert.AreEqual(Status.InConflict, theTopic.Status, "Checking that status is now 'InConflict'"); 

      sync.Commit("CraigAndera", true); 

      Assert.AreEqual(Status.InConflict, theTopic.Status, 
        "Checking that conflicting file is still InConflict");
      
      int inConflict = 0; 
      int upToDate = 0; 
      int other = 0; 
      foreach (LocalNamespace ns in sync.Namespaces)
      {
        foreach (LocalTopic topic in ns.Topics)
        {
          if (topic.Status == Status.UpToDate)
          {
            ++upToDate; 
          }
          else if (topic.Status == Status.InConflict)
          {
            ++inConflict; 
          }
          else
          {
            ++other;
          }
        }
      }

      Assert.AreEqual(1, inConflict, "Checking that only one file is InConflict");
      Assert.AreEqual(8, upToDate, "Checking that all other files are upToDate");
      Assert.AreEqual(0, other, "Checking that no other files have changed status"); 
    }

    [Test]
    public virtual void UpdateOutOfDate()
    {
      sync.Initialize(); 
      sync.Update(); 
      LocalTopic theTopic = sync.Namespaces["A"].Topics["OneTopic"]; 
      proxy.Populate(theTopic.Namespace.Name, theTopic.Name, "NewVersion", "blah blah blah"); 
      
      sync.SyncToLocal(); 
      sync.SyncToRemote();
      Assert.AreEqual(Status.LocallyOutOfDate, theTopic.Status, "Checking that status is now 'OutOfDate'"); 

      sync.Update(); 
      Assert.AreEqual(Status.UpToDate, theTopic.Status, "Checking that status is now 'UpToDate'"); 

    }



    protected void SetupAddMultipleLocally()
    {
      sync.Initialize();

      UnitTests.CreateTopicFile(sync, "A", "FooBar", "A-FooBar");
      UnitTests.CreateTopicFile(sync, "A", "BarFoo", "A-BarFoo");
      UnitTests.CreateTopicFile(sync, "B", "FooBar", "B-FooBar");

    }

    protected void SetUpConflict(LocalTopic topic)
    {
      string content = "A change at " + DateTime.Now.ToString(); 
      UnitTests.AppendToTopic(topic, content);
      proxy.Populate("A", topic.Name, topic.RepositoryVersion + "PlusOne", content); 
    }

  }
}
