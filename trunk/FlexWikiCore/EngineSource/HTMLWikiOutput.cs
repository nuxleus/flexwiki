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
				styles = " style='" + styles + "'";
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
<script type=""text/javascript"" language='javascript'>
			function LinkMenu(anArray)
			{
				var src = """";
				var h = 0;
				var w = 0;

				window.event.cancelBubble = 'true';

				
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
					each += '<DIV class=""MenuItemNormal"" onmouseover=""MenuItemIn(this);"" onmouseout=""MenuItemOut(this);"">';
					each += '<SPAN id=""' + itemName + '"" onclick=""MenuClick(' + ""'"" + pair[1] + ""'"" + ')"">' + pair[0] + '</SPAN>';
					each += '</DIV>';
					menu.innerHTML += each;
					var item = document.getElementById(itemName);
					if (item.offsetWidth > w)
						w = item.offsetWidth;
					h += item.offsetHeight;
				}
				menu.innerHTML = '<div onmouseover=""MenuIn(this);"" onmouseout=""MenuOut(this);"">' + menu.innerHTML + '</div>';
				menu.style.left = event.clientX;
				menu.style.top = event.clientY;
				menu.style.height = h;
				menu.style.width = w;
				timeout = null;
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
				window.navigate(url);
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
			
			function MenuItemIn(obj)
			{
				obj.className = 'MenuItemHover';
			}
			
			function MenuItemOut(obj)
			{
				obj.className = 'MenuItemNormal';
			}
</script>	
<span style='display:none'>.</span>
");
	// not sure why we need the extra span, but we get display bugs on some pages
		}

		public override void WriteRule()
		{
			Write("<div class='Rule'></div>");
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

		override public void WriteOpenTable(TableCellInfo.AlignOption alignment, bool hasBorder)
		{
			string styles = "";
			Write("<table ");
			switch (alignment)
			{
				case TableCellInfo.AlignOption.None:
					break;

				case TableCellInfo.AlignOption.Left:
					styles += ";margin-left: 0; float: left ";
					Write(" align='left' ");
					break;

				case TableCellInfo.AlignOption.Right:
					styles += ";margin-left: 0; float: right";
					break;

				case TableCellInfo.AlignOption.Center:
					styles += ";margin-left: 0";
					Write(" align='center' ");
					break;
			}
			string cls = "TableClass";
			if (!hasBorder)
				cls = "TableWithoutBorderClass";
			WriteLine("cellpadding='2' cellspacing='1' class='" + cls + "'" + css(styles) + ">");
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

		public override void WriteLink(string URL, string tip, string content)
		{
			Write("<a href=\"" + URL + "\" ");
			if (tip!= null)
				Write("title=\"" + Formatter.EscapeHTML(tip) + "\"");
			Write(">");
			Write(Formatter.EscapeHTML(content));
			Write("</a>");
		}

		public override void WriteImage(string title, string URL, string linkToURL, string height, string width)
		{
			if (linkToURL != null)
			{
				Write("<a href=\"" + linkToURL + "\" ");
				if (title != null)
					Write("title=\"" + Formatter.EscapeHTML(title) + "\"");
				Write(">");
			}
			Write("<img ");
			if (linkToURL != null)
				Write("border='0' "); 
			Write("src=\"" + URL + "\" ");
			if (title != null)
				Write("alt=\"" + Formatter.EscapeHTML(title) + "\"");
			if (width != null)
				Write(" width=\"" + width + "\"");
			if (height != null)
				Write(" height=\"" + height + "\"");
			Write(">");
			if (linkToURL != null)
			{
				Write("</a>");
			}
		}

		public override void WriteErrorMessage(string title, string body)
		{
			Write("<span class='ErrorMessage'>");
			if (title != null)
			{
				Write("<span class='ErrorMessageTitle'>" + Formatter.EscapeHTML(title));
				if (body != null)
					Write(" ");
				Write("</span>");
			}
			if (body != null)
				Write("<span class='ErrorMessageBody'>" + Formatter.EscapeHTML(body) + "</span>");
			Write("</span>");
		}


		override public void WriteTableCell(string s,  bool isHighlighted, TableCellInfo.AlignOption alignment, int colSpan, int rowSpan, bool hasBorder, bool allowBreaks)
		{
			Write("<td  ");
			if (isHighlighted)
			{
				if (hasBorder)
					Write("class='TableCellHighlighted'");
				else
					Write("class='TableCellHighlightedNoBorder'");
			}			
			else
			{
				if (hasBorder)
					Write("class='TableCell'");
				else
					Write("class='TableCellNoBorder'");
			}

			if (!allowBreaks)
				Write(" nowrap ");

			switch (alignment)
			{
				case TableCellInfo.AlignOption.None:
					break;

				case TableCellInfo.AlignOption.Left:
					Write(" align='left' ");
					break;

				case TableCellInfo.AlignOption.Right:
					Write(" align='right' ");
					break;

				case TableCellInfo.AlignOption.Center:
					Write(" align='center' ");
					break;
			}

			if (colSpan != 1)
				Write(" colspan='" + colSpan + "' ");
			if (rowSpan != 1)
				Write(" rowspan='" + rowSpan + "' ");
				
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
			WriteLine("<fieldset " + css() + " class='Property' style='width: auto'>");
			WriteLine("<legend class='PropertyName'>" + name + "</legend> <span class='PropertyValue'>");
		}
		
		override public void WriteCloseProperty()
		{
			WriteLine("</span>");
			WriteLine("</fieldset>");
		}


		override public void FormStart(string method, string URI)
		{
			WriteLine("<form method='" + method + "' action='" + URI + "'>");
		}

		override public void FormEnd()
		{
			WriteLine("</form>");
		}

		override public void FormImageButton(string FieldName, string ImageURI, string TipString)
		{
			WriteLine("<INPUT type='image' src='" + ImageURI + @"' title='" + Formatter.EscapeHTML(TipString) + "'>");
		}

		override public void FormSubmitButton(string FieldName, string label)
		{
			WriteLine("<INPUT name='" + FieldName + "' value ='" + Formatter.EscapeHTML(label) + @"' type='submit'>");
		}

		override public void FormInputBox(string FieldName, string fieldValue, int fieldLength)
		{
			WriteLine("<input size='" + fieldLength + "' type='text'  name='" + FieldName + "' value ='" + Formatter.EscapeHTML(fieldValue) + @"'>");
		}

		
		override public void FormHiddenField(string FieldName, string fieldValue)
		{
			WriteLine("<input style='display: none' type='text'  name='" + FieldName + "' value ='" + Formatter.EscapeHTML(fieldValue) + @"'>");
		}



	}
}
