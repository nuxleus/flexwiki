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
	/// Summary description for BELDateTime.
	/// </summary>
	[ExposedClass("DateTime", "Represents a specific point in time (a date and a time)")]
	public class BELDateTime : BELObject, IComparable
	{
		public BELDateTime(DateTime aDateTime)
		{
			_DateTime = aDateTime;
		}

		DateTime _DateTime;

		public DateTime DateTime
		{
			get
			{
				return _DateTime;
			}
		}

		public override string ToString()
		{
			return DateTime.ToString();
		}

		public override IOutputSequence ToOutputSequence()
		{
			return new WikiSequence(ToString());
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyForever, "Answer the earliest date that can be represented")]
		public static DateTime MinValue
		{
			get
			{
				return DateTime.MinValue;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyForever, "Answer the latest date that can be represented")]
		public static DateTime MaxValue
		{
			get
			{
				return DateTime.MaxValue;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the Date component of this DateTime")]
		public DateTime Date
		{
			get
			{
				return DateTime.Date;
			}
		}


		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the day of the month represented by this DateTime")]
		public int Day
		{
			get
			{
				return DateTime.Day;
			}
		}


		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the name of the day of the week represented by this DateTime")]
		public string DayOfWeek
		{
			get
			{
				return DateTime.DayOfWeek.ToString();
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the day of the year represented by this DateTime")]
		public int DayOfYear
		{
			get
			{
				return DateTime.DayOfYear;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the hour of the day represented by this DateTime")]
		public int Hour
		{
			get
			{
				return DateTime.Hour;
			}
		}


		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the millisecond component of the time represented by this DateTime")]
		public int Millisecond
		{
			get
			{
				return DateTime.Millisecond;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the minute of the hourrepresented by this DateTime")]
		public int Minute
		{
			get
			{
				return DateTime.Minute;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the month of the year represented by this DateTime")]
		public int Month
		{
			get
			{
				return DateTime.Month;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the seconds component of the time represented by this DateTime")]
		public int Second
		{
			get
			{
				return DateTime.Second;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the time of day represented by this DateTime")]
		public TimeSpan TimeOfDay
		{
			get
			{
				return DateTime.TimeOfDay;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a DateTime representing today")]
		public static DateTime Today
		{
			get
			{
				return DateTime.Today;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the current date and time on this computer expressed as the coordinated universal time")]
		public static DateTime UtcNow
		{
			get
			{
				return DateTime.UtcNow;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the current date and time")]
		public static DateTime Now
		{
			get
			{
				return DateTime.Now;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the year represented by this DateTime")]
		public int Year
		{
			get
			{
				return DateTime.Year;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Add the specified TimeSpan to this DateTime")]
		public DateTime Add(TimeSpan span)
		{
			return DateTime.Add(span);
		}
		
		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Add the specified number of hours to this DateTime")]
		public DateTime AddHours(int delta)
		{
			return DateTime.AddHours(delta);
		}
		
		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Add the specified number of milliseconds to this DateTime")]
		public DateTime AddMilliseconds(int delta)
		{
			return DateTime.AddMilliseconds(delta);
		}
		
		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Add the specified number of minutes to this DateTime")]
		public DateTime AddMinutes(int delta)
		{
			return DateTime.AddMinutes(delta);
		}
		
		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Add the specified number of months to this DateTime")]
		public DateTime AddMonths(int delta)
		{
			return DateTime.AddMonths(delta);
		}
		
		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Add the specified number of days to this DateTime")]
		public DateTime AddDays(int delta)
		{
			return DateTime.AddDays(delta);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Add the specified number of seconds to this DateTime")]
		public DateTime AddSeconds(int delta)
		{
			return DateTime.AddSeconds(delta);
		}
		
		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Add the specified number of years to this DateTime")]
		public DateTime AddYears(int delta)
		{
			return DateTime.AddYears(delta);
		}
		
		public int CompareTo(object obj)
		{
			if (obj is BELDateTime)
				return DateTime.CompareTo(((BELDateTime)obj).DateTime);
			if (obj is DateTime)
				return DateTime.CompareTo(obj);
			throw new ArgumentException("When using CompareTo() to compare dates, the argument must be a DateTime; got " + BELType.ExternalTypeNameForType(obj.GetType()));
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer true if the year represented by this DateTime is a leap year; else answer false")]
		public static bool IsLeapYear(int year)
		{
			return DateTime.IsLeapYear(year);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Subtract the given time span from this DateTime")]
		public DateTime Subtract(TimeSpan span)
		{
			return DateTime.Subtract(span);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Calculate the difference between this DateTime and the supplied DateTime")]
		public TimeSpan SpanBetween(DateTime aDateTime)
		{
			return DateTime.Subtract(aDateTime);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Convert this DateTime from universal coordinated time (UTC) to local time")]
		public DateTime ToLocalTime
		{
			get
			{
				return DateTime.ToLocalTime();
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Convert this local DateTime to universal coordinated time (UTC)")]
		public DateTime ToUniversalTime
		{
			get
			{
				return DateTime.ToUniversalTime();
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a long-form string representation for the date component of this DateTime")]
		public string ToLongDateString
		{
			get
			{
				return DateTime.ToLongDateString();
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a short-form string representation for the date component of this DateTime")]
		public string ToShortDateString
		{
			get
			{
				return DateTime.ToShortDateString();
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a long-form string representation for the time component of this DateTime")]
		public string ToLongTimeString
		{
			get
			{
				return DateTime.ToLongTimeString();
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a short-form string representation for the time component of this DateTime")]
		public string ToShortTimeString
		{
			get
			{
				return DateTime.ToShortTimeString();
			}
		}		

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Determine whether this object is equal to another object")]
		public override bool Equals(object obj)
		{
			if (!(obj is DateTime))
				return false;
			return this.DateTime.Equals(obj);
		}

		public override int GetHashCode()
		{
			return this.DateTime.GetHashCode();
		}


	}
}
