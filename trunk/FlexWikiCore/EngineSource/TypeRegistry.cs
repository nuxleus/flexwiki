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
using FlexWiki.Formatting;

namespace FlexWiki
{
	/// <summary>
	/// Summary description for TypeRegistry.
	/// </summary>
	public class TypeRegistry : DynamicObject
	{
		public TypeRegistry()
		{
		}

		Hashtable _Registry = null;

		void Reg(Hashtable hash, Type t)
		{
			BELType bt = BELType.BELTypeForType(t);
			hash[bt.Name] = bt;
		}

		public Hashtable Registry
		{
			get

			{
				if (_Registry != null)
					return _Registry;
				_Registry= new Hashtable();
				Reg(_Registry, typeof(BELObject));
				Reg(_Registry, typeof(BELType));
				Reg(_Registry, typeof(Block));
				Reg(_Registry, typeof(Presentations));
				Reg(_Registry, typeof(BELMember));
				Reg(_Registry, typeof(BELArray));
				Reg(_Registry, typeof(BELDateTime));
				Reg(_Registry, typeof(BELTimeSpan));
				Reg(_Registry, typeof(Home));
				Reg(_Registry, typeof(BELInteger));
				Reg(_Registry, typeof(BELString));
				Reg(_Registry, typeof(UndefinedObject));
				Reg(_Registry, typeof(ClassicBehaviors));
				Reg(_Registry, typeof(CompositePresentation));
				Reg(_Registry, typeof(ImagePresentation));
				Reg(_Registry, typeof(LinkPresentation));
				Reg(_Registry, typeof(Presentation));
				Reg(_Registry, typeof(PresentationPrimitive));
				Reg(_Registry, typeof(StringPresenation));
				Reg(_Registry, typeof(TopicContext));
				Reg(_Registry, typeof(Federation));
				Reg(_Registry, typeof(ContentBase));
				Reg(_Registry, typeof(TopicInfo));
				Reg(_Registry, typeof(LinkMaker));
				Reg(_Registry, typeof(TopicChange));
				Reg(_Registry, typeof(BELBoolean));
				Reg(_Registry, typeof(LinkMaker));
				Reg(_Registry, typeof(Presentations));
				Reg(_Registry, typeof(Request));
				Reg(_Registry, typeof(BlockParameter));
				
				return _Registry;
			}
		}

		ArrayList _AllMetaTypes;
		public ArrayList AllMetaTypes
		{
			get
			{
				if (_AllMetaTypes != null)
					return _AllMetaTypes;
				_AllMetaTypes = new ArrayList();
				foreach (BELType t in Registry.Values)
					_AllMetaTypes.Add(t.Type);
				return _AllMetaTypes;
			}
		}

		ArrayList _AllTypes;
		public ArrayList AllTypes
		{
			get
			{
				if (_AllTypes != null)
					return _AllTypes;
				_AllTypes = new ArrayList();
				foreach (BELType t in Registry.Values)
					_AllTypes.Add(t);
				return _AllTypes;
			}
		}

		public override IOutputSequence ToOutputSequence()
		{
			return new StringPresenation("types");
		}


		public override IBELObject ValueOf(string name, ArrayList arguments, ExecutionContext ctx)
		{
			IBELObject answer = (IBELObject)(Registry[name]);
			if (answer == null)
				return null;
			if (arguments != null && arguments.Count > 0)
				throw new ArgumentException("No arguments allowed when accessing types in the type registry.");
			return answer;
		}

	}
}
