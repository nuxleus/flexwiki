using System;
using System.Collections;

namespace FlexWiki
{
	/// <summary>
	/// Summary description for TopicInfoArray.
	/// </summary>
	[ExposedClass("TopicInfoArray", "Provides an array of topic info")]
	public class TopicInfoArray: BELArray
	{
		public TopicInfoArray(): base()
		{
		}

		public TopicInfoArray(ArrayList list): base(list)
		{
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer with the intersection of topics with the supplied array of topics (no duplicates).")]
		public TopicInfoArray Intersect(ArrayList topicInfoArrayToIntersect)
		{
			TopicInfoArray answer = new TopicInfoArray();
			this.Array.Sort(null);
			topicInfoArrayToIntersect.Sort(null);

			foreach(TopicInfo topicInfo in this.Array)
			{
				if(topicInfoArrayToIntersect.BinarySearch(topicInfo) >= 0)
				{
					if( answer.Array.BinarySearch(topicInfo) < 0 )
					{
						answer.Add(topicInfo);
					}
				}
			}

			return answer;
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer with the union of topics with the supplied array of topics (no duplicates).")]
		public TopicInfoArray Union(ArrayList topicInfoArrayToIntersect)
		{
			TopicInfoArray answer = new TopicInfoArray();
			this.Array.Sort(null);
			topicInfoArrayToIntersect.Sort(null);

			foreach(TopicInfo topicInfo in this.Array)
			{
				if( answer.Array.BinarySearch(topicInfo) < 0 )
				{
					answer.Add(topicInfo);
				}
			}

			foreach(TopicInfo topicInfo in topicInfoArrayToIntersect)
			{
				if( answer.Array.BinarySearch(topicInfo) < 0 )
				{
					answer.Add(topicInfo);
				}
			}

			return answer;
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer with the difference of topics with the supplied array of topics (no duplicates).")]
		public TopicInfoArray Difference(ArrayList topicInfoArrayToIntersect)
		{
			TopicInfoArray answer = new TopicInfoArray();
			this.Array.Sort(null);
			topicInfoArrayToIntersect.Sort(null);

			foreach(TopicInfo topicInfo in this.Array)
			{
				if(topicInfoArrayToIntersect.BinarySearch(topicInfo) < 0)
				{
					if( answer.Array.BinarySearch(topicInfo) < 0 )
					{
						answer.Add(topicInfo);
					}
				}
			}

			return answer;
		}

	}
}
