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
using System.Collections.Specialized;
using System.Xml; 
using System.Xml.XPath; 
using System.Xml.Serialization;

using NUnit.Framework; 

using FlexWiki.FwSyncLib; 

namespace FlexWiki.FwSyncLib.Tests
{
  [TestFixture]
  public class SynchronizerTests : SynchronizerTestBase
  {
    protected override string TestDirectory
    {
      get { return "SynchronizerTests"; }
    }

    public override void SetUp()
    {
      base.SetUp(); 
    }

    [Test]
    public void DealWithRelativePaths()
    {
      // This one checks for a bug I found where relative path names confuse
      // the engine

      // Set up the wiki directory
      sync.Initialize(); 

      Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory); 

      // Make a new synchronizer that's pointed at a relative path
      sync = new Synchronizer(dir, proxy); 

      // A simple update should succeed. As long as we get no exceptions, we're good
      sync.Update(); 
    }

    [Test]
    public void Initialize()
    {
      sync.Initialize(); 

      string path = Path.Combine(sync.BaseDirectory, "fwstatus.xml");
      Assert.IsTrue(File.Exists(path), "Checking to see if sync file was created"); 

      FileStream fs = File.OpenRead(path); 
      XmlSerializer ser = new XmlSerializer(typeof(SynchronizerState)); 
      SynchronizerState state = (SynchronizerState) ser.Deserialize(fs); 
      fs.Close(); 

      Assert.AreEqual("http://server/vdir", state.ProxyUri, "Checking that URI was stored correctly"); 

      Assert.AreEqual(2, state.Namespaces.Count, "Checking that proper number of namespaces were stored"); 

      Assert.AreEqual(3, state.Namespaces["A"].Topics.Count, "Checking that proper number of topics were stored in A"); 
      Assert.AreEqual(3, state.Namespaces["A"].Topics.Count, "Checking that proper number of topics were stored in B"); 

      LocalTopic atwo = state.Namespaces["A"].Topics["TwoTopic"]; 

      Assert.AreEqual(0, atwo.BasedOnChecksum, "Checking that BasedOnChecksum was stored properly"); 
      Assert.AreEqual("AVersion2", atwo.BasedOnRepositoryVersion, "Checking that BasedOnRepositoryVersion was stored properly"); 
      Assert.AreEqual("TwoTopic", atwo.Name, "Checking that name was stored properly"); 
      Assert.AreEqual("AVersion2", atwo.RepositoryVersion, "Checking that RepositoryVersion was stored properly"); 

      foreach (LocalNamespace ns in sync.Namespaces)
      {
        foreach (LocalTopic topic in ns.Topics)
        {
          Assert.AreEqual(Status.NoLocalFile, topic.Status, "Checking for status of 'No Local File'"); 
        }
      }

      LocalTopic aone = sync.Namespaces["A"].Topics["OneTopic"]; 
      Assert.AreEqual("AVersion1", aone.RepositoryVersion, "Checking for correct A1 version"); 
      Assert.AreEqual("AVersion1", aone.BasedOnRepositoryVersion, "Checking for correct A1 based-on version"); 
      Assert.AreEqual(0, aone.BasedOnChecksum, "Checking for correct A1 based-on checksum"); 
				
      LocalTopic btwo = sync.Namespaces["B"].Topics["TwoTopic"];
      Assert.AreEqual("BVersion2", btwo.RepositoryVersion, "Checking for correct B2 version"); 
      Assert.AreEqual("BVersion2", btwo.BasedOnRepositoryVersion, "Checking for correct B2 based-on version"); 
      Assert.AreEqual(0, btwo.BasedOnChecksum, "Checking for correct B2 based-on checksum"); 

      string adir = Path.Combine(dir, "A"); 
      string bdir = Path.Combine(dir, "B"); 
      Assert.IsTrue(!Directory.Exists(adir), "Checking that directory A does not exist"); 
      Assert.IsTrue(!Directory.Exists(bdir), "Checking that directory B does not exist"); 
			
      Assert.IsTrue(!File.Exists(Path.Combine(adir, "OneTopic.wiki")), "Checking that A.OneTopic file does not exist"); 
      Assert.IsTrue(!File.Exists(Path.Combine(bdir, "_ContentBaseDefinition.wiki")), "Checking that B._ContentBaseDefinition file does not exist"); 

		
    }

