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

namespace FlexWiki.FwSyncLib.Tests
{
  public class TestConflictResolver
  {
    public enum ResolutionMode
    {
      Cancel, 
      Concatenate,
      KeepLocal,
      KeepRemote
    }

    private ResolutionMode mode = ResolutionMode.Cancel; 

    public ResolutionMode Mode
    {
      get { return mode; }
      set { mode = value; }
    }


    public string ResolveConflict(LocalTopic localContents, string remoteContents)
    {
      if (mode == ResolutionMode.Cancel)
      {
        return null; 
      }
      else if (mode == ResolutionMode.Concatenate)
      {
        return localContents.ReadContents() + remoteContents; 
      }
      else if (mode == ResolutionMode.KeepLocal)
      {
        return localContents.ReadContents(); 
      }
      else if (mode == ResolutionMode.KeepRemote)
      {
        return remoteContents; 
      }
      else 
      {
        throw new Exception("Unrecognized mode " + mode.ToString()); 
      }
    }
  }
}