#region License Statement
// Copyright (c) Microsoft Corporation.  All rights reserved.
//
// The use and distribution terms for this software are covered by the 
// Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file CPL.TXT at the root of this distribution.
// By using this software in any fashion, you are agreeing to be bound by 
// the terms of this license.
//
// You must not remove this notice, or any other, from this software.
#endregion
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
		private Federation _Federation;

		public UpdateMonitor(Federation aFed)
		{
			_Federation = aFed;
		}

		private Federation Federation
		{
			get
			{
				return _Federation;
			}
		}

		public void Start()
		{
			Federation.FederationUpdated += new FederationUpdateEventHandler(FederationUpdateMonitor);
			_Updates = new ArrayList();
		}

		private void FederationUpdateMonitor(object sender, FederationUpdateEventArgs  e) 
		{
			UpdateInfo info = new UpdateInfo();
			info.Timestamp = DateTime.Now;
			info.Update = e.Updates;
			_Updates.Add(info);
			if (_Updates.Count > MAX_UPDATES_TO_HOLD)
				_Updates.RemoveAt(0);
		}

		private const int MAX_UPDATES_TO_HOLD = 50;

		private ArrayList _Updates;

		public void Stop()
		{
			Federation.FederationUpdated -= new FederationUpdateEventHandler(FederationUpdateMonitor);
		}

		public IList Updates
		{
			get
			{
				return _Updates;
			}
		}


	}
}
