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
using System.Text.RegularExpressions; 

namespace FlexWiki.FwDocGen
{
  public class FwXsltHelper
  {
    private const string XMLNS = 
      "http://www.flexwiki.com/schames/fwdocgen/xslthelper.xsd"; 

    public static string XmlNamespaceUri
    {
      get { return XMLNS; }
    }

    public string HashFromID(string id)
    {
      return Utilities.HashFromID(id); 
    }

    public string RegexMatch(string pattern, string input)
    {
      Match match = Regex.Match(input, pattern, RegexOptions.None); 
      return match.Value;
    }
  }
}