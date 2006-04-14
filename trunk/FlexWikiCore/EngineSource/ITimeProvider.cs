using System;

namespace FlexWiki
{
	public interface ITimeProvider
	{
    DateTime Now { get; }
	}
}
