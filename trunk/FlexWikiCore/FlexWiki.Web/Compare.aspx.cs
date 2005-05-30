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
using System.ComponentModel;
using System.Drawing;
using System.Web;
using System.Web.SessionState;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using FlexWiki;
using FlexWiki.Formatting;

namespace FlexWiki.Web
{
	/// <summary>
	/// Zusammenfassung für Compare.
	/// </summary>
	public class Compare : BasePage
	{

		private LogEvent		  _mainEvent	  = null;
		private AbsoluteTopicName _requestedTopic = null;

		private string _topicString	 = string.Empty;
		private int _diff		 = 0;
		private int _oldid		 = 0;

		protected AbsoluteTopicName RequestedTopic
		{
			get 
			{ 
				return _requestedTopic; 
			}
		}

		#region Vom Web Form-Designer generierter Code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: Dieser Aufruf ist für den ASP.NET Web Form-Designer erforderlich.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Erforderliche Methode für die Designerunterstützung. 
		/// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
		/// </summary>
		private void InitializeComponent()
		{    
			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion

		private void Page_Load(object sender, System.EventArgs e)
		{
			_topicString  = Request.QueryString["topic"];
			
			try
			{
				_diff		= Convert.ToInt32(Request.QueryString["diff"]);
			}
			catch {}
			try
			{
				_oldid		= Convert.ToInt32(Request.QueryString["oldid"]);
			}
			catch {}

			try
			{
				_requestedTopic = new AbsoluteTopicName(_topicString);
			}
			catch {}

			if (_requestedTopic == null || _diff >= _oldid)
			{
				Response.Redirect("default.aspx");
			}
		}

		protected string GetTitle()
		{
			string title = TheFederation.GetTopicProperty(RequestedTopic, "Title");
			if (title == null || title == "")
			{
				title = string.Format("{0} - {1}", GetTopicName().FormattedName, GetTopicName().Namespace);
			}
			return HTMLStringWriter.Escape(title);
		}
		
		protected void StartPage()
		{
			if (Federation.GetPerformanceCounter(Federation.PerformanceCounterNames.TopicsCompared) != null)
			{
				Federation.GetPerformanceCounter(Federation.PerformanceCounterNames.TopicsCompared).Increment();
			}

			_mainEvent = TheFederation.LogEventFactory.CreateAndStartEvent(Request.UserHostAddress, VisitorIdentityString, RequestedTopic.Name, LogEvent.LogEventType.CompareTopic);
			VisitorEvent e = new VisitorEvent(RequestedTopic, VisitorEvent.Compare, DateTime.Now);
			LogVisitorEvent(e);
		}

		protected void EndPage()
		{
			_mainEvent.Record();
		}

		protected void DoPage()
		{
			AbsoluteTopicName newestTopicVersion = null;
			AbsoluteTopicName oldTopicVersion	 = null;
			int counter = 0;
			IEnumerable changeList = TheFederation.GetTopicChanges(RequestedTopic);
			foreach (TopicChange change in changeList)
			{
				if (counter == _diff)
				{
					newestTopicVersion = new AbsoluteTopicName(change.Fullname);
				}
				else if (counter == _oldid)
				{
					oldTopicVersion = new AbsoluteTopicName(change.Fullname);
					break;
				}
				counter++;
			}

			if (newestTopicVersion != null && oldTopicVersion != null)
			{
				Response.Write(@"<div id='MainRegion' class='TopicBody'>
<form method='post' action='" + TheLinkMaker.LinkToQuicklink() + @"?QuickLinkNamespace=" + RequestedTopic.Namespace + @"' name='QuickLinkForm'>
<div id='TopicBar' title='Click here to quickly jump to or create a topic' class='TopicBar' onmouseover='javascript:TopicBarMouseOver()'  onclick='javascript:TopicBarClick(event)'  onmouseout='javascript:TopicBarMouseOut()'>
<div  id='StaticTopicBar'  class='StaticTopicBar' style='display: block'>" + GetTitle() + @"</div>
<div id='DynamicTopicBar' class='DynamicTopicBar' style='display: none'>
<!-- <input id='TopicBarNamespace' style='display: none' type='text'  name='QuickLinkNamespace'> -->
<input id='TopicBarInputBox' title='Enter a topic here to go to or create' class='QuickLinkInput' type=""text""  name=""QuickLink"">
<div class='DynamicTopicBarHelp'>Enter a topic name to show or a new topic name to create; then press Enter</div>
</div>
</div>
</form>
");
				string formattedBody = TheFederation.GetTopicFormattedContent(newestTopicVersion, oldTopicVersion);
				Response.Write(formattedBody);
				Response.Write("</div>");

			}
			
		}
	}
}
