using System;
using FlexWiki;
using System.Collections;

namespace FlexWiki.Web
{
	/// <summary>
	/// Summary description for UpdateMonitor.
	/// </summary>
	public class UpdateMonitor
	{
		Federation _TheFederation;

		public UpdateMonitor(Federation aFed)
		{
			_TheFederation = aFed;
		}

		Federation TheFederation
		{
			get
			{
				return _TheFederation;
			}
		}

		public void Start()
		{
			TheFederation.FederationUpdated += new Federation.FederationUpdateEventHandler(FederationUpdateMonitor);
			_Updates = new ArrayList();
		}

		void FederationUpdateMonitor(object sender, FederationUpdateEventArgs  e) 
		{
			UpdateInfo info = new UpdateInfo();
			info.Timestamp = DateTime.Now;
			info.Update = e.Updates;
			_Updates.Add(info);
			if (_Updates.Count > MAX_UPDATES_TO_HOLD)
				_Updates.RemoveAt(0);
		}

		const int MAX_UPDATES_TO_HOLD = 50;

		ArrayList _Updates;

		public void Stop()
		{
			TheFederation.FederationUpdated -= new Federation.FederationUpdateEventHandler(FederationUpdateMonitor);
		}

		public IList Updates
		{
			get
			{
				return _Updates;
			}
		}

		public class UpdateInfo
		{
			public DateTime				Timestamp;
			public FederationUpdate	Update;
			// TODO -- add other info like Requesting IP address, VisitorIdentity, etc.
		}

	}
}
