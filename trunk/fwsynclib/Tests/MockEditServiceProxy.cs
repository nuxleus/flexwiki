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

namespace FlexWiki.FwSyncLib
{
	public class MockEditServiceProxy : IEditServiceProxy
	{
		Hashtable namespaces = new Hashtable(); 
		private string uri; 

		public MockEditServiceProxy(string uri)
		{
			this.uri = uri; 
		}


    public string Uri
		{
			get { return uri; }
			set { uri = value; }
		}

		public ContentBase[] GetAllNamespaces()
		{
			ContentBase[] cbs = new ContentBase[namespaces.Count]; 

			int i = 0; 
			foreach (string ns in namespaces.Keys)
			{
				cbs[i] = new ContentBase(); 
				cbs[i].Namespace = ns; 
				++i; 
			}
			return cbs; 
		}

		public AbsoluteTopicName[] GetAllTopics(ContentBase cb)
		{
			Hashtable ns = (Hashtable) namespaces[cb.Namespace]; 
		
			ArrayList atns = new ArrayList(); 

			foreach (string topic in ns.Keys)
			{
				AbsoluteTopicName atn = new AbsoluteTopicName(); 
				atn.Namespace = cb.Namespace; 
				atn.Name = topic; 
				atn.Version = GetLatestVersion((Hashtable) ns[topic]); 
				atns.Add(atn); 
			}

			return (AbsoluteTopicName[]) atns.ToArray(typeof(AbsoluteTopicName)); 
		}

    public string GetLatestText(string nsName, string topicName)
    {
      Hashtable t = GetTopic(nsName, topicName);

      if (t != null)
      {
        string version = GetLatestVersion(t); 

        if (version != null)
        {
          return t[version] as string; 
        }
      }

      return null; 
    }

		public string GetTextForTopic(LocalTopic topic)
		{
      Hashtable t  = GetTopic(topic.Namespace.Name, topic.Name); 

			if (t != null)
			{
				return t[topic.RepositoryVersion] as string;
			}
			
			return null; 
		}

		public void SetTextForTopic(AbsoluteTopicName atn, string content, string identity)
		{
			Hashtable ns = (Hashtable) namespaces[atn.Namespace]; 
			Hashtable topic = (Hashtable) ns[atn.Name]; 

			if (topic == null)
			{
				Populate(atn.Namespace, atn.Name, "Version1", content); 
			}
			else
			{
				string version = GetLatestVersion(topic); 
				Populate(atn.Namespace, atn.Name, version + "PlusOne", content); 
			}
		}

		public void Populate(string ns, string topic, string version, string content)
		{
			if (namespaces[ns] == null)
			{
				namespaces[ns] = new Hashtable(); 
			}
			
			Hashtable nmspace = (Hashtable) namespaces[ns]; 

			if (nmspace[topic] == null)
			{
				nmspace[topic] = new Hashtable(); 
			}

			Hashtable tp = (Hashtable) nmspace[topic]; 

			tp[version] = content; 

		}

		private string GetLatestVersion(Hashtable topic)
		{
			string latest = null; 
			foreach (string version in topic.Keys)
			{
				if (latest == null || version.CompareTo(latest) > 0)
				{
					latest = version; 
				}
			}
			return latest; 
		}

    private Hashtable GetTopic(string nsName, string topicName)
    {
      Hashtable t = namespaces[nsName] as Hashtable; 

      if (t != null)
      {
        Hashtable t2 = t[topicName] as Hashtable; 

        return t2;
      }

      return null; 
    }


  }
}

