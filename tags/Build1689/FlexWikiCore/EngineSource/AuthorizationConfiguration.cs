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
using System.Collections.Specialized; 
using System.Xml; 
using System.Xml.Serialization; 

namespace FlexWiki
{
	public class AuthorizationConfiguration
	{
    private readonly StringCollection _administrators = new StringCollection(); 
    private readonly StringCollection _editors        = new StringCollection(); 
    private readonly StringCollection _readers        = new StringCollection(); 

    public StringCollection Administrators
    {
      get { return _administrators; }
    }

    public StringCollection Editors
    {
      get { return _editors; }
    }

    public StringCollection Readers
    {
      get { return _readers; }
    }
	}
}
