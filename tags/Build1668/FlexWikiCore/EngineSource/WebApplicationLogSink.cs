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
using System.IO;
using System.Collections;

namespace FlexWiki
{
	/// <summary>
	/// Summary description for WebApplicationLogSink.
	/// </summary>
	public class WebApplicationLogSink : ILogSink
	{
		System.Web.HttpApplicationState State;
		string _LogFile;

		public WebApplicationLogSink(System.Web.HttpApplicationState state, string file)
		{
			State = state;
			LogFile = file;
		}

		public static ArrayList LogEventsForApplication(System.Web.HttpApplicationState app)
		{
			ArrayList answer = (ArrayList)app["LogEvents"];
			if (answer != null)
				return answer;
			answer = new ArrayList();
			app["LogEvents"] = answer;
			return answer;
		}

		public string LogFile
		{
			get
			{
				return _LogFile;
			}
			set
			{
				if (_LogFile == value)
					return;
				StreamWriter s = (StreamWriter)State["LogStream"];
				if (s != null)
					s.Close();
				_LogFile = value;
				if (value == null)
					return;
				s = new StreamWriter(new FileStream(_LogFile, FileMode.Append, FileAccess.Write, FileShare.Read));
				State["LogStream"] = s;
			}
		}

		#region ILogSink Members

		public void Log(LogEvent ev)
		{
			lock (this)
			{
				LogEventsForApplication(State).Add(ev);
				StreamWriter wr =  (StreamWriter)State["LogStream"];
				if (wr != null)
					ev.WriteToStream(wr);
			}
		}

		#endregion
	}
}
