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

using FlexWiki.Formatting;

namespace FlexWiki.Web
{
	/// <summary>
	/// Summary description for WebForm1.
	/// </summary>
	public class Default : BasePage
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

		private static bool IsAbsoluteURL(string pattern)
		{
			if (pattern.StartsWith(System.Uri.UriSchemeHttp + System.Uri.SchemeDelimiter))
				return true;
			if (pattern.StartsWith(System.Uri.UriSchemeHttps + System.Uri.SchemeDelimiter))
				return true;
			if (pattern.StartsWith(System.Uri.UriSchemeFile + System.Uri.UriSchemeFile))
				return true;
			return false;
		}


		protected void DoHead()
		{
			QualifiedTopicRevision topic = GetTopicVersionKey();	 
			LinkMaker lm = TheLinkMaker;

			if (Request.QueryString["version"] == null || Request.QueryString["version"] == "")
			{
				// Consider establishing a redirect if there's a redirect to a topic or an URL
				string redir = Federation.GetTopicPropertyValue(GetTopicVersionKey(), "Redirect");
				if (redir != "")
				{
					string URI = null;
					if (IsAbsoluteURL(redir))
						URI = redir;
					else
					{
						// Might be a topic
						Regex name = new Regex(Formatter.s_wikiName);
						string trimmed = redir.Trim();
						if (name.IsMatch(trimmed))
						{
							IList all = Federation.NamespaceManagerForTopic(GetTopicVersionKey()).AllQualifiedTopicNamesThatExist(trimmed);
							if (all.Count == 1)
								URI = lm.LinkToTopic((TopicRevision)(all[0]));
							else
							{
								if (all.Count == 0)
									Response.Write("<!-- Redirect topic does not exist -->\n");
								else
									Response.Write("<!-- Redirect topic is ambiguous -->\n");
							}
						}
					}
					if (URI != null)
					{
						if (Request.QueryString["DelayRedirect"] == "1")
							Response.Write(@"<meta http-equiv='refresh' content='10;URL=" + URI + "'>\n");
						else
							Response.Redirect(URI);
					}
				}

				string keywords = Federation.GetTopicPropertyValue(GetTopicVersionKey(), "Keywords");
				if (keywords != "")
					Response.Write("<META name=\"keywords\" content=\"" + keywords + "\">\n");
				string description = Federation.GetTopicPropertyValue(GetTopicVersionKey(), "Summary");
				if (description != "")
					Response.Write("<META name=\"description\" content=\"" + description + "\">\n");
				Response.Write("<META name=\"author\" content=\"" + Federation.GetTopicLastModifiedBy(
                    GetTopicVersionKey().AsQualifiedTopicName()) + "\">\n");

			}
			else
			{
				// Don't index the versions
				Response.Write("<meta name=\"Robots\" content=\"NOINDEX, NOFOLLOW\">");
			}
        
			urlForDiffs = lm.LinkToTopic(topic, true);
			urlForNoDiffs = lm.LinkToTopic(topic, false);
		}

