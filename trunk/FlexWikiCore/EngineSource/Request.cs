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
using System.Web;
using System.Collections;
using System.Security.Principal;

namespace FlexWiki
{
	/// <summary>
	/// Summary description for Request.
	/// </summary>
	[ExposedClass("Request", "Provides information about the current user's request for this topic")]
	public class Request : BELObject
	{
		public Request()
		{
		}

		HttpRequest HTTPRequest
		{
			get
			{
				if (System.Web.HttpContext.Current == null)
					return null;
				return System.Web.HttpContext.Current.Request;
			}
		}



		IPrincipal User
		{
			get
			{
				if (System.Web.HttpContext.Current == null)
					return null;
				return System.Web.HttpContext.Current.User;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the named parameter from the query string of the request; null if absent")]
		public string GetParameterNamed(string parm)
		{
			if (HTTPRequest == null)
				return null;
			return HTTPRequest.QueryString[parm];
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer an Array the names of all of the parameters in the query string")]
		public ArrayList ParameterNames
		{
			get
			{
				ArrayList answer = new ArrayList();
				if (HTTPRequest == null)
					return answer;
				foreach (string s in HTTPRequest.QueryString.Keys)
					answer.Add(s);
				return answer;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNever, "Answer an Array of VisitorEvents describing the current user's visits during the session")]
		public ArrayList VisitorEvents   
		{
			get
			{
				ArrayList answer = new ArrayList();
				IEnumerable events = (IEnumerable)(System.Web.HttpContext.Current.Session["VisitorEvents"]);
				foreach (VisitorEvent each in events)
					answer.Add(each);
				return answer;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNever, "Answer an Array of Unique VisitorEvents describing the current user's visits during the session")]
		public ArrayList UniqueVisitorEvents
		{
			get
			{
				VisitorEvent previousEvent = new VisitorEvent(new AbsoluteTopicName(""),"",System.DateTime.Now);
				ArrayList answer = new ArrayList();
				IEnumerable events = (IEnumerable)(System.Web.HttpContext.Current.Session["VisitorEvents"]);
				foreach (VisitorEvent currentEvent in events)
				{
					if ( previousEvent.Name != currentEvent.Name )
					{
						previousEvent = currentEvent;
						answer.Add(currentEvent);
					}
				}
				return answer;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a string identifying the visitor (not authenticated)")]
		public string VisitorIdentityString
		{
			get
			{
				if (System.Web.HttpContext.Current == null)
					return "";
				return (string)(System.Web.HttpContext.Current.Items["VisitorIdentityString"]);
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a string identifying the authenticated identity of the user (or null if none)")]
		public string AuthenticatedUserName
		{
			get
			{
				if (User == null)
					return null;
				if (!User.Identity.IsAuthenticated)
					return null;
				return User.Identity.Name;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer true if the current user is authenticated")]
		public bool IsAuthenticated
		{
			get
			{
				if (User == null)
					return false;
				return User.Identity.IsAuthenticated;
			}
		}


		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer true if the user can log in and out; else false")]
		public bool CanLogInAndOut
		{
			get
			{
				// The next line is designed to hide the loggoff command if the user is logged in via NTLM; 
				// I'm not 100% sure the test is right
				return ("Negotiate" != User.Identity.AuthenticationType);
			}
		}


		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer true if differences are being shown; else false")]
		public bool AreDifferencesShown
		{
			get
			{
				if (HTTPRequest == null)
					return false;
				return HTTPRequest.QueryString["diff"] == "y";
			}
		}

	}
}