    [Test]
    public void Update()
    {
      sync.Initialize(); 
      sync.Update(); 

      LocalTopic aone = sync.Namespaces["A"].Topics["OneTopic"]; 
      Assert.AreEqual("AVersion1", aone.RepositoryVersion, "Checking for correct A1 version"); 
      Assert.AreEqual("AVersion1", aone.BasedOnRepositoryVersion, "Checking for correct A1 based-on version"); 
      Assert.AreEqual(1273676647, aone.BasedOnChecksum, "Checking for correct A1 based-on checksum"); 
				
      LocalTopic btwo = sync.Namespaces["B"].Topics["TwoTopic"];
      Assert.AreEqual("BVersion2", btwo.RepositoryVersion, "Checking for correct B2 version"); 
      Assert.AreEqual("BVersion2", btwo.BasedOnRepositoryVersion, "Checking for correct B2 based-on version"); 
      Assert.AreEqual(-372903819, btwo.BasedOnChecksum, "Checking for correct B2 based-on checksum"); 

      string adir = Path.Combine(dir, "A"); 
      string bdir = Path.Combine(dir, "B"); 
      Assert.IsTrue(Directory.Exists(adir), "Checking that directory A exists"); 
      Assert.IsTrue(Directory.Exists(bdir), "Checking that directory B exists"); 
			
      Assert.IsTrue(File.Exists(Path.Combine(adir, "OneTopic.wiki")), "Checking that A.OneTopic file exists"); 
      Assert.IsTrue(File.Exists(Path.Combine(bdir, "_ContentBaseDefinition.wiki")), "Checking that B._ContentBaseDefinition file exists"); 

    }

    [Test]
    public void UpdateAfterInitBug()
    {
      sync.Initialize(); 

      LocalTopic topic = sync.Namespaces["A"].Topics["OneTopic"];
      Assert.AreEqual(0, topic.BasedOnChecksum, "Checking that based-on checksum is zero after init"); 

      sync.Update(); 
      Assert.AreEqual(1273676647, topic.Checksum, "Checking that checksum is right after update"); 
      Assert.AreEqual(1273676647, topic.BasedOnChecksum, "Checking that based-on checksum is zero after update"); 

      sync = new Synchronizer(dir, UnitTests.GetMockServiceProxy()); 
      Assert.AreEqual(1273676647, topic.Checksum, "Checking that checksum is right with new synchronizer"); 
      Assert.AreEqual(1273676647, topic.BasedOnChecksum, "Checking that based-on checksum is right with new synchronizer"); 

    }

    [Test]
    public void SyncToLocal()
    {
      sync.Initialize(); 
      UnitTests.CreateTopicFile(sync, "A", "Foo", "blah"); 
      sync.SyncToLocal(); 

      Assert.IsNotNull(sync.Namespaces["A"].Topics["Foo"], "Checking that SyncToLocal picked up on new file"); 
    }

    [Test]
    public void SyncToRemote()
    {
      sync.Initialize(); 
      sync.Update(); 
      proxy.Populate("A", "BlahTopic", "Version1", "A-BlahTopic-Version1"); 

      int acount = sync.Namespaces["A"].Topics.Count; 

      sync.SyncToRemote(); 

      LocalTopic theTopic = sync.Namespaces["A"].Topics["BlahTopic"]; 

      Assert.IsNotNull(theTopic, "Checking that new topic was picked up on"); 

      foreach (LocalNamespace ns in sync.Namespaces)
      {
        foreach (LocalTopic topic in ns.Topics)
        {
          if (topic == theTopic)
          {
            Assert.AreEqual(Status.NoLocalFile, topic.Status, "Checking that status is 'No local file'"); 
          }
          else
          {
            Assert.AreEqual(Status.UpToDate, topic.Status, "Checking that all other files are 'UpToDate'");
          }

        }
      }
    }

