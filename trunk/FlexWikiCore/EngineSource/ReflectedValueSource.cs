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
using System.Reflection;
using System.Text;

namespace FlexWiki
{
	public abstract class ReflectedValueSource : IValueSource
	{
		public IBELObject ValueOf(string name, ArrayList arguments, ExecutionContext ctx)
		{
			BELMember mi = (BELMember)(Type.BELMembers[name]);
			if (mi== null)
				return null;

			ParameterInfo []parms = mi.MethodInfo.GetParameters();
			int need = parms.Length;
			int got = (arguments == null) ? 0 : arguments.Count;
			bool needExecutionContext = mi.NeedsExecutionContext;
			if (needExecutionContext && (parms.Length == 0 || parms[0].ParameterType != typeof(ExecutionContext)))
				throw new ImplementationException("Incorrectly defined ExposedMethod property or function.  First parameter of " + mi.MethodInfo.DeclaringType.FullName + "." + mi.MethodInfo.Name + " must be an ExecutionContext");
			if (needExecutionContext)
				need--;
			if (got > need && !(mi.ExposedMethod.AllowsVariableArguments))
				throw new MemberInvocationException("Incorrect number of arguments (too many) for " + ExternalTypeName + "." + name + " (need " + need + ", got " + got + ")");

			// Figure out what arguments we should be passing
			ArrayList args = new ArrayList();
			InvocationFrame invocationFrame = new InvocationFrame();
			if (needExecutionContext)
				args.Add(ctx);
			
			ArrayList parameterPresentFlags = null;
			if (needExecutionContext)
			{
				parameterPresentFlags = new ArrayList();
				parameterPresentFlags.Add(true);		// we know for sure the first arg is supplied; it's the execution context :-)					
			}
			int offset = (needExecutionContext ? 1 : 0);
			for (int each = offset; each < parms.Length; each++)
			{
				object arg = null;
				
				if (arguments != null && (each - offset) < arguments.Count)
					arg = (ParseTreeNode)(arguments[each - offset]);
				if (!BELMember.IsOptionalParameter(parms[each]) && arg == null)
					throw new MemberInvocationException("Missing argument " + (each - offset) + " for " + ExternalTypeName + "." + name);
				if (parameterPresentFlags != null)
					parameterPresentFlags.Add(arg != null);
				if (mi.ExposedMethod.IsCustomArgumentProcessor)
				{
					args.Add(arg);
				}
				else
				{
					if (arg == null)
						args.Add(AbsentValueForParameter(parms[each]));
					else
						args.Add(ConvertFromBELObjectIfNeeded(((ExposableParseTreeNode)arg).Expose(ctx)));
				}
			}
			invocationFrame.WasParameterSuppliedFlags = parameterPresentFlags;

			// If we have extras (beyond those needed) and they're allowed, stash them in the MIC, too
			if (mi.ExposedMethod.AllowsVariableArguments)
			{
				ArrayList extras = new ArrayList();
				int extraCount = got - need;
				if (arguments != null)
				{
					for (int i = need; i < got; i++)
					{
						object arg = arguments[i];
						if (mi.ExposedMethod.IsCustomArgumentProcessor)
						{
							extras.Add(arg);
						}
						else
						{
							if (arg == null)
								extras.Add(null);
							else
								extras.Add(ConvertFromBELObjectIfNeeded(((ExposableParseTreeNode)arg).Expose(ctx)));
						}
					}						
				}
				invocationFrame.ExtraArguments = extras;
			}

			// Check types
			for (int each = 0; each < parms.Length; each++)
			{
				bool bad = false;
				if (args[each] == null)
				{
					if (parms[each].ParameterType.IsValueType)
						bad = true;
				}
				else
				{
					if (!parms[each].ParameterType.IsAssignableFrom(args[each].GetType()))
						bad = true;
				}
				if (bad)
					throw new MemberInvocationException("Argument " + each + " is not of the correct type (was " 
						+ ExternalTypeNameForType(args[each].GetType()) + 
						", but needed " + ExternalTypeNameForType(parms[each].ParameterType) + ")");
			}

			// And now, invoke away!!
			object result = null;
			invocationFrame.PushScope(ctx.CurrentScope); // the new frame starts with the same scope as this one
			ctx.PushFrame(invocationFrame);
			try
			{
				result = mi.MethodInfo.Invoke(this, args.ToArray());
			}
			catch (TargetInvocationException ex)
			{
				throw ex.InnerException;
			}
			ctx.PopFrame();
			if (mi.ExposedMethod.CachePolicy == ExposedMethodFlags.CachePolicyNever)
				ctx.AddCacheRule(new CacheRuleNever());
			return Wrap(result);
		}
		protected string SignatureFor(string name, ArrayList args)
		{
			StringBuilder sig = new StringBuilder();
			sig.Append(name);
			if (args != null && args.Count > 0)
			{
				sig.Append("(");
				bool isFirst = true;
				foreach (Type aType in args)
				{
					if (!isFirst)
						sig.Append(", ");
					isFirst = false;
					sig.Append(ExternalTypeNameForType(aType));
				}
				sig.Append(")");
			}
			return sig.ToString();
		}
		protected object AbsentValueForParameter(ParameterInfo pi)
		{
			if (!pi.ParameterType.IsValueType)
				return null;
			if (pi.ParameterType == typeof(int))
				return int.MinValue;
			if (pi.ParameterType == typeof(DateTime))
				return DateTime.MinValue;

