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
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Reflection; 

using FlexWiki.Formatting;
using FlexWiki;

using NUnit.Framework;


namespace FlexWiki.UnitTests
{
	[TestFixture] public class TableFormattingOptionsTests 
	{
		public void TestDefaults()
		{
			TableCellInfo info = new TableCellInfo();
			info.Parse("");
			Assert.IsTrue(info.HasBorder);
			Assert.IsTrue(!info.IsHighlighted);
		}

		public void TestError()
		{
			TableCellInfo info = new TableCellInfo();
			Assert.IsTrue(info.Parse("T%") != null);
		}


		public void TestTable()
		{
			TableCellInfo info = new TableCellInfo();
			info.Parse("T-T]");
			Assert.IsTrue(!info.HasBorder);
			Assert.AreEqual(info.TableAlignment, TableCellInfo.AlignOption.Right);
			info.Parse("T[");
			Assert.AreEqual(info.TableAlignment, TableCellInfo.AlignOption.Left);
			info.Parse("T^");
			Assert.AreEqual(info.TableAlignment, TableCellInfo.AlignOption.Center);
		}

		public void TestCellAlignment()
		{
			TableCellInfo info = new TableCellInfo();
			info.Parse("]");
			Assert.AreEqual(info.CellAlignment, TableCellInfo.AlignOption.Right);
			info.Parse("[");
			Assert.AreEqual(info.CellAlignment, TableCellInfo.AlignOption.Left);
			info.Parse("^");
			Assert.AreEqual(info.CellAlignment, TableCellInfo.AlignOption.Center);
		}

		public void TestHighlight()
		{
			TableCellInfo info = new TableCellInfo();
			info.Parse("!");
			Assert.IsTrue(info.IsHighlighted);
		}

		public void TestSpans()
		{
			TableCellInfo info = new TableCellInfo();
			info.Parse("C10!R5T-");
			Assert.IsTrue(info.IsHighlighted);
			Assert.AreEqual(info.ColSpan, 10);
			Assert.AreEqual(info.RowSpan, 5);
		}

		public void TestWidths()
		{
			TableCellInfo info = new TableCellInfo();
			info.Parse("");
			Assert.IsTrue(info.TableWidth == TableCellInfo.UnspecifiedWidth);
			Assert.IsTrue(info.CellWidth == TableCellInfo.UnspecifiedWidth);
			info.Parse("TW1");
			Assert.AreEqual(1, info.TableWidth);
			info.Parse("TW100");
			Assert.AreEqual(100, info.TableWidth);
			info.Parse("TW100C2");
			Assert.AreEqual(100, info.TableWidth);
			info.Parse("W1");
			Assert.AreEqual(1, info.CellWidth);
			info.Parse("W100");
			Assert.AreEqual(100, info.CellWidth);
			info.Parse("W100C2");
			Assert.AreEqual(100, info.CellWidth);
			info.Parse("W100TW200C2");
			Assert.AreEqual(100, info.CellWidth);
			Assert.AreEqual(200, info.TableWidth);


		}

		public void TestErrorOnMissingIntegers()
		{
			TableCellInfo info = new TableCellInfo();
			string missing = "Missing";
			Assert.IsTrue(info.Parse("TW").StartsWith(missing));
			Assert.IsTrue(info.Parse("R").StartsWith(missing));
			Assert.IsTrue(info.Parse("C").StartsWith(missing));
			Assert.IsTrue(info.Parse("W").StartsWith(missing));
		}


	}

}