    [Test]
    public void UpdateRemoteAdded()
    {
      // Initialize does an update, too
      sync.Initialize(); 

      proxy.Populate("A", "FooBar", "Version1", "A-FooBar-Version1"); 

      sync.Update(); 

      LocalTopic theTopic = sync.Namespaces["A"].Topics["FooBar"]; 

      Assert.IsNotNull(theTopic, "Checking that topic was retrieved"); 

      Assert.AreEqual(Status.UpToDate, theTopic.Status, "Checking that topic is 'UpToDate'"); 

      int first;
      int second; 
      FileStream fs = File.OpenRead(theTopic.Path); 
      try
      {
        StreamReader sr = new StreamReader(fs); 
        try
        {
          first = sr.Read(); 
          second = sr.Read(); 
        }
        finally
        {
          sr.Close(); 
        }
      }
      finally
      {
        fs.Close(); 
      }

      Assert.AreEqual(65, first, "Checking that topic starts with the right character");		
      Assert.AreEqual(45, second, "Checking that topic has the right second character");

      Assert.IsTrue(File.Exists(Path.Combine(sync.BaseDirectory, "fwstatus.xml")), "Checking that wikistate file exists"); 


    }

    private void SetupMultipleAddedRemoteFiles()
    {
      // Initialize does an update, too
      sync.Initialize(); 

      proxy.Populate("A", "FooBar", "Version1", "A-FooBar-Version1"); 
      proxy.Populate("A", "BarFoo", "Version1", "A-BarFoo-Version1"); 
      proxy.Populate("B", "FooBar", "Version1", "B-FooBar-Version1"); 

      sync.SyncToRemote(); 
    }

    [Test]
    public void UpdateRemoteAddedSingleIgnored()
    {
      SetupMultipleAddedRemoteFiles(); 

      LocalTopic a1 = sync.Namespaces["A"].Topics["FooBar"]; 
      LocalTopic a2 = sync.Namespaces["A"].Topics["BarFoo"]; 
      LocalTopic b1 = sync.Namespaces["B"].Topics["FooBar"]; 

      Assert.IsNotNull(a1, "Checking that A1 topic's status was updated"); 
      Assert.IsNotNull(a2, "Checking that A2 topic's status was updated"); 
      Assert.IsNotNull(b1, "Checking that B2 topic's status was updated"); 

      Assert.AreEqual(Status.NoLocalFile, a1.Status, "Checking that topic is 'NoLocalFile'"); 
      Assert.AreEqual(Status.NoLocalFile, a2.Status, "Checking that topic is 'NoLocalFile'"); 
      Assert.AreEqual(Status.NoLocalFile, b1.Status, "Checking that topic is 'NoLocalFile'"); 

      StringCollection files = new StringCollection(); 
      files.Add(a1.Path); 
      sync.Update(files); 
      
      Assert.AreEqual(Status.UpToDate, a1.Status, "Checking that topic is 'UpToDate'"); 
      Assert.AreEqual(Status.NoLocalFile, a2.Status, "Checking that topic is 'NoLocalFile'"); 
      Assert.AreEqual(Status.NoLocalFile, b1.Status, "Checking that topic is 'NoLocalFile'"); 

    }

