using System;
using System.Collections;

namespace FlexWiki
{
	/// <summary>
	/// This class describes changes to a set of topics, including a list of topics Created and also specific property adds, removes and changes.
	/// </summary>
	public class TopicUpdateBatch
	{
		public TopicUpdateBatch()
		{
		}

		ArrayList _CreatedTopics = new ArrayList();

		public IList CreatedTopics
		{
			get
			{
				return _CreatedTopics;
			}
		}

		public void AddCreatedTopic(AbsoluteTopicName name)
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

		public void AddDeletedTopic(AbsoluteTopicName name)
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

		public void AddUpdatedTopic(AbsoluteTopicName name)
		{
			if (_UpdatedTopics.Contains(name))
				return;
			_UpdatedTopics.Add(name);
		}
	}
}
