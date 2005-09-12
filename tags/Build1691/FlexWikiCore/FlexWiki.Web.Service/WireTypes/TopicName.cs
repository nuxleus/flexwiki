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
using System.Xml.Serialization; 

namespace FlexWiki.Web.Services.WireTypes
{
  [XmlType(Namespace="http://www.flexwiki.com/webservices/")]
  [XmlInclude(typeof(AbsoluteTopicName))]
  public abstract class TopicName
  {
    private string fullnameWithVersion;
    private string version;
    private string fullname;
    private string name;
    private string ns;

    public string FullnameWithVersion
    {
      get { return fullnameWithVersion; }
      set { fullnameWithVersion = value; }
    }
    
    public string Version
    {
      get { return version; }
      set { version = value; }
    }
    
    public string Fullname
    {
      get { return fullname; }
      set { fullname = value; }
    }
    
    public string Name
    {
      get { return name; }
      set { name = value; }
    }
    
    public string Namespace
    {
      get { return ns; }
      set { ns = value; }
    }
  
  }
}