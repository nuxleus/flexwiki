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
	/// Summary description for Block.
	/// </summary>
	[ExposedClass("Block", "A chunk of WikiTalk code, ready to be evaluated")]
	public class Block : BELObject
	{
		public Block(ExposableParseTreeNode tree, ArrayList parameters, IScope containingScope)
		{
			ParseTree = tree;
			if (parameters == null)
				_Parameters = new ArrayList();
			else
				_Parameters = parameters;
			_ContainingScope = containingScope;
		}

		ArrayList _Parameters;
		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer an Array of all of the parameters for this block")]
		public ArrayList Parameters
		{
			get
			{
				return _Parameters;
			}
		}

		IScope _ContainingScope;
		public IScope ContainingScope
		{
			get
			{
				return _ContainingScope;
			}
		}

		public int ParameterCount
		{
			get
			{
				return Parameters == null ? 0 : Parameters.Count;
			}
		}

		[ExposedMethod("Value", ExposedMethodFlags.CachePolicyNever | ExposedMethodFlags.AllowsVariableArguments, "Answer the value of evaluating the block")]
		public IBELObject ExposedValue(ExecutionContext ctx)
		{
			return Value(ctx, ctx.TopFrame.ExtraArguments);
		}

		public IBELObject Value(ExecutionContext ctx)
		{
			return Value(ctx, null);
		}

		public IBELObject Value(ExecutionContext ctx, ArrayList args)
		{
			if (ParseTree == null)
				return UndefinedObject.Instance;
			BlockScope blockScope = new BlockScope(ContainingScope);
			if (args != null)
			{
				if (args.Count != ParameterCount)
					throw new ArgumentException("Incorrect number of parameters for block.  Need " + ParameterCount + "; got " + args.Count);
				// Determine desired types
				ArrayList neededTypes = new ArrayList();
				foreach (BlockParameter parm in Parameters)
				{
					if (parm.TypeName == null) 
					{
						neededTypes.Add(null);	 // they said it could be anything
						continue;
					}
					BELType t = (BELType)(ctx.TypeRegistry.Registry[parm.TypeName]);
					if (t == null)
						throw new ArgumentException("Block parameter (" + parm.Identifier + ") requires unknown type (" + parm.TypeName + ")");
					neededTypes.Add(t);
				}

				// Check types, as requested
				ArrayList convertedArgs = new ArrayList();
				for(int i = 0; i < ParameterCount; i++)
				{
					BlockParameter bp = (BlockParameter)(Parameters[i]);
					convertedArgs.Add(BELType.ConvertToBELObjectIfNeeded(args[i]));
					if (bp.TypeName == null)
						continue; 	 // they said it could be anything
					IBELObject arg = (IBELObject)(convertedArgs[i]); 
					Type parmType = ((BELType)(neededTypes[i])).CLRType;
					if (!parmType.IsAssignableFrom(arg.GetType()))
						throw new MemberInvocationException("Block parameter " + bp.Identifier + " is not of the correct type (was " 
							+ ExternalTypeNameForType(arg.GetType()) + 
							", but needed " + ExternalTypeNameForType(parmType) + ")");
				}
				
				// OK, we have them and they're type correct and converted to CLR types where needed.  Add them into the block scope
				for(int i = 0; i < ParameterCount; i++)
				{
					BlockParameter bp = (BlockParameter)(Parameters[i]);
					blockScope.AddParameter(bp.Identifier, (IBELObject)(convertedArgs[i]));
				}
			}
			ctx.PushScope(blockScope);
			IBELObject answer = ParseTree.Expose(ctx);
			ctx.PopScope();
			return answer;
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "Evaluate the given block and continue to evaluate it while it evaluates to true")]
		public IBELObject WhileTrue(ExecutionContext ctx, Block block)
		{
			if (ParameterCount != 0)
				throw new ArgumentException("Conditional block for WhileTrue can't accept parameters.");
			return Loop(ctx, block, true);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone | ExposedMethodFlags.NeedContext, "Evaluate the given block and continue to evaluate it while it evaluates to true")]
		public IBELObject WhileFalse(ExecutionContext ctx, Block block)
		{
			if (ParameterCount != 0)
				throw new ArgumentException("Conditional block for WhileFalse can't accept parameters.");
			return Loop(ctx, block, false);
		}

		IBELObject Loop(ExecutionContext ctx, Block block, bool halt)
		{
			while (true)
			{
				IBELObject objValue = Value(ctx);
				BELBoolean test = objValue as BELBoolean;
				if (test == null)
					throw new ExecutionException("WhileTrue/WhileFalse block must evaluate to a boolean.  Got " + BELType.BELTypeForType(objValue.GetType()).ExternalTypeName + " instead.");
				if (test.Value == halt)
					break;
			}
			return BELString.Empty;
		}


		public ExposableParseTreeNode ParseTree;

		public override IOutputSequence ToOutputSequence()
		{
			return new WikiSequence(ToString());
		}

	}
}
