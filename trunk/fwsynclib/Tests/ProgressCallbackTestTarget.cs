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

namespace FlexWiki.FwSyncLib.Tests
{
  public class ProgressCallbackTestTarget
  {
    private ArrayList events = new ArrayList(); 

    public int Count
    {
      get { return events.Count; }
    }

    public ProgressEventArgs this[int i]
    {
      get { return (ProgressEventArgs) events[i]; }
    }

    public ArrayList Find(EventType eventType)
    {
      ArrayList al = new ArrayList(); 

      foreach (ProgressEventArgs pea in events)
      {
        if (pea.EventType == eventType)
        {
          al.Add(pea); 
        }
      }

      return al; 
    }

    public void OnProgress(object sender, ProgressEventArgs args)
    {
      events.Add(args); 
    }

  }
}