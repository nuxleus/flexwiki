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
using System.Reflection;
using System.Collections;
using System.Text;

namespace FlexWiki
{
	/// <summary>
	/// 
	/// </summary>
	[ExposedClass("Object", "Provides common behavior for most objects")]
	public abstract class BELObject : ReflectedValueSource, IBELObject
	{

		public virtual IOutputSequence ToOutputSequence()
		{
			return new WikiSequence(ToString());
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer true if this object is null; else false")]
		public virtual bool IsNull
		{
			get
			{
				return false;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "If this object is null, answer the result of evaluating the block; else answer null")]
		public virtual IBELObject IfNull(ExecutionContext ctx, Block block)
		{
			return UndefinedObject.Instance;
		}

		
		[ExposedMethod(ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "If this object is not null, answer the result of evaluating the block; else answer null" )]
		public virtual IBELObject IfNotNull(ExecutionContext ctx, Block block)
		{
			return block.Value(ctx);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Determine whether this object is equal to another object")]
		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode ();
		}


		[ExposedMethod(ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "If this object is null, answer the result of evaluating the first block; else answer the result of evaluating the second block" )]
		public virtual IBELObject IfNullElse(ExecutionContext ctx, Block nullBlock, Block notNullBlock)
		{
			return notNullBlock.Value(ctx);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "If this object is not null, answer the result of evaluating the first block; else answer the result of evaluating the second block" )]
		public virtual IBELObject IfNotNullElse(ExecutionContext ctx, Block notNullBlock, Block nullBlock)
		{
			return notNullBlock.Value(ctx);
		}

		[ExposedMethod("ToString", ExposedMethodFlags.CachePolicyNone, "Answer this object converted to a string")]
		public string AsString
		{
			get
			{
				return ToString();
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the Type of this object")]
		public override BELType Type
		{
			get
			{
				return base.Type;
			}
		}


	}
}
