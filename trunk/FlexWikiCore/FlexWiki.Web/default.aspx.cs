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
using System.Text.RegularExpressions;
using FlexWiki;
using FlexWiki.Formatting;

namespace FlexWiki.Web
{
	/// <summary>
	/// Summary description for WebForm1.
	/// </summary>
	public class Default2 : BasePage
	{
		private void Page_Load(object sender, System.EventArgs e)
		{
			// Put user code to initialize the page here
		}

		#region Web Form Designer generated code
		override protected void OnInit(EventArgs e)
		{
			//
			// CODEGEN: This call is required by the ASP.NET Web Form Designer.
			//
			InitializeComponent();
			base.OnInit(e);
		}
		
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{    
			this.Load += new System.EventHandler(this.Page_Load);
		}
		#endregion

		protected string urlForDiffs;
		protected string urlForNoDiffs;

		static bool IsAbsoluteURL(string pattern)
		{
			if (pattern.StartsWith(System.Uri.UriSchemeHttp + System.Uri.SchemeDelimiter))
				return true;
			if (pattern.StartsWith(System.Uri.UriSchemeHttps + System.Uri.SchemeDelimiter))
				return true;
			if (pattern.StartsWith(System.Uri.UriSchemeFile + System.Uri.UriSchemeFile))
				return true;
			return false;
		}

		LogEvent MainEvent;
		protected void StartPage()
		{
			if (Federation.GetPerformanceCounter(Federation.PerformanceCounterNames.TopicReads) != null)
				Federation.GetPerformanceCounter(Federation.PerformanceCounterNames.TopicReads).Increment();

			MainEvent = TheFederation.LogEventFactory.CreateAndStartEvent(Request.UserHostAddress, VisitorIdentityString, GetTopicName().ToString(), LogEvent.LogEventType.ReadTopic);
			VisitorEvent e = new VisitorEvent(GetTopicName(), VisitorEvent.Read, DateTime.Now);
			LogVisitorEvent(e);
		}

		protected void EndPage()
		{
			MainEvent.Record();
		}

		protected string GetTitle()
		{
			string title = TheFederation.GetTopicProperty(GetTopicName(), "Title");
			if (title == null || title == "")
				title = GetTopicName().Name;
			return HTMLStringWriter.Escape(title);
		}

		protected void DoHead()
		{


			AbsoluteTopicName topic = GetTopicName();	 
			LinkMaker lm = TheLinkMaker;

			// Consider establishing a redirect if there's a redirect to a topic or an URL
			string redir = TheFederation.GetTopicProperty(GetTopicName(), "Redirect");
			if (redir != "")
			{
				UriBuilder URI = null;
				if (IsAbsoluteURL(redir)) 
				{
					URI = new UriBuilder(redir);
				}
				else
				{
					// Must be a topic name
					string trimmed = redir.Trim();
					RelativeTopicName rel = new RelativeTopicName(trimmed);
					IList all = TheFederation.ContentBaseForTopic(GetTopicName()).AllAbsoluteTopicNamesThatExist(rel);

					if (all.Count == 1) 
					{
						URI = new UriBuilder(lm.LinkToTopic((TopicName)(all[0]), false, Request.QueryString));
					}
					else
					{
						if (all.Count == 0)
							Response.Write("<!-- Redirect topic does not exist -->\n");
						else
							Response.Write("<!-- Redirect topic is ambiguous -->\n");
					}
				}
				if (URI != null)
				{
					if (Request.QueryString["DelayRedirect"] == "1")
						Response.Write(@"<meta http-equiv='refresh' content='10;URL=" + URI + "'>\n");
					else
					{
						Response.Redirect(URI.Uri.ToString());
						return;
					}
				}
			}

			string keywords = TheFederation.GetTopicProperty(GetTopicName(), "Keywords");
			if (keywords != "")
				Response.Write("<META name=\"keywords\" content=\"" + keywords + "\">\n");
			string description = TheFederation.GetTopicProperty(GetTopicName(), "Summary");
			if (description != "")
				Response.Write("<META name=\"description\" content=\"" + description + "\">\n");
			Response.Write("<META name=\"author\" content=\"" + TheFederation.GetTopicLastModifiedBy(GetTopicName()) + "\">\n");

			if (GetTopicName().Version != null)
			{
				// Don't index the versions
				Response.Write("<meta name=\"Robots\" content=\"NOINDEX, NOFOLLOW\">");
			}
        
			urlForDiffs = lm.LinkToTopic(topic, true);
			urlForNoDiffs = lm.LinkToTopic(topic, false);
		}

