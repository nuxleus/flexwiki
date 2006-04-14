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

namespace FlexWiki.UnitTests
{
  internal sealed class TestTopic
  {
    private string _author; 
    private string _content; 
    private string _name; 

    internal TestTopic(string name, string author, string content)
    {
      _name = name; 
      _author = author; 
      _content = content; 
    }

    internal string Author
    {
      get { return _author; }
    }

    internal string Content
    {
      get { return _content; }
    }

    internal string Name
    {
      get { return _name; }
    }
  }
}
