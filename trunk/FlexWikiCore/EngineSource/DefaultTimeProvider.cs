using System;

namespace FlexWiki
{
  /// <summary>
  /// The default implemenation of <see cref="ITimeProvider"/>, that simply uses
  /// <see cref="DateTime"/>. 
  /// </summary>
	public class DefaultTimeProvider : ITimeProvider
	{
    DateTime ITimeProvider.Now
    {
      get { return DateTime.Now; }
    }
	}
}
