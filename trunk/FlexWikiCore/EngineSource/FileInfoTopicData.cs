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
using System.Diagnostics;
using System.Xml.Serialization;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using FlexWiki.Formatting;


namespace FlexWiki
{
  internal class FileInfoTopicData : TopicData
  {
    public FileInfoTopicData(FileInfo info, string ns)
    {
      _Info = info;
      _Namespace = ns;
    }


    private string _Namespace;
    private FileInfo _Info;

    public override string Author
    {
      get
      {
        throw new NotImplementedException("This used to be a nested class, so I need to figure out how to decouple it.");
        /*
        string filename = _Info.Name;
        // remove the extension
        filename = filename.Substring(0, filename.Length - _Info.Extension.Length);
        Match m = s_historicalFileNameRegex.Match(filename);
        if (!m.Success)
        {
          return null;
        }
        if (m.Groups["name"].Captures.Count == 0)
        {
          return null;
        }
        return m.Groups["name"].Value;
        */
      }
    }
    public string FullName
    {
      get
      {
        return _Info.FullName;
      }
    }

    public override DateTime LastModificationTime
    {
      get
      {
        return _Info.LastWriteTime;
      }
    }

    public override string Name
    {
      get
      {
        return Path.GetFileNameWithoutExtension(_Info.ToString());
      }
    }

    public override string Namespace
    {
      get
      {
        return _Namespace;
      }
    }

    public override string Version
    {
      get
      {
        throw new NotImplementedException("This used to be a nested class, so I need to figure out how to decouple it.");
        
        //return ExtractVersionFromHistoricalFilename(Path.GetFileNameWithoutExtension(_Info.ToString()));
      }
    }

  }
}
