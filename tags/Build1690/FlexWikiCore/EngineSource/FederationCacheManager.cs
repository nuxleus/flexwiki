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
using FlexWiki;
using System.Collections;

namespace FlexWiki
{
	/// <summary>
	/// Summary description for FederationCacheManager.
	/// </summary>
	public class FederationCacheManager
	{
		public FederationCacheManager(Federation aFed, IFederationCache cache)
		{
			// Hook the update events
			aFed.FederationUpdated += new Federation.FederationUpdateEventHandler(FederationUpdated);
			_Cache = cache;
		}

		void FederationUpdated(object sender, FederationUpdateEventArgs  e) 
		{
			FederationUpdate updates = e.Updates;

			ProcessFederationPropertiesChanged(updates);
			ProcessNamespaceListChanged(updates);
			ProcessChangedTopics(updates);
			ProcessChangedProperties(updates);
		}

		void Remove(string key)
		{
			Cache.Remove(key);
		}

		Set AllKeysToInvalidateForFederationPropertiesChanged = new Set();
		public void AddKeyToInvalidateOnFederationPropertiesChange(string key)
		{
			AllKeysToInvalidateForFederationPropertiesChanged.Add(key);
		}

		void ProcessFederationPropertiesChanged(FederationUpdate updates)
		{
			if (!updates.FederationPropertiesChanged)
				return;
			foreach (string each in AllKeysToInvalidateForFederationPropertiesChanged)
				Remove(each);
			AllKeysToInvalidateForFederationPropertiesChanged = new Set();
		}


		public void AddKeyToInvalidateOnFederationNamespacesChange(string key)
		{
			AllKeysToInvalidateForNamespaceListChanged.Add(key);
		}

		Set AllKeysToInvalidateForNamespaceListChanged = new Set();
		void ProcessNamespaceListChanged(FederationUpdate updates)
		{
			if (!updates.NamespaceListChanged)
				return;
			foreach (string each in AllKeysToInvalidateForNamespaceListChanged)
				Remove(each);
			AllKeysToInvalidateForNamespaceListChanged = new Set();
		}

		Hashtable _KeysToInvalidateOnTopicsInNamespaceChange = new Hashtable();
		Hashtable KeysToInvalidateOnTopicsInNamespaceChange
		{
			get
			{
				return _KeysToInvalidateOnTopicsInNamespaceChange;
			}
		}

		public void AddKeyToInvalidateOnTopicsInNamespaceChange(string key, string ns)
		{
			Set list = (Set)(KeysToInvalidateOnTopicsInNamespaceChange[ns]);
			if (list == null)
			{
				list = new Set();
				KeysToInvalidateOnTopicsInNamespaceChange[ns] = list;
			}
			list.Add(key);
		}

		void ProcessChangedTopics(FederationUpdate updates)
		{
			Set topics = new Set();
			topics.AddRange(updates.CreatedTopics);
			topics.AddRange(updates.UpdatedTopics);
			topics.AddRange(updates.DeletedTopics);

			foreach (AbsoluteTopicName topic in topics)
			{
				IList keysToInvalidate = KeysToInvalidateForTopic(topic);
				foreach (string each in keysToInvalidate)
					Remove(each);
				keysToInvalidate.Clear();
			}

			// if the list of topics in the namespace changed, invalidate appropriate keys
			Set namespaces = new Set();
			foreach (AbsoluteTopicName each in updates.CreatedTopics)
				namespaces.Add(each.Namespace);
			foreach (AbsoluteTopicName each in updates.DeletedTopics)
				namespaces.Add(each.Namespace);
			foreach (string each in namespaces)
			{
				// get the list of keys to invalidate for this namespace when its topic list changes
				IEnumerable list = (IEnumerable)(KeysToInvalidateOnTopicsInNamespaceChange[each]);
				if (list != null)
				{
					foreach (string k in list)
						Remove(k);
					KeysToInvalidateOnTopicsInNamespaceChange[each] = new Set();
				}
			}
		}

		void ProcessChangedProperties(FederationUpdate updates)
		{
			foreach (string propertyName in updates.AllAddedOrDeletedProperties)
			{
				string key = KeyForProperty(propertyName);
				Remove(key);		
				IList list = KeysToInvalidateForProperty(propertyName);
				foreach (string innerKey in list)
					Remove(innerKey);
				list.Clear();
			}
		}


		IFederationCache _Cache;
		IFederationCache Cache
		{
			get
			{
				return _Cache;
			}
		}

