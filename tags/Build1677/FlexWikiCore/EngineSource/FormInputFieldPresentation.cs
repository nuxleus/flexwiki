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
	[ExposedClass("FormInputFieldPresentation", "Presents a form input field")]
	public class FormInputFieldPresentation : FlexWiki.PresentationPrimitive
	{
		public FormInputFieldPresentation(string fieldName, string fieldValue, int fieldLength)
		{
			_FieldName = fieldName;
			_FieldValue = fieldValue;
			_FieldLength = fieldLength;
		}

		public string _FieldName;
		public string FieldName
		{
			get
			{
				return _FieldName;
			}
		}

		public int _FieldLength;
		public int FieldLength
		{
			get
			{
				return _FieldLength;
			}
		}


		public string _FieldValue;
		public string FieldValue
		{
			get
			{
				return _FieldValue;
			}
		}

		public override void OutputTo(WikiOutput output)
		{
			output.FormInputBox(FieldName, FieldValue, FieldLength);
		}


	}
}
