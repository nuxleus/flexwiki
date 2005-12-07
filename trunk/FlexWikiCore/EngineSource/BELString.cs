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

namespace FlexWiki
{
	/// <summary>
	/// 
	/// </summary>
	[ExposedClass("String", "A sequence of characters")]
	public class BELString : BELObject, IComparable
	{
		public BELString() : base()
		{
			Value = "";
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNever, "Answer a new string than is the concatenation of the given number of copies of this string")]
		public string Repeat(int count)
		{
			StringBuilder answer = new StringBuilder();
			for (int i = 0; i < count; i++)
				answer.Append(Value);
			return answer.ToString();
		}

		static BELString _Empty = new BELString("");
		static public BELString Empty
		{
			get
			{
				return _Empty;
			}
		}	

		public BELString(string s) : base()
		{
			Value = s;
		}

		public string Value
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

		string _Value;

		public override string ToString()
		{
			return Value;
		}


		public override IOutputSequence ToOutputSequence()
		{
			return new WikiSequence(Value);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the number of characters in this string")]
		public int Length
		{
			get
			{
				return Value.Length;
			}
		}
		
		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a copy of this string with the characters reversed")] 
		public string Reverse
		{
			get
			{
				StringBuilder b = new StringBuilder();
				foreach (char c in Value)
					b.Insert(0, c);
				return b.ToString();
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a substring of this string starting at the given character and possibly limited to the given number of characters")]
		public string Substring(ExecutionContext ctx, int index, [ExposedParameter(true)] int length)
		{
			if (ctx.TopFrame.WasParameterSupplied(2))
				return Value.Substring(index, length);
			else
				return Value.Substring(index);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Determine whether this object is equal to another object")]
		public override bool Equals(object obj)
		{
			if (!(obj is string))
				return false;
			return Value.Equals(obj as string);
		}
		
		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Determine whether this string is equal to another string (ignoring case)")]
		public  bool EqualsCaseInsensitive(object obj)
		{
			if (!(obj is string))
				return false;
			return String.Compare(Value, obj as string, true) == 0;
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Determine whether this string contains another string")]
		public  bool Contains(object obj)
		{
			if (!(obj is string))
				return false;
			return Value.IndexOf(obj as string) != -1;
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the string converted to an integer")]
		public int AsInteger
		{
			get
			{
				if (Value == null)
					return 0;
				try
				{
					return int.Parse(Value);
				}
				catch (Exception)
				{
					return 0;	
				}
			}
		}

		public override int GetHashCode()
		{
			if (Value == null)
				return 0;
			return Value.GetHashCode();
		}

		[ExposedMethod(ExposedMethodFlags.NeedContext, "Limit a string to a particular length.")]
		public static string MaxLengthString(ExecutionContext ctx, string s, int length, 
			[ExposedParameter(true)] string suffix)
		{
			// Check to see if a suffix parameter was supplied. Default to "...".
			if (false == ctx.TopFrame.WasParameterSupplied(3))
			{
				suffix = "...";
			}

			return MaxLengthString2(s, length, suffix);
		}

		public static string MaxLengthString2(string s, int length, string suffix)
		{
			// Make sure we haven't been given a negative length.
			if (length < 0)
			{
				throw new ArgumentException("Requested length may not be negative", "length");
			}

			if (null == suffix)
			{
				suffix = "...";
			}

			// Only return the requested length if length is less than or equal to suffix.Length.
			if (length <= suffix.Length)
			{
				// Return the string if it's shorter than or equal to the requested length.
				if (s.Length <= length)
				{
					return s;
				}

				// Otherwise return the string without the suffix.
				return s.Substring(0, length);
			}

			// Return the string if it's shorter than or equal to the requested length.
			if (s.Length <= length)
			{
				return s;
			}

			// Otherwise return the string with the suffix.
			return s.Substring(0, length - suffix.Length) + suffix;
		}

		public static string MaxLengthString2(string s, int length)
		{
			return MaxLengthString2(s, length, "...");
		}

		#region IComparable Members

		public int CompareTo(object obj)
		{
			BELString other = obj as BELString;
			if (other == null)
				throw new ExecutionException(null, "Can't compare String to object of type " + BELType.ExternalTypeNameForType(obj.GetType()));
			return Value.CompareTo(other.Value);
		}

		#endregion
	}
}
