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
using System.Configuration;
using System.Web.Mail;
using System.Web;
using System.Threading;
using System.Text;
using FlexWiki.Web;

namespace FlexWiki.Newsletters
{
	/// <summary>
	/// 
	/// </summary>
	public class NewsletterDaemon
	{
		public NewsletterDaemon(string configFilePath, string rootURL, string newslettersFrom, 
			string headInsert) : this(configFilePath, rootURL, newslettersFrom, headInsert, false)
		{}

		public NewsletterDaemon(string configFilePath, string rootURL, string newslettersFrom, 
			string headInsert, bool sendAsAttachments)
		{
			ConfigurationFilePath = configFilePath;
			RootURL = rootURL;
			_NewslettersFrom = newslettersFrom;
			_HeadInsert = headInsert;
			_sendAsAttachments = sendAsAttachments; 
		}

		string ConfigurationFilePath;
		string RootURL;
		string _NewslettersFrom;
		string _HeadInsert;
		bool _sendAsAttachments; 

		Thread MonitorThread;

		public void EnsureRunning()
		{
			if (MonitorThread != null && MonitorThread.IsAlive)
				return;

			lock (this)
			{
				if (MonitorThread == null || !(MonitorThread.IsAlive))
				{
					Start();
				}
				else
				{
					MakeSureThreadHasRecentlyCheckedIn();
				}
			}
		}

		public DateTime LastCheckin = DateTime.MinValue;
		public DateTime Started = DateTime.MinValue;

		void MakeSureThreadHasRecentlyCheckedIn()
		{
			// TODO
		}

		const int CheckinInterval = 5000;
		const int WorkInterval = 10000;

		public DateTime NextWorkDue = DateTime.MinValue;

		void ThreadProc() 
		{
			while (true)
			{
				lock (this)
				{
					Checkin();
					DoWorkIfItIsTime();
				}
				Thread.Sleep(CheckinInterval);
			}
		}

		void DoWorkIfItIsTime()
		{
			if (NextWorkDue > DateTime.Now)
				return;
			NextWorkDue = DateTime.Now.AddMilliseconds(WorkInterval);
			DoWork();
		}

		public bool WorkUnderway = false;

		void DoWork()
		{
			try
			{
				if (WorkUnderway)
					return;
				WorkUnderway = true;
				WorkLastStarted = DateTime.Now;
				ReallyDoWork();
			}
			finally
			{
				WorkLastCompleted = DateTime.Now;			
				WorkUnderway = false;
			}
		}

		public DateTime WorkLastStarted = DateTime.MinValue;
		public DateTime WorkLastCompleted = DateTime.MinValue;

		Federation _TheFederation;
		Federation TheFederation
		{
			get
			{
				if (_TheFederation != null)
					return _TheFederation;
				LinkMaker lm = new LinkMaker(RootURL);
				_TheFederation = new Federation(ConfigurationFilePath, FlexWiki.Formatting.OutputFormat.HTML, lm);
				_TheFederation.LogEventFactory = LogEventFactory;		
				_TheFederation.FederationCache = new Cache();
				return _TheFederation;
			}
		}


		public class Cache : IFederationCache
		{
			Hashtable Hash = new Hashtable();

			#region IFederationCache Members

			class Entry
			{
				public object Value;
				public CacheRule Rule;
				public Entry(object v)
				{
					Value = v;
				}
				public Entry(object v, CacheRule r)
				{
					Value = v;
					Rule = r;
				}
			}

			public object this[string key]
			{
				get
				{
					Entry e = (Entry)Hash[key];
					if (e == null)
						return null;
					return e.Value;
				}
				set
				{
					Hash[key] = new Entry(value);
				}
			}

			public void Put(string key, object val, CacheRule rule)
			{
				Hash[key] = new Entry(val, rule);
			}

			void FlexWiki.IFederationCache.Put(string key, object val)
			{
				this[key] = val;
			}

			public CacheRule GetRuleForKey(string key)
			{
				Entry e = (Entry)Hash[key];
				if (e == null)
					return null;
				return e.Rule;
			}

			public object Get(string key)
			{
				return this[key];
			}

			public System.Collections.ICollection Keys
			{
				get
				{
					return Hash.Keys;
				}
			}

