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
using System.Xml; 
using System.IO; 
using System.Collections;
using System.Collections.Specialized; 

namespace FlexWiki.FwSyncLib
{
  public class LocalTopicList : LocalTopicListBase
  {
#region Fields
#endregion Fields

#region Constructors
#endregion Constructors

#region Properties
    public LocalTopic this[string key]
    {
      get { return (LocalTopic) Utilities.LookupByName(this, key); }
    }
#endregion Properties

#region Methods
#endregion Methods
  }
}