    [Test]
    public void UpdateRemoteAddedNamespaceIgnored()
    {
      SetupMultipleAddedRemoteFiles(); 

      LocalTopic a1 = sync.Namespaces["A"].Topics["FooBar"]; 
      LocalTopic a2 = sync.Namespaces["A"].Topics["BarFoo"]; 
      LocalTopic b1 = sync.Namespaces["B"].Topics["FooBar"]; 

      Assert.IsNotNull(a1, "Checking that A1 topic's status was updated"); 
      Assert.IsNotNull(a2, "Checking that A2 topic's status was updated"); 
      Assert.IsNotNull(b1, "Checking that B2 topic's status was updated"); 

      Assert.AreEqual(Status.NoLocalFile, a1.Status, "Checking that topic is 'NoLocalFile'"); 
      Assert.AreEqual(Status.NoLocalFile, a2.Status, "Checking that topic is 'NoLocalFile'"); 
      Assert.AreEqual(Status.NoLocalFile, b1.Status, "Checking that topic is 'NoLocalFile'"); 

      StringCollection files = new StringCollection(); 
      files.Add(a1.Namespace.Directory); 
      sync.Update(files); 
      
      Assert.AreEqual(Status.UpToDate, a1.Status, "Checking that topic is 'UpToDate'"); 
      Assert.AreEqual(Status.UpToDate, a2.Status, "Checking that topic is 'UpToDate'"); 
      Assert.AreEqual(Status.NoLocalFile, b1.Status, "Checking that topic is 'NoLocalFile'"); 

    }


    [Test]
    [ExpectedException(typeof(ApplicationException))]
    public void UpdateOutsideBaseDir()
    {
      sync.Initialize(); 
      StringCollection files = new StringCollection(); 
      files.Add(@"..\..\foo.wiki"); 
      sync.Update(files);
    }

    [Test]
    public void UpdateLocalAdded()
    {
      sync.Initialize(); 
      UnitTests.CreateTopicFile(sync, "A", "FooBar", "A-FooBar-Version1"); 
      sync.Update(); 

      LocalTopic theTopic = sync.Namespaces["A"].Topics["FooBar"]; 

      Assert.IsNull(proxy.GetTextForTopic(theTopic), "Checking that topic was not pushed to remote server"); 

      Assert.AreEqual(Status.LocallyAdded, theTopic.Status, "Checking that topic is 'LocallyAdded'"); 

    }

    [Test]
    public void AddSingle()
    {
      sync.Initialize(); 

      string topicname = "ThreeTopic"; 
      string content = "A-" + topicname; 

      string path = UnitTests.CreateTopicFile(sync, "A", topicname, content); 

      sync.SyncToLocal(); 

      AssertTopicAddedState(sync, "A", topicname, path); 

    }

    private void AssertTopicAddedState(Synchronizer sync, string nsname, string topicname, string path)
    {
      LocalTopic topic = sync.Namespaces[nsname].Topics[topicname]; 
      Assert.IsNotNull(topic, "Checking that topic was added"); 

      Assert.AreEqual(Status.LocallyAdded, topic.Status, "Checking that status is 'LocallyAdded'"); 

      Assert.AreEqual(topicname, topic.Name, "Checking that topic name is correct"); 

      Assert.AreEqual(nsname, topic.Namespace.Name, "Checking that namespace was set correctly");

      Assert.AreEqual(Path.GetFullPath(path), topic.Path, "Checking that path is correct"); 

      Assert.IsNull(topic.RepositoryVersion, "Checking that repository version is null"); 
			
    }

    [Test]
    public void AddMultiple()
    {
      sync.Initialize(); 

      string aname = "ThreeTopic"; 
      string bname = "FourTopic"; 

      string apath = UnitTests.CreateTopicFile(sync, "A", aname, "A-ThreeTopic"); 
      string bpath = UnitTests.CreateTopicFile(sync, "B", bname, "B-FourTopic"); 

      sync.SyncToLocal(); 

      AssertTopicAddedState(sync, "A", aname, apath); 
      AssertTopicAddedState(sync, "B", bname, bpath); 

    }

