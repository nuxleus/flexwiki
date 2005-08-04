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
	/// Summary description for UndefinedObject.
	/// </summary>
	[ExposedClass("UndefinedObject", "The type whose sole instance is 'null'")]
	public class UndefinedObject : ReflectedValueSource, IBELObject
	{
		public virtual IOutputSequence ToOutputSequence()
		{
			return new WikiSequence(ToString());
		}

		static public UndefinedObject Instance = new UndefinedObject();

		UndefinedObject() : base()
		{
		}

		public override string ToString()
		{
			return "null";
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer true if this object is null; else false")]
		public bool IsNull
		{
			get
			{
				return true;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "If this object is null, answer the result of evaluating the block; else answer null" )]
		public IBELObject IfNull(ExecutionContext ctx, Block block)
		{
			return block.Value(ctx);
		}

		
		[ExposedMethod(ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "If this object is not null, answer the result of evaluating the block; else answer null" )]
		public IBELObject IfNotNull(ExecutionContext ctx, Block block)
		{
			return Instance;
		}


		[ExposedMethod(ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "If this object is null, answer the result of evaluating the first block; else answer the result of evaluating the second block" )]
		public IBELObject IfNullElse(ExecutionContext ctx, Block nullBlock, Block notNullBlock)
		{
			return nullBlock.Value(ctx);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext,"If this object is not null, answer the result of evaluating the first block; else answer the result of evaluating the second block" )]
		public IBELObject IfNotNullElse(ExecutionContext ctx, Block notNullBlock, Block nullBlock)
		{
			return nullBlock.Value(ctx);
		}


	}
}