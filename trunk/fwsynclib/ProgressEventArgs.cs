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

namespace FlexWiki.FwSyncLib
{
  public class ProgressEventArgs : EventArgs
  {
    private EventType eventType; 
    private LocalNamespace ns; 
    private LocalTopic topic; 
    private Status oldStatus; 
    private Status newStatus; 
    
    public ProgressEventArgs(EventType eventType, LocalNamespace ns) :
      this(eventType, ns, null, Status.NoLocalFile, Status.NoLocalFile)
    {
    }
    
    public ProgressEventArgs(EventType eventType, LocalTopic topic) :
      this(eventType, topic.Namespace, topic, Status.NoLocalFile, Status.NoLocalFile)
    {
    }
    
    public ProgressEventArgs(EventType eventType, 
      LocalNamespace ns, LocalTopic topic, Status oldStatus, 
      Status newStatus)
    {
      this.eventType = eventType;
      this.ns = ns; 
      this.topic = topic; 
      this.oldStatus = oldStatus; 
      this.newStatus = newStatus; 
    }

    public EventType EventType
    {
      get { return eventType; }
    }

    public LocalNamespace Namespace
    {
      get { return ns; }
    }

    public LocalTopic Topic
    {
      get { return topic; }
    }

    public Status OldStatus
    {
      get { return oldStatus; }
    }

    public Status NewStatus
    {
      get { return newStatus; }
    }
  }
}
