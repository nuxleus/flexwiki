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
  public class ProgressCallbackTests : SynchronizerTestBase
  {
    private ProgressCallbackTestTarget target; 

    protected override string TestDirectory
    {
      get { return "ProgressCallbackTests"; }
    }

    public override void SetUp()
    {
      base.SetUp();
      target = new ProgressCallbackTestTarget(); 
      sync.Progress += new ProgressCallback(target.OnProgress); 
    }

    [Test]
    public void AddAndCommit()
    {
      TestHelper.AddAndCommit(sync); 

      ArrayList events = target.Find(EventType.Committed); 

      Assert.AreEqual(1, events.Count, "Checking that the right number of commit events were fired"); 
      AssertEventArgsCorrect((ProgressEventArgs) events[0], EventType.Committed, 
        sync.Namespaces["A"], sync.Namespaces["A"].Topics["ThreeTopic"], 
        Status.LocallyAdded, Status.UpToDate, "commit");  
    }

    [Test]
    public override void UpdateOutOfDate()
    {
      base.UpdateOutOfDate(); 

      ArrayList events = target.Find(EventType.UpdatedLocal); 

      Assert.AreEqual(7, events.Count, "Checking that right number of update events were fired"); 
        
      AssertEventArgsCorrect((ProgressEventArgs) events[0], EventType.UpdatedLocal, 
        sync.Namespaces["A"], sync.Namespaces["A"].Topics["_ContentBaseDefinition"], 
        Status.NoLocalFile, Status.UpToDate, "first update"); 
      
      AssertEventArgsCorrect((ProgressEventArgs) events[6], EventType.UpdatedLocal, 
        sync.Namespaces["A"], sync.Namespaces["A"].Topics["OneTopic"], 
        Status.LocallyOutOfDate, Status.UpToDate, "last update"); 
    }

    [Test]
    public override void CommitWithConflictIgnored()
    {
      base.CommitWithConflictIgnored ();

      ArrayList events = target.Find(EventType.ConflictSkipped); 
      Assert.AreEqual(1, events.Count, "Checking that right number of conflict skips were fired");

      AssertEventArgsCorrect((ProgressEventArgs) events[0], EventType.ConflictSkipped, 
        sync.Namespaces["A"], sync.Namespaces["A"].Topics["TwoTopic"], 
        Status.InConflict, Status.InConflict, "conflict test"); 
    }


    private void AssertEventArgsCorrect(ProgressEventArgs actual, EventType et, 
      LocalNamespace ns, LocalTopic topic, Status oldStatus, Status newStatus, 
      string message)
    {
      Assert.AreEqual(et, actual.EventType, 
        "Checking that event types are equivalent for " + message); 
      Assert.AreEqual(ns.Name, actual.Namespace.Name, 
        "Checking that namespaces are equivalent for " + message); 
      Assert.AreEqual(topic.Name, actual.Topic.Name, 
        "Checking that topics are equivalent for " + message); 
      Assert.AreEqual(oldStatus, actual.OldStatus, 
        "Checking that old statuses are equivalent for " + message); 
      Assert.AreEqual(newStatus, actual.NewStatus, 
        "Checking that new statuses are equivalent for " + message); 
    }

  }
}