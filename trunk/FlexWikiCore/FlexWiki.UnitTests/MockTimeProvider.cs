using System;

namespace FlexWiki.UnitTests
{
	/// <summary>
	/// Provides an implementation of <see cref="ITimeProvider"/> for use during 
	/// unit tests. 
	/// </summary>
	public class MockTimeProvider : ITimeProvider
	{
		public MockTimeProvider(TimeSpan interval)
		{
      _interval = interval; 
    }

    private TimeSpan _interval; 
    private DateTime _now = new DateTime(2004, 10, 28, 14, 10, 59); 

    public DateTime Now
    {
      get
      {
        _now += _interval; 
        return _now; 
      }
    }

  }
}
