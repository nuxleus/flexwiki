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

namespace FlexWiki
{
  public abstract class TopicData
  {
    abstract public string Author {get;}
    abstract public DateTime LastModificationTime {get;}
    abstract public string Name {get;}	// get the name of the topic (unqualified and without version)
    abstract public string Namespace {get;}
    abstract public string Version {get;}
  }
}
