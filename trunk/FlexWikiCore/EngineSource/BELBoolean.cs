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
using FlexWiki;

namespace FlexWiki
{
	/// <summary>
	/// Summary description for BELBoolean.
	/// </summary>
	[ExposedClass("Boolean", "Represents true or false")]
	public class BELBoolean : BELObject
	{
		BELBoolean() : base()
		{
		}

		BELBoolean(bool f)
		{
			_Value = f;
		}

		static public BELBoolean True = new BELBoolean(true);
		static public BELBoolean False = new BELBoolean(false);

		bool _Value;
		public bool Value
		{
			get
			{
				return _Value;
			}
		}

		public static BELBoolean OfValue(bool val)
		{
			if (val)
				return True;
			else
				return False;
		}

		public override string ToString()
		{
			return Value ? "true" : "false";
		}

		public override IOutputSequence ToOutputSequence()
		{
			return new WikiSequence(ToString());
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the logical reverse of this boolean (true for false; false for true)")]
		public IBELObject Not
		{
			get
			{
				return Value ? False : True;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "If this object is true, evaluate the block and answer the result of the evaluation; otherwise answer null" )]
		public IBELObject IfTrue(ExecutionContext ctx, Block block)
		{
			if (Value)
				return block.Value(ctx);
			else
				return UndefinedObject.Instance;
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "If this object is true, evaluate the first block and answer the result; otherwise evaluate the other block and return the result" )]
		public IBELObject IfTrueIfFalse(ExecutionContext ctx, Block trueBlock, Block falseBlock)
		{
			if (Value)
				return trueBlock.Value(ctx);
			else
				return falseBlock.Value(ctx);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "If this object is false, evaluate the first block and answer the result; otherwise evaluate the other block and return the result" )]
		public IBELObject IfFalseIfTrue(ExecutionContext ctx, Block falseBlock, Block trueBlock)
		{
			if (Value)
				return trueBlock.Value(ctx);
			else
				return falseBlock.Value(ctx);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "If this object is false, evaluate the block and answer the result of the evaluation; otherwise answer null" )]
		public IBELObject IfFalse(ExecutionContext ctx, Block block)
		{
			if (!Value)
				return block.Value(ctx);
			else
				return UndefinedObject.Instance;
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Determine whether this object is equal to another object")]
		public override bool Equals(object obj)
		{
			if (!(obj is Boolean))
				return false;
			return Value.Equals(obj);
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode ();
		}


	}
}
