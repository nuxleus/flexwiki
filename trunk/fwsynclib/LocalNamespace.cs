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

namespace FlexWiki.FwSyncLib
{
	public class LocalNamespace : INamedObject
	{
		#region Fields
		private Synchronizer sync; 
		private LocalTopicList topics = new LocalTopicList(); 
		private string name; 
		#endregion Fields
	
		#region Constructors
		// Used only for XML Serialization
		public LocalNamespace()
		{
		}

		public LocalNamespace(Synchronizer sync, string name)
		{
			this.sync = sync; 
			this.name = name; 
		}
		#endregion Constructors

		#region Properties
		public LocalTopicList Topics
		{
			get	{ return topics; }
			set { topics = value; }
		}

		public string Name
		{
			get {	return name; }
			set { name = value; }
		}

		[XmlIgnore]
		public string Directory
		{
			get { return Path.Combine(sync.BaseDirectory, name); }
		}

		[XmlIgnore]
		public Synchronizer Synchronizer
		{
			get { return sync; } 
      set { sync = value; }
		}
		#endregion Properties

		#region Methods
		public LocalTopic AddTopic(string name, string version, string basedOnVersion)
		{
			LocalTopic lt = new LocalTopic(this, name, version, basedOnVersion);
			topics.Add(lt); 
			return lt; 
		}
		#endregion Methods

	}
}

