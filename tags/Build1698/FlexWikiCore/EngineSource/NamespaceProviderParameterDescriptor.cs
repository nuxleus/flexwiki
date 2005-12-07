using System;
using System.Collections;

namespace FlexWiki
{
	/// <summary>
	/// Summary description for NamespaceProviderParameterDescriptor.
	/// </summary>
	public class NamespaceProviderParameterDescriptor
	{
		public NamespaceProviderParameterDescriptor(string id, string title, string description, string defaultValue, bool isPersistent)
		{
			_ID = id;
			_Title = title;
			_Description = description;
			_DefaultValue = defaultValue;
			_IsPersistent = isPersistent;
		}

		bool _IsPersistent;
		public bool IsPersistent
		{
			get
			{
				return _IsPersistent;
			}
		}


		string _ID;
		public string ID
		{
			get
			{
				return _ID;
			}
		}

		string _Title;
		public string Title
		{
			get
			{
				return _Title;
			}
		}

		string _Description;
		public string Description
		{
			get
			{
				return _Description;
			}
		}

		string _DefaultValue;
		public string DefaultValue
		{
			get
			{
				return _DefaultValue;
			}
		}

		IList _Choices;
		public IList Choices
		{
			get
			{
				if (_Choices == null)
					return _Choices;
				return ArrayList.ReadOnly(_Choices);
			}
		}

		public void AddChoice(string choice)
		{
			if (_Choices == null)
				_Choices = new ArrayList();
			_Choices.Add(choice);
		}


	}
}
