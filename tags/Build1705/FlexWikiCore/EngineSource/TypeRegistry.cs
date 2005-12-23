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
using System.Reflection;
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

				LoadRegistry();

				return _Registry;
			}
		}

		private void LoadRegistry()
		{
			this._Registry = new Hashtable();
			foreach (Assembly each in AppDomain.CurrentDomain.GetAssemblies())
				RegisterTypesFromAssembly(each);
		}

		void RegisterTypesFromAssembly(Assembly a)
		{
			IEnumerable types = null;
			try
			{
				types = a.GetExportedTypes();
			}
			catch (NotSupportedException)
			{
				return;		// some types of assemblies (e.g., dynamic ones) don't let you enum exported types
			}

			// explore types
			foreach(Type type in types)
			{
				// ignore interfaces, etc...
				if (!type.IsClass)
					continue;

				// check if ExposedClass is present
				Object[] attributes = type.GetCustomAttributes(typeof(ExposedClass),false);
				if (attributes==null || attributes.Length==0)
					continue;

				// register type
				Reg(this._Registry, type);
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
			return new StringPresentation("types");
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
