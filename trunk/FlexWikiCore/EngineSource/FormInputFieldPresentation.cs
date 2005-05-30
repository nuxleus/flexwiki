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
		private int		_fieldLength;
		private string	_fieldValue;

		public FormInputFieldPresentation(string fieldName, string fieldValue)
		{
			Init(fieldName, fieldValue, int.MinValue, null);
		}
		public FormInputFieldPresentation(string fieldName, string fieldValue, int fieldLength)
		{
			Init(fieldName, fieldValue, fieldLength, null);
		}
		public FormInputFieldPresentation(string fieldName, string fieldValue, int fieldLength, string attributes)
		{
			Init(fieldName, fieldValue, fieldLength, attributes);
		}
		private void Init(string fieldName, string fieldValue, int fieldLength, string attributes)
		{
			_fieldName = fieldName;
			_fieldValue = fieldValue;
			_fieldLength = fieldLength;
			_attributes = attributes;
		}


		public int FieldLength
		{
			get
			{
				return _fieldLength;
			}
		}

		public string FieldValue
		{
			get
			{
				return _fieldValue;
			}
		}

		public override void OutputTo(WikiOutput output)
		{
			output.FormInputBox(FieldName, FieldValue, FieldLength, Attributes);
		}


	}
}
