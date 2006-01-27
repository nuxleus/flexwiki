using System;

namespace FlexWiki
{
	/// <summary>
	/// Summary description for TopicsUpdateEventArgs.
	/// </summary>
	public class TopicsUpdateEventArgs : EventArgs
	{
		TopicUpdateBatch _Updates;

		public TopicUpdateBatch Updates
		{
			get
			{
				return _Updates;
			}
		}

		public TopicsUpdateEventArgs(TopicUpdateBatch batch)
		{
			_Updates = batch;
		}

	}
}
