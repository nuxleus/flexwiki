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

namespace FlexWiki
{
	/// <summary>
	/// Summary description for Presentations.
	/// </summary>
	[ExposedClass("Presentations", "Provides access to specific kinds of presentations")]
	public class Presentations :BELObject
	{
		public Presentations()
		{
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Present an image")]
		public static ImagePresentation Image(ExecutionContext ctx, string URL, [ExposedParameter(true)] string title, [ExposedParameter(true)] string linkToURL, [ExposedParameter(true)] string height, [ExposedParameter(true)] string width, [ExposedParameter(true)] string attributes)
		{
			return new ImagePresentation(title, URL, linkToURL, height, width, attributes);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Present a hyperlink")]
		public static LinkPresentation Link(ExecutionContext ctx, string URL, string content, [ExposedParameter(true)] string tip, [ExposedParameter(true)] string attributes)
		{
			return new LinkPresentation(content, URL, tip, attributes);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Present a hidden field")]
		public static FormHiddenFieldPresentation HiddenField(ExecutionContext ctx, string fieldName, string fieldValue, [ExposedParameter(true)] string attributes)
		{
			return new FormHiddenFieldPresentation(fieldName, fieldValue, attributes);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Present an input field")]
		public static FormInputFieldPresentation InputField(ExecutionContext ctx, string fieldName, string fieldValue, [ExposedParameter(true)] int fieldLength, [ExposedParameter(true)] string attributes)
		{
			return new FormInputFieldPresentation(fieldName, fieldValue, fieldLength, attributes);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Present a textarea")]
		public static FormTextareaPresentation Textarea(ExecutionContext ctx, string fieldName, string fieldValue, [ExposedParameter(true)] int rows, [ExposedParameter(true)] int columns, [ExposedParameter(true)] string attributes)
		{
			return new FormTextareaPresentation(fieldName, fieldValue, rows, columns, attributes);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Present a checkbox")]
		public static FormCheckboxPresentation Checkbox(ExecutionContext ctx, string fieldName, string fieldValue, bool isChecked, [ExposedParameter(true)] string attributes)
		{
			return new FormCheckboxPresentation(fieldName, fieldValue, isChecked, attributes);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Present a radio")]
		public static FormRadioPresentation Radio(ExecutionContext ctx, string fieldName, string fieldValue, bool isChecked, [ExposedParameter(true)] string attributes)
		{
			return new FormRadioPresentation(fieldName, fieldValue, isChecked, attributes);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Present a label")]
		public static LabelPresentation Label(ExecutionContext ctx, string forId, string text, [ExposedParameter(true)] string attributes)
		{
			return new LabelPresentation(forId, text, attributes);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Start a form")]
		public static FormStartPresentation FormStart(ExecutionContext context, string URI, string method, [ExposedParameter(true)] string attributes)
		{
			return new FormStartPresentation(URI, method, attributes);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "End a form")]
		public static FormEndPresentation FormEnd()
		{
			return new FormEndPresentation();
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Present a submit button")]
		public static FormSubmitButtonPresentation SubmitButton(ExecutionContext ctx, string fieldName, string label, [ExposedParameter(true)] string attributes)
		{
			return new FormSubmitButtonPresentation(fieldName, label, attributes);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Present a reset button")]
		public static FormResetButtonPresentation ResetButton(ExecutionContext ctx, string fieldName, string label, [ExposedParameter(true)] string attributes)
		{
			return new FormResetButtonPresentation(fieldName, label, attributes);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Present an image button")]
		public static FormImageButtonPresentation ImageButton(ExecutionContext ctx, string fieldName, string imageURI, string tipString, [ExposedParameter(true)] string attributes)
		{
			return new FormImageButtonPresentation(fieldName, imageURI, tipString, attributes);
		}

		[ExposedMethod(ExposedMethodFlags.NeedContext, "Present a combobox select field")]
		public static FormSelectFieldPresentation ComboSelectField(ExecutionContext context, string fieldName, ArrayList options,
			[ExposedParameter(true)] string selectedOption, [ExposedParameter(true)] ArrayList values,
			[ExposedParameter(true)] object selectedValue, [ExposedParameter(true)] string attributes)
		{
			if (true == context.TopFrame.WasParameterSupplied(4))
			{
				if (options.Count != values.Count)
				{
					throw new ArgumentException("The values array does not contain the same number of items as the options array", "values");
				}
			}
			return new FormSelectFieldPresentation(fieldName, 1, false, options, selectedOption, values, selectedValue, attributes);
		}

		[ExposedMethod(ExposedMethodFlags.NeedContext, "Present a listbox select field")]
		public static FormSelectFieldPresentation ListSelectField(ExecutionContext context, string fieldName,  
			int size, bool multiple, ArrayList options, [ExposedParameter(true)] string selectedOption,	 
			[ExposedParameter(true)] ArrayList values, [ExposedParameter(true)] object selectedValue,
			[ExposedParameter(true)] string attributes)
		{
			if (true == context.TopFrame.WasParameterSupplied(6))
			{
				if (options.Count != values.Count)
				{
					throw new ArgumentException("The values array does not contain the same number of items as the options array", "values");
				}
			}
			return new FormSelectFieldPresentation(fieldName, size, multiple, options, selectedOption, values, selectedValue, attributes);
		}

	}
}
