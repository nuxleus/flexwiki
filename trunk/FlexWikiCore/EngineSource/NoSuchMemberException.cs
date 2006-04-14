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
	/// Summary description for NoSuchMemberException.
	/// </summary>
	public class NoSuchMemberException : Exception
	{
    private string _Member;
	
		public NoSuchMemberException() : base()
		{
		}

		public NoSuchMemberException(string message) : base(message)
		{
		}


		public string Member
		{
			get
			{
				return _Member;
			}
			set
			{
				_Member = value;
			}
		}

		public static NoSuchMemberException ForMember(BELLocation loc, string member)
		{
			NoSuchMemberException answer = new NoSuchMemberException(loc.ToString() + " : " + "No such property or function: " + member);
			answer.Member = member;
			return answer;
		}

		public static NoSuchMemberException ForMemberAndType(BELLocation loc, string member, string typeName)
		{
			NoSuchMemberException answer = new NoSuchMemberException(loc.ToString() + " : " + "No such property or function: " + typeName + "." + member);
			answer.Member = member;
			return answer;
		}

		
	
	}
}
