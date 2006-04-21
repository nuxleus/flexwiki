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
using System.Collections;
using System.Collections.Specialized;


namespace FlexWiki
{
	/// <summary>
	/// Summary description for FederationUpdate.
	/// </summary>
	public class FederationUpdate 
	{
		public FederationUpdate()
		{
		}

		public enum PropertyChangeType
		{
			PropertyAdd,
			PropertyRemove,
			PropertyUpdate
		};

		// key = topic
		// value = iEnum of property names
		HybridDictionary _AddedPropertiesByTopic = new HybridDictionary();
		HybridDictionary _RemovedPropertiesByTopic = new HybridDictionary();
		HybridDictionary _ChangedPropertiesByTopic = new HybridDictionary();

		// key = propertyname
		// value = iEnum of topic names
		HybridDictionary _AddedPropertiesByProperty = new HybridDictionary();
		HybridDictionary _RemovedPropertiesByProperty = new HybridDictionary();
		HybridDictionary _ChangedPropertiesByProperty = new HybridDictionary();

		public void RecordPropertyChange(AbsoluteTopicName topic, string propertyName, PropertyChangeType aType)
		{
			switch (aType)
			{
				case PropertyChangeType.PropertyAdd:
					AddToDictionaries(_AddedPropertiesByProperty, _AddedPropertiesByTopic, topic, propertyName);
					break;

				case PropertyChangeType.PropertyRemove:
					AddToDictionaries(_RemovedPropertiesByProperty, _RemovedPropertiesByTopic, topic, propertyName);
					break;

				case PropertyChangeType.PropertyUpdate:
					AddToDictionaries(_ChangedPropertiesByProperty, _ChangedPropertiesByTopic, topic, propertyName);
					break;
			}
		}

		public void RecordNamespaceListChanged()
		{
			NamespaceListChanged = true;
		}

		public void RecordFederationPropertiesChanged()
		{
			FederationPropertiesChanged = true;
		}

		public void AddUpdatesFrom(FederationUpdate src)
		{
			foreach (AbsoluteTopicName top in src.CreatedTopics)
				RecordCreatedTopic(top);
			foreach (AbsoluteTopicName top in src.DeletedTopics)
				RecordDeletedTopic(top);			
			foreach (AbsoluteTopicName top in src.UpdatedTopics)
				RecordUpdatedTopic(top);
			foreach (DictionaryEntry e in src._AddedPropertiesByTopic)
				_AddedPropertiesByTopic[e.Key] = e.Value;
			foreach (DictionaryEntry e in src._RemovedPropertiesByTopic)
				_RemovedPropertiesByTopic[e.Key] = e.Value;
			foreach (DictionaryEntry e in src._ChangedPropertiesByTopic)
				_ChangedPropertiesByTopic[e.Key] = e.Value;
			foreach (DictionaryEntry e in src._AddedPropertiesByProperty)
				_AddedPropertiesByProperty[e.Key] = e.Value;
			foreach (DictionaryEntry e in src._RemovedPropertiesByProperty)
				_RemovedPropertiesByProperty[e.Key] = e.Value;
			foreach (DictionaryEntry e in src._ChangedPropertiesByProperty)
				_ChangedPropertiesByProperty[e.Key] = e.Value;
			if (src.FederationPropertiesChanged)
				RecordFederationPropertiesChanged();
			if (src.NamespaceListChanged)
				RecordNamespaceListChanged();
		}

		void AddToDictionaries(HybridDictionary byProperty, HybridDictionary byTopic, AbsoluteTopicName topic, string prop)
		{
			ArrayList tList;
			ArrayList pList;

			tList = (ArrayList)(byProperty[prop]);
			if (tList == null)
			{
				tList = new ArrayList();
				byProperty[prop] = tList;
			}
			tList.Add(topic);

			pList = (ArrayList)(byTopic[topic]);
			if (pList == null)
			{
				pList = new ArrayList();
				byTopic[topic] = pList;
			}
			pList.Add(prop);
		}

		public IList AllTopicsWithChangedProperties
		{
			get
			{
				Set s = new Set();
				s.AddRange(_AddedPropertiesByTopic.Keys);
				s.AddRange(_ChangedPropertiesByTopic.Keys);
				s.AddRange(_RemovedPropertiesByTopic.Keys);
				ArrayList answer = new ArrayList();
				foreach (object each in s)
					answer.Add(each);
				return answer;
			}
		}

		public IList AllProperties
		{
			get
			{
				Set s = new Set();
				s.AddRange(_AddedPropertiesByProperty.Keys);
				s.AddRange(_ChangedPropertiesByProperty.Keys);
				s.AddRange(_RemovedPropertiesByProperty.Keys);
				ArrayList answer = new ArrayList();
				foreach (object each in s)
					answer.Add(each);
				return answer;
			}
		}

		public IList AllAddedOrDeletedProperties
		{
			get
			{
				Set s = new Set();
				s.AddRange(_AddedPropertiesByProperty.Keys);
				s.AddRange(_RemovedPropertiesByProperty.Keys);
				ArrayList answer = new ArrayList();
				foreach (object each in s)
					answer.Add(each);
				return answer;
			}
		}

		ArrayList _EmptyList = new ArrayList();

		public IList RemovedPropertiesForTopic(AbsoluteTopicName topic)
		{
			IList answer = (IList)(_RemovedPropertiesByTopic[topic]);
			if (answer == null)
				return _EmptyList;
			return answer;
		}

		public IList AddedPropertiesForTopic(AbsoluteTopicName topic)
		{
			IList answer = (IList)(_AddedPropertiesByTopic[topic]);
			if (answer == null)
				return _EmptyList;
			return answer;
		}

		public IList ChangedPropertiesForTopic(AbsoluteTopicName topic)
		{
			IList answer = (IList)(_ChangedPropertiesByTopic[topic]);
			if (answer == null)
				return _EmptyList;
			return answer;
		}

		ArrayList _CreatedTopics = new ArrayList();

		public IList CreatedTopics
		{
			get
			{
				return _CreatedTopics;
			}
		}

		public void RecordCreatedTopic(AbsoluteTopicName name)
		{
			if (_CreatedTopics.Contains(name))
				return;
			_CreatedTopics.Add(name);
		}

		ArrayList _DeletedTopics = new ArrayList();

		public IList DeletedTopics
		{
			get
			{
				return _DeletedTopics;
			}
		}

		public void RecordDeletedTopic(AbsoluteTopicName name)
		{
			if (_DeletedTopics.Contains(name))
				return;
			_DeletedTopics.Add(name);
		}

		ArrayList _UpdatedTopics = new ArrayList();

		public IList UpdatedTopics
		{
			get
			{
				return _UpdatedTopics;
			}
		}

		public void RecordUpdatedTopic(AbsoluteTopicName name)
		{
			if (_UpdatedTopics.Contains(name))
				return;
			_UpdatedTopics.Add(name);
		}

		bool _NamespaceListChanged = false;
		public bool NamespaceListChanged
		{
			get
			{
				return _NamespaceListChanged;
			}
			set
			{
				_NamespaceListChanged = value;
			}
		}

		bool _FederationPropertiesChanged = false;
		public bool FederationPropertiesChanged
		{
			get
			{
				return _FederationPropertiesChanged;
			}
			set
			{
				_FederationPropertiesChanged = value;
			}
		}
	}
}
