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
using System.IO; 
using System.Collections;
using System.Collections.Specialized; 
using System.Net; 

using FlexWiki.FwSyncLib;

namespace FlexWiki.FwSync
{
	public class Options
	{
		private Commands command; 
		private string uri; 
    private StringCollection files = new StringCollection(); 
		private bool debug; 
    private string attribution; 
    private bool useDefaultCredentials; 
    private NetworkCredential credentials; 
    private bool localOnly; 
    private bool confirmAll; 
    private bool ignoreConflict; 
    private HelpTopics helpTopic = HelpTopics.General; 
    private Status show; 
    private string resolveWith; 

    
    public string Attribution
    {
      get { return attribution; }
      set { attribution = value; } 
    }
    
    public Commands Command
		{
			get { return command; } 
			set { command = value; }
		}

    public bool ConfirmAll
    {
      get { return confirmAll; }
      set { confirmAll = value; }
    }
    public NetworkCredential Credentials
    {
      get { return credentials; }
      set { credentials = value; }
    }
    
    public bool Debug
    {
      get { return debug; }
      set { debug = value; }
    }
    
    public string EditServiceUri
		{
			get { return uri; }
			set { uri = value; }
		}

    public StringCollection Files
    {
      get { return files; } 
    }

    public HelpTopics HelpTopic
    {
      get { return helpTopic; } 
      set { helpTopic = value; }
    }
    public bool IgnoreConflict
    {
      get { return ignoreConflict; } 
      set { ignoreConflict = value; }
    }
    public bool LocalOnly
    {
      get { return localOnly; }
      set { localOnly = value; }
    }

    public string ResolveWith
    {
      get { return resolveWith; } 
      set { resolveWith = value; }
    }

    public Status Show
    {
      get { return show; }
      set { show = value; }
    }
    public bool UseDefaultCredentials
    {
      get { return useDefaultCredentials; }
      set { useDefaultCredentials = value; }
    }
	}
}