		protected void DoPage()
		{

			string script = @"
<script  type=""text/javascript"" language='javascript'>

function TopicBarMouseOver()
{
	TopicBar.className='TopicBarHover';
}

function TopicBarMouseOut()
{
	TopicBar.className='TopicBar';
}

function IsEditing()
{
	return DynamicTopicBar.style.display == 'block';
}

function SetEditing(flag)
{
	var isEditing = IsEditing();
	if (isEditing == flag)
		return; 
	isEditing = flag;
	if (isEditing)
	{
		StaticTopicBar.style.display = 'none';
		DynamicTopicBar.style.display = 'block';
	}
	else
	{
		StaticTopicBar.style.display = 'block';
		DynamicTopicBar.style.display = 'none';
	}
}

function BodyClick()
{
	SetEditing(false);
}

function TopicBarClick(event)
{
	event.cancelBubble = 'true';
	if (IsEditing())
		return;

	// Grab these dimensions before SetEditng() to get them before the control is hidden (thus h=0;w=0!)
	var staticWide = StaticTopicBar.offsetWidth;
	var staticHigh = StaticTopicBar.offsetHeight;

	SetEditing(true);

	DynamicTopicBar.left = TopicBar.offsetLeft;
	DynamicTopicBar.top = TopicBar.offsetTop;
	DynamicTopicBar.width = TopicBar.width;
	DynamicTopicBar.height = TopicBar.height;

	var tbi = tbinput();
	tbi.left = DynamicTopicBar.left;
	tbi.top = DynamicTopicBar.top;
	tbi.width = staticWide;
	tbi.height = staticHigh;
	tbi.value = '';
	tbi.focus();
	tbi.select();
}

function tbinput()
{
	return 	document.getElementById('TopicBarInputBox');
}

</script>
";
						
			ContentBase cb = DefaultContentBase;
			LinkMaker lm = TheLinkMaker;
			AbsoluteTopicName topic = GetTopicName();	
			bool diffs = Request.QueryString["diff"] == "y";
			bool restore = (Request.RequestType == "POST" && Request.Form["RestoreTopic"] != null);
			bool isBlacklistedRestore = false;
			if (restore==true)
			{
				// Prevent restoring a topic with blacklisted content
				if (TheFederation.IsBlacklisted(TheFederation.Read(topic)))
				{
					isBlacklistedRestore = true;
				}
				else
				{
					Response.Redirect(lm.LinkToTopic(this.RestorePreviousVersion(new AbsoluteTopicName(Request.Form["RestoreTopic"]))));
				}
			}

			// Go edit if we try to view it and it doesn't exist
			if (!cb.TopicExists(topic))
			{ 
				Response.Redirect(lm.LinkToEditTopic(topic));
				return;
			}

			Response.Write("<body onclick='javascript: BodyClick()' ondblclick=\"location.href='" + this.TheLinkMaker.LinkToEditTopic(topic) + "'\">");
			Response.Write(script);
			Response.Write(@"<div id='TopicTip' class='TopicTip' ></div>");

			//////////////////////////
			///

			// Get the core data (the formatted topic and the list of changes) from the cache.  If it's not there, generate it!
			string formattedBody = TheFederation.GetTopicFormattedContent(topic, diffs);

			// Now calculate the borders
			int span = 1;
			string leftBorder = TheFederation.GetTopicFormattedBorder(topic, Border.Left);
			if (leftBorder != null)
			{
				span++;
				leftBorder = "<td valign='top' class='BorderLeft'>" + leftBorder + "</td>";
			}
			string rightBorder =TheFederation.GetTopicFormattedBorder(topic, Border.Right);
			if (rightBorder != null)
			{
				span++;
				rightBorder = "<td valign='top' class='BorderRight'>" + rightBorder + "</td>";
			}
			string topBorder = TheFederation.GetTopicFormattedBorder(topic, Border.Top);
			if (topBorder != null)
				topBorder = "<tr><td valign='top'  class='BorderTop' colspan='" + span + "'>" + topBorder + "</td></tr>";
			string bottomBorder = TheFederation.GetTopicFormattedBorder(topic, Border.Bottom);
			if (bottomBorder != null)
				bottomBorder = "<tr><td valign='top'  class='BorderBottom' colspan='" + span + "'>" + bottomBorder + "</td></tr>";

			Response.Write("<table width='100%' height='100%' border='0' cellpadding='0' cellspacing='0'>");
			if (topBorder != null)
				Response.Write(topBorder);
			Response.Write("<tr>");
			if (leftBorder != null)
				Response.Write(leftBorder);
			Response.Write("<td valign='top' width='100%'>");
			Response.Write(@"<div id='MainRegion' class='TopicBody'>
<form method='post' action='" + lm.LinkToQuicklink() + @"?QuickLinkNamespace=" + topic.Namespace + @"' name='QuickLinkForm'>
<div id='TopicBar' title='Click here to quickly jump to or create a topic' class='TopicBar' onmouseover='javascript:TopicBarMouseOver()'  onclick='javascript:TopicBarClick(event)'  onmouseout='javascript:TopicBarMouseOut()'>
<div  id='StaticTopicBar'  class='StaticTopicBar' style='display: block'>" + topic.FormattedName + @"
</div>
<div id='DynamicTopicBar' class='DynamicTopicBar' style='display: none'>
<!-- <input id='TopicBarNamespace' style='display: none' type='text'  name='QuickLinkNamespace'> -->
<input id='TopicBarInputBox' title='Enter a topic here to go to or create' class='QuickLinkInput' type=""text""  name=""QuickLink"">
<div class='DynamicTopicBarHelp'>Enter a topic name to show or a new topic name to create; then press Enter</div>
</div>
</div>
</form>
");

			if (isBlacklistedRestore)
			{
				Response.Write("<div class='BlacklistedRestore'><font color='red'><b>The version of the topic you are trying to restore contains content that has been banned by policy of this site.  Restore can not be completed.</b></font></div>");
			}
			
			Response.Write(formattedBody);

			Response.Write("</div>");
			Response.Write("</td>");
			if (rightBorder != null)
				Response.Write(rightBorder);
			Response.Write("</tr>");
			if (bottomBorder != null)
				Response.Write(bottomBorder);
			Response.Write("</table>");

			Response.Write("</body>");

		}

		void Command(LinkMaker lm, string command, string helptext, string url)
		{
			Response.Write("<table cellspacing='0' cellpadding='1' class='CommandTable' border='0'><tr><td valign='middle'>");
			Response.Write("<img src='" + lm.LinkToImage("images/go-dark.gif") + "'>");
			Response.Write("</td>");
			
			Response.Write("<td valign='middle'>");
			Response.Write("<a title='" + helptext + "' href=\"" + url + "\">" + command + "</a>");
			Response.Write("</td>");
			Response.Write("</tr></table>");

		}

		void WriteRecentPane()
		{
			OpenPane(Response.Output, "Recent Topics");
			Response.Write("    ");
			ClosePane(Response.Output);
		}
	}
}
