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
using System.Collections.Specialized;
using System.Configuration; 
using System.Text;
using System.Web;


namespace FlexWiki
{
	/// <summary>
	/// LinkMaker understand how to make links to the various pages that make the wiki work
	/// </summary>
	[ExposedClass("LinkMaker", "Builds hyperlinks to various important pages")]
	public class LinkMaker : BELObject
	{
		private string	_SiteURL;
		bool _MakeAbsoluteURLs = false;

		public LinkMaker(string siteURL)
		{
			_SiteURL = siteURL;
			string mau = ConfigurationSettings.AppSettings["MakeAbsoluteURLs"];
			if (mau != null && mau.Equals("true"))
				_MakeAbsoluteURLs = true;
		}

		bool MakeAbsoluteURLs
		{
			get
			{
				return _MakeAbsoluteURLs;
			}
		}

		public string SiteURL()
		{
			if (MakeAbsoluteURLs)
			{
				HttpRequest r = 	System.Web.HttpContext.Current.Request;
				string answer = new UriBuilder(r.Url.Scheme, r.Url.Host, r.Url.Port, _SiteURL).ToString();
				return answer;
			}
			else
			{
				return _SiteURL;
			}
		}

		public LinkMaker Clone()
		{
			return new LinkMaker(_SiteURL);
		}

		AbsoluteTopicName _ReturnToTopicForEditLinks;

		public AbsoluteTopicName ReturnToTopicForEditLinks
		{
			get
			{
				return _ReturnToTopicForEditLinks;
			}
			set
			{
				_ReturnToTopicForEditLinks = value;
			}
		}

