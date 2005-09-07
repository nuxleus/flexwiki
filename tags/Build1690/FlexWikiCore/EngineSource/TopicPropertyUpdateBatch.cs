using System;
using System.Collections;
using System.Collections.Specialized;


namespace FlexWiki
{
	/// <summary>
	/// Summary description for TopicPropertyUpdateBatch.
	/// </summary>
	public class TopicPropertyUpdateBatch
	{
		public TopicPropertyUpdateBatch()
		{
		}

		public enum ChangeType
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

		public void RecordChange(AbsoluteTopicName topic, string propertyName, ChangeType aType)
		{
			switch (aType)
			{
				case ChangeType.PropertyAdd:
					AddToDictionaries(_AddedPropertiesByProperty, _AddedPropertiesByTopic, topic, propertyName);
					break;

				case ChangeType.PropertyRemove:
					AddToDictionaries(_RemovedPropertiesByProperty, _RemovedPropertiesByTopic, topic, propertyName);
					break;

				case ChangeType.PropertyUpdate:
					AddToDictionaries(_ChangedPropertiesByProperty, _ChangedPropertiesByTopic, topic, propertyName);
					break;
			}
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

		public bool IsEmpty
		{
			get
			{
				return (_AddedPropertiesByProperty.Count + _RemovedPropertiesByProperty.Count + _ChangedPropertiesByProperty.Count) == 0;
			}
		}

		public IList AllTopics
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

	}
}
