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
using System.Collections;
using System.Text;
using FlexWiki;


namespace FlexWiki.Formatting
{
	/// <summary>
	/// Diff class for doing comparisons
	/// </summary>
	public class Diff
	{
		public static IEnumerable Compare(IList left, IList right)
		{
			return new Diff().Work(left, right);
		}

		IList _leftLines;
		IList _rightLines;
		int _curLeftIndex;
		int _curRightIndex;
		IList _results = new ArrayList();

		Diff()
		{
		}

		bool CompareLines(string left, string right)
		{
			return left.CompareTo(right) == 0;
		}

		IEnumerable Work(IList left, IList right)
		{
			_leftLines = left;
			_rightLines = right;
			
			while (_curLeftIndex < _leftLines.Count)
			{
				if (CurrentRight == null) 
				{ 
					PushLeftOnly(); 
				}
				else if (CompareLines(CurrentLeft, CurrentRight)) 
				{
					PushCommon();
				}
				else
				{
					string tempRight;
					int i=0;
					bool foundMatch = false;
					do
					{
						i++;
						tempRight = PeekRight(i);
						if (tempRight != null && CompareLines(CurrentLeft, tempRight))
						{
							// This can't be the right way to do this...
							//
							//  Basically the problem was that you would get a miss on the current
							//  line, we would walk forward and find a future match... but
							//  in the process, we would consume important text from the right.
							//  This showed up in Wiki, which has lots of similiar lines (blank).
							//
							//  Imagine something like:
							//      LEFT              RIGHT
							//       a                 a
							//       -                 -
							//       c1                c
							//       _                 -
							//       c                 d
							//       -                 -
							//       d                 e
							//       _
							//       e
							//  
							//   Obviously, "c1" folowed by a blank was the addition... however 
							//   since the blank after "c1" consumes the blank after "c", you
							//   are now off, and everything will be thought to be a diff.
							//
							// For now, I hack this, we will check the about to be consumed
							// lines against the next 20 lines on the left to ensure we
							// don't consume an important line...  ugh... 
							// 
							for (int j=0; j<i && tempRight != null; j++)
							{
								string aboutToConsume = PeekRight(j);
								for (int checkLeft=0; checkLeft<20 && tempRight != null; checkLeft++)
								{
									if (CompareLines(aboutToConsume, PeekLeft(checkLeft))) 
									{
										tempRight = null;
										break;
									}
								}
							}

							if (tempRight != null)
							{
								foundMatch = true;
								for (;i>0; i--)
								{
									PushRightOnly();
								}
							}
						}
					}
					while (tempRight != null && !foundMatch);

					if (!foundMatch) PushLeftOnly();
				}
			}

			while (_curRightIndex < _rightLines.Count)
			{
				PushRightOnly();
			}

			return _results;
		}

		string CurrentLeft 
		{ 
			get 
			{ 
				if (_curLeftIndex >= _leftLines.Count) return null;
				return (string)_leftLines[_curLeftIndex]; 
			} 
		}
		string CurrentRight 
		{ 
			get 
			{ 
				if (_curRightIndex >= _rightLines.Count) return null;
				return (string)_rightLines[_curRightIndex]; 
			} 
		}

		string PeekRight(int forward) 
		{ 
			if (_curRightIndex + forward >= _rightLines.Count) return null;
			return (string)_rightLines[_curRightIndex + forward]; 
		}
		string PeekLeft(int forward) 
		{ 
			if (_curLeftIndex + forward >= _leftLines.Count) return null;
			return (string)_leftLines[_curLeftIndex + forward]; 
		}

		void PushLeftOnly()
		{
			_results.Add(new LineData(CurrentLeft, _curLeftIndex, -1, LineType.LeftOnly));
			_curLeftIndex++;
		}
		void PushRightOnly()
		{
			_results.Add(new LineData(CurrentRight, -1, _curRightIndex, LineType.RightOnly));
			_curRightIndex++;
		}
		void PushCommon()
		{
			_results.Add(new LineData(CurrentRight, _curLeftIndex,  _curRightIndex, LineType.Common));
			_curLeftIndex++;
			_curRightIndex++;
		}
	}

	public enum LineType
	{
		LeftOnly,
		RightOnly,
		Common,
	}

	public class LineData
	{
		string _text;
		int _left;
		int _right;
		LineType _type;

		public LineData(string text, int left, int right, LineType type)
		{
			_text = text;
			_left = left;
			_right = right;
			_type = type;
		}
		public string Text { get { return _text; } set { _text = value; } }
		public int Left { get { return _left; } set { _left = value; } }
		public int Right { get { return _right; } set { _right = value; } }
		public LineType Type { get { return _type; } set { _type = value; } }
	}
}
