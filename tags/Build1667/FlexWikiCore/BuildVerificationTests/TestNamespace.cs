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

namespace FlexWiki.BuildVerificationTests
{
  internal sealed class TestNamespace
  {
    private string name; 
    private TestTopic[] topics; 

    internal TestNamespace(string name, params TestTopic[] topics)
    {
      this.name = name;
      this.topics = topics; 
    }

    internal string Name
    {
      get { return name; }
    }

    internal TestTopic[] Topics
    {
      get { return topics; }
    }
  }
}