			#endregion
		}




		public ILogEventFactory _LogEventFactory;

		public ILogEventFactory LogEventFactory
		{
			get
			{
				return _LogEventFactory;
			}
			set
			{
				_LogEventFactory = value;
			}
		}


		void ReallyDoWork()
		{
			DaemonBasedDeliveryBoy boy;
			boy = new DaemonBasedDeliveryBoy(SMTPServer, SMTPUser, SMTPPassword, _sendAsAttachments);
			LinkMaker lm = new LinkMaker(RootURL);
			NewsletterManager manager = new NewsletterManager(TheFederation, lm , boy, _NewslettersFrom, _HeadInsert);
			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);
			boy.Log = sw;
			LogEvent ev = TheFederation.LogEventFactory.CreateAndStartEvent(null, null, null, LogEvent.LogEventType.NewsletterGeneration);
			sw.WriteLine("Begin: " + DateTime.Now.ToString());
			try
			{
				manager.Notify(sw);
			}
			finally
			{
				ev.Record();
				sw.WriteLine("End: " + DateTime.Now.ToString());
				LogResult(sb.ToString());
			}
		}

		public IEnumerable Results
		{
			get
			{
				return _Results;
			}
		}

		const int MaxResults = 10;

		void LogResult(string s)
		{
			_Results.Insert(0, s);
			while (_Results.Count > MaxResults)
				_Results.RemoveAt(_Results.Count - 1);
		}

		ArrayList _Results = new ArrayList();

		public string SMTPServer;
		public string SMTPUser;
		public string SMTPPassword;

		protected class DaemonBasedDeliveryBoy : IDeliveryBoy
		{
			string SMTPHost;
			string SMTPUser;
			string SMTPPassword;
			bool sendAsAttachments; 

			public DaemonBasedDeliveryBoy(string smtphost, string smtpuser, string smtppass)
				: this(smtphost, smtpuser, smtppass, false)
			{
			}

			public DaemonBasedDeliveryBoy(string smtphost, string smtpuser, string smtppass, bool sendAsAttachments)
			{
				SMTPHost = smtphost;
				SMTPUser = smtpuser;
				SMTPPassword = smtppass;
				this.sendAsAttachments = sendAsAttachments; 
			}

			public StringWriter Log;

			public void Deliver(string to, string from, string subject, string body)
			{
				FlexWiki.Newsletters.SmtpMail mailer = new FlexWiki.Newsletters.SmtpMail();
				StringWriter smtpLog = new StringWriter(new StringBuilder());
				mailer.sw = smtpLog;
				MailMessage message = new MailMessage();
				message.To = to;
				message.From = from;
				message.Subject = subject;
				if (!sendAsAttachments)
				{
					message.Body = body;
					message.BodyFormat = MailFormat.Html;
				}
				else
				{
					message.Body = "The wiki has been updated. See the attached files for changes."; 
				}

				string success = null;
				try
				{
					if (!sendAsAttachments)
					{ 
						success = mailer.Send(message, SMTPHost, SMTPUser, SMTPPassword);
					}
					else
					{
						Hashtable atts = new Hashtable(); 
						atts.Add("Changes.htm", System.Text.Encoding.UTF8.GetBytes(body)); 
						success = mailer.Send(message, SMTPHost, SMTPUser, SMTPPassword, atts); 
					}
				}
				catch (Exception e)
				{
					Log.WriteLine("Exception while sending to host '" + SMTPHost + "': " + e.ToString());
				}
				if (success == null)
				{
					Log.WriteLine("SMTP deliver succeeded to: " + to);
				}
				else
				{
					Log.WriteLine("SMTP deliver failed to: " + to);
					Log.Write(smtpLog.ToString());
				}
			}
		}

		void Start()
		{
			bool newslettersDisabled = false; 

			try
			{
				newslettersDisabled = bool.Parse(ConfigurationSettings.AppSettings["DisableNewsletters"]); 
			}
			catch
			{
			}
			if (newslettersDisabled)
				return;
			MonitorThread = new Thread(new ThreadStart(ThreadProc));
			MonitorThread.Start();
			Started = DateTime.Now;
		}

		void Checkin()
		{  
			LastCheckin = DateTime.Now;
		}

	}
}
