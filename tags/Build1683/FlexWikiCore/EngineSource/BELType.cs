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
using System.Reflection;
using System.Collections;

namespace FlexWiki
{
	/// <summary>
	/// Summary description for BELType.
	/// </summary>
	[ExposedClass("Type", "Describes a type of object")]
	public class BELType : BELObject
	{
		public BELType() : base()
		{
		}

		public BELType(Type clrType) : base()
		{
			_CLRType = clrType;
		}

		Type _CLRType;
		public Type CLRType
		{
			get
			{
				return _CLRType;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Determine whether this object is equal to another object")]
		public override bool Equals(object obj)
		{
			if (!(obj is Type))
				return false;
			return CLRType.Equals((obj as BELType).CLRType);
		}

		public override int GetHashCode()
		{
			return CLRType.GetHashCode ();
		}




		BELType _InstanceType;

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "For MetaTypes, answer the Type for instances")]		
		public BELType InstanceType
		{
			get
			{
				return _InstanceType;
			}
			set
			{
				_InstanceType = value;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer true if this is a MetaType")]		
		public bool IsMetaType
		{
			get
			{
				return InstanceType != null;
			}
		}

		public override IOutputSequence ToOutputSequence()
		{
			return new WikiSequence(Name);
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer how many super-types live above this type in the inheritance hierarchy")]
		public int InheritDepth
		{
			get
			{
				int answer = 0;
				BELType t = this;
				while (t != null)
				{
					answer++;
					t = t.BaseType;
				}
				return answer;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer a description of this type")]
		public string Description
		{
			get
			{
				return ClassAttributeForType(CLRType).Description;
			}
		}


		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the base type that this type is 'based' on; null for none")]
		public BELType BaseType
		{
			get
			{
				if (CLRType == typeof(BELObject))
					return null;
				return BELTypeForType(CLRType.BaseType);
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the name of this type")]
		public string Name
		{
			get
			{
				if (IsMetaType)
					return BELObject.ExternalTypeNameForType(InstanceType.CLRType) + "Type";
				else
					return BELObject.ExternalTypeNameForType(CLRType);
			}
		}

		public override string ToString()
		{
			return "Type " + Name;
		}


		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer an Array of all of the Members of this type (including inherited ones)")]
		public ArrayList AllMembers
		{
			get
			{
				ArrayList answer = new ArrayList();
				foreach (DictionaryEntry each in BELMembers)
					answer.Add(each.Value);
				return answer;
			}
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer an Array of all of the Members of this type (not including inherited ones)")]
		public ArrayList Members
		{
			get
			{
				ArrayList answer = new ArrayList();
				foreach (BELMember each in BELMembers.Values)
				{
					if (each.DeclaringType == (IsMetaType ? InstanceType.CLRType : CLRType))
						answer.Add(each);
				}
				return answer;
			}
		}

		static Hashtable _Types = new Hashtable();
		public static BELType BELTypeForType(Type aType)
		{
			BELType v = (BELType)(_Types[aType]);
			if (v != null)
				return v;
			v= new BELType(aType);
			_Types[aType] = v;
			return v;
		}

		BELType _MetaType;
		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the meta-Type of this Type")]
		public override BELType Type
		{
			get
			{
				if (_MetaType != null)
					return _MetaType;
				_MetaType = new BELType(GetType());
				_MetaType.InstanceType = this; // make it a metatype by telling it the type for which it's meta
				return _MetaType;
			}
		}


		//		static ArrayList _ExternalTypes = null;
		//		static public ArrayList ExternalTypes
		//		{
		//			get
		//			{
		//				if (_ExternalTypes != null)
		//					return _ExternalTypes;
		//				_ExternalTypes = new ArrayList();
		//				_ExternalTypes.Add(BELTypeForType(typeof(ObjectImpl)));
		//				_ExternalTypes.Add(BELTypeForType(typeof(BELType)));
		//				_ExternalTypes.Add(BELTypeForType(typeof(Block)));
		//				_ExternalTypes.Add(BELTypeForType(typeof(Member)));
		//				_ExternalTypes.Add(BELTypeForType(typeof(BELArray)));
		//				_ExternalTypes.Add(BELTypeForType(typeof(BELDateTime)));
		//				_ExternalTypes.Add(BELTypeForType(typeof(Home)));
		//				_ExternalTypes.Add(BELTypeForType(typeof(BELInteger)));
		//				_ExternalTypes.Add(BELTypeForType(typeof(BELString)));
		//				_ExternalTypes.Add(BELTypeForType(typeof(UndefinedObject)));
		//				return _ExternalTypes;
		//			}
		//		}
		//
		Hashtable _Members = null;
		public Hashtable BELMembers
		{
			get
			{
				if (_Members!= null)
					return _Members;
				_Members = new Hashtable();
				AddInstanceMembers(_Members);
				if (IsMetaType)
					AddTypeMembers(_Members);
				return _Members;
			}
		}

		
		void AddTypeMembers(Hashtable answer)
		{
			Type instanceCLRType = InstanceType.CLRType;
			foreach (MemberInfo each in instanceCLRType.GetMembers())
			{
				ExposedMethod vis = ExposedMethodAttributeFor(each);
				if (vis == null)
					continue;
				MethodInfo mi = null;
				switch (each.MemberType)
				{
					case MemberTypes.Property:
						mi = (each as PropertyInfo).GetGetMethod(false);
						break;

					case MemberTypes.Method:
						mi = each as MethodInfo;
						break;
				}
				if (mi == null)
					continue;
				if (!mi.IsStatic)
					continue;
				string visibleName = (vis.OverrideName == null) ? each.Name : vis.OverrideName;
				if (answer.Contains(visibleName))
					throw new ExecutionException("Overloading not allowed for ExposedMethod members (" + instanceCLRType.FullName + "." + each.Name + ")");
				BELMember member = new BELMember(visibleName, vis, mi);
				answer[visibleName] = member;
			}
		}

		void AddInstanceMembers(Hashtable answer)
		{
			foreach (MemberInfo each in CLRType.GetMembers())
			{
				ExposedMethod vis = ExposedMethodAttributeFor(each);
				if (vis == null)
					continue;
				MethodInfo mi = null;
				switch (each.MemberType)
				{
					case MemberTypes.Property:
						mi = (each as PropertyInfo).GetGetMethod(false);
						break;

					case MemberTypes.Method:
						mi = each as MethodInfo;
						break;
				}
				if (mi == null)
					continue;
				if (mi.IsStatic)
					continue;
				string visibleName = (vis.OverrideName == null) ? each.Name : vis.OverrideName;
				if (answer.Contains(visibleName))
					throw new ExecutionException("Overloading not allowed for ExposedMethod members (" + CLRType.FullName + "." + each.Name + ")");
				BELMember member = new BELMember(visibleName, vis, mi);
				answer[visibleName] = member;
			}
		}
		
		ExposedMethod ExposedMethodAttributeFor(MemberInfo mi)
		{
			foreach (Attribute each in mi.GetCustomAttributes(true))
			{
				if (each is ExposedMethod)
					return each as ExposedMethod;
			}
			return null;
		}

		[ExposedMethod(ExposedMethodFlags.CachePolicyNone, "Answer the type given a name.")]
		public BELType TypeForName(string typeName)
		{
			return (BELType)(new TypeRegistry().Registry[typeName]);
		}

	}
}
