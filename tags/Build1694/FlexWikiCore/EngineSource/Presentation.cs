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
using System.Collections;


namespace FlexWiki
{
	/// <summary>
	/// Summary description for Presentation.
	/// </summary>
	[ExposedClass("Presentation", "Presents something (see subtypes for details)")]
	public abstract class Presentation : BELObject, IPresentation
	{
		public Presentation()
		{
		}

		#region IOutputSequence Members

		public IPresentation ToPresentation(IWikiToPresentation p)
		{
			return this;
		}

		public void AddAllTo(ArrayList list)
		{
			list.Add(this);
		}

		#endregion

		public override IOutputSequence ToOutputSequence()
		{
			return this;			
		}


		#region IPresentation Members

		public abstract void OutputTo(WikiOutput output);

		#endregion
	}
}
