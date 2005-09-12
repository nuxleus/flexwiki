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
	/// Summary description for BackingTopic.
	/// </summary>
	public class BackingTopic
	{
		public BackingTopic(AbsoluteTopicName name)
		{
			_FullName = name;
		}

		public BackingTopic(AbsoluteTopicName name, string body, bool canOverride)
		{
			_FullName = name;
			Body = body;
			CanOverride = canOverride;
		}

		AbsoluteTopicName _FullName;

		public AbsoluteTopicName FullName
		{
			get
			{
				return _FullName;
			}
		}

		string _Body = "";
		public string Body
		{
			get
			{
				return _Body;
			}
			set
			{
				_Body = value;
			}
		}

		bool _CanOverride;
		public bool CanOverride
		{
			get
			{
				return _CanOverride;
			}
			set
			{
				_CanOverride = value;
			}
		}

		string _LastAuthor = ContentBase.AnonymousUserName;
		public string LastAuthor
		{
			get
			{
				return _LastAuthor;
			}
			set
			{
				_LastAuthor = value;
			}
		}

		DateTime _CreationTime = DateTime.MinValue;
		public DateTime CreationTime
		{
			get
			{
				return _CreationTime;
			}
			set
			{
				_CreationTime = value;
			}
		}

		DateTime _LastModificationTime = DateTime.MinValue;
		public DateTime LastModificationTime
		{
			get
			{
				return _LastModificationTime;
			}
			set
			{
				_LastModificationTime = value;
			}
		}



	}
}