		Hashtable _TopicToKeys = new Hashtable();
		/// <summary>
		/// Answer a read-write list of the cache keys that should be invalidated for the given topic
		/// </summary>
		/// <param name="topic"></param>
		/// <returns></returns>
		public IList KeysToInvalidateForTopic(AbsoluteTopicName topic)
		{
			ArrayList answer = (ArrayList)(_TopicToKeys[topic]);
			if (answer != null)
				return answer;
			answer = new ArrayList();
			_TopicToKeys[topic] = answer;
			return answer;
		}

		public void AddKeyToInvalidateOnTopicChange(string key, AbsoluteTopicName name)
		{
			KeysToInvalidateForTopic(name).Add(key);
		}



		Hashtable _PropertyToKeys = new Hashtable();
		/// <summary>
		/// Answer a read-write list of the cache keys that should be invalidated for the given Property
		/// </summary>
		/// <param name="Property"></param>
		/// <returns></returns>
		public IList KeysToInvalidateForProperty(string property)
		{
			ArrayList answer = (ArrayList)(_PropertyToKeys[property]);
			if (answer != null)
				return answer;
			answer = new ArrayList();
			_PropertyToKeys[property] = answer;
			return answer;
		}

		public void AddKeyToInvalidateOnPropertyChange(string key, string propertyName)
		{
			KeysToInvalidateForProperty(propertyName).Add(key);
		}



		void Put(string key, object val, CacheRule rule)
		{
			Cache.Put(key, val, rule);
		}

		void Put(string key, object val)
		{
			Cache.Put(key, val);
		}

		object Get(string key)
		{
			return Cache.Get(key);
		}

		public CachedTopic GetCachedTopic(AbsoluteTopicName name)
		{
			return (CachedTopic)(Get(KeyForTopicInfo(name)));
		}

		public void PutCachedTopic(AbsoluteTopicName name, CachedTopic val, CacheRule rule)
		{
			string key = KeyForTopicInfo(name);
			Put(key, val, rule);
			rule.SetupInvalidation(this, key);
		}

		public TopicInfoArray GetTopicsWithProperty(string propertyName)
		{
			return (TopicInfoArray)(Get(KeyForProperty(propertyName)));
		}

		public void PutCachedTopicsWithProperty(string propertyName, TopicInfoArray val, CacheRule rule)
		{
			string key = KeyForProperty(propertyName);
			Put(key, val, rule);
			rule.SetupInvalidation(this, key);
		}



		public string GetCachedTopicFormattedBorder(AbsoluteTopicName name, Border border)
		{
			return (string)(Get(KeyForTopicFormattedBorder(name, border)));
		}

		public void PutCachedTopicFormattedBorder(AbsoluteTopicName name, Border border, string val, CacheRule rule)
		{
			string key = KeyForTopicFormattedBorder(name, border);
			Put(key, val, rule);
			rule.SetupInvalidation(this, key);
		}

		public string GetCachedTopicFormattedContent(AbsoluteTopicName name, AbsoluteTopicName withDiffsToThisTopic)
		{
			return (string)(Get(KeyForTopicFormattedContent(name, withDiffsToThisTopic)));
		}

		public IList GetCachedNamespaceHistory(string ns)
		{
			object list = Get(KeyForNamespaceHistory(ns));
			if (list == null)
				return null;
			return ArrayList.ReadOnly((IList)(list));
		}

		public void PutCachedNamespaceHistory(string ns, IList history, CacheRule rule)
		{
			string key = KeyForNamespaceHistory(ns);
			Put(key, history, rule);
			rule.SetupInvalidation(this, key);
		}

		public void PutCachedTopicFormattedContent(AbsoluteTopicName name, AbsoluteTopicName withDiffsToThisTopic, string val, CacheRule rule)
		{
			string key = KeyForTopicFormattedContent(name, withDiffsToThisTopic);
			Put(key, val, rule);
			rule.SetupInvalidation(this, key);
		}

		static string KeyForTopicFormattedBorder(AbsoluteTopicName name, Border border)
		{
			return "Formatted.Border." + name.FullnameWithVersion + "." + border.ToString();
		}

		static string KeyForTopicFormattedContent(AbsoluteTopicName name, AbsoluteTopicName withDiffsToThisTopic)
		{
			return "Formatted.Page." + name.FullnameWithVersion + ( (withDiffsToThisTopic != null) ? "/diffTo" + withDiffsToThisTopic.FullnameWithVersion : "");
		}

		static string KeyForTopicInfo(AbsoluteTopicName name)
		{
			return "TopicInfo." + name.FullnameWithVersion;
		}

		static string KeyForProperty(string property)
		{
			return "Property." + property;
		}

		static string KeyForNamespaceHistory(string ns)
		{
			return "NamespaceHistory." + ns;
		}



	}
}
