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
using System.Diagnostics;
using System.Collections;
using System.Text;

namespace FlexWiki
{
	/// <summary>
	/// Summary description for Tracer.
	/// </summary>
	public class Tracer 
	{
		static StringBuilder _History;

		public Tracer()
		{
			Clear();
		}

		static public void Clear()
		{
			_History = new StringBuilder();
		}

		static public StringBuilder AppendFormat(
			string format,
			object arg0
			)
		{
			return _History.AppendFormat(format, arg0);
		}

		static public StringBuilder AppendFormat(
			string format,
			params object[] args
			)
		{
			return _History.AppendFormat(format, args);
		}

		static public StringBuilder AppendFormat(
			string format,
			object arg0,
			object arg1
			)
		{
			return _History.AppendFormat(format, arg0, arg1);
		}

		static public StringBuilder AppendFormat(
			string format,
			object arg0,
			object arg1,
			object arg2
			)
		{
			return _History.AppendFormat(format, arg0, arg1, arg2);
		}

		static public StringBuilder Append(
			string value
			)
		{
			return _History.Append(value);
		}

	}
}
