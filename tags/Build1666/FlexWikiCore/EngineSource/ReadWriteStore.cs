using System;

namespace FlexWiki
{
	/// <summary>
	/// Summary description for ReadWriteStore.
	/// </summary>
	public abstract class ReadWriteStore : ContentBase
	{
		override public bool IsReadOnly
		{
			get
			{
				return false;
			}
		}
	}
}
