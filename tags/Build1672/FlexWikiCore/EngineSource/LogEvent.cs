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
using System.Collections;
using FlexWiki;
using System.IO;
using System.Text.RegularExpressions;

namespace FlexWiki
{
	/// <summary>
	/// Summary description for LogEvent.
	/// </summary>
	public class LogEvent
	{

		public enum LogEventType
		{
			ReadTopic,
			WriteTopic, 
			DeleteTopic,
			NewsletterGeneration
		};

		public LogEvent()
		{
		}

		bool Running = false;

		public void Start()
		{
			Running = true;
			StartTime = DateTime.Now;
		}

		public void Stop()
		{
			Running = false;
			EndTime = DateTime.Now;
		}

		public LogEvent(string ipAddress, string user, string topic, LogEventType type, ILogSink sink)
		{
			IP = ipAddress;
			User = user;
			Topic = topic;
			Type = type;
			Sink = sink;
		}

		public void Record()
		{
			if (Running)
				Stop();
			Sink.Log(this);
		}

		ILogSink Sink;

		public void WriteToStream(StreamWriter s)
		{
			WriteCSVField(s, User);
			s.Write(",");
			WriteCSVField(s, StartTime.ToString());
			s.Write(",");
			WriteCSVField(s, EndTime.ToString());
			s.Write(",");
			WriteCSVField(s, Duration.ToString());
			s.Write(",");
			WriteCSVField(s, TypeString());
			s.Write(",");
			WriteCSVField(s, Topic);
			s.Write(",");
			WriteCSVField(s, IP);
			s.WriteLine("");
			s.Flush();
		}

		public double Duration
		{
			get
			{
				if (EndTime == DateTime.MinValue)
					return 0;
				if (StartTime == DateTime.MinValue)
					return 0;
				TimeSpan span = EndTime.Subtract(StartTime);
				return span.TotalMilliseconds;
			}
		}

		protected string TypeString()
		{
			string action = "";

			switch (Type)
			{
				case LogEvent.LogEventType.ReadTopic:
					action = "read";
					break;

				case LogEvent.LogEventType.WriteTopic:
					action = "write";
					break;

				case LogEvent.LogEventType.DeleteTopic:
					action = "delete";
					break;

				case LogEvent.LogEventType.NewsletterGeneration:
					action = "newsletter";
					break;
			}

			return action;
		}

		protected void WriteCSVField(StreamWriter s, string v)
		{
			s.Write("\"" + v + "\"");
		}

		public string User;
		public DateTime StartTime = DateTime.MinValue;
		public DateTime EndTime = DateTime.MinValue;
		public string Topic;
		public LogEventType Type;
		public string IP;

	}
}