    [Test]
    public void AddNonWikiFile()
    {
      sync.Initialize();
      sync.Update(); 

      int acount = sync.Namespaces["A"].Topics.Count; 
      int bcount = sync.Namespaces["B"].Topics.Count; 

      string file = "FourTopic.foo"; 
      // Put it in the parent of the base directory
      string dir = sync.Namespaces["A"].Directory; 
      string path = Path.Combine(dir, file); 

      try
      {
        FileStream fs = new FileStream(path, FileMode.CreateNew, FileAccess.Write); 
        StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.ASCII); 
        sw.WriteLine("Some content"); 
        sw.Close(); 

        sync.SyncToLocal(); 

        Assert.AreEqual(acount, sync.Namespaces["A"].Topics.Count, "Checking that no files were added to A"); 
        Assert.AreEqual(bcount, sync.Namespaces["B"].Topics.Count, "Checking that no files were added to B"); 
      }
      finally
      {
        if (File.Exists(path))
        {
          File.Delete(path); 
        }
      }
    }

    [Test]
    public void AddAndCommit()
    {
      TestHelper.AddAndCommit(sync); 

      foreach (LocalNamespace ns in sync.Namespaces)
      {
        foreach (LocalTopic topic in ns.Topics)
        {
          Assert.AreEqual(Status.UpToDate, topic.Status, "Checking that status is 'UpToDate'"); 
        }
      }

      LocalTopic theTopic = sync.Namespaces["A"].Topics["ThreeTopic"]; 
			
      AssertCommittedTopic(theTopic); 

    }


    [Test]
    public void AddAndCommitSingle()
    {
      SetupAddMultipleLocally(); 
     
      sync.SyncToLocal(); 

      LocalTopic a1 = sync.Namespaces["A"].Topics["FooBar"];
      LocalTopic a2 = sync.Namespaces["A"].Topics["BarFoo"];
      LocalTopic b1 = sync.Namespaces["B"].Topics["FooBar"];

      Assert.IsNotNull(a1, "Checking that local file a1 has been picked up"); 
      Assert.IsNotNull(a2, "Checking that local file a2 has been picked up");
      Assert.IsNotNull(b1, "Checking that local file b1 has been picked up");

      Assert.AreEqual(Status.LocallyAdded, a1.Status, "Checking that local file a1 has status of 'LocallyAdded'");
      Assert.AreEqual(Status.LocallyAdded, a2.Status, "Checking that local file a2 has status of 'LocallyAdded'");
      Assert.AreEqual(Status.LocallyAdded, b1.Status, "Checking that local file b1 has status of 'LocallyAdded'");

      StringCollection files = new StringCollection(); 
      files.Add(Path.Combine(dir, @"A\FooBar.wiki")); 
      sync.Commit("CraigAndera", files); 

      Assert.AreEqual(Status.UpToDate, a1.Status, "Checking that topic a1 has status of 'UpToDate'"); 
      Assert.AreEqual(Status.LocallyAdded, a2.Status, "Checking that topic a2 has status of 'LocallyAdded'"); 
      Assert.AreEqual(Status.LocallyAdded, b1.Status, "Checking that topic b1 has status of 'LocallyAdded'"); 

      AssertCommittedTopic(a1); 

    }

    [Test]
    public void AddAndCommitSingleWithoutExtension()
    {
      SetupAddMultipleLocally(); 
     
      sync.SyncToLocal(); 

      LocalTopic a1 = sync.Namespaces["A"].Topics["FooBar"];
      LocalTopic a2 = sync.Namespaces["A"].Topics["BarFoo"];
      LocalTopic b1 = sync.Namespaces["B"].Topics["FooBar"];

      Assert.IsNotNull(a1, "Checking that local file a1 has been picked up"); 
      Assert.IsNotNull(a2, "Checking that local file a2 has been picked up");
      Assert.IsNotNull(b1, "Checking that local file b1 has been picked up");

      Assert.AreEqual(Status.LocallyAdded, a1.Status, "Checking that local file a1 has status of 'LocallyAdded'");
      Assert.AreEqual(Status.LocallyAdded, a2.Status, "Checking that local file a2 has status of 'LocallyAdded'");
      Assert.AreEqual(Status.LocallyAdded, b1.Status, "Checking that local file b1 has status of 'LocallyAdded'");

      StringCollection files = new StringCollection(); 
      files.Add(Path.Combine(dir, @"A\FooBar")); 
      sync.Commit("CraigAndera", files); 

      Assert.AreEqual(Status.UpToDate, a1.Status, "Checking that topic a1 has status of 'UpToDate'"); 
      Assert.AreEqual(Status.LocallyAdded, a2.Status, "Checking that topic a2 has status of 'LocallyAdded'"); 
      Assert.AreEqual(Status.LocallyAdded, b1.Status, "Checking that topic b1 has status of 'LocallyAdded'"); 

      AssertCommittedTopic(a1); 

    }

    [Test]
    public void AddAndCommitNamespace()
    {
      SetupAddMultipleLocally(); 
     
      sync.SyncToLocal(); 

      LocalTopic a1 = sync.Namespaces["A"].Topics["FooBar"];
      LocalTopic a2 = sync.Namespaces["A"].Topics["BarFoo"];
      LocalTopic b1 = sync.Namespaces["B"].Topics["FooBar"];

      Assert.IsNotNull(a1, "Checking that local file a1 has been picked up"); 
      Assert.IsNotNull(a2, "Checking that local file a2 has been picked up");
      Assert.IsNotNull(b1, "Checking that local file b1 has been picked up");

      Assert.AreEqual(Status.LocallyAdded, a1.Status, "Checking that local file a1 has status of 'LocallyAdded'");
      Assert.AreEqual(Status.LocallyAdded, a2.Status, "Checking that local file a2 has status of 'LocallyAdded'");
      Assert.AreEqual(Status.LocallyAdded, b1.Status, "Checking that local file b1 has status of 'LocallyAdded'");

      StringCollection files = new StringCollection(); 
      files.Add(Path.Combine(dir, @"A")); 
      sync.Commit("CraigAndera", files); 

      Assert.AreEqual(Status.UpToDate, a1.Status, "Checking that topic a1 has status of 'UpToDate'"); 
      Assert.AreEqual(Status.UpToDate, a2.Status, "Checking that topic a2 has status of 'UpToDate'"); 
      Assert.AreEqual(Status.LocallyAdded, b1.Status, "Checking that topic b1 has status of 'LocallyAdded'"); 

      AssertCommittedTopic(a1); 
      AssertCommittedTopic(a2); 

    }


    [Test]
    public void ModifyAndCommit()
    {
      sync.Initialize();
      sync.Update(); 

      string topicname = "TwoTopic"; 
      string content = "A change at " + DateTime.Now.ToString(); 

      LocalTopic theTopic = sync.Namespaces["A"].Topics[topicname]; 

      UnitTests.AppendToTopic(theTopic, content);

      Assert.AreEqual(Status.LocallyModified, theTopic.Status, "Checking that status is now 'Modified'"); 

      sync.Commit("CraigAndera"); 

      foreach (LocalNamespace ns in sync.Namespaces)
      {
        foreach (LocalTopic topic in ns.Topics)
        {
          Assert.AreEqual(Status.UpToDate, topic.Status, "Checking that status is 'UpToDate'"); 
        }
      }

      AssertCommittedTopic(theTopic); 
    }


    [Test]
    [ExpectedException(typeof(ConflictException), 
       "At least one file is in conflict, meaning someone has updated the copy in the repository since your last update. You must resolve all conflicts before committing.")]
    public void CommitWithConflict()
    {
      sync.Initialize();
      sync.Update(); 

      string topicname = "TwoTopic";
      LocalTopic theTopic = sync.Namespaces["A"].Topics[topicname]; 

      SetUpConflict(theTopic); 

      sync.SyncToLocal(); 
      sync.SyncToRemote(); 

      Assert.AreEqual(Status.InConflict, theTopic.Status, "Checking that status is now 'InConflict'"); 

      sync.Commit("CraigAndera"); 
    }

    [Test]
    public void CalculateBaseDir1()
    {
      sync.Initialize(); 
      StringCollection files = new StringCollection(); 
      string path = Synchronizer.CalculateBaseDir(files); 
      Assert.AreEqual(Directory.GetCurrentDirectory(), path, "Checking that base directory is calculated correctly"); 
    }

    [Test]
    public void CalculateBaseDir2()
    {
      sync.Initialize(); 
      StringCollection files = new StringCollection(); 
      files.Add("."); 
      string path = Synchronizer.CalculateBaseDir(files); 
      Assert.AreEqual(Directory.GetCurrentDirectory(), path, "Checking that base directory is calculated correctly"); 
    }

    [Test]
    public void CalculateBaseDir2a()
    {
      sync.Initialize(); 
      StringCollection files = new StringCollection(); 
      files.Add(dir); 
      string path = Synchronizer.CalculateBaseDir(files); 
      Assert.AreEqual(Path.GetFullPath(dir), path, "Checking that base directory is calculated correctly"); 
    }

    [Test]
    public void CalculateBaseDir3()
    {
      sync.Initialize(); 
      StringCollection files = new StringCollection(); 
      files.Add(Path.Combine(dir, @"A")); 
      string path = Synchronizer.CalculateBaseDir(files); 
      Assert.AreEqual(Path.GetFullPath(dir), path, "Checking that base directory is calculated correctly"); 
    }

    [Test]
    public void CalculateBaseDir4()
    {
      sync.Initialize(); 
      StringCollection files = new StringCollection(); 
      files.Add(Path.Combine(dir, "A")); 
      files.Add(Path.Combine(dir, "B")); 
      string path = Synchronizer.CalculateBaseDir(files); 
      Assert.AreEqual(Path.GetFullPath(dir), path, "Checking that base directory is calculated correctly"); 
    }

    [Test]
    public void CalculateBaseDir5()
    {
      sync.Initialize(); 
      StringCollection files = new StringCollection(); 
      files.Add(Path.Combine(dir, @"A\OneTopic")); 
      files.Add(Path.Combine(dir, @"B")); 
      string path = Synchronizer.CalculateBaseDir(files); 
      Assert.AreEqual(Path.GetFullPath(dir), path, "Checking that base directory is calculated correctly"); 
    }

    [Test]
    public void CalculateBaseDir6()
    {
      sync.Initialize(); 
      StringCollection files = new StringCollection(); 
      files.Add(Path.Combine(dir, @"A\OneTopic.wiki")); 
      files.Add(Path.Combine(dir, @"B")); 
      string path = Synchronizer.CalculateBaseDir(files); 
      Assert.AreEqual(Path.GetFullPath(dir), path, "Checking that base directory is calculated correctly"); 
    }

    [Test]
    public void CalculateBaseDir7()
    {
      sync.Initialize(); 
      StringCollection files = new StringCollection(); 
      files.Add(Path.Combine(dir, @"A\NoSuchTopic")); 
      files.Add(Path.Combine(dir, @"B")); 
      string path = Synchronizer.CalculateBaseDir(files); 
      Assert.AreEqual(Path.GetFullPath(dir), path, "Checking that base directory is calculated correctly"); 
    }

    [Test]
    public void CalculateBaseDir8()
    {
      sync.Initialize(); 
      sync.Update(); 
      string pwd = Directory.GetCurrentDirectory(); 
      Directory.SetCurrentDirectory(Path.GetFullPath(Path.Combine(dir, @"A")));
      StringCollection files = new StringCollection(); 
      string path = Synchronizer.CalculateBaseDir(files); 
      Directory.SetCurrentDirectory(pwd); 
      Assert.AreEqual(Path.GetFullPath(dir), path, "Checking that base directory is calculated correctly"); 
    }

    [Test]
    public void CalculateBaseDir9()
    {
      sync.Initialize(); 
      sync.Update(); 
      Directory.SetCurrentDirectory(Path.GetFullPath(Path.Combine(dir, @"A")));
      StringCollection files = new StringCollection(); 
      files.Add("OneTopic");
      string path = Synchronizer.CalculateBaseDir(files); 
      Assert.AreEqual(Path.GetFullPath(dir), path, "Checking that base directory is calculated correctly"); 
    }

    [Test]
    public void CalculateBaseDir9a()
    {
      sync.Initialize(); 
      sync.Update(); 
      string pwd = Directory.GetCurrentDirectory(); 
      Directory.SetCurrentDirectory(Path.GetFullPath(Path.Combine(dir, @"A")));
      StringCollection files = new StringCollection(); 
      files.Add(@"OneTopic.wiki");
      string path = Synchronizer.CalculateBaseDir(files); 
      Directory.SetCurrentDirectory(pwd); 
      Assert.AreEqual(Path.GetFullPath(dir), path, "Checking that base directory is calculated correctly"); 
    }

    [Test]
    public void CalculateBaseDir10()
    {
      sync.Initialize(); 
      sync.Update(); 
      Directory.SetCurrentDirectory(Path.GetFullPath(Path.Combine(dir, @"A")));
      StringCollection files = new StringCollection(); 
      files.Add(@"..\B\OneTopic.wiki");
      string path = Synchronizer.CalculateBaseDir(files); 
      Assert.AreEqual(Path.GetFullPath(dir), path, "Checking that base directory is calculated correctly"); 
    }


