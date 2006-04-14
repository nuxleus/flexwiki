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

		#region Static Instance methods for DateTime construction.
		[ExposedMethod(ExposedMethodFlags.NeedContext, "Answer an instance of a DateTime")]
		public static DateTime InstanceFromString(ExecutionContext ctx, string s)
		{
			DateTime answer = DateTime.MinValue;
			answer = DateTime.Parse(s);
			return answer;
		}

		[ExposedMethod(ExposedMethodFlags.NeedContext, "Answer an instance of a DateTime")]
		public static DateTime Instance(ExecutionContext ctx, int year, int month, int day, 
			[ExposedParameter(true)] int hour, [ExposedParameter(true)] int minute, 
			[ExposedParameter(true)] int second, [ExposedParameter(true)] int millisecond)
		{
			if (false == ctx.TopFrame.WasParameterSupplied(4))
				hour = 0;
			if (false == ctx.TopFrame.WasParameterSupplied(5))
				minute = 0;
			if (false == ctx.TopFrame.WasParameterSupplied(6))
				second = 0;
			if (false == ctx.TopFrame.WasParameterSupplied(7))
				millisecond = 0;
			return Instance2(year, month, day, hour, minute, second, millisecond);
		}

		public static DateTime Instance2(int year, int month, int day, 
			int hour, int minute, int second, int millisecond)
		{
			return new DateTime(year, month, day, hour, minute, second, millisecond);
		}
		#endregion

		[ExposedMethod(ExposedMethodFlags.Default, "Answer the earliest date that can be represented")]
		public static DateTime MinValue
		{
			get
			{
				return DateTime.MinValue;
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer the latest date that can be represented")]
		public static DateTime MaxValue
		{
			get
			{
				return DateTime.MaxValue;
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer the Date component of this DateTime")]
		public DateTime Date
		{
			get
			{
				return DateTime.Date;
			}
		}


		[ExposedMethod(ExposedMethodFlags.Default, "Answer the day of the month represented by this DateTime")]
		public int Day
		{
			get
			{
				return DateTime.Day;
			}
		}


		[ExposedMethod(ExposedMethodFlags.Default, "Answer the name of the day of the week represented by this DateTime")]
		public string DayOfWeek
		{
			get
			{
				return DateTime.DayOfWeek.ToString();
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer the day of the year represented by this DateTime")]
		public int DayOfYear
		{
			get
			{
				return DateTime.DayOfYear;
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer the hour of the day represented by this DateTime")]
		public int Hour
		{
			get
			{
				return DateTime.Hour;
			}
		}


		[ExposedMethod(ExposedMethodFlags.Default, "Answer the millisecond component of the time represented by this DateTime")]
		public int Millisecond
		{
			get
			{
				return DateTime.Millisecond;
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer the minute of the hourrepresented by this DateTime")]
		public int Minute
		{
			get
			{
				return DateTime.Minute;
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer the month of the year represented by this DateTime")]
		public int Month
		{
			get
			{
				return DateTime.Month;
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer the seconds component of the time represented by this DateTime")]
		public int Second
		{
			get
			{
				return DateTime.Second;
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer the time of day represented by this DateTime")]
		public TimeSpan TimeOfDay
		{
			get
			{
				return DateTime.TimeOfDay;
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer a DateTime representing today")]
		public static DateTime Today
		{
			get
			{
				return DateTime.Today;
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer the current date and time on this computer expressed as the coordinated universal time")]
		public static DateTime UtcNow
		{
			get
			{
				return DateTime.UtcNow;
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer the current date and time")]
		public static DateTime Now
		{
			get
			{
				return DateTime.Now;
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer the year represented by this DateTime")]
		public int Year
		{
			get
			{
				return DateTime.Year;
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Add the specified TimeSpan to this DateTime")]
		public DateTime Add(TimeSpan span)
		{
			return DateTime.Add(span);
		}
		
		[ExposedMethod(ExposedMethodFlags.Default, "Add the specified number of hours to this DateTime")]
		public DateTime AddHours(int delta)
		{
			return DateTime.AddHours(delta);
		}
		
		[ExposedMethod(ExposedMethodFlags.Default, "Add the specified number of milliseconds to this DateTime")]
		public DateTime AddMilliseconds(int delta)
		{
			return DateTime.AddMilliseconds(delta);
		}
		
		[ExposedMethod(ExposedMethodFlags.Default, "Add the specified number of minutes to this DateTime")]
		public DateTime AddMinutes(int delta)
		{
			return DateTime.AddMinutes(delta);
		}
		
		[ExposedMethod(ExposedMethodFlags.Default, "Add the specified number of months to this DateTime")]
		public DateTime AddMonths(int delta)
		{
			return DateTime.AddMonths(delta);
		}
		
		[ExposedMethod(ExposedMethodFlags.Default, "Add the specified number of days to this DateTime")]
		public DateTime AddDays(int delta)
		{
			return DateTime.AddDays(delta);
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Add the specified number of seconds to this DateTime")]
		public DateTime AddSeconds(int delta)
		{
			return DateTime.AddSeconds(delta);
		}
		
		[ExposedMethod(ExposedMethodFlags.Default, "Add the specified number of years to this DateTime")]
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

		[ExposedMethod(ExposedMethodFlags.Default, "Answer true if the year represented by this DateTime is a leap year; else answer false")]
		public static bool IsLeapYear(int year)
		{
			return DateTime.IsLeapYear(year);
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Subtract the given time span from this DateTime")]
		public DateTime Subtract(TimeSpan span)
		{
			return DateTime.Subtract(span);
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Calculate the difference between this DateTime and the supplied DateTime")]
		public TimeSpan SpanBetween(DateTime aDateTime)
		{
			return DateTime.Subtract(aDateTime);
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Convert this DateTime from universal coordinated time (UTC) to local time")]
		public DateTime ToLocalTime
		{
			get
			{
				return DateTime.ToLocalTime();
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Convert this local DateTime to universal coordinated time (UTC)")]
		public DateTime ToUniversalTime
		{
			get
			{
				return DateTime.ToUniversalTime();
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer a long-form string representation for the date component of this DateTime")]
		public string ToLongDateString
		{
			get
			{
				return DateTime.ToLongDateString();
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer a short-form string representation for the date component of this DateTime")]
		public string ToShortDateString
		{
			get
			{
				return DateTime.ToShortDateString();
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer a long-form string representation for the time component of this DateTime")]
		public string ToLongTimeString
		{
			get
			{
				return DateTime.ToLongTimeString();
			}
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer a short-form string representation for the time component of this DateTime")]
		public string ToShortTimeString
		{
			get
			{
				return DateTime.ToShortTimeString();
			}
		}		

		[ExposedMethod(ExposedMethodFlags.Default, "Determine whether this object is equal to another object")]
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

		[ExposedMethod(ExposedMethodFlags.Default, "Answer the number of days in the specified month of the specified year")]
		public static int DaysInMonth(int year, int month)
		{
			return DateTime.DaysInMonth(year, month);
		}

		[ExposedMethod(ExposedMethodFlags.Default, "Answer the number of calendar weeks in the specified month of the specified year including incomplete weeks")]
		public static int WeeksInMonth(int year, int month, int firstDayOfWeek)
		{
			int numberOfWeeks = 1;
			DateTime firstOfMonth = new DateTime(year, month, 1);
			int daysInMonth = DateTime.DaysInMonth(year, month);
			//Calculate the number of days in the first calendar week.
			int daysInFirstWeek = (7 - ((int)firstOfMonth.DayOfWeek - firstDayOfWeek)) % 7;
			if (0 == daysInFirstWeek)
				daysInFirstWeek = 7;
			int remainingDays = 0;
			int wholeWeeks = Math.DivRem(daysInMonth - daysInFirstWeek, 7, out remainingDays);
			numberOfWeeks += wholeWeeks;
			if (remainingDays > 0)
			{
				numberOfWeeks++;
			}
			return numberOfWeeks;
		}
	}
}
