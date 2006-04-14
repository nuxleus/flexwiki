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
using System.Globalization;

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

		[ExposedMethod(ExposedMethodFlags.Default, "Determine whether this object is equal to another object")]
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

		[ExposedMethod(ExposedMethodFlags.Default, "Answer the addition of this integer to the supplied rawValue")]
		public int Add(object value)
		{
			return Value + Int32.Parse(value.ToString(), System.Globalization.CultureInfo.CurrentCulture);
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer the subtraction of the supplied rawValue from this integer")]
		public int Subtract(object value)
		{
			return Value - Int32.Parse(value.ToString(), System.Globalization.CultureInfo.CurrentCulture);
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer the integer division of this integer by the supplied rawValue")]
		public int Divide(object value)
		{
			return Value / Int32.Parse(value.ToString(), System.Globalization.CultureInfo.CurrentCulture);
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer the multiplication of this integer with the supplied rawValue")]
		public int Multiply(object value)
		{
			return Value * Int32.Parse(value.ToString(), System.Globalization.CultureInfo.CurrentCulture);
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer the integer addition of the supplied values")]
		public static int Add(object firstValue, object secondValue)
		{
			return Int32.Parse(firstValue.ToString(), CultureInfo.CurrentCulture) + 
				Int32.Parse(secondValue.ToString(), CultureInfo.CurrentCulture);
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer the subtraction of the second supplied rawValue from the first")]
		public static int Subtract(object firstValue, object secondValue)
		{
			return Int32.Parse(firstValue.ToString(), CultureInfo.CurrentCulture) - 
				Int32.Parse(secondValue.ToString(), CultureInfo.CurrentCulture);
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer the integer division of the first supplied rawValue by the second")]
		public static int Divide(object firstValue, object secondValue)
		{
			return Int32.Parse(firstValue.ToString(), CultureInfo.CurrentCulture) / 
				Int32.Parse(secondValue.ToString(), CultureInfo.CurrentCulture);
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer the multiplication of first supplied rawValue with the second")]
		public static int Multiply(object firstValue, object secondValue)
		{
			return Int32.Parse(firstValue.ToString(), CultureInfo.CurrentCulture) * 
				Int32.Parse(secondValue.ToString(), CultureInfo.CurrentCulture);
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer true or false depending on whether this number is less than or equal to the supplied integer")]
		public bool LessThanOrEqualTo(int otherValue)
		{
			return Value <= otherValue;
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer true or false depending on whether this number is less than the supplied integer")]
		public bool LessThan(int otherValue)
		{
			return Value < otherValue;
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer true or false depending on whether this number is greater than or equal to the supplied integer")]
		public bool GreaterThanOrEqualTo(int otherValue)
		{
			return Value >= otherValue;
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer true or false depending on whether this number is greater than the supplied integer")]
		public bool GreaterThan(int otherValue)
		{
			return Value > otherValue;
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
				throw new ExecutionException(null, "Can't compare Integer to object of type " + BELType.ExternalTypeNameForType(obj.GetType()));
			return Value.CompareTo(other.Value);
		}

		#endregion
	}
}
