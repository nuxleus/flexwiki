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

namespace FlexWiki.Web
{
	/// <summary>
	/// Summary description for WebApplicationLogEventFactory.
	/// </summary>
	public class WebApplicationLogEventFactory : ILogEventFactory
	{
		WebApplicationLogSink Sink;

		public WebApplicationLogEventFactory(System.Web.HttpApplicationState state, string logPath)
		{
			Sink = new WebApplicationLogSink(state, logPath);
		}

		#region ILogEventFactory Members

		public LogEvent CreateEvent(string ipAddress, string user, string topic, LogEvent.LogEventType type)
		{
			lock (this)
			{
				LogEvent answer = new LogEvent(ipAddress, user, topic, type, Sink);
				return answer;
			}
		}

		public LogEvent CreateAndStartEvent(string ipAddress, string user, string topic, LogEvent.LogEventType type)
		{
			lock (this)
			{
				LogEvent answer = new LogEvent(ipAddress, user, topic, type, Sink);
				answer.Start();
				return answer;
			}
		}

		#endregion
	}
}