		protected void DoPage()
		{
			NamespaceManager storeManager = DefaultNamespaceManager;
			LinkMaker lm = TheLinkMaker;
			QualifiedTopicRevision topic = GetTopicVersionKey();	
			bool diffs = Request.QueryString["diff"] == "y";
			QualifiedTopicRevision diffVersion = null;
			bool restore = Request.QueryString["restore"] == "y";
			if (restore==true)
			{
				Response.Redirect(lm.LinkToTopic(this.RestorePreviousVersion(topic)));
			}

			// Go edit if we try to view it and it doesn't exist
			if (!storeManager.TopicExists(topic.LocalName, ImportPolicy.DoNotIncludeImports))
			{ 
				Response.Redirect(lm.LinkToEditTopic(topic.AsQualifiedTopicName()));
				return;
			}

			if (diffs)
			{
				diffVersion = storeManager.VersionPreviousTo(topic.LocalName, topic.Version);
			}

			Response.Write("<body onclick='javascript: BodyClick()' ondblclick=\"location.href='" + 
                this.TheLinkMaker.LinkToEditTopic(topic.AsQualifiedTopicName()) + 
                "'\" scroll='no'>");
			Response.Write(@"<div id='TopicTip' class='TopicTip' ></div>");

			bool first = true;
			string options = "";
			string mostRecentVersion = null;

			//////////////////////////
			///

			// Get the core data (the formatted topic and the list of changes) from the cache.  If it's not there, generate it!
			string formattedBody = Federation.GetTopicFormattedContent(topic, diffVersion);
			IEnumerable changeList = Federation.GetTopicChanges(topic.AsQualifiedTopicName());

			// Now generate the page!

			foreach (TopicChange change in changeList)
			{
				if (first)
					mostRecentVersion = change.Version;
				string s = "";
				string sel = "";
				if (change.Version == topic.Version || (first &&  topic.Version == null))
				{
					s += "&rarr;&nbsp;";
					sel = " selected ";
				}
				else
					s += "&nbsp;&nbsp;&nbsp;&nbsp;";
				if (change.Created.Date == DateTime.Now.Date)
					s += change.Created.ToString("HH:mm");
				else
				{
					if (change.Created.Date.Year == DateTime.Now.Date.Year)
						s += change.Created.ToString("MMM d  H:mm");
					else
						s += change.Created.ToString("MMM d yyyy  H:mm");
				}	
				s += "&nbsp;&nbsp;(" + change.Author + ")";	
				QualifiedTopicRevision linkTo = change.Topic;
				if (first)
					linkTo.Version = null;	// don't include the version for the latest one
				options += "<option rawValue='"+ lm.LinkToTopic(linkTo)  + "' " + sel + ">" + s + "</option>";
				first = false;
			}					

			QualifiedTopicRevision permaTopic = new QualifiedTopicRevision(topic.QualifiedNameWithVersion);
			if (permaTopic.Version == null)
			{
				permaTopic.Version = mostRecentVersion;
			}

			Response.Write("<table width='100%' border='0' cellpadding='0' cellspacing='0'><tr><td valign='top'>");
			Response.Write("<div id='MainRegion' class='Main'>");

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
	tbi.rawValue = '';
	tbi.focus();
	tbi.select();
}

function tbinput()
{
	return 	document.getElementById('TopicBarInputBox');
}

</script>
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
";

			Response.Write(script);

			Response.Write(formattedBody);


			Response.Write("</div></td>");
			Response.Write("<td valign='top' id='Sidebar' class='Sidebar'>");

			Response.Write("<table style='height: 100%'><tr><td height='100%' valign='top'>");

			/////////////////////////////

			NamespaceManager cb1 = Federation.NamespaceManagerForNamespace(topic.Namespace);
			OpenPane(Response.Output, cb1.FriendlyTitle, lm.LinkToImage("images/home.gif"), lm.LinkToTopic(new QualifiedTopicRevision(cb1.HomePage, cb1.Namespace)), "Go to the home page");
			if (cb1.ImageURL != null)
				Response.Write("<img align='right' src='" + cb1.ImageURL + "'>");
			Response.Write(Formatter.FormattedString(topic, cb1.Description, OutputFormat.HTML, cb1, lm));
			ClosePane(Response.Output);

			/////////////////////////////
			Response.Write("<table border='0' cellpadding='0' cellspacing='0'><tr>");
			Response.Write("<td valign='top' width='100%'>");

			if (topic.Version == null)
			{
				Command(lm, "Edit", "Edit this topic", lm.LinkToEditTopic(topic.AsQualifiedTopicName()));
			}
			else
			{
				Command(lm, "Restore", "Restore this version to be the most recent", lm.LinkToRestore(topic));
			}

			Command(lm, "Print", "Show a printable version of this topic", lm.LinkToPrintView(topic));
			Command(lm, "Recent&nbsp;Changes", "Show recently changed topics", lm.LinkToRecentChanges(topic.Namespace));
			Command(lm, "Subscriptions", "Get notified when topics on this site change", RootUrl(Request) + lm.LinkToSubscriptions(topic.Namespace));

			Response.Write("</td>");
			Response.Write("<td valign='top'>&nbsp;</td>");

			Response.Write("<td valign='top'>");

			Command(lm, "Lost&nbsp;and&nbsp;Found", "Show unreferenced topics", lm.LinkToLostAndFound(topic.Namespace));
			Command(lm, "Find&nbsp;References", "Find mentions of this topic in other topics", RootUrl(Request) + "Search.aspx?search=" + topic.LocalName);
			Command(lm, "Rename", "Rename this topic (use with care)", RootUrl(Request) + "Rename.aspx?topic=" + topic.QualifiedName);
			Response.Write("</td>");
			Response.Write("</tr></table>");


