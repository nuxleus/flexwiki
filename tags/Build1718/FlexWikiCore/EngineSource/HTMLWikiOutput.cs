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
using System.IO;
using FlexWiki;
using System.Text;

namespace FlexWiki.Formatting
{
	/// <summary>
	/// Output writer that can generate HTML output
	/// </summary>
	public class HTMLWikiOutput : WikiOutput
	{
		LineStyle _CurrentStyle = LineStyle.Unchanged;

		public HTMLWikiOutput(WikiOutput parent) : base(parent)
		{
		}


		string css()
		{
			return css("");
		}

		string css(string extraStyles)
		{
			string styles = extraStyles;
			switch (_CurrentStyle)
			{
				case LineStyle.Add:
					if (styles != "")
						styles += ";";
					styles += "background: palegreen";
					break;

				case LineStyle.Delete:
					if (styles != "")
						styles += ";";
					styles += "color: silver; text-decoration: line-through";
					break;

				case LineStyle.Unchanged:
					break;
			}			
			if (styles.Length > 0)
				styles = " style=\"" + styles + "\"";
			return styles;
		}

		override public void Begin()
		{
			if (!IsNested)
				WriteScripts();
		}

		void WriteScripts()
		{
Write(@"
<script type=""text/javascript"" language=""javascript"">
			function LinkMenu(anArray, e)
			{
				var src = """";
				var h = 0;
				var w = 0;

				e.cancelBubble = 'true';

				
				var menu = document.getElementById(""LinkMenu"");
				if (menu == null)
				{
					menu = document.createElement(""div"");
					menu.id = 'LinkMenu';
					menu.className = 'Menu';
					document.body.appendChild(menu);
				}
				menu.innerHTML = """";
				for (var i = 0; i < anArray.length; i++)
				{
					var pair = anArray[i];
					var each = """";
					var itemName = 'LinkMenu' + i;
					each += '<div class=""MenuItemNormal"" onmouseover=""MenuItemIn(this);"" onmouseout=""MenuItemOut(this);"">';
					each += '<span id=""' + itemName + '"" onclick=""MenuClick(' + ""'"" + pair[1] + ""'"" + ')"">' + pair[0] + '<' + '/span>';
					each += '<' + '/div>';
					menu.innerHTML += each;
					var item = document.getElementById(itemName);
					if (item.offsetWidth > w)
						w = item.offsetWidth;
					h += item.offsetHeight;
				}
				menu.innerHTML = '<div class=""MenuItems"" onmouseover=""MenuIn(this);"" onmouseout=""MenuOut(this);"">' + menu.innerHTML + '<' + '/div>';
				menu.style.left = document.body.scrollLeft + e.clientX;
				menu.style.top = document.body.scrollTop + e.clientY;
				menu.style.height = h;
				menu.style.width = w;
				timeout = window.setTimeout(""MenuTimeout()"", 4000, ""JavaScript"");
				menu.style.display = 'block';
			}
			
			var timeout = null;
			var tipOffTimeout = null;

			function TopicTipOn(anchor, id, event)
			{
				var targetY = document.body.scrollTop + event.clientY + 18;
				var targetX = document.body.scrollLeft + event.clientX + 12;
				
				TopicTip.style.left = targetX;
				TopicTip.style.top = targetY;
				var tip = 	document.getElementById(id);
				TopicTip.innerHTML = tip.innerHTML;
				TopicTip.style.display = 'block';
				if (tipOffTimeout != null)
					window.clearTimeout(tipOffTimeout);
				tipOffTimeout = window.setTimeout(""TopicTipOff()"", 4000, ""JavaScript"");
			}

			function TopicTipOff()
			{
				if (tipOffTimeout != null)
					window.clearTimeout(tipOffTimeout);
				tipOffTimeout = null;				
				TopicTip.style.display = 'none';
			}


			function MenuClick(url)
			{
				MenuHide();
				document.location.href = url;
			}
			
			function MenuIn(obj)
			{
				if (timeout == null)
					return;
				window.clearTimeout(timeout);
				timeout = null;
			}
			
			function MenuOut(obj)
			{
				timeout = window.setTimeout(""MenuTimeout()"", 1000, ""JavaScript"");
			}
			
			function MenuTimeout()
			{
				MenuHide();
			}
			
			function MenuHide()
			{
				var menu = document.getElementById(""LinkMenu"");
				menu.style.display = 'none';
			}

			function cleanObfuscatedLink(obj, text, URL)
			{
				obj.innerText = text;
				obj.href = URL;
			}

			function ShowObfuscatedLink(link)
			{
				var i = 0;
				for (i = 0; i < link.length; i++)
				{
					document.write(link.substr(i, 1));
					if (i < 10)
						document.write('..');
				}
			}
			
			function MenuItemIn(obj)
			{
				obj.className = 'MenuItemHover';
			}
			
			function MenuItemOut(obj)
			{
				obj.className = 'MenuItemNormal';
			}
</script>	
<span style=""display:none"">.</span>
");
	// not sure why we need the extra span, but we get display bugs on some pages
		}

		public override void WriteRule()
		{
			Write("<div class=\"Rule\"></div>");
		}


		StringBuilder _Footer = new StringBuilder();
		public override void AddToFooter(string s)
		{
			_Footer.Append(s);
		}

		override  public void End()
		{
			Write(_Footer.ToString());
		}

		override public void WriteClosePara()
		{
			Write("</p>");
		}

		override public void WriteOpenPara()
		{
			Write("<p" + css() + ">");
		}

		override public void WriteSingleLine(string each)
		{
			Write("<div " + css() + ">" + Formatter.EscapeHTML(each)  + "</div>");
		}

		override public void WriteOpenTable(TableCellInfo.AlignOption alignment, bool hasBorder, int width)
		{
			string styles = "";
			Write("<table ");
			switch (alignment)
			{
				case TableCellInfo.AlignOption.None:
					break;

				case TableCellInfo.AlignOption.Left:
					styles += ";margin-left: 0; float: left ";
					Write(" align=\"left\" ");
					break;

				case TableCellInfo.AlignOption.Right:
					styles += ";margin-left: 0; float: right";
					break;

				case TableCellInfo.AlignOption.Center:
					Write(" align=\"center\" ");
					break;
			}
			if (width > 0)
				Write(" width=\"" + width + "%\" ");
			string cls = "TableClass";
			if (!hasBorder)
				cls = "TableWithoutBorderClass";
			WriteLine("cellpadding=\"2\" cellspacing=\"1\" class=\"" + cls + "\"" + css(styles) + ">");
		}

		override public void WriteCloseTable()
		{
			WriteLine("</table>");
		}

		override public void WriteOpenTableRow()
		{
			WriteLine("<tr" + css() + ">");
		}

		override public void WriteCloseTableRow()
		{
			Write("</tr>");
		}

		public override void WriteLink(string URL, string tip, string content, string attributes)
		{
			Write("<a href=\"" + URL + "\" ");
			if (tip!= null)
				Write("title=\"" + Formatter.EscapeHTML(tip) + "\"");
			Write(GetAttributeString(attributes));
			Write(">");
			Write(Formatter.EscapeHTML(content));
			Write("</a>");
		}

		override public void WriteLabel(string forId, string text, string attributes)
		{
			Write(string.Format("<label{0} for=\"{1}\">{2}</label>", GetAttributeString(attributes), forId, Formatter.EscapeHTML(text)));
		}

		public override void WriteImage(string title, string URL, string linkToURL, string height, string width, string attributes)
		{
			if (linkToURL != null)
			{
				Write("<a href=\"" + linkToURL + "\" ");
				if (title != null)
					Write("title=\"" + Formatter.EscapeHTML(title) + "\"");
				Write(">");
			}
			Write(string.Format("<img src=\"{0}\" {1}alt=\"{2}\"{3}{4}{5}/>", 
				URL, 
				((linkToURL != null) ? "border=\"0\" " : string.Empty), 
				Formatter.EscapeHTML(title),
				((width != null) ? " width=\"" + width + "\"" : string.Empty ),
				((height != null) ? " height=\"" + height + "\"" : string.Empty ),
				GetAttributeString(attributes)
			));
			if (linkToURL != null)
			{
				Write("</a>");
			}
		}

		public override void NonBreakingSpace()
		{
			Write("&nbsp;");
		}


		public override void WriteErrorMessage(string title, string body)
		{
			Write("<span class=\"ErrorMessage\">");
			if (title != null)
			{
				Write("<span class=\"ErrorMessageTitle\">" + Formatter.EscapeHTML(title));
				if (body != null)
					Write(" ");
				Write("</span>");
			}
			if (body != null)
				Write("<span class=\"ErrorMessageBody\">" + Formatter.EscapeHTML(body) + "</span>");
			Write("</span>");
		}


		override public void WriteTableCell(string s,  bool isHighlighted, TableCellInfo.AlignOption alignment, int colSpan, int rowSpan, bool hasBorder, bool allowBreaks, int width, string bgColor)
		{
			Write("<td  ");
			if (isHighlighted)
			{
				if (hasBorder)
					Write("class=\"TableCellHighlighted\"");
				else
					Write("class=\"TableCellHighlightedNoBorder\"");
			}			
			else
			{
				if (hasBorder)
					Write("class=\"TableCell\"");
				else
					Write("class=\"TableCellNoBorder\"");
			}

			if (bgColor != null)
				Write(" style=\" background: " + Formatter.EscapeHTML(bgColor) + "\" ");

			if (!allowBreaks)
				Write(" nowrap ");
			if (width > 0)
				Write(" width=\"" + width + "%\" ");

			switch (alignment)
			{
				case TableCellInfo.AlignOption.None:
					break;

				case TableCellInfo.AlignOption.Left:
					Write(" align=\"left\" ");
					break;

				case TableCellInfo.AlignOption.Right:
					Write(" align=\"right\" ");
					break;

				case TableCellInfo.AlignOption.Center:
					Write(" align=\"center\" ");
					break;
			}

			if (colSpan != 1)
				Write(" colspan=\"" + colSpan + "\" ");
			if (rowSpan != 1)
				Write(" rowspan=\"" + rowSpan + "\" ");
				
			WriteLine(css() + ">" + s + "</td>");
		}


		override public void WriteOpenUnorderedList()
		{
			WriteLine("<ul" + css() + ">");
		}

		override public void WriteCloseUnorderedList()
		{
			WriteLine("</ul>");
		}

		override public void WriteCloseOrderedList()
		{
			WriteLine("</ol>");
		}

		override public void WriteOpenOrderedList()
		{
			WriteLine("<ol" + css() + ">");
		}

		override public void WriteOpenPreformatted()
		{
			WriteLine("<pre" + css() + ">");
		}

		override public void WriteClosePreformatted()
		{
			WriteLine("</pre>");
		}
		
		override public void WriteEndLine()
		{
			WriteLine("");
		}

		override public LineStyle Style 
		{
			get
			{
				return _CurrentStyle;
			}
			set
			{
				_CurrentStyle = value;
			}
		}


		override public void WriteListItem(string each)
		{
			WriteLine("<li" + css() + ">" + each + "</li>");
		}

		override public OutputFormat Format 
		{ 
			get 
			{
				return OutputFormat.HTML;
			}
		}

		override public void WriteHeading(string text, int level)
		{
			WriteLine("<h" + level  + css() + ">" + text + "</h" + level + ">");
		}

		override public void WriteOpenProperty(string name)
		{
			Write("<fieldset " + css() + " class=\"Property\" style=\"width: auto\">");
			Write("<legend class=\"PropertyName\">" + name + "</legend>");
			Write("<span class=\"PropertyValue\">");
		}

		override public void WriteCloseProperty()
		{
			WriteLine("</span>");
			WriteLine("</fieldset>");
		}

		override public void WriteOpenAnchor(string name)
		{
			Write("<a name=\"" + name + "\" class=\"Anchor\">");
		}

		override public void WriteCloseAnchor()
		{
			Write("</a>");
		}
		
		override public void FormStart(string method, string URI, string attributes)
		{
			WriteLine(string.Format("<form action=\"{0}\" method=\"{1}\" {2}>", URI, method, GetAttributeString(attributes)));
		}

		override public void FormEnd()
		{
			WriteLine("</form>");
		}

		override public void FormImageButton(string FieldName, string ImageURI, string TipString, string attributes)
		{
			Write("<input type=\"image\" src=\"" + ImageURI + "\"" + GetAttributeString(attributes) + " title=\"" + Formatter.EscapeHTML(TipString) + "\" />");
		}

		override public void FormSubmitButton(string FieldName, string label, string attributes)
		{
			Write("<input type=\"submit\" name=\"" + FieldName + "\"" + GetAttributeString(attributes) + " value=\"" + Formatter.EscapeHTML(label) + "\" />");
		}

		override public void FormResetButton(string FieldName, string label, string attributes)
		{
			Write("<input type=\"reset\" name=\"" + FieldName + "\"" + GetAttributeString(attributes) + " value=\"" + Formatter.EscapeHTML(label) + "\" />");
		}

		override public void FormInputBox(string FieldName, string fieldValue, int fieldLength, string attributes)
		{
			Write("<input type=\"text\" name=\"" + FieldName + "\" id=\"" + FieldName + "\"");
			Write((fieldLength>0)?" size=\"" + fieldLength.ToString() + "\"":string.Empty);
			Write(GetAttributeString(attributes));
			Write(" value=\"" + Formatter.EscapeHTML(fieldValue) + "\" />");
		}

		override public void FormTextarea(string FieldName, string fieldValue, int rows, int cols, string attributes)
		{
			
			Write("<textarea name=\"" + FieldName + "\"" 
				+ ((rows>0)?" rows=\"" + rows.ToString() + "\"" : string.Empty)
				+ ((cols>0)?" cols=\"" + cols.ToString() + "\"" : string.Empty) 
				+ GetAttributeString(attributes)
				+ ">"
				+ Formatter.EscapeHTML(fieldValue) 
				+ "</textarea>");
		}

		override public void FormCheckbox(string fieldName, string fieldValue, bool isChecked, string attributes)
		{
			Write("<input type=\"checkbox\" name=\"" + fieldName + "\" value=\"" + Formatter.EscapeHTML(fieldValue) + "\"" + GetAttributeString(attributes) + ((isChecked)?" checked=\"true\"" : string.Empty) + "/>");
		}
		override public void FormRadio(string fieldName, string fieldValue, bool isChecked, string attributes)
		{
			Write("<input type=\"radio\" name=\"" + fieldName + "\" value=\"" + Formatter.EscapeHTML(fieldValue) + "\"" + GetAttributeString(attributes) + ((isChecked)?" checked=\"true\" " : string.Empty) + "/>");
		}
		
		override public void FormHiddenField(string FieldName, string fieldValue, string attributes)
		{
			Write("<input style=\"display: none\" type=\"text\" name=\"" + FieldName + "\" value=\"" + Formatter.EscapeHTML(fieldValue) + "\"" + GetAttributeString(attributes) + " />");
		}

		public override void FormSelectField(string fieldName, int size, bool multiple, ArrayList options, string selectedOption, ArrayList values, object selectedValue, string attributes)
		{
			StringBuilder stringBuilder = new StringBuilder();
			// Start the select field.
			stringBuilder.Append(string.Format("<select name=\"{0}\" id=\"{0}\" size=\"{1}\"", fieldName, size));
			// Add the multiple attribute if required.
			if (true == multiple)
			{
				stringBuilder.Append(" multiple=\"multiple\"");
			}
			stringBuilder.Append(GetAttributeString(attributes));
			stringBuilder.Append(">"); // End the select element.
			// Add the options.
			if (null != values)
			{
				for (int i = 0; i < options.Count; i++)
				{
					stringBuilder.Append(string.Format("<option value=\"{0}\"",
						values[i].ToString()));
					if ((null != selectedValue) && (selectedValue.ToString().Length > 0) && (selectedValue.ToString() == values[i].ToString()))
					{
						stringBuilder.Append(" selected=\"selected\"");
					}
					stringBuilder.Append(string.Format(">{0}</option>",
						options[i].ToString()));
				}
			}
			else
			{
				foreach (object option in options)
				{
					stringBuilder.Append("<option");
					if ((null != selectedOption) && (selectedOption.Length > 0) && (selectedOption == option.ToString()))
					{
						stringBuilder.Append(" selected=\"selected\"");
					}
					stringBuilder.Append(string.Format(">{0}</option>", option.ToString()));
				}
			}
			// Close the select element.
			stringBuilder.Append("</select>");
			string selectField = stringBuilder.ToString();
			Write(selectField);
		}

		private string GetAttributeString (string attr)
		{
			if (attr == null || attr.IndexOf(">") > 0)
				return string.Empty;
			
			return " " + attr;
		}


	}
}
