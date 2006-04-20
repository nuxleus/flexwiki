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
	/// Summary description for PresentationPrimitive.
	/// </summary>
	[ExposedClass("PresentationPrimitive", "Presents something specific (see subtypes for details")]
	public abstract class PresentationPrimitive : Presentation
	{
		protected string	_attributes = string.Empty;
		protected string	_fieldName	= string.Empty;


		public PresentationPrimitive()
		{
		}
		public PresentationPrimitive(string fielName)
		{
			_fieldName = fielName;
		}
		public PresentationPrimitive(string fielName, string attributes)
		{
			_fieldName = fielName;
			_attributes = attributes;
		}

		public string Attributes
		{
			get
			{
				return _attributes;
			}
		}

		public string FieldName
		{
			get
			{
				return _fieldName;
			}
		}
	}
}
