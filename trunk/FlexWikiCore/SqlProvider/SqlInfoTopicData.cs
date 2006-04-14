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

using FlexWiki;

namespace FlexWiki.SqlProvider
{
  internal class SqlInfoTopicData : TopicData
  {
    string _Namespace;
    SqlInfoForTopic _Info;

    public SqlInfoTopicData(SqlInfoForTopic info, string ns)
    {
      _Info = info;
      _Namespace = ns;
    }

    public override string Namespace
    {
      get
      {
        return _Namespace;
      }
    }

    public override DateTime LastModificationTime
    {
      get
      {
        return _Info.LastWriteTime;
      }
    }

    public override string Version
    {
      get
      {
        throw new NotImplementedException("This class was extracted from an inner class. Need to figure out how to deal with missing context.");
        //return StoreManager.ExtractVersionFromTopicName(_Info.Name);
      }
    }

    public override string Name
    {
      get
      {
        throw new NotImplementedException("This class was extracted from an inner class. Need to figure out how to deal with missing context.");
        //return NamespaceManager.ExtractNameFromTopicName(_Info.Name);
      }
    }


    public string FullName
    {
      get
      {
        return _Namespace + "." + Name;;
      }
    }

    public override string Author
    {
      get
      {
        throw new NotImplementedException("This class was extracted from an inner class. Need to figure out how to deal with missing context.");
        /*
         * Match m = TopicNameRegex.Match(_Info.Name);
        if (!m.Success)
          return null;
        if (m.Groups["name"].Captures.Count == 0)
          return null;
        return m.Groups["name"].Value;
        */
      }
    }

  }
}
