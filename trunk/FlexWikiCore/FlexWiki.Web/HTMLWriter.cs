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
using System.Text;


namespace FlexWiki.Web
{
	public abstract class HTMLWriter
	{

		public string Break()
		{
			return "<br />";
		}

		public void WriteBreak()
		{
			Write(Break());
		}
		
		public string Bold(string htmlText)
		{
			return "<b>" + htmlText + "</b>";
		}

		public string Smaller(string htmlText)
		{
			return "<font size='-1'>" + htmlText + "</font>";
		}

		public string Heading(string htmlText, int level)
		{
			return "<h" + level + ">" + htmlText + "</h" + level + ">";
		}

		public void WriteHeading(string htmlText, int level)
		{
			Write(Heading(htmlText, level));
		}


		public void WriteLine(string htmlText)
		{
			Write(Line(htmlText));
		}

		public string Line(string htmlText)
		{
			return htmlText + "<br />";
		}


		public void WriteBold(string htmlText)
		{
			Write(Bold(htmlText));
		}

		public string Center(string htmlText)
		{
			return "<div align='center'>" + htmlText + "</div>";
		}

		public void WriteCenter(string htmlText)
		{
			Write(Center(htmlText));
		}

		public string Para(string htmlText)
		{
			return "<p>" + htmlText + "</p>";
		}

		public void WritePara(string htmlText)
		{
			Write(Para(htmlText));
		}

		public string Image(string url)
		{
			return "<img src='" + url + "' border=0>";
		}

		public void WriteImage(string url)
		{
			Write(Image(url));
		}

		/// <summary>
		/// Escape the given string for special HTML characters (greater-than, etc.).
		/// </summary>
		/// <param name="input"></param>
		/// <returns>The new string</returns>
		public static string Escape(string input)
		{
			return Escape(input, false);
		}

		/// <summary>
		/// Escape the given string for special HTML characters (greater-than, etc.).
		/// </summary>
		/// <param name="input"></param>
		/// <returns>The new string</returns>
		public static string Escape(string input, bool includeLineBreaks)
		{
			if (input == null)
				return "";
			// replace HTML special characters with character entities
			// this has the side-effect of stripping all markup from text
			string str = input;
			str = str.Replace ("&", "&amp;");
			str = str.Replace ("\"", "&quot;");
			str = str.Replace ("<", "&lt;");
			str = str.Replace (">", "&gt;");
			str = str.Replace ("'", "&#39;");
			if (includeLineBreaks)
				str = str.Replace ("\n", "<br />") ;
			return str;
		}

		public void WriteDivider()
		{
			Write(Divider());
		}

		public string Divider()
		{
			return "<hr size=1 noshade>";
		}

		public string Link(string url, string escapedTitle)
		{
			return Link(url, escapedTitle, null);
		}

		public string Link(string url, string escapedTitle, string tip)
		{
			string answer = "<a href='" + url + "'";
			if (tip != null)
				answer += " title='" + Escape(tip, false) + "'";
			answer += ">"+ escapedTitle + "</a>";
			return answer;
		}

		public void WriteLink(string url, string escapedTitle, string tip)
		{
			Write(Link(url, escapedTitle, tip));
		}

		public void WriteLink(string url, string escapedTitle)
		{
			WriteLink(url, escapedTitle, null);
		}

		public abstract void Write(string s);


		public string EndCell()
		{
			return "</td>";
		}

		public string Cell(UIColumn col, string contents)
		{
			return StartCell(col) + contents + "&nbsp;" + EndCell();
		}

		public void WriteCell(UIColumn col, string contents)
		{
			Write(Cell(col, contents));
		}

		public string StartCell(UIColumn col)
		{
			return "<td class='GenericCell' valign='top'>";
		}

		public void WriteStartCell(UIColumn col)
		{
			Write(StartCell(col));
		}

		public string Cell(string contents)
		{
			return StartCell() + contents + "&nbsp;" + EndCell();
		}

		public void WriteCell(string contents)
		{
			Write(Cell(contents));
		}

		public string StartCell()
		{
			return "<td class='GenericCell' valign='top'>";
		}

		public void WriteStartCell()
		{
			Write(StartCell());
		}

		public void WriteEndCell()
		{
			Write(EndCell());
		}

		public void WriteStartTable(UITable table)
		{
			Write(StartTable(table));
		}

		public void WriteStartTable()
		{
			Write(StartTable());
		}

		public string StartTable()
		{
			return StartTable(null);
		}

		public string StartTable(UITable table)
		{
			StringBuilder b = new StringBuilder();
			b.Append("<table  class='GenericTable' cellpadding='2' cellspacing='0' border='0'>");
			b.Append("<tr class='GenericHeaderRow'>");
			if (table != null)
			{
				foreach (UIColumn each in table.Columns)
				{
					string cls;
					if (each.Name == null)
						cls = "class='GenericEmptyHeaderCell'";
					else
						cls = "class='GenericHeaderCell'";

					b.Append("<td " + cls + ">" + Escape(each.Name, false) +"</td>");
				}
				b.Append("</tr>");
			}
			return b.ToString();
		}

