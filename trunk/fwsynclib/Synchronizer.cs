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
	public class Synchronizer 
	{
		private const string STATUS_FILENAME = "fwstatus.xml"; 
		private LocalNamespaceList namespaces; 
		private string basedir; 
		private IEditServiceProxy proxy; 
    
    public Synchronizer(string basedir, IEditServiceProxy proxy)
		{
			this.proxy = proxy; 
			this.basedir = Path.GetFullPath(basedir); 
			ReadLocalStatusFile(); 
		}
		

		public string BaseDirectory
		{
			get { return basedir; }
		}

		public LocalNamespaceList Namespaces
		{
			get { return namespaces; }
			set { namespaces = value; }
		}

	
    public event ProgressCallback Progress; 
		
    public void Initialize()
    {
      Initialize(false); 
    }

		public void Initialize(bool localOnly)
		{
			if (!Directory.Exists(basedir))
			{
				Directory.CreateDirectory(basedir); 
			}

      SyncToLocal(); 
      if (!localOnly)
      {
        SyncToRemote();   
      }
		}

		public void SyncToRemote()
		{
			ContentBase[] cbs = proxy.GetAllNamespaces(); 
			
      try
      {
        foreach (ContentBase cb in cbs)
        {
          LocalNamespace ln = this.Namespaces[cb.Namespace]; 
          if (ln == null)
          {
            // There's a remote namespace with no local namespace
            ln = new LocalNamespace(this, cb.Namespace); 
            namespaces.Add(ln); 

          }

          AbsoluteTopicName[] atns = proxy.GetAllTopics(cb); 

          foreach (AbsoluteTopicName atn in atns)
          {
            LocalTopic lt = ln.Topics[atn.Name]; 
            if (lt == null)
            {
              // There's a remote topic with no local topic
              lt = ln.AddTopic(atn.Name, atn.Version, atn.Version); 
            }
            lt.RepositoryVersion = atn.Version; 
          }
        }
      }
      finally
      {
        FlushStatus(); 
      }
		}

		public void SyncToLocal()
		{
			string[] dirs = Directory.GetDirectories(basedir); 

			foreach (string dir in dirs)
			{
				LocalNamespace ns = namespaces[Path.GetFileName(dir)]; 

				if (ns != null)
				{
					string[] files = Directory.GetFiles(ns.Directory, "*.wiki"); 

					foreach (string file in files)
					{
						string topicname = Path.GetFileNameWithoutExtension(file); 

						LocalTopic topic = ns.Topics[topicname]; 

						if (topic == null)
						{
							ns.AddTopic(topicname, null, null); 
						}
					}
				}
			}

      FlushStatus(); 
		}

    public void Commit(string identity)
    {
      Commit(identity, new StringCollection()); 
    }

    public void Commit(string identity, bool ignoreConflicts)
    {
      Commit(identity, new StringCollection(), ignoreConflicts); 
    }

    public void Commit(string identity, StringCollection files)
    {
      Commit(identity, files, false); 
    }

    public void Commit(string identity, StringCollection files, bool ignoreConflicts)
    {
      try
      {
        // TODO: There's a race condition here: if someone updates a topic between the time we read
        // status and the time we actually commit, we'll think a file is only modified, when in fact
        // it is in conflict. The fix to this would be to change SetTextForTopic to accept a "only 
        // take this change if the current version equals this" argument. Then we could fail if an update
        // had occurred between the initial UpdateStatus and the SetTextForTopic below. 
        SyncToLocal(); 
        SyncToRemote(); 

        LocalTopicList topics = GetTopicsForFiles(files); 

        if (!ignoreConflicts)
        {
          foreach (LocalTopic topic in topics)
          {
            if (topic.Status == Status.InConflict)
            {
              throw new ConflictException("At least one file is in conflict, meaning someone has updated the copy in the repository since your last update. You must resolve all conflicts before committing."); 
            }
          }
        }

        LocalTopicList updated = new LocalTopicList();
        foreach (LocalTopic topic in topics)
        {
          if (topic.Status == Status.LocallyAdded || topic.Status == Status.LocallyModified)
          {
            AbsoluteTopicName atn = new AbsoluteTopicName(); 
            atn.Name = topic.Name;
            atn.Namespace = topic.Namespace.Name; 
            string content = Utilities.GetTopicContent(topic); 
            proxy.SetTextForTopic(atn, content, identity); 

            if (Progress != null)
            {
              ProgressEventArgs pea = new ProgressEventArgs(EventType.Committed, 
                topic.Namespace, topic, topic.Status, Status.UpToDate); 
              Progress(this, pea); 
            }

            updated.Add(topic); 
          }
          else if (topic.Status == Status.InConflict)
          {
            if (Progress != null)
            {
              ProgressEventArgs pea = new ProgressEventArgs(EventType.ConflictSkipped, 
                topic.Namespace, topic, topic.Status, Status.InConflict); 
              Progress(this, pea); 
            }
          }
        }

        // TODO: There's a race condition here: if someone changes the version in the repository right after we 
        // check in our files, we'll think that we have the latest version, when we don't. The solution to this
        // would be to force SetTextForTopic to return the new version number. 
        SyncToRemote(); 

        foreach (LocalTopic topic in updated)
        {
          topic.BasedOnRepositoryVersion = topic.RepositoryVersion; 
          topic.BasedOnChecksum = topic.Checksum; 
        }
      }
      finally
      {
        FlushStatus(); 
      }
    }

    public void Resolve(ResolveConflictCallback rccb)
    {
      Resolve(rccb, new StringCollection()); 
    }

    public void Resolve(ResolveConflictCallback rccb, StringCollection files)
    {
      try
      {
        SyncToLocal();
        SyncToRemote(); 

        LocalTopicList topics = GetTopicsForFiles(files); 

        foreach (LocalTopic topic in topics)
        {
          if (topic.Status == Status.InConflict)
          {
            string remoteContents = proxy.GetTextForTopic(topic);
            string contents = rccb(topic, remoteContents); 

            if (contents != null)
            {
              topic.WriteContents(contents); 
              topic.BasedOnRepositoryVersion = topic.RepositoryVersion; 
            }

          }
        }
      }
      finally
      {
        FlushStatus();
      }
    }

    public void Update()
    {
      Update(new StringCollection()); 
    }

		public void Update(StringCollection files)
		{
      try
      {
        SyncToLocal(); 
        SyncToRemote(); 

        LocalTopicList topics = GetTopicsForFiles(files); 
      
        foreach (LocalTopic topic in topics)
        {
          if (topic.Status == Status.LocallyOutOfDate || topic.Status == Status.NoLocalFile)
          {
            UpdateTopic(topic); 
          }
        }
      }
      finally
      {
        FlushStatus();
      }								
		}

    public LocalTopicList GetTopicsForFiles(StringCollection files)
    {
      LocalTopicList topics = new LocalTopicList(); 

      if (files == null || files.Count == 0)
      {
        foreach (LocalNamespace ns in namespaces)
        {
          topics.AddRange(ns.Topics); 
        }
      }

      foreach (string file in files)
      {
        LocalTopicList found = GetTopicsForFile(file); 
        topics.AddRange(found); 
      }

      return topics; 
    }

    public static string CalculateBaseDir(StringCollection files)
    {
      string path = ".";
      if (files.Count > 0)
      {
        path = files[0];
      }

      path = Path.GetFullPath(path); 

      string parent = Path.GetDirectoryName(path); 

      if (Directory.Exists(path))
      {
        parent = path; 
      }

      string grandparent = Path.GetDirectoryName(parent); 

      if (File.Exists(Path.Combine(path, STATUS_FILENAME)))
      {
        return Path.GetFullPath(path); 
      }
      else if (File.Exists(Path.Combine(parent, STATUS_FILENAME)))
      {
        return Path.GetFullPath(parent); 
      }
      else if (File.Exists(Path.Combine(grandparent, STATUS_FILENAME)))
      {
        return Path.GetFullPath(grandparent); 
      }
      else
      {
        return Path.GetFullPath("."); 
      }
    
    }

		private void UpdateTopic(LocalTopic topic)
		{
			string dir = topic.Namespace.Directory; 

			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir); 
			}

			string contents = proxy.GetTextForTopic(topic); 
			
      Status oldStatus = topic.Status; 

      topic.WriteContents(contents); 

      topic.BasedOnRepositoryVersion = topic.RepositoryVersion; 
      topic.BasedOnChecksum = topic.Checksum;

      if (Progress != null)
      {
        ProgressEventArgs args = new ProgressEventArgs(EventType.UpdatedLocal, 
          topic.Namespace, topic, oldStatus, Status.UpToDate); 
        Progress(this, args); 
      }

		}

    
    private void FlushStatus()
    {
      SynchronizerState state = new SynchronizerState(); 
      state.Namespaces = namespaces; 
      state.ProxyUri = proxy.Uri; 

      XmlSerializer ser = new XmlSerializer(typeof(SynchronizerState)); 
      string path = Path.Combine(basedir, STATUS_FILENAME); 
      XmlWriter wtr = new XmlTextWriter(path, System.Text.Encoding.UTF8); 
      ser.Serialize(wtr, state); 
      wtr.Close();
    }
    private LocalTopicList GetTopicsForFile(string file)
    {
      LocalTopicList topics = new LocalTopicList(); 
      string path = Path.GetFullPath(file); 

      if (IsOutsideBaseDir(path))
      {
        string message = string.Format("Cannot work with files outside " + 
          "the base directory. " + 
          "Base directory is {0}, specified file is {1}", basedir, path); 
        throw new ApplicationException(message); 
      }

      // This isn't particularly efficient, but it's probably more than 
      // fast enough for our purposes. 
      foreach (LocalNamespace ns in namespaces)
      {
        if (string.Compare(path, ns.Directory, true) == 0)
        {
          topics.AddRange(ns.Topics); 
          return topics; 
        }

        foreach (LocalTopic topic in ns.Topics)
        {
          bool pathEqual = string.Compare(path, topic.Path, true) == 0; 
          bool wikiPathEqual = string.Compare(path + ".wiki", topic.Path, true) == 0; 
          if (pathEqual || wikiPathEqual)
          {
            topics.Add(topic); 
            return topics; 
          }
        }
      }

      return topics; 

    }

    private bool IsOutsideBaseDir(string path)
    {
      path = Path.GetFullPath(path); 

      if (path.StartsWith(basedir))
      {
        return false; 
      }
      else
      {
        return true; 
      }
    }
      
    private void ReadLocalStatusFile()
		{
			string path = Path.Combine(basedir, STATUS_FILENAME);

			if (!File.Exists(path))
			{
				namespaces = new LocalNamespaceList(); 
				return; 
			}

			XmlSerializer ser = new XmlSerializer(typeof(SynchronizerState)); 
			XmlReader rdr = new XmlTextReader(path); 
			SynchronizerState state = (SynchronizerState) ser.Deserialize(rdr);
			rdr.Close(); 

			this.namespaces = state.Namespaces; 

      // Hook the new objects up to each other so they can do things like calculate directories
      foreach (LocalNamespace ns in namespaces)
      {
        ns.Synchronizer = this; 
        foreach (LocalTopic topic in ns.Topics)
        {
          topic.Namespace = ns; 
        }
      }

      this.proxy.Uri = state.ProxyUri; 
		}

		
	}
}