#if false
		[Test]
		public void CommitSingle()
		{
			sync.Initialize();

			LocalTopic atopic = sync.Namespaces["A"].Topics["OneTopic"]; 
			LocalTopic btopic = sync.Namespaces["B"].Topics["OneTopic"]; 

			UnitTests.AppendToTopic(atopic, "A change at " + DateTime.Now.ToString()); 
			UnitTests.AppendToTopic(btopic, "A change at " + DateTime.Now.ToString()); 

			sync.Commit(atopic.Path); 

			foreach (LocalNamespace ns in sync.Namespaces)
			{
				foreach (LocalTopic topic in ns.Topics)
				{
					if (topic == btopic)
					{
						Assertion.AssertEquals("Checking for changed file to have status 'Modified'", 
							Status.LocallyModified, topic.Status);
					}
					else
					{
						Assertion.AssertEquals("Checking that all other topics are up to date", 
							Status.UpToDate, topic.Status); 
					}
				}
			}

		}
#endif 

#if false
		[Test]
		public void CommitDirectory()
		{
			sync.Initialize();

			LocalTopic atopic = sync.Namespaces["A"].Topics["OneTopic"]; 
			LocalTopic btopic = sync.Namespaces["B"].Topics["OneTopic"]; 

			UnitTests.AppendToTopic(atopic, "A change at " + DateTime.Now.ToString()); 
			UnitTests.AppendToTopic(btopic, "A change at " + DateTime.Now.ToString()); 

			sync.Commit(atopic.Namespace.Directory); 

			foreach (LocalNamespace ns in sync.Namespaces)
			{
				foreach (LocalTopic topic in ns.Topics)
				{
					if (topic == btopic)
					{
						Assertion.AssertEquals("Checking for changed file to have status 'Modified'", 
							Status.LocallyModified, topic.Status);
					}
					else
					{
						Assertion.AssertEquals("Checking that all other topics are up to date", 
							Status.UpToDate, topic.Status); 
					}
				}
			}

		}
#endif


		private void AssertCommittedTopic(LocalTopic topic)
		{
			Assert.IsNotNull(topic.RepositoryVersion, "Checking that there is a repository version now"); 

			Assert.IsNotNull(topic.BasedOnRepositoryVersion, "Checking that there is a based-on version now"); 

			Assert.AreEqual(topic.BasedOnRepositoryVersion, topic.RepositoryVersion, "Checking that repository version equals based on version"); 

			Assert.AreEqual(topic.BasedOnChecksum, topic.Checksum, "Checking that local checksum equals based on checksum"); 

		}

#if false
		[Test]
		public void UpdateNothingChanged()
		{

		}

		[Test]
		public void UpdateRemoteOnlyChanged()
		{
		}

		[Test]
		public void UpdateLocalOnlyChanged()
		{
		}

		[Test]
		public void UpdateBothChanged()
		{
		}

		[Test]
		public void CommitNothingChanged()
		{
		}

		[Test]
		public void CommitRemoteOnlyChanged()
		{
		}

		[Test]
		public void CommitLocalOnlyChanged()
		{
		}

		[Test]
		public void CommitBothChangedNoOverwrite()
		{
		}

		[Test]
		public void CommitBothChangedForceOverwrite()
		{
		}
#endif 


	}
}