			throw new ExecutionException("Unsupported type (" + pi.ParameterType.FullName + ") for optional parameter " + pi.Name + " in method " + pi.Member.Name);
		}
		protected static IBELObject Wrap(object obj)
		{
			return ConvertToBELObjectIfNeeded(obj);
		}
		public static IBELObject ConvertToBELObjectIfNeeded(object obj)
		{
			if (obj is IBELObject)
				return (IBELObject)obj;
			if (obj == null)
				return UndefinedObject.Instance;
			if (obj is Boolean)
				return BELBoolean.OfValue((bool)obj);
			if (obj is DateTime)
				return new BELDateTime((DateTime)obj);
			if (obj is TimeSpan)
				return new BELTimeSpan((TimeSpan)obj);
			if (obj is Type)
				return BELType.BELTypeForType((Type)obj);
			if (obj is int)
				return new BELInteger((int)obj);
			if (obj is string)
				return new BELString(obj as string);
			if (obj is ArrayList)
				return new BELArray(obj as ArrayList);
			throw new ExecutionException("Object of type " + obj.GetType().FullName + " can not be converted to a BEL object");
		}
		public static object ConvertFromBELObjectIfNeeded(IBELObject obj)
		{
			if (obj is BELDateTime)
				return (obj as BELDateTime).DateTime;
			if (obj is BELTimeSpan)
				return (obj as BELTimeSpan).TimeSpan;
			if (obj is BELBoolean)
				return (obj as BELBoolean).Value;
			if (obj is BELInteger)
				return (obj as BELInteger).Value;
			if (obj is BELString)
				return (obj as BELString).Value;
			if (obj is BELArray)
				return (obj as BELArray).Array;
			if (obj is UndefinedObject)
				return null;
			if (obj is BELType)
				return (obj as BELType).CLRType;
			return obj;
		}

		public virtual BELType Type
		{
			get
			{
				Type t = GetType();
				return BELType.BELTypeForType(t);
			}
		}

		public string ExternalTypeName
		{
			get
			{
				return ExternalTypeNameForType(GetType());
			}
		}

		public static string ExternalTypeNameForType(Type aType)
		{
			ExposedClass c = ClassAttributeForType(aType);
			if (c == null)
				return aType.FullName;
			return c.Name;
		}
		
		public static ExposedClass ClassAttributeForType(Type aType)
		{
			foreach(Attribute attr in aType.GetCustomAttributes(true)) 
			{
				if (attr is ExposedClass) 

					return attr as ExposedClass;
			}
			return null;
		}
		protected static bool IsExposedClass(Type aType)
		{
			return ClassAttributeForType(aType) != null;
		}
		
		protected static bool IsBELObject(object anObject)
		{
			return IsExposedClass(anObject.GetType());
		}
	}
}