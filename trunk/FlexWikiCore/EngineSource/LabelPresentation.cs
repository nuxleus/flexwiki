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
using FlexWiki.Formatting;

namespace FlexWiki
{
	/// <summary>
	/// 
	/// </summary>
	[ExposedClass("LabelPresentation", "Presents a label")]
	public class LabelPresentation : PresentationPrimitive
	{

		public LabelPresentation(string forId, string content, string attributes)
		{
			Init(forId, content, attributes);
		}
		private void Init(string forId, string content, string attributes)
		{
			_text = content;
			_for = forId;
			_attributes = attributes;
		}

		private string _for;
		public string ForId
		{
			get
			{
				return _for;
			}
		}

		private string _text;
		public string Text
		{
			get
			{
				return _text;
			}
		}


		public override void OutputTo(WikiOutput output)
		{
			output.WriteLabel(ForId, Text, Attributes);
		}

	}
}
