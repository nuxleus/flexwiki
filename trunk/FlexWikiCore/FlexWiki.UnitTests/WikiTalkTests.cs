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
using System.IO;
using System.Data;
using NUnit.Framework;

namespace FlexWiki.UnitTests
{
	/// <summary>
	/// Summary description for WikiTalkTests.
	/// </summary>
	[TestFixture]
	public class WikiTalkTests
	{
		#region BELDateTime.Instance tests
		[Test]
		public void DateTimeInstanceNormalTest()
		{
			DateTime expectedDateTime = new DateTime(2005, 1, 13, 13, 32, 45, 416);
			DateTime resultDateTime = BELDateTime.Instance2(2005, 1, 13, 13, 32, 45, 416);
			Assert.AreEqual(expectedDateTime, resultDateTime, "Checking that the resulting DateTime is 13-Jan-2005 13:32:45:416.");
		}
		[Test]
		public void DateTimeInstanceMinTest()
		{
			DateTime expectedDateTime = DateTime.MinValue;
			DateTime resultDateTime = BELDateTime.Instance2(expectedDateTime.Year, expectedDateTime.Month, expectedDateTime.Day,
				expectedDateTime.Hour, expectedDateTime.Minute, expectedDateTime.Second, expectedDateTime.Millisecond);
			Assert.AreEqual(expectedDateTime, resultDateTime, "Checking that the resulting DateTime is the same as DateTime.MinValue.");
		}
		[Test]
		public void DateTimeInstanceMaxTest()
		{
			DateTime expectedDateTime = DateTime.MaxValue - new TimeSpan(TimeSpan.TicksPerMillisecond - 1);
			DateTime resultDateTime = BELDateTime.Instance2(expectedDateTime.Year, expectedDateTime.Month, expectedDateTime.Day,
				expectedDateTime.Hour, expectedDateTime.Minute, expectedDateTime.Second, expectedDateTime.Millisecond);
			Assert.AreEqual(expectedDateTime, resultDateTime, "Checking that the resulting DateTime is the same as DateTime.MaxValue.");
		}
		#endregion
		#region BELDateTime DaysInMonth tests
		[Test]
		public void DateTimeDaysInMonthNormalTest()
		{
			int daysInMonth = BELDateTime.DaysInMonth(2005, 1);
			Assert.AreEqual(31, daysInMonth, "Checking that BELDateTime.DaysInMonth gives 31 days for January 2005");
		}
		[Test]
		public void DateTimeDaysInMonthNotLeapYearTest()
		{
			int daysInMonth = BELDateTime.DaysInMonth(1998, 2);
			Assert.AreEqual(28, daysInMonth, "Checking that BELDateTime.DaysInMonth gives 28 days for February 1998, which was not a leap year");
		}
		[Test]
		public void DateTimeDaysInMonthLeapYearTest()
		{
			int daysInMonth = BELDateTime.DaysInMonth(1996, 2);
			Assert.AreEqual(29, daysInMonth, "Checking that BELDateTime.DaysInMonth gives 29 days for February 1996, which was a leap year");
		}
		#endregion
		#region BELDateTime WeeksInMonth tests
		[Test]
		public void DateTimeWeeksInMonthNormalTest()
		{
			int weeksInMonth = BELDateTime.WeeksInMonth(2005, 1, (int)DayOfWeek.Monday);
			Assert.AreEqual(6, weeksInMonth, "Checking that BELDateTime.WeeksInMonth gives 6 weeks for January 2005");
		}
		[Test]
		public void DateTimeWeeksInMonthNotLeapYearTest()
		{
			int weeksInMonth = BELDateTime.WeeksInMonth(1998, 2, (int)DayOfWeek.Monday);
			Assert.AreEqual(5, weeksInMonth, "Checking that BELDateTime.WeeksInMonth gives 5 weeks for February 1998, which was not a leap year");
		}
		[Test]
		public void DateTimeWeeksInMonthLeapYearTest()
		{
			int weeksInMonth = BELDateTime.WeeksInMonth(1996, 2, (int)DayOfWeek.Monday);
			Assert.AreEqual(5, weeksInMonth, "Checking that BELDateTime.WeeksInMonth gives 5 weeks for February 1996, which was a leap year");
		}
		[Test]
		public void DateTimeWeeksInMonthStartsOnSundayTest()
		{
			int weeksInMonth = BELDateTime.WeeksInMonth(2004, 8, (int)DayOfWeek.Monday);
			Assert.AreEqual(6, weeksInMonth, "Checking that BELDateTime.WeeksInMonth gives 6 weeks for August 2004, which starts on a Sunday");
		}
		[Test]
		public void DateTimeWeeksInMonthStartsOnMondayTest()
		{
			int weeksInMonth = BELDateTime.WeeksInMonth(2004, 11, (int)DayOfWeek.Monday);
			Assert.AreEqual(5, weeksInMonth, "Checking that BELDateTime.WeeksInMonth gives 5 weeks for November 2004, which starts on a Monday");
		}
		[Test]
		public void DateTimeWeeksInMonthCalendarMonday()
		{
			int weeksInMonth = BELDateTime.WeeksInMonth(1999, 2, (int)DayOfWeek.Monday);
			Assert.AreEqual(4, weeksInMonth, "Checking that BELDateTime.WeeksInMonth gives 4 weeks for February 1999 for a calendar that starts on a Monday");
		}
		[Test]
		public void DateTimeWeeksInMonthCalendarTuesday()
		{
			int weeksInMonth = BELDateTime.WeeksInMonth(1999, 2, (int)DayOfWeek.Tuesday);
			Assert.AreEqual(5, weeksInMonth, "Checking that BELDateTime.WeeksInMonth gives 5 weeks for February 1999 for a calendar that starts on a Tuesday");
		}
		[Test]
		public void DateTimeWeeksInMonthCalendarWednesday()
		{
			int weeksInMonth = BELDateTime.WeeksInMonth(1999, 2, (int)DayOfWeek.Wednesday);
			Assert.AreEqual(5, weeksInMonth, "Checking that BELDateTime.WeeksInMonth gives 5 weeks for February 1999 for a calendar that starts on a Wednesday");
		}
		[Test]
		public void DateTimeWeeksInMonthCalendarThursday()
		{
			int weeksInMonth = BELDateTime.WeeksInMonth(1999, 2, (int)DayOfWeek.Thursday);
			Assert.AreEqual(5, weeksInMonth, "Checking that BELDateTime.WeeksInMonth gives 5 weeks for February 1999 for a calendar that starts on a Thursday");
		}
		[Test]
		public void DateTimeWeeksInMonthCalendarFriday()
		{
			int weeksInMonth = BELDateTime.WeeksInMonth(1999, 2, (int)DayOfWeek.Friday);
			Assert.AreEqual(5, weeksInMonth, "Checking that BELDateTime.WeeksInMonth gives 5 weeks for February 1999 for a calendar that starts on a Friday");
		}
		[Test]
		public void DateTimeWeeksInMonthCalendarSaturday()
		{
			int weeksInMonth = BELDateTime.WeeksInMonth(1999, 2, (int)DayOfWeek.Saturday);
			Assert.AreEqual(5, weeksInMonth, "Checking that BELDateTime.WeeksInMonth gives 5 weeks for February 1999 for a calendar that starts on a Saturday");
		}
		[Test]
		public void DateTimeWeeksInMonthCalendarSunday()
		{
			int weeksInMonth = BELDateTime.WeeksInMonth(1999, 2, (int)DayOfWeek.Sunday);
			Assert.AreEqual(5, weeksInMonth, "Checking that BELDateTime.WeeksInMonth gives 5 weeks for February 1999 for a calendar that starts on a Sunday");
		}
		#endregion
		#region BELTimeSpan.Instance tests
		[Test]
		public void TimeSpanInstanceNormalTest()
		{
			TimeSpan expectedTimeSpan = new TimeSpan(1, 2, 3, 4, 5);
			TimeSpan resultTimeSpan = BELTimeSpan.Instance2(1, 2, 3, 4, 5);
			Assert.AreEqual(expectedTimeSpan, resultTimeSpan, "Checking that the resulting TimeSpan is 1 day, 2 hours, 3 minutes, 4 seconds a 5 milliseconds.");
		}
		[Test]
		public void TimeSpanInstanceMinTest()
		{
			TimeSpan expectedTimeSpan = TimeSpan.MinValue + new TimeSpan(5808);
			TimeSpan resultTimeSpan = BELTimeSpan.Instance2(expectedTimeSpan.Days, expectedTimeSpan.Hours, expectedTimeSpan.Minutes, 
				expectedTimeSpan.Seconds, expectedTimeSpan.Milliseconds);
			Assert.AreEqual(expectedTimeSpan, resultTimeSpan, "Checking that the resulting TimeSpan is the same as TimeSpan.MinValue.");
		}
		[Test]
		public void TimeSpanInstanceMaxTest()
		{
			TimeSpan expectedTimeSpan = TimeSpan.MaxValue - new TimeSpan(5807);
			TimeSpan resultTimeSpan = BELTimeSpan.Instance2(expectedTimeSpan.Days, expectedTimeSpan.Hours, expectedTimeSpan.Minutes, 
				expectedTimeSpan.Seconds, expectedTimeSpan.Milliseconds);
			Assert.AreEqual(expectedTimeSpan, resultTimeSpan, "Checking that the resulting TimeSpan is the same as TimeSpan.MaxValue.");
		}
		#endregion
		#region BELArray.Unique tests
		[Test]
		public void BELArrayUniqueTest()
		{
			BELArray sourceArray = new BELArray();
			sourceArray.Add("one");
			sourceArray.Add("one");
			sourceArray.Add("two");
			sourceArray.Add("three");
			sourceArray.Add("three");
			sourceArray.Add("four");
			sourceArray.Add("five");
			sourceArray.Add("five");
			ArrayList expectedArray = new ArrayList();
			expectedArray.Add("one");
			expectedArray.Add("two");
			expectedArray.Add("three");
			expectedArray.Add("four");
			expectedArray.Add("five");
			ArrayList resultArray = sourceArray.Unique();
			Assert.AreEqual(expectedArray.Count, resultArray.Count, "Checking that the resulting array is the correct size");
			for (int i = 0; i < resultArray.Count; i++)
			{
				Assert.AreEqual(expectedArray[i].GetHashCode(), resultArray[i].GetHashCode(), "Checking the element value hash codes are correct");
			}
		}
		[Test]
		public void BELArrayUniqueEmptyTest()
		{
			BELArray sourceArray = new BELArray();
			ArrayList resultArray = sourceArray.Unique();
			Assert.AreEqual(0, resultArray.Count, "Checking that the result array is empty");
		}
		[Test]
		public void BELArrayUniqueAllUniqueTest()
		{
			BELArray sourceArray = new BELArray();
			sourceArray.Add("one");
			sourceArray.Add("two");
			sourceArray.Add("three");
			sourceArray.Add("four");
			sourceArray.Add("five");
			ArrayList resultArray = sourceArray.Unique();
			Assert.AreEqual(sourceArray.Count, resultArray.Count, "Checking that the resulting array is the correct size");
			for (int i = 0; i < resultArray.Count; i++)
			{
				Assert.AreEqual(sourceArray.Array[i].GetHashCode(), resultArray[i].GetHashCode(), "Checking the element value hash codes are correct");
			}
		}
		[Test]
		public void BELArrayUniqueAllSameTest()
		{
			BELArray sourceArray = new BELArray();
			sourceArray.Add("one");
			sourceArray.Add("one");
			sourceArray.Add("one");
			sourceArray.Add("one");
			sourceArray.Add("one");
			ArrayList expectedArray = new ArrayList();
			expectedArray.Add("one");
			ArrayList resultArray = sourceArray.Unique();
			Assert.AreEqual(1, resultArray.Count, "Checking that the resulting array has a single element");
			Assert.AreEqual(sourceArray.Array[0].GetHashCode(), resultArray[0].GetHashCode(), "Checking that the result element has the correct hash code");
		}
		#endregion
		#region BELInteger arithmetic tests
		#region Add
		[Test]
		public void BELIntegerAddTest()
		{
			BELInteger source = new BELInteger(4);
			int expectedResult = 10;
			int result = source.Add(6);
			Assert.AreEqual(expectedResult, result, "Checking that BELInteger(4).Add(6) = 10");
		}
		[Test]
		public void BELIntegerAddNegativeTest()
		{
			BELInteger source = new BELInteger(4);
			int expectedResult = 2;
			int result = source.Add(-2);
			Assert.AreEqual(expectedResult, result, "Checking that BELInteger(4).Add(-2) = 2");
		}
		[Test]
		public void BELIntegerAddZeroTest()
		{
			BELInteger source = new BELInteger(4);
			int result = source.Add(0);
			Assert.AreEqual(source.Value, result, "Checking that BELInteger(4).Add(0) = 4");
		}
		[Test]
		public void BELIntegerAddNegativeSourceTest()
		{
			BELInteger source = new BELInteger(-4);
			int expectedResult = -2;
			int result = source.Add(2);
			Assert.AreEqual(expectedResult, result, "Checking that BELInteger(-4).Add(2) = -2");
		}
		[Test]
		public void BELIntegerAddZeroResultTest()
		{
			BELInteger source = new BELInteger(-10);
			int expectedResult = 0;
			int result = source.Add(10);
			Assert.AreEqual(expectedResult, result, "Checking that BELInteger(-10).Add(10) = 0");
		}
		[Test]
		public void BELIntegerAddMaxValueTest()
		{
			BELInteger source = new BELInteger(int.MaxValue);
			int max = int.MaxValue;
			int expectedResult = max + 10;
			int result = source.Add(10);
			Assert.AreEqual(expectedResult, result, "Checking that BELInteger(int.MaxValue).Add(10) 'wraps' correctly");
		}
		[Test]
		public void BELIntegerAddMaxValueNegativeTest()
		{
			BELInteger source = new BELInteger(int.MaxValue);
			int expectedResult = int.MaxValue - 10;
			int result = source.Add(-10);
			Assert.AreEqual(expectedResult, result, "Checking that BELInteger(int.MaxValue).Add(-10) works");
		}
		[Test]
		public void BELIntegerAddMinValueTest()
		{
			BELInteger source = new BELInteger(int.MinValue);
			int expectedResult = int.MinValue + 10;
			int result = source.Add(10);
			Assert.AreEqual(expectedResult, result, "Checking that BELInteger(int.MinValue).Add(10) works");
		}
		[Test]
		public void BELIntegerAddMinValueNegativeTest()
		{
			BELInteger source = new BELInteger(int.MinValue);
			int min = int.MinValue;
			int expectedResult = min - 10;
			int result = source.Add(-10);
			Assert.AreEqual(expectedResult, result, "Checking that BELInteger(int.MinValue).Add(-10) 'wraps' correctly");
		}
		#endregion
		#region Subtract
		[Test]
		public void BELIntegerSubtractTest()
		{
			BELInteger source = new BELInteger(10);
			int expectedResult = 4;
			int result = source.Subtract(6);
			Assert.AreEqual(expectedResult, result, "Checking that BELInteger(10).Subtract(6) = 4");
		}
		[Test]
		public void BELIntegerSubtractNegativeTest()
		{
			BELInteger source = new BELInteger(4);
			int expectedResult = 6;
			int result = source.Subtract(-2);
			Assert.AreEqual(expectedResult, result, "Checking that BELInteger(4).Subtract(-2) = 6");
		}
		[Test]
		public void BELIntegerSubtractZeroTest()
		{
			BELInteger source = new BELInteger(4);
			int result = source.Subtract(0);
			Assert.AreEqual(source.Value, result, "Checking that BELInteger(4).Subtract(0) = 4");
		}
		[Test]
		public void BELIntegerSubtractNegativeSourceTest()
		{
			BELInteger source = new BELInteger(-4);
			int expectedResult = -6;
			int result = source.Subtract(2);
			Assert.AreEqual(expectedResult, result, "Checking that BELInteger(-4).Subtract(2) = -6");
		}
		[Test]
		public void BELIntegerSubtractZeroResultTest()
		{
			BELInteger source = new BELInteger(10);
			int expectedResult = 0;
			int result = source.Subtract(10);
			Assert.AreEqual(expectedResult, result, "Checking that BELInteger(10).Subtract(10) = 0");
		}
		[Test]
		public void BELIntegerSubtractMaxValueTest()
		{
			BELInteger source = new BELInteger(int.MaxValue);
			int max = int.MaxValue;
			int expectedResult = max - 10;
			int result = source.Subtract(10);
			Assert.AreEqual(expectedResult, result, "Checking that BELInteger(int.MaxValue).Subtract(10) works");
		}
		[Test]
		public void BELIntegerSubtractMaxValueNegativeTest()
		{
			BELInteger source = new BELInteger(int.MaxValue);
			int max = int.MaxValue;
			int expectedResult = max + 10;
			int result = source.Subtract(-10);
			Assert.AreEqual(expectedResult, result, "Checkng that BELInteger(int.MaxValue).Subtract(-10) wraps correctly");
		}
		[Test]
		public void BELIntegerSubtractMinValueTest()
		{
			BELInteger source = new BELInteger(int.MinValue);
			int expectedResult = int.MinValue + 10;
			int result = source.Subtract(-10);
			Assert.AreEqual(expectedResult, result, "Checking that BELInteger(int.MinValue).Subtract(-10) works");
		}
		[Test]
		public void BELIntegerSubtractMinValueNegativeTest()
		{
			BELInteger source = new BELInteger(int.MinValue);
			int min = int.MinValue;
			int expectedResult = min - 10;
			int result = source.Subtract(10);
			Assert.AreEqual(expectedResult, result, "Checking that BELInteger(int.MaxValue).Subtract(10) 'wraps' correctly");
		}
		#endregion
		#region Multiply
		[Test]
		public void BELIntegerMultiplyTest()
		{
			BELInteger source = new BELInteger(10);
			int expectedResult = 60;
			int result = source.Multiply(6);
			Assert.AreEqual(expectedResult, result, "Checking that BELInteger(10).Multiply(6) = 60");
		}
		[Test]
		public void BELIntegerMultiplyNegativeTest()
		{
			BELInteger source = new BELInteger(4);
			int expectedResult = -8;
			int result = source.Multiply(-2);
			Assert.AreEqual(expectedResult, result, "Checking that BELInteger(4).Multiply(-2) = -8");
		}
		[Test]
		public void BELIntegerMultiplyZeroTest()
		{
			BELInteger source = new BELInteger(4);
			int result = source.Multiply(0);
			Assert.AreEqual(0, result, "Checking that BELInteger(4).Multiply(0) = 0");
		}
		[Test]
		public void BELIntegerMultiplyNegativeSourceTest()
		{
			BELInteger source = new BELInteger(-4);
			int expectedResult = -8;
			int result = source.Multiply(2);
			Assert.AreEqual(expectedResult, result, "Checking that BELInteger(-4).Multiply(2) = -8");
		}
		[Test]
		public void BELIntegerMultiplyZeroResultTest()
		{
			BELInteger source = new BELInteger(10);
			int expectedResult = 0;
			int result = source.Multiply(0);
			Assert.AreEqual(expectedResult, result, "Checking that BELInteger(10).Multiply(0) = 0");
		}
		[Test]
		public void BELIntegerMultiplyMaxValueTest()
		{
			BELInteger source = new BELInteger(int.MaxValue);
			int max = int.MaxValue;
			int expectedResult = max * 2;
			int result = source.Multiply(2);
			Assert.AreEqual(expectedResult, result, "Checking that BELInteger(int.MaxValue).Multiply(2) works");
		}
		[Test]
		public void BELIntegerMultiplyMaxValueNegativeTest()
		{
			BELInteger source = new BELInteger(int.MaxValue);
			int max = int.MaxValue;
			int expectedResult = max * -2;
			int result = source.Multiply(-2);
			Assert.AreEqual(expectedResult, result, "Checkng that BELInteger(int.MaxValue).Multiply(-2) 'wraps' correctly");
		}
		[Test]
		public void BELIntegerMultiplyMinValueTest()
		{
			BELInteger source = new BELInteger(int.MinValue);
			int min = int.MinValue;
			int expectedResult = min * 2;
			int result = source.Multiply(2);
			Assert.AreEqual(expectedResult, result, "Checking that BELInteger(int.MinValue).Multiply(2) works");
		}
		[Test]
		public void BELIntegerMultiplyMinValueNegativeTest()
		{
			BELInteger source = new BELInteger(int.MinValue);
			int min = int.MinValue;
			int expectedResult = min * -2;
			int result = source.Multiply(-2);
			Assert.AreEqual(expectedResult, result, "Checking that BELInteger(int.MaxValue).Multiply(-2) 'wraps' correctly");
		}
		[Test]
		public void BELIntegerMultiplyByOneTest()
		{
			BELInteger source = new BELInteger(8);
			int result = source.Multiply(1);
			Assert.AreEqual(source.Value, result, "Checking that BELInteger(8).Multiply(1) = 8");
		}
		#endregion
		#region Divide
		[Test]
		public void BELIntegerDivideTest()
		{
			BELInteger source = new BELInteger(10);
			int expectedResult = 5;
			int result = source.Divide(2);
			Assert.AreEqual(expectedResult, result, "Checking that BELInteger(10).Divide(2) = 5");
		}
		[Test]
		public void BELIntegerDivideNegativeTest()
		{
			BELInteger source = new BELInteger(4);
			int expectedResult = -2;
			int result = source.Divide(-2);
			Assert.AreEqual(expectedResult, result, "Checking that BELInteger(4).Divide(-2) = -2");
		}
		[Test]
		[ExpectedException(typeof(DivideByZeroException))]
		public void BELIntegerDivideZeroTest()
		{
			BELInteger source = new BELInteger(4);
			int result = source.Divide(0);
		}
		[Test]
		public void BELIntegerDivideNegativeSourceTest()
		{
			BELInteger source = new BELInteger(-4);
			int expectedResult = -2;
			int result = source.Divide(2);
			Assert.AreEqual(expectedResult, result, "Checking that BELInteger(-4).Divide(2) = -8");
		}
		[Test]
		public void BELIntegerDivideMaxValueTest()
		{
			BELInteger source = new BELInteger(int.MaxValue);
			int max = int.MaxValue;
			int expectedResult = max / 2;
			int result = source.Divide(2);
			Assert.AreEqual(expectedResult, result, "Checking that BELInteger(int.MaxValue).Divide(2) works");
		}
		[Test]
		public void BELIntegerDivideMaxValueNegativeTest()
		{
			BELInteger source = new BELInteger(int.MaxValue);
			int max = int.MaxValue;
			int expectedResult = max / -2;
			int result = source.Divide(-2);
			Assert.AreEqual(expectedResult, result, "Checkng that BELInteger(int.MaxValue).Divide(-2) 'wraps' correctly");
		}
		[Test]
		public void BELIntegerDivideMinValueTest()
		{
			BELInteger source = new BELInteger(int.MinValue);
			int min = int.MinValue;
			int expectedResult = min / 2;
			int result = source.Divide(2);
			Assert.AreEqual(expectedResult, result, "Checking that BELInteger(int.MinValue).Divide(2) works");
		}
		[Test]
		public void BELIntegerDivideMinValueNegativeTest()
		{
			BELInteger source = new BELInteger(int.MinValue);
			int min = int.MinValue;
			int expectedResult = min / -2;
			int result = source.Divide(-2);
			Assert.AreEqual(expectedResult, result, "Checking that BELInteger(int.MaxValue).Divide(-2) 'wraps' correctly");
		}
		[Test]
		public void BELIntegerDivideByOneTest()
		{
			BELInteger source = new BELInteger(8);
			int result = source.Divide(1);
			Assert.AreEqual(source.Value, result, "Checking that BELInteger(8).Divide(1) = 8");
		}
		#endregion
		#endregion
	}
}
