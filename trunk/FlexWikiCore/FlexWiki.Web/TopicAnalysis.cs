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

namespace FlexWiki.Web
{
  public class TopicAnalysis
  {
    private int	_refCount;
    private Set _island;

    public int RefCount
    {
      get { return _refCount; }
      set { _refCount = value; }
    }

    public Set Island
    {
      get { return _island; }
      set { _island = value; }
    }
  }
}
