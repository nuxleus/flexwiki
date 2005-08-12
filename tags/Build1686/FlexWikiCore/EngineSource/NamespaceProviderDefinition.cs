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
using System.Xml.Serialization;
using System.Reflection;
using System.Collections;

namespace FlexWiki
{
	/// <summary>
	/// 
	/// </summary>
	public class NamespaceProviderDefinition
	{

		/// <summary>
		/// Answer a definition object for the given namespace
		/// </summary>
		/// <param name="type">Type (class name)</param>
		/// <param name="assembly">Assembly containing the type (or null for the core engine)</param>
		/// <param name="uniqueId">Unique identifier for the provider.</param>
		public NamespaceProviderDefinition(string assembly, string type, string uniqueId)
		{
			_Type = type;
			_AssemblyName = assembly;
			_Id = uniqueId;
		}

		[XmlAttribute]
		public string Id
		{
			get
			{
				return _Id;
			}
			set
			{
				_Id = value;
			}
		}
		string _Id;
		
		ArrayList _Parameters = new ArrayList();
		[XmlArray(ElementName = "Parameters"), 
		XmlArrayItem(ElementName= "Parameter", 
			Type = typeof(NamespaceProviderParameter))
		]
		public ArrayList Parameters
		{
			get
			{
				return _Parameters;
			}
			set
			{
				_Parameters = value;
			}
		}

		public void SetParameter(string name, string value)
		{
			NamespaceProviderParameter parm = null;
			foreach (NamespaceProviderParameter each in Parameters)
			{
				if (each.Name == name)
				{
					parm = each;
					break;
				}
			}
			if (parm == null)
			{
				parm = new NamespaceProviderParameter(name, value);
				Parameters.Add(parm);
			}
			else
				parm.Value = value;
		}

		public void SetParametersInProvider(INamespaceProvider provider)
		{
			foreach (DictionaryEntry each in ParametersAsHash)
				provider.SetParameter((string)each.Key, (string)each.Value);
		}


		[XmlIgnore]
		public Hashtable ParametersAsHash
		{
			get
			{
				Hashtable answer = new Hashtable();
				foreach (NamespaceProviderParameter each in Parameters)
					answer[each.Name] = each.Value;
				return answer;
			}
		}

		public NamespaceProviderDefinition()
		{
		}

		[XmlAttribute]
		public string Type
		{
			get
			{
				return _Type;
			}
			set
			{
				_Type = value;
			}
		}
		string _Type;

		[XmlAttribute]
		public string AssemblyName
		{
			get
			{
				return _AssemblyName;
			}
			set
			{
				_AssemblyName = value;
			}
		}
		string _AssemblyName;

		public Type ProviderType
		{
			get
			{
				return GetAssemblyType(AssemblyName, Type);
			}
		}

		private static Type GetAssemblyType(string assyName, string className)
		{
			if (className == null)
				return null;
			Assembly assy=null;
			if (assyName == null)
				assy = Assembly.GetExecutingAssembly();
			else
				assy = Assembly.Load(assyName);
			if (assy != null)
			{
				return assy.GetType(className);
			}
			return null;
		}

	}
}
