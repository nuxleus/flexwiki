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
	[ExposedClass("FormTextareaPresentation", "Presents a form textarea")]
	public class FormTextareaPresentation : FlexWiki.PresentationPrimitive
	{
		public FormTextareaPresentation(string fieldName, string fieldValue, int rows, int cols, string attributes)
		{
			Init(fieldName, fieldValue, rows, cols, attributes);
		}

		private void Init(string fieldName, string fieldValue, int rows, int cols, string attributes)
		{
			_fieldName = fieldName;
			_fieldValue = fieldValue;
			_rows = rows;
			_cols = cols;
			_attributes = attributes;
		}

		public int _rows;
		public int Rows
		{
			get
			{
				return _rows;
			}
		}
		public int _cols;
		public int Cols
		{
			get
			{
				return _cols;
			}
		}



		public string _fieldValue;
		public string FieldValue
		{
			get
			{
				return _fieldValue;
			}
		}

		public override void OutputTo(WikiOutput output)
		{
			output.FormTextarea(FieldName, FieldValue, Rows, Cols, Attributes);
		}


	}
}