			/////////////////////////////
			OpenPane(Response.Output, "Search", lm.LinkToImage("images/search.gif"), null, null);
			Response.Write("<table border='0' cellpadding='0' cellspacing='0' width='100%'><tr>");
			Response.Write("<td valign='top'>");
			Response.Write(@"<form  style='margin-bottom:0;'  method='get' action='" + lm.LinkToSearchNamespace("") + @"'  id='SearchForm' >
<input class='SearchBox' type='text'  name='search' length='25' rawValue =''><INPUT type='image' src='" + lm.LinkToImage("images/go-dark.gif") + @"' title='Start search'>
<input style='display: none' type='text'  name='namespace' rawValue ='" + topic.Namespace + @"'>
");
			Response.Write("</form>");
			Response.Write("</td>");
			Response.Write("<td valign='top' align='right'>");
			Response.Write("<a class='AdvancedSearchLink' href='" + lm.LinkToSearchNamespace(topic.Namespace) + @"'>advanced</a>");
			Response.Write("</td>");
			Response.Write("</tr></table>");
			ClosePane(Response.Output);

			/////////////////////////////
			OpenPane(Response.Output, "Versions", lm.LinkToImage("images/versions.gif"), null, null);
			Response.Write("<select class='VersionList' id='VersionList'>");
			Response.Write(options);
			Response.Write("</select>");
			Response.Write("<table width='100%' border='0' cellspacing='0' cellpadding='0'><tr>");
			Response.Write("<td align='left'><input class='ShowDiffCheckbox' type='checkbox' id='showDiffs' " + (diffs ? " checked " : "") + " onclick='javascript:diffToggle()' title='Show differences between this version and the previous' class='VersionButton'><label for='ShowDiffs'>Highlight changes</label></td>");
			Response.Write("<td align='right'><button onclick='javascript:showVersion();' class='VersionButton'>Show</button></td>");	
			Response.Write("</tr></table>");
			ClosePane(Response.Output);

			/////////////////////////////
			string about = Federation.AboutWikiString;
			if (about != null)
			{
				OpenPane(Response.Output, "About", lm.LinkToImage("images/help.gif"), null, null);
				Response.Write(Formatter.FormattedString(topic, about, OutputFormat.HTML, Federation.DefaultNamespaceManager, lm));
				ClosePane(Response.Output);
			}
			Response.Write("</td></tr>");

			// Begin Security Side Bar links and information
			if (User.Identity.IsAuthenticated)
			{
				// Show the logged on user info
				Response.Write((char)13 + "<tr><td>");
				OpenPane(Response.Output, "Logged On", lm.LinkToImage("images/help.gif"), null, null);
				string sideBarUserInfo = "<table><tr><td>Name:</td><td>" + Context.User.Identity.Name + "</td></tr></table>";
				Response.Write(sideBarUserInfo);
				// The next line is designed to hide the loggoff command if the user is logged in via NTLM; 
				// I'm not 100% sure the test is right
				if ("Negotiate" != User.Identity.AuthenticationType)
					Command(lm, "Logoff","Log off of the application",lm.LinkToLogoff(GetTopicVersionKey()));
				ClosePane(Response.Output);
				
			}
			// End Security Side Bar links and information

			Response.Write((char)13 + "</td></tr>");
			Response.Write("</table>");

			Response.Write("</td></tr></table>");

			Response.Write("</body>");

		}

		private void Command(LinkMaker lm, string command, string helptext, string url)
		{
			Response.Write("<table cellspacing='0' cellpadding='1' class='CommandTable' border='0'><tr><td valign='middle'>");
			Response.Write("<img src='" + lm.LinkToImage("images/go-dark.gif") + "'>");
			Response.Write("</td>");
			
			Response.Write("<td valign='middle'>");
			Response.Write("<a title='" + helptext + "' href=\"" + url + "\">" + command + "</a>");
			Response.Write("</td>");
			Response.Write("</tr></table>");

		}

		private void WriteRecentPane()
		{
			OpenPane(Response.Output, "Recent Topics");
			Response.Write("    ");
			ClosePane(Response.Output);
		}
	}
}