		public void WriteEndTable()
		{
			Write(EndTable());
		}

		public string EndTable()
		{
			return "</table>";
		}

		public string StartRow()
		{
			return "<tr class='GenericRow'>";
		}

		public string EndRow()
		{
			return "</tr>";
		}

		public void WriteStartRow()
		{
			Write(StartRow());
		}

		public void WriteEndRow()
		{
			Write(EndRow());
		}

		public void WriteRow(string contents)
		{
			Write(Row(contents));
		}

		public string Row(string contents)
		{
			return StartRow() + contents + EndRow();
		}



		public void WriteHiddenField(string fieldName, string value)
		{	
			Write("<span style='display: none'><input type='text' value='" + Escape(value) + "' name='" + fieldName + "'></span>");
		}
		
		public void WriteInputField(string fieldName, string fieldLabel, string help, string value)
		{	
			WriteInputField(fieldName, fieldLabel, help, value, false);
		}

		public void WriteInputField(string fieldName, string fieldLabel, string help, string value, bool isReadOnly)
		{	
			WriteFieldHTML(fieldLabel, help, 
				"<input type='text' size='50' " + (isReadOnly ? " READONLY " : "") + "value='" + Escape(value) + "' name='" + fieldName + "'>");
		}

		public void WriteCheckbox(string fieldName, string fieldLabel, string help, bool value)
		{	
			WriteFieldHTML(fieldLabel, help, 
				"<input type='checkbox' value='yes' " + (value ? "checked" : "") + " name='" + fieldName + "'>");
		}

		public class ChoiceSet
		{
			ArrayList _DisplayStrings = new ArrayList();
			ArrayList _ValueStrings = new ArrayList();

			public void Add(string display, string value)
			{
				_DisplayStrings.Add(display);
				_ValueStrings.Add(value);
			}

			public IList DisplayStrings
			{
				get
				{
					return ArrayList.ReadOnly(_DisplayStrings);
				}
			}

			public IList ValueStrings
			{
				get
				{
					return ArrayList.ReadOnly(_ValueStrings);
				}
			}
		};

		public void WriteCombobox(string fieldName, string fieldLabel, string help, ChoiceSet choices, string currentValue)
		{	
			StringBuilder b = new StringBuilder();
			b.Append("<select name='" + fieldName + "' id='" + fieldName + "'>");
			for (int i = 0; i < choices.DisplayStrings.Count; i++)
			{
				string disp = (string)(choices.DisplayStrings[i]);
				string val = (string)(choices.ValueStrings[i]);
				string sel = (val == currentValue) ? " selected " : "";
				b.Append("<option " + sel + " value='"+ Escape(val) + "'>" + disp + "</option>");
			}
			b.Append(@"</select>");		
			WriteFieldHTML(fieldLabel, help, b.ToString());
		}

		public void WriteStartButtons()
		{
			Write("<div align=right>");
		}

		public void WriteEndButtons()
		{
			Write("</div>");
		}

		public void WriteStartUnorderedList()
		{
			Write(StartUnorderedList());
		}

		public void WriteEndUnorderedList()
		{
			Write(EndUnorderedList());
		}

		public void WriteListItem(string html)
		{
			Write(ListItem(html));
		}

		public string ListItem(string html)
		{
			return "<li>" + html + "</li>";
		}

		public string StartUnorderedList()
		{
			return "<ul>";
		}
		
		public string EndUnorderedList()
		{
			return "</ul>";
		}
		
		public void WriteSubmitButton(string name, string label)
		{
			Write("<INPUT name='" + name + "' value ='" + Escape(label) + @"' type='submit'>");
		}

		public void WriteTextAreaField(string fieldName, string fieldLabel, string help, string value)
		{	
			string html = "<textarea onkeydown='if (document.all && event.keyCode == 9) {  event.returnValue= false; document.selection.createRange().text = String.fromCharCode(9)} ' rows='5' cols='40' name='" + fieldName +"'>";
			html += Escape(value);
			html += "</textarea>";
			WriteFieldHTML(fieldLabel, help, html);
		}

		void WriteFieldHTML(string fieldLabel, string help, string html)
		{
			Write("<tr>");
			Write("<td valign='top' class='FieldName'>" + Escape(fieldLabel) + ":</td>");
			Write("<td valign='top' class='FieldValue'>" + html + "</td>");
			Write("</tr>");
			Write("<tr>");
			Write("<td></td><td class='FieldHelp'>" + Escape(help) + "</td>");
			Write("</tr>");
		}

		public void WriteFieldError(string htmlError)
		{
			Write("<tr>");
			Write("<td valign='top'></td>");
			Write("<td valign='top' class='FieldError'>" + htmlError + "</td>");
			Write("</tr>");
		}

		public void WriteEndFields()
		{
			Write("</table>");
		}

		public void WriteStartFields()
		{
			Write("<table class='FieldTable' cellpadding=2 cellspacing=0>");
		}

		public void WriteStartForm(string action)
		{
			Write("<form method='post' ACTION='" + action + "'>");
		}

		public void WriteEndForm()
		{
			Write("</form>");
		}


















	}
}
