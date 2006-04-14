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

namespace FlexWiki.Web
{
  public class ChoiceSet
  {
    private readonly ArrayList _DisplayStrings = new ArrayList();
    private readonly ArrayList _ValueStrings = new ArrayList();

    public void Add(string display, string value)
    {
      _DisplayStrings.Add(display);
      _ValueStrings.Add(value);
    }

    public IList DisplayStrings
    {
      get
      {
        return ArrayList.ReadOnly(_DisplayStrings);
      }
    }

    public IList ValueStrings
    {
      get
      {
        return ArrayList.ReadOnly(_ValueStrings);
      }
    }
  };
}
