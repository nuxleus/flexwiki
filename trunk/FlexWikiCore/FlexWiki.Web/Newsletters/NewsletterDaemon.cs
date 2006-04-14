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
using FlexWiki;

namespace FlexWiki.Web.Newsletters
{
    /// <summary>
    /// 
    /// </summary>
    public class NewsletterDaemon
    {
        private const int c_checkinInterval = 5000;
        private const int c_maxResults = 30;
        private const int c_workInterval = 60000;

        private Federation _federation;
        private string _headInsert;
        private DateTime _lastCheckin = DateTime.MinValue;
        private Thread _monitorThread;
        private string _newslettersFrom;
        private DateTime _nextWorkDue = DateTime.MinValue;
        private readonly ArrayList _Results = new ArrayList();
        private string _rootUrl;
        private bool _sendAsAttachments;
        private string _smtpPassword;
        private string _smtpServer;
        private string _smtpUser;

        private DateTime _started = DateTime.MinValue;
        private DateTime _workLastCompleted = DateTime.MinValue;
        private DateTime _workLastStarted = DateTime.MinValue;
        private bool _workUnderway = false;

        
        public NewsletterDaemon(Federation fed, string rootURL, string newslettersFrom,
            string headInsert)
            : this(fed, rootURL, newslettersFrom, headInsert, false)
        { }

        public NewsletterDaemon(Federation fed, string rootURL, string newslettersFrom,
            string headInsert, bool sendAsAttachments)
        {
            _federation = fed;
            _rootUrl = rootURL;
            _newslettersFrom = newslettersFrom;
            _headInsert = headInsert;
            _sendAsAttachments = sendAsAttachments;
        }



        public DateTime LastCheckin
        {
            get { return _lastCheckin; }
            set { _lastCheckin = value; }
        }
        public ILogEventFactory LogEventFactory
        {
            get
            {
                return Federation.LogEventFactory;
            }
        }
        public DateTime NextWorkDue
        {
            get { return _nextWorkDue; }
            set { _nextWorkDue = value; }
        }
        public IEnumerable Results
        {
            get
            {
                return _Results;
            }
        }
        public string SmtpPassword
        {
            get { return _smtpPassword; }
            set { _smtpPassword = value; }
        }
        public string SmtpServer
        {
            get { return _smtpServer; }
            set { _smtpServer = value; }
        }
        public string SmtpUser
        {
            get { return _smtpUser; }
            set { _smtpUser = value; }
        }
        public DateTime Started
        {
            get { return _started; }
            set { _started = value; }
        }
        public DateTime WorkLastCompleted
        {
            get { return _workLastCompleted; }
            set { _workLastCompleted = value; }
        }
        public DateTime WorkLastStarted
        {
            get { return _workLastStarted; }
            set { _workLastStarted = value; }
        }
        public bool WorkUnderway
        {
            get { return _workUnderway; }
            set { _workUnderway = value; }
        }

        private Federation Federation
        {
            get
            {
                return _federation;
            }
        }

        public void EnsureRunning()
        {
            if (_monitorThread != null && _monitorThread.IsAlive)
            {
                return;
            }

            lock (this)
            {
                if (_monitorThread == null || !(_monitorThread.IsAlive))
                {
                    Start();
                }
                else
                {
                    MakeSureThreadHasRecentlyCheckedIn();
                }
            }
        }

        private void Checkin()
        {
            _lastCheckin = DateTime.Now;
        }
        private void DoWork()
        {
            try
            {
                if (WorkUnderway)
                    return;
                WorkUnderway = true;
                _workLastStarted = DateTime.Now;
                ReallyDoWork();
            }
            finally
            {
                _workLastCompleted = DateTime.Now;
                WorkUnderway = false;
            }
        }
        private void DoWorkIfItIsTime()
        {
            if (_nextWorkDue > DateTime.Now)
                return;
            DoWork();
            _nextWorkDue = DateTime.Now.AddMilliseconds(c_workInterval);
        }
        private void LogResult(StringBuilder b)
        {
            _Results.Insert(0, b);
            while (_Results.Count > c_maxResults)
                _Results.RemoveAt(_Results.Count - 1);
        }
        private void MakeSureThreadHasRecentlyCheckedIn()
        {
            // TODO
        }
        private void ReallyDoWork()
        {
            DaemonBasedDeliveryBoy boy;
            boy = new DaemonBasedDeliveryBoy(SmtpServer, SmtpUser, SmtpPassword, _sendAsAttachments);
            LinkMaker lm = new LinkMaker(_rootUrl);
            NewsletterManager manager = new NewsletterManager(Federation, lm, boy, _newslettersFrom, _headInsert);
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            boy.Log = sw;
            LogEvent ev = Federation.LogEventFactory.CreateAndStartEvent(null, null, null, LogEventType.NewsletterGeneration);
            LogResult(sb);
            sw.WriteLine("Begin: " + DateTime.Now.ToString());
            sw.WriteLine("Thread: " + Thread.CurrentThread.Name);
            try
            {
                manager.Notify(sw);
            }
            finally
            {
                ev.Record();
                sw.WriteLine("End: " + DateTime.Now.ToString());
            }
            // Append to the newsletters details file:

            Federation.Application.AppendToLog("Newsletter.details", sb.ToString());
        }
        private void Start()
        {
            bool newslettersDisabled = false;

            try
            {
                newslettersDisabled = bool.Parse(ConfigurationManager.AppSettings["DisableNewsletters"]);
            }
            catch
            {
            }
            if (newslettersDisabled)
                return;
            _monitorThread = new Thread(new ThreadStart(ThreadProc));
            _monitorThread.Name = "Newsletter " + DateTime.Now;
            _monitorThread.Priority = ThreadPriority.BelowNormal;	// be kind
            _monitorThread.Start();
            _started = DateTime.Now;
        }
        private void ThreadProc()
        {
            while (true)
            {
                lock (this)
                {
                    Checkin();
                    DoWorkIfItIsTime();
                }
                Thread.Sleep(c_checkinInterval);
            }
        }


    }
}
