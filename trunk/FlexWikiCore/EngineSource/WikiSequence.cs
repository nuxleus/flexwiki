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
using System.Text;
using System.Collections;
using FlexWiki.Formatting;

namespace FlexWiki
{
	/// <summary>
	/// 
	/// </summary>
	public class WikiSequence : IOutputSequence
	{
		public WikiSequence()
		{	
		}

		public WikiSequence(string s)
		{
			_Value.Append(s);
		}

		public void Append(string s)
		{
			_Value.Append(s);
		}

		StringBuilder _Value = new StringBuilder();

		public IPresentation ToPresentation(IWikiToPresentation p)
		{
			return new StringPresenation(p.WikiToPresentation(_Value.ToString()));
		}

		public void AddAllTo(ArrayList list)
		{
			list.Add(this);
		}

		public string Value
		{
			get
			{
				return _Value.ToString();
			}
		}

	}
}
