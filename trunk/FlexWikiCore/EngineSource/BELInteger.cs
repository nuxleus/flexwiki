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
	/// 
	/// </summary>
	[ExposedClass("Integer", "A positive or negitive number (without a fractional component)")]
	public class BELInteger : BELObject, IComparable
	{
		public BELInteger() : base()
		{
			_Value = 0;
		}

		public BELInteger(int val) : base()
		{
			_Value = val;
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Determine whether this object is equal to another object")]
		public override bool Equals(object obj)
		{
			if (!(obj is int))
				return false;
			return Value.Equals(obj);
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode ();
		}


		int _Value;
		public int Value
		{
			get
			{
				return _Value;
			}
			set
			{
				_Value = value;
			}
		}

		public override string ToString()
		{
			return Value.ToString();
		}


		#region IWikiSequenceProducer Members

		public override IOutputSequence ToOutputSequence()
		{
			return new WikiSequence(Value.ToString());
		}


		#endregion

		#region IComparable Members

		public int CompareTo(object obj)
		{
			BELInteger other = obj as BELInteger;
			if (other == null)
				throw new ExecutionException("Can't compare Integer to object of type " + BELType.ExternalTypeNameForType(obj.GetType()));
			return Value.CompareTo(other.Value);
		}

		#endregion
	}
}
