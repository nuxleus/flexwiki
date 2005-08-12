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
	/// Summary description for ExposableParseTreeNode.
	/// </summary>
	public abstract class ExposableParseTreeNode : ParseTreeNode
	{
		public ExposableParseTreeNode() : base()
		{
		}

		/// <summary>
		/// Expose the reciever.  If it's something like a literal, we'll 
		/// just end up answering a BELObject for it.  If it's a property or 
		/// function reference, we'll use the given 
		/// context to look it up.
		/// </summary>
		/// <param name="ctx"></param>
		/// <returns></returns>
		public abstract IBELObject Expose(ExecutionContext ctx);

	}
}
