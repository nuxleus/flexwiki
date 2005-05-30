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
	[ExposedClass("FormCheckboxPresentation", "Presents a checkbox")]
	public class FormCheckboxPresentation : FlexWiki.PresentationPrimitive
	{
		public FormCheckboxPresentation(string fieldName, string fieldValue, bool isChecked, string attributes)
		{
			Init(fieldName, fieldValue, isChecked, attributes);
		}
		private void Init(string fieldName, string fieldValue, bool isChecked, string attributes)
		{
			_fieldName = fieldName;
			_FieldValue = fieldValue;
			_isChecked = isChecked;
			_attributes = attributes;
		}


		public bool _isChecked;
		public bool IsChecked
		{
			get
			{
				return _isChecked;
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
			output.FormCheckbox(FieldName, FieldValue, IsChecked, Attributes);
		}


	}
}
