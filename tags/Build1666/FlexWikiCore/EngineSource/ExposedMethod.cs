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
	[Flags]
	public enum ExposedMethodFlags
	{
		CachePolicyNever = 0,
		CachePolicyNone = 1,
		CachePolicyForever = 2,
		CachePolicyComplex = 3,
		/// <summary>
		/// true if the object is willing to be fully responsible for evaluating all arguments directly as ParseTreeNodes
		/// </summary>
		IsCustomArgumentProcessor = 4,
		AllowsVariableArguments = 8,
		NeedContext = 16,

		Default = 0
	}

	/// <summary>
	/// 
	/// </summary>
	[AttributeUsage(AttributeTargets.Property|AttributeTargets.Method)]
	public class ExposedMethod : System.Attribute
	{

		const int CachePolicyMask = 0x03;
		
		ExposedMethodFlags Flags = 0;

		/// <summary>
		/// Visible as a BEL method with a specified cache policy
		/// </summary>
		public ExposedMethod(ExposedMethodFlags fl, string description)
		{
			Flags = fl;
			Description = description;
		}
	
		/// <summary>
		/// Visible as a BEL method with a specified cache policy
		/// </summary>
		public ExposedMethod(string overrideName, ExposedMethodFlags fl, string description)
		{
			Flags = fl;
			Description = description;
			_OverrideName = overrideName;
		}

		public ExposedMethodFlags CachePolicy
		{
			get
			{
				return (ExposedMethodFlags)((int)Flags & CachePolicyMask);
			}
		}

		public string Description;

		string _OverrideName;
		public string OverrideName
		{
			get
			{
				return _OverrideName;
			}
		}

		public bool IsCustomArgumentProcessor
		{
			get
			{
				return (Flags & ExposedMethodFlags.IsCustomArgumentProcessor) != 0;
			}
		}

		
		public bool NeedContext
		{
			get
			{
				return (Flags & ExposedMethodFlags.NeedContext) != 0;
			}
		}

		public bool AllowsVariableArguments
		{
			get
			{
				return (Flags & ExposedMethodFlags.AllowsVariableArguments) != 0;
			}
		}

	}
}
