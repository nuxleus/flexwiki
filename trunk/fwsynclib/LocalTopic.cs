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
using System.IO; 
using System.Collections;
using System.Collections.Specialized; 
using System.Text; 

namespace FlexWiki.FwSyncLib
{
  public class LocalTopic : INamedObject
  {
#region Fields
    private LocalNamespace ns; 
    private string name; 
    private string version; 
    private long basedOnChecksum; 
    private string basedOnVersion; 
#endregion Fields

#region Constructors

    // Needed for XML Serialization
    public LocalTopic()
    {
    }

    public LocalTopic(LocalNamespace ns, string name, string version, string basedOnVersion)
    {
      this.ns = ns; 
      this.name = name; 
      this.version = version; 
      this.basedOnVersion = basedOnVersion; 
    }
#endregion Constructors

    [XmlIgnore]
    public LocalNamespace Namespace
    {
      get { return ns; }
      set { ns = value; }
    }

    public string Name
    {
      get { return name; } 
			set { name = value; }
    }

    public string RepositoryVersion
    {
      get { return version; } 
			set { version = value; }
    }

    public string BasedOnRepositoryVersion
    {
      get	{ return basedOnVersion;	}
			set { basedOnVersion = value; }
    }

		public long BasedOnChecksum
		{
			get { return basedOnChecksum; }
			set { basedOnChecksum = value; }
		}

		[XmlIgnore] 
		public long Checksum
		{
			get { return Utilities.ComputeChecksumFromFile(Path); }
		}

    public string Path
    {
      get 
			{ 
				return System.IO.Path.Combine(Namespace.Directory, name + ".wiki"); 
			} 
    }

    public Status Status
    {
			get 
			{
				bool exists = File.Exists(Path); 
				bool modified = false; 
				if (exists)
				{
					modified = Checksum != BasedOnChecksum; 
				}
				bool inRepository = version != null; 
				bool versionDiffers  = version != basedOnVersion; 

				if (!exists)
				{
					return Status.NoLocalFile; 
				}
				else if (!modified && !versionDiffers)
				{
					return Status.UpToDate; 
				}
				else if (modified && !versionDiffers && inRepository)
				{
					return Status.LocallyModified; 
				}
				else if (!inRepository)
				{
					return Status.LocallyAdded; 
				}
				else if (!modified && versionDiffers)
				{
					return Status.LocallyOutOfDate; 
				}
				else if (modified && versionDiffers)
				{
					return Status.InConflict; 
				}

				throw new ApplicationException(
					string.Format("Unknown status. Local file {0}exists, has {1}been modified, is {2}in repository, and its version {3}.", 
					exists ? "" : "does not ", 
					modified ? "" : "not ", 
					inRepository ? "" : "not ",
					versionDiffers ? "" : "does not ")); 
			}
    }


    public void WriteContents(string contents)
    {
      FileStream fs = new FileStream(Path, FileMode.Create, FileAccess.Write); 
      try
      {
        StreamWriter wtr = new StreamWriter(fs, Encoding.UTF8); 
        try
        {
          wtr.Write(contents); 
        }
        finally
        {
          wtr.Close(); 
        }
      }
      finally
      {
        fs.Close(); 
      }
    }

    public string ReadContents()
    {
      return Utilities.GetTopicContent(this); 
    }

  }
}

