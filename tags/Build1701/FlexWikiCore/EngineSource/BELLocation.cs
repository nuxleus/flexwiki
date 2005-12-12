using System;

namespace FlexWiki
{
	/// <summary>
	/// Summary description for BELLocation.
	/// </summary>
	public class BELLocation
	{
		public BELLocation(string ctx, int line, int col)
		{
			_ContextString = ctx;
			_Line = line;
			_Column = col;
		}

		string _ContextString;
		int _Line;
		int _Column;

		public override string ToString()
		{
			return ContextString + "(" + Line + ":" + Column + ")";
		}


		public string ContextString
		{
			get
			{
				return _ContextString;
			}
		}

		public int Line
		{
			get
			{
				return _Line;
			}
		}
		
		public int Column
		{
			get
			{
				return _Column;
			}
		}

	}
}