		public string LinkToUser(string user)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append("<a href=\"mailto:");
			builder.Append(user);
			builder.Append("\">");
			builder.Append(user);
			builder.Append("</a>");			
			return builder.ToString();
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a link to the given image")]
		public string LinkToImage(string s)
		{
			return SimpleLinkTo(s);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a link to the search page for the given namespace")]
		public string LinkToSearchNamespace(string ns)
		{
			return SimpleLinkTo("search.aspx" + (ns != null ? "?namespace=" + ns : "") );
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a link to search for the given string in the given namespace")]
		public string LinkToSearchFor(string Namespace, string searchExpression)
		{
			return SimpleLinkTo("search.aspx" + (Namespace != null ? "?namespace=" + Namespace : "") + (searchExpression != null ? "?search=" + HttpUtility.UrlEncode(searchExpression) : "") );
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a link to a list of all the versions for the given topic")]
		public string LinkToVersions(string fullTopicName)
		{
			return SimpleLinkTo("versions.aspx" + (fullTopicName != null ? "?topic=" + HttpUtility.UrlEncode(fullTopicName) : "") );
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a link to compare two versions for the given topic")]
		public string LinkToCompare(string fullTopicName, int diff, int oldid)
		{
			return SimpleLinkTo("compare.aspx" + (fullTopicName != null ? "?topic=" + HttpUtility.UrlEncode(fullTopicName) : "") + ((diff>=0)?"&diff=" + diff.ToString():string.Empty) + ((oldid>=0)?"&oldid=" + oldid.ToString():string.Empty));
		}

		public string LinkToQuicklink()
		{
			return SimpleLinkTo("quicklink.aspx");
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a link to the recent changes list for the given namespace")]
		public string LinkToRecentChanges(string ns)
		{
			return SimpleLinkTo("lastmodified.aspx"+ (ns != null ? "?namespace=" + ns : "") );
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a link to the page that allows the given topic to be renamed")]
		public string LinkToRename(string fullyQualifiedTopicName)
		{
			return SimpleLinkTo("rename.aspx"+ (fullyQualifiedTopicName != null ? "?topic=" + fullyQualifiedTopicName: "") );
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a link to the list of subscriptions for the given namespace (or null for all)")]
		public string LinkToSubscriptions(string ns)
		{
			return SimpleLinkTo("rssface.aspx"+ (ns != null ? "?namespace=" + ns : "") );
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a link to the lost and found for the given namespace")]
		public string LinkToLostAndFound(string ns)
		{
			return SimpleLinkTo("lostandfound.aspx"+ (ns != null ? "?namespace=" + ns : "") );
		}

		string SimpleLinkTo(string s)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(SiteURL());
			builder.Append(s);
			return builder.ToString();
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a link to the given topic")]
		public string LinkToTopic(string topic)
		{
			return TopicLink(topic, false, null);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a link to display the given topic with diffs highlighted")]
		public string LinkToTopicWithDiffs(string topic)
		{
			return TopicLink(topic, true, null);
		}

		public string LinkToTopic(TopicName topic)
		{
			return LinkToTopic(topic, false);
		}

		public string LinkToTopic(TopicName topic, bool showDiffs)
		{
			return TopicLink(topic.FullnameWithVersion, showDiffs, null);
		}

		public string LinkToTopic(TopicName topic, bool showDiffs, NameValueCollection extraQueryParms)
		{
			return TopicLink(topic.FullnameWithVersion, showDiffs, extraQueryParms);
		}

		string TopicLink(string top, bool showDiffs, NameValueCollection extraQueryParms)
		{
			StringBuilder builder = new StringBuilder();
			RelativeTopicName topic = new RelativeTopicName(top);
			builder.Append(SiteURL());
			builder.Append("default.aspx/");
			if (topic.Namespace != null && topic.Namespace != "")
				builder.Append(HttpUtility.UrlEncode(topic.Namespace) + "/");
			builder.Append(topic.Name);
			if (topic.Version != null)
				builder.Append("(" + HttpUtility.UrlEncode(topic.Version) + ")");
			builder.Append(".html");		// hard coded for now -- later we'll be cooler!
			StringBuilder query = new StringBuilder();
			if (showDiffs)
				query.Append("diff=y");
			if (extraQueryParms != null)
			{
				foreach (string each in extraQueryParms)
				{
					if (query.Length != 0)
						query.Append("&");
					query.Append(each + "=" + HttpUtility.UrlEncode((string)(extraQueryParms[each])));
				}
			}
			if (query.Length != 0)
				builder.Append("?" + query.ToString());
			return builder.ToString();
		}
	

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a link to the page that allows a user to login")]
		public string LinkToLogin(string topic)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(SiteURL());
			builder.Append("login.aspx?ReturnURL=" + HttpUtility.UrlEncode(TopicLink(topic, false, null)));
			return builder.ToString();
		}

		public string LinkToLogin(TopicName topic)
		{
			return LinkToLogin(topic.FullnameWithVersion);
		}

		public string LinkToLogoff(TopicName topic)
		{
			return LinkToLogoff(topic);
		}


		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a link to the page that logs off the current user")]
		public string LinkToLogoff(string topic)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(SiteURL());
			builder.Append("logoff.aspx");
			return builder.ToString();
		}
		
		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a link to the 'print' view of the given topic")]
		public string LinkToPrintView(string topic)
		{
			return PrintLink(topic);
		}
			
		public string LinkToPrintView(TopicName topic)
		{
			return PrintLink(topic.FullnameWithVersion);
		}

		string PrintLink(string top)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(SiteURL());
			builder.Append("print.aspx/");
			builder.Append(top);
			return builder.ToString();
		}

		public string LinkToEditTopic(AbsoluteTopicName topic)
		{
			return EditLink(topic.Fullname);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a link to the page that lets the user edit the given topic")]
		public string LinkToEditTopic(string topic)
		{
			return EditLink(topic);
		}

		string EditLink(string top)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(SiteURL());
			builder.Append("wikiedit.aspx?topic=");
			builder.Append(HttpUtility.UrlEncode(top));
			if (ReturnToTopicForEditLinks != null)
				builder.Append("&return=" + HttpUtility.UrlEncode(ReturnToTopicForEditLinks.Fullname));

			return builder.ToString();
		}


		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a link to the topic that processes a restore")]
		public string LinkToRestore(string topic)
		{
			return RestoreLink(topic);
		}

		public string LinkToRestore(TopicName topic)
		{
			return RestoreLink(topic.FullnameWithVersion);
		}

		/// <summary>
		/// Creates the Restore link
		/// </summary>
		/// <param name="topic">TopicName for the link</param>
		/// <returns></returns>
		string RestoreLink(string top)
		{
			NameValueCollection extras = new NameValueCollection();
			extras.Add("restore","y");
			return TopicLink(top, false, extras);
		}
		/// <summary>
		/// Creates the Change User Profile link
		/// </summary>
		/// <param name="topic">TopicName for the link</param>
		/// <returns></returns>
		public string LinkToChangeUserProfile(int userID)
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(SiteURL());
			builder.Append("UserProfile.aspx?UserID=");
			builder.Append(userID.ToString());
			builder.Append("&Mode=2");
			return builder.ToString();
		}
		/// <summary>
		/// Creates the Create User Profile link
		/// </summary>
		/// <param name="topic">TopicName for the link</param>
		/// <returns></returns>
		public string LinkToCreateUserProfile()
		{
			StringBuilder builder = new StringBuilder();
			builder.Append(SiteURL());
			builder.Append("UserProfile.aspx?UserID=0");
			builder.Append("&Mode=1");
			return builder.ToString();
		}

	}
}
