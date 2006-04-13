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
using System.Reflection;
using System.Collections;

namespace FlexWiki
{
	/// <summary>
	/// Summary description for BELMember.
	/// </summary>
	[ExposedClass("Member", "Describes a property or method of an object")]
	class BELMember : BELObject
	{
		public BELMember(string name, ExposedMethod vis, MethodInfo info)
		{
			_Name = name;
			ExposedMethod = vis;
			MethodInfo = info;
				
		}
		public MethodInfo MethodInfo;
		public ExposedMethod ExposedMethod;
		public string _Name;

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a description of this member")]
		public string Description
		{
			get
			{
				return ExposedMethod.Description;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the name of this member")]
		public string Name
		{
			get
			{
				return _Name;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the Type of object that actually declares this member")]
		public Type DeclaringType
		{
			get
			{
				return MethodInfo.DeclaringType;
			}
		}

		public override IOutputSequence ToOutputSequence()
		{
			return new WikiSequence("BELMember: " + Name);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the full signature used to invoke this method")]
		public string Signature
		{
			get
			{
				StringBuilder b = new StringBuilder();
				if (MethodInfo.ReturnType != null)
				{
					b.Append(BELObject.ExternalTypeNameForType(BELTypeForParameterType(MethodInfo.ReturnType)));
					b.Append(" ");
				}
				b.Append(Name);
				b.Append(ArgumentsSignature);
				return b.ToString();
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer just teh arguments component of the signature used to invoke this method")]
		public string ArgumentsSignature
		{
			get
			{
				StringBuilder b = new StringBuilder();
				ParameterInfo []parms = MethodInfo.GetParameters();
				bool first = true;
				for (int each = 0; each < parms.Length; each++)
				{
					if (each == 0 && NeedsExecutionContext)
						continue;	// skip the method invocation context if present
					if (first)
						b.Append("(");
					else
						b.Append(", ");
					ParameterInfo pi = parms[each];
					b.Append(BELObject.ExternalTypeNameForType(BELTypeForParameterType(pi.ParameterType)));
					b.Append(" ");
					b.Append(parms[each].Name);
					if (IsOptionalParameter(pi))
						b.Append(" {optional}");
					first = false;
				}
				if (ExposedMethod.AllowsVariableArguments)
				{
					if (first)
						b.Append("(...");
					else
						b.Append(", ...");
					first = false;
				}

				if (!first)
					b.Append(")");
				return b.ToString();
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the Type of object returned by this property (or null if none)")]
		public Type ReturnType
		{
			get
			{
				if (MethodInfo.ReturnType == null)
					return null;
				return BELTypeForParameterType(MethodInfo.ReturnType);
			}
		}


		static Type BELTypeForParameterType(Type t)
		{
			if (t.GetInterface(typeof(IBELObject).Name) != null)
				return t;
			if (t == typeof(DateTime))
				return typeof(BELDateTime);
			if (t == typeof(Type))
				return typeof(BELType);
			if (t == typeof(int))
				return typeof(BELInteger);
			if (t == typeof(string))
				return typeof(BELString);
			if (t == typeof(ArrayList))
				return typeof(BELArray);
			return typeof(BELObject);
		}


		public bool NeedsExecutionContext
		{
			get
			{
				if (ExposedMethod.IsCustomArgumentProcessor)
					return true;
				if (ExposedMethod.NeedContext)
					return true;
				if (ExposedMethod.CachePolicy == ExposedMethodFlags.CachePolicyComplex)
					return true;
				if (ExposedMethod.AllowsVariableArguments)
					return true;				
				ParameterInfo []parms = MethodInfo.GetParameters();
				for (int each = 0; each < parms.Length; each++)
				{
					if (IsOptionalParameter(parms[each]))
						return true;
				}
				return false;
			}
		}

		static public bool IsOptionalParameter(ParameterInfo pi)
		{
			ExposedParameter pa = ExposedParameterAttributeFor(pi);
			if (pa == null)
				return false;
			return pa.IsOptional;
		}

		static ExposedParameter ExposedParameterAttributeFor(ParameterInfo pi)
		{
			foreach (Attribute each in pi.GetCustomAttributes(true))
			{
				if (each is ExposedParameter)
					return each as ExposedParameter;
			}
			return null;
		}

	};

}
