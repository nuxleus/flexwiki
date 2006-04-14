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

namespace FlexWiki.Web
{
	/// <summary>
	/// Summary description for UIResponse.
	/// </summary>
	public class UIResponse : HtmlWriter
	{
		public delegate void MenuWriter();
		public delegate void BodyWriter();
		
		public UIResponse(HttpResponse resp, string relativeBase)
		{
			_Response = resp;
			_RelativeBase = relativeBase;
		}    


		public void ShowPage(string title, MenuWriter menu, BodyWriter body)
		{
			Write("<table cellspacing='0' width='100%' height='100%' cellpadding='4' border='0'>");
			Write("<tr><td colspan='2' class='GenericPageTitle'>" + Escape(title, true) + "</td></tr>");
			Write("<tr><td class='GenericPageMenuColumn' valign=top>");
			menu();
			Write("</td><td width='100%' height='100%' valign=top>");
			body();
			Write("</td></tr></table>");
		}

		private string _RelativeBase;


		private HttpResponse _Response;

    private HttpResponse Response
		{
			get
			{
				return _Response;
			}
		}

		public string CommandLink(string url, string cmd, string tip)
		{
			string answer = "<a href='" + url + "'";
			if (tip != null)
				answer += " title='" + Escape(tip, false) + "'";
			answer += ">"+ GetCommandVisual(cmd) + "</a>";
			return answer;
		}

		private Hashtable _CommandVisuals = null;

		private Hashtable CommandVisuals
		{
			get
			{
				if (_CommandVisuals != null)
					return _CommandVisuals;
				_CommandVisuals = BuildCommandVisuals();
				return _CommandVisuals;
			}
		}

		public string GetCommandVisual(string command)
		{
			return (string)(CommandVisuals[command]);
		}

		public void SetCommandVisual(string command, string visual)
		{
			CommandVisuals[command] = visual;
		}

		public void WriteCommandLink(string url, string cmd, string tip)
		{
			Write(CommandLink(url, cmd, tip));
		}

		public override void Write(string s)
		{
			Response.Write(s);
		}


		Hashtable BuildCommandVisuals()
		{
			Hashtable answer = new Hashtable();
			answer[Command.Edit] = "<b>edit</b>";
			answer[Command.Delete] = "<b>delete</b>";
			answer[Command.Create] = "<b>create</b>";
			return answer;
		}

		
		public void WriteStartMenu(string title)
		{
			Write(StartMenu(title));
		}

		public string StartMenu(string title)
		{
			return @"<table width='100%' class='GenericMenuTable' cellpadding='2' cellspacing='0' border='0'>
<tr class='GenericMenuHeaderRow'>
<td class='GenericMenuHeaderLeft'></td>
<td class='GenericMenuHeaderText' width='100%'>" + (title == null ? "" : title) + @"</td>
</tr>";
		}

		public void WriteMenuItem(string url, string unescapedText, string tip)
		{
			Write(MenuItem(url, unescapedText, tip));
		}

		public string MenuItem(string url, string unescapedText, string tip)
		{
			return @"
<tr class='GenericMenuRow'>
<td class='GenericMenuLeft'><a title='" + Escape(tip, false) +"' href='" + url + @"'><img border=0 src='" + _RelativeBase + @"images/go.gif'></a></td>
<td nowrap class='GenericMenuText'><a title='" + Escape(tip, false) +"' href='" + url + @"'>" + Escape(unescapedText, true) + @"</a></td>
</tr>";
		}

		public void WriteEndMenu()
		{
			Write(EndMenu());
		}

		public string EndMenu()
		{
			return "</table>";
		}

		public void WriteStartKVTable()
		{
			Write(StartKVTable());
		}

		public string StartKVTable()
		{
			return "<table  class='GenericKVTable' cellpadding='2' cellspacing='0' border='0'>";
		}

		public string KVRow(string htmlKey, string htmlValue)
		{
			return 
				@"<tr>
				<td valign='top' class='GenericKVKey'>" + htmlKey + @"</td>
				<td valign='top' class='GenericKVValue'>" + htmlValue + @"</td>
				</tr>";
		}

		public void WriteKVRow(string htmlKey, string htmlValue)
		{
			Write(KVRow(htmlKey, htmlValue));
		}

		public void WriteEndKVTable()
		{
			Write(EndTable());
		}

		public string EndKVTable()
		{
			return "</table>";
		}


	}
}
