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

using NUnit.Framework; 

namespace FlexWiki.FwSyncLib.Tests
{
  public class TestHelper
  {
    public static void AddAndCommit(Synchronizer sync)
    {
      sync.Initialize();
      sync.Update(); 

      string topicname = "ThreeTopic"; 
      string content = "A-" + topicname; 

      string path = UnitTests.CreateTopicFile(sync, "A", topicname, content);

      sync.SyncToLocal(); 

      Assert.IsNotNull(sync.Namespaces["A"].Topics[topicname], "Checking that local file has been picked up"); 

      Assert.AreEqual(Status.LocallyAdded, sync.Namespaces["A"].Topics[topicname].Status, "Checking that local file has status of 'LocallyAdded'");

      sync.Commit("CraigAndera"); 

    }

  }
}