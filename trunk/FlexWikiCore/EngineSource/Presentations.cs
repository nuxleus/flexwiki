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
		public static ImagePresentation Image(ExecutionContext ctx, string URL, [ExposedParameter(true)] string title, [ExposedParameter(true)] string linkToURL, [ExposedParameter(true)] string height, [ExposedParameter(true)] string width)
		{
			return new ImagePresentation(title, URL, linkToURL, height, width);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Present a hyperlink")]
		public static LinkPresentation Link(ExecutionContext ctx, string URL, string content, [ExposedParameter(true)] string tip)
		{
			return new LinkPresentation(content, URL, tip);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Present a hidden field")]
		public static FormHiddenFieldPresentation HiddenField(string fieldName, string fieldValue)
		{
			return new FormHiddenFieldPresentation(fieldName, fieldValue);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Present an input field")]
		public static FormInputFieldPresentation InputField(string fieldName, string fieldValue, int fieldLength)
		{
			return new FormInputFieldPresentation(fieldName, fieldValue, fieldLength);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Start a form")]
		public static FormStartPresentation FormStart(string URI, string method)
		{
			return new FormStartPresentation(URI, method);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "End a form")]
		public static FormEndPresentation FormEnd()
		{
			return new FormEndPresentation();
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Present a submit button")]
		public static FormSubmitButtonPresentation SubmitButton(string fieldName, string label)
		{
			return new FormSubmitButtonPresentation(fieldName, label);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Present an image button")]
		public static FormImageButtonPresentation ImageButton(string fieldName, string imageURI, string tipString)
		{
			return new FormImageButtonPresentation(fieldName, imageURI, tipString);
		}

	}
}
