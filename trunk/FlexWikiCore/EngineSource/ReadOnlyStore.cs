using System;

namespace FlexWiki
{
	/// <summary>
	/// Summary description for ReadOnlyStore.
	/// </summary>
	public abstract class ReadOnlyStore : ContentBase
	{
		override public bool IsReadOnly
		{
			get
			{
				return true;
			}
		}

		public override bool IsExistingTopicWritable(LocalTopicName topic)
		{
			return false;
		}


		public override void DeleteTopic(LocalTopicName topic)
		{
			throw ReadOnlyException();
		}

		public override void Delete()
		{
			throw ReadOnlyException();
		}

		public override System.Collections.ArrayList RenameTopic(LocalTopicName oldName, string newName, bool fixup)
		{
			throw ReadOnlyException();
		}

		protected override void WriteTopic(LocalTopicName topic, string content, FederationUpdateGenerator gen)
		{
			throw ReadOnlyException();
		}


		Exception ReadOnlyException()
		{
			return new InvalidOperationException("The request operation is not valid for a read-only namespace.");
		}


	}
}
