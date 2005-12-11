using System;

namespace FlexWiki
{
	/// <summary>
	/// Summary description for TopicPropertiesUpdateEventArgs.
	/// </summary>
	public class TopicPropertiesUpdateEventArgs : EventArgs
	{
		TopicPropertyUpdateBatch _Updates;

		public TopicPropertyUpdateBatch Updates
		{
			get
			{
				return _Updates;
			}
		}

		public TopicPropertiesUpdateEventArgs(TopicPropertyUpdateBatch batch)
		{
			_Updates = batch;
		}

	}
}
