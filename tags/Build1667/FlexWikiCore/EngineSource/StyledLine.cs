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
using FlexWiki;


namespace FlexWiki.Formatting
{
	/// <summary>
	/// Summary description for StyledLine.
	/// </summary>
	/// 
	
	public enum LineStyle
	{
		Add,
		Delete,
		Unchanged
	};

	public class StyledLine
	{
		string _Text;
		LineStyle _Style;

		public StyledLine(string s, LineStyle style)
		{
			_Text = s;
			_Style = style;
		}

		public StyledLine CombinedWith(StyledLine followingLine)
		{
			return new StyledLine(Text + Environment.NewLine + followingLine.Text, CombineStyles(Style, followingLine.Style));
		}

		LineStyle CombineStyles(LineStyle one, LineStyle two)
		{
			if (one == LineStyle.Add || two == LineStyle.Add)
				return LineStyle.Add;
			if (one == LineStyle.Delete || two == LineStyle.Delete)
				return LineStyle.Delete;
			return LineStyle.Unchanged;
		}

		public string Text
		{
			get
			{
				return _Text;
			}
		}

		public LineStyle Style
		{
			get
			{
				return _Style;
			}
		}

	}
}
