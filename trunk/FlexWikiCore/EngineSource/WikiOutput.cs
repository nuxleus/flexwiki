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
using System.Text;
using FlexWiki;

namespace FlexWiki.Formatting
{
	/// <summary>
	/// Summary description for WikiOutput.
	/// </summary>
	/// 
	public enum OutputFormat
	{
		/* 
		Text,
		XAML,
		Wiki, // ?????
		*/
		HTML,
		Testing
	}

	public abstract class WikiOutput
	{
		public static WikiOutput ForFormat(OutputFormat aFormat, WikiOutput parent)
		{
			switch (aFormat)
			{
				case OutputFormat.HTML:
					return new HTMLWikiOutput(parent);

				case OutputFormat.Testing:
					return new TestWikiOutput(parent);

				default:
					throw new Exception("Unsupported output type requested: " + aFormat.ToString());
			}
		}

		WikiOutput _Parent;

		public bool IsNested
		{
			get
			{
				return _Parent == null;
			}
		}

		public int GetNestingLevel() 
		{
			if (_Parent != null) 
			{
				return 1 + _Parent.GetNestingLevel();
			} 
			else 
			{
				return 0;
			}
		}

		public WikiOutput(WikiOutput parent)
		{
			_TextWriter = new StringWriter();
			_Parent = parent;
		}

		public void Write(string s)
		{
			TextWriter().Write(s);
		}
		public void WriteLine(string s)
		{
			TextWriter().WriteLine(s);
		}

		public TextWriter TextWriter()
		{
			return _TextWriter;
		}




		override public string ToString()
		{
			return TextWriter().ToString();
		}

		abstract public void Begin();
		abstract public void End();
		abstract public void AddToFooter(string s);
		abstract public void WriteErrorMessage(string title, string body);
		abstract public void WriteOpenProperty(string name);
		abstract public void WriteCloseProperty();
		abstract public void WriteCloseUnorderedList();
		abstract public void WriteOpenUnorderedList();
		abstract public void WriteCloseOrderedList();
		abstract public void WriteOpenOrderedList();
		abstract public void WriteOpenPreformatted();
		abstract public void WriteRule();
		abstract public void WriteClosePreformatted();
		abstract public void WriteEndLine();
		abstract public void WriteSingleLine(string each);
		abstract public void WriteListItem(string each);
		abstract public void WriteHeading(string text, int level);
		abstract public LineStyle Style {get; set;}
		abstract public void WriteOpenPara();
		abstract public void WriteClosePara();
		abstract public void WriteOpenTable(TableCellInfo.AlignOption alignment, bool hasBorder);
		abstract public void WriteCloseTable();
		abstract public void WriteOpenTableRow();
		abstract public void WriteCloseTableRow();
		abstract public void WriteTableCell(string s,  bool isHighlighted, TableCellInfo.AlignOption alignment, int colSpan, int RowSpan, bool hasBorder, bool allowBreaks);
		abstract public OutputFormat Format { get; }
		abstract public void WriteImage(string title, string URL, string linkToURL, string height, string width);
		abstract public void WriteLink(string URL, string tip, string content);
		abstract public void FormStart(string method, string URI);
		abstract public void FormEnd();
		abstract public void FormImageButton(string FieldName, string ImageURI, string TipString);
		abstract public void FormSubmitButton(string FieldName, string label);
		abstract public void FormInputBox(string FieldName, string fieldValue, int fieldLength);
		abstract public void FormHiddenField(string FieldName, string fieldValue);



		private TextWriter	_TextWriter;
	}
}
