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
using System.Collections; 

using NUnit.Framework;

namespace FlexWiki.FwSyncLib.Tests
{
  [TestFixture]
  public class ConflictResolutionTests : SynchronizerTestBase
  {
    private LocalTopic theTopic; 
    private string localContents; 
    private string remoteContents; 
    private ProgressCallbackTestTarget target; 

    protected override string TestDirectory
    {
      get { return "ConflictResolutionTests";  }
    }

    public override void SetUp()
    {
      base.SetUp();

      target = new ProgressCallbackTestTarget(); 
      sync.Progress += new ProgressCallback(target.OnProgress); 

      sync.Initialize();
      sync.Update(); 

      string topicname = "TwoTopic";
      theTopic = sync.Namespaces["A"].Topics[topicname]; 

      SetUpConflict(theTopic); 

      sync.SyncToLocal(); 
      sync.SyncToRemote(); 

      Assert.AreEqual(Status.InConflict, theTopic.Status, "Checking that status is now 'InConflict'"); 

      localContents = Utilities.GetTopicContent(theTopic); 
      remoteContents = proxy.GetLatestText(theTopic.Namespace.Name, theTopic.Name); 
    }
    
    [Test]
    public void ResolveToLocal()
    {
      TestConflictResolver tcr = new TestConflictResolver(); 
      tcr.Mode = TestConflictResolver.ResolutionMode.KeepLocal; 
      ResolveConflictCallback rccb = new ResolveConflictCallback(tcr.ResolveConflict);
      sync.Resolve(rccb); 

      Assert.AreEqual(Status.LocallyModified, theTopic.Status, "Checking that status is now LocallyModified");
      Assert.AreEqual(localContents, Utilities.GetTopicContent(theTopic), "Checking that local contents were kept"); 

      ArrayList events = target.Find(EventType.ConflictSkipped);

      Assert.AreEqual(0, events.Count, "Checking that no conflicts were reported skipped"); 
    }

    [Test]
    public void ResolveToRemote()
    {
      TestConflictResolver tcr = new TestConflictResolver(); 
      tcr.Mode = TestConflictResolver.ResolutionMode.KeepRemote; 
      ResolveConflictCallback rccb = new ResolveConflictCallback(tcr.ResolveConflict);
      sync.Resolve(rccb); 

      Assert.AreEqual(Status.LocallyModified, theTopic.Status, "Checking that status is now LocallyModified"); 
      Assert.AreEqual(remoteContents, Utilities.GetTopicContent(theTopic), "Checking that local contents were overwritten");
    }

    [Test]
    public void ResolveConcatenate()
    {
      TestConflictResolver tcr = new TestConflictResolver(); 
      tcr.Mode = TestConflictResolver.ResolutionMode.Concatenate; 
      ResolveConflictCallback rccb = new ResolveConflictCallback(tcr.ResolveConflict);
      sync.Resolve(rccb); 

      Assert.AreEqual(Status.LocallyModified, theTopic.Status, "Checking that status is now LocallyModified"); 
      Assert.AreEqual(localContents + remoteContents, Utilities.GetTopicContent(theTopic), 
        "Checking that local and remote contents were 'merged'");
    }

    [Test]
    public void ResolveCancelled()
    {
      TestConflictResolver tcr = new TestConflictResolver(); 
      tcr.Mode = TestConflictResolver.ResolutionMode.Cancel; 
      ResolveConflictCallback rccb = new ResolveConflictCallback(tcr.ResolveConflict);
      sync.Resolve(rccb); 

      Assert.AreEqual(Status.InConflict, theTopic.Status, "Checking that status is still InConflict"); 
      Assert.AreEqual(localContents, Utilities.GetTopicContent(theTopic), "Checking that local contents were unchanged");

    }
 
  }
}