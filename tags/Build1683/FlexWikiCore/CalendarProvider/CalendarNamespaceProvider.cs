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
using FlexWiki;
using System.Collections;

namespace CalendarProvider
{
	/// <summary>
	/// Summary description for CalendarNamespaceProvider.
	/// </summary>
	public class CalendarNamespaceProvider : INamespaceProvider
	{
		public CalendarNamespaceProvider()
		{
		}
		#region INamespaceProvider Members

		public string OwnerMailingAddress
		{
			get
			{
				return null;
			}
		}

		public int Year;

		public string GetParameter(string paramID)
		{
			if (paramID == "Year")
				return "" + Year;
			throw new Exception("Only parameter is 'Year'.");
		}

		public void UpdateNamespaces(Federation aFed)
		{
			// just kill the old ones and reload new ones
			foreach (string each in NamespaceNames)
				aFed.UnregisterNamespace(aFed.ContentBaseForNamespace(each));
			LoadNamespaces(aFed);
		}

		public string Description
		{
			get
			{
				return "Demo - provider a namespace for each month in a year";
			}
		}

		ArrayList _NamespaceNames;
		IList NamespaceNames
		{
			get
			{
				if (_NamespaceNames != null)
					return _NamespaceNames;
				_NamespaceNames = new ArrayList();
				foreach (string month in MonthNames)
					_NamespaceNames.Add(month + "-" + Year);
				return _NamespaceNames;
			}
		}

		ArrayList _MonthNames;
		IList MonthNames
		{
			get
			{
				if (_MonthNames != null)
					return _MonthNames;
				_MonthNames = new ArrayList();
				for (int m = 1; m <= 12; m++)
				{
					DateTime t = new DateTime(DateTime.Now.Year, m,  1);
					_MonthNames.Add(t.ToString("MMMM"));
				}
				_MonthNames = ArrayList.ReadOnly(_MonthNames);
				return _MonthNames;
			}
		}


		public System.Collections.IList ParameterDescriptors
		{
			get
			{
				ArrayList answer = new ArrayList();
				answer.Add(new NamespaceProviderParameterDescriptor("Year", "Year", "The year for which you want namespaces and topics", "1964", true));
				return answer;
			}
		}

		public IList CreateNamespaces(Federation aFed)
		{
			int month = 1;
			ArrayList answer = new ArrayList();
			foreach (string each in NamespaceNames)
			{
				CalendarStore store = new CalendarStore(aFed, each, Year, month);
				answer.Add(store.Namespace);
				aFed.RegisterNamespace(store);
			}
			return ArrayList.ReadOnly(answer);
		}

		public string ValidateParameter(Federation aFed, string paramID, string proposedValue, bool isCreate)
		{
			if (paramID != "Year")
				throw new Exception("Only parameter is 'Year'.");

			int val;
			try
			{
				val = Int32.Parse(proposedValue);
			}
			catch (Exception e)
			{
				return e.Message;
			}
			return null;
		}

		public void SavePersistentParametersToDefinition(NamespaceProviderDefinition def)
		{
			def.SetParameter("Year", "" + Year);
		}

		public void SetParameter(string paramID, string proposedValue)
		{
			if (paramID != "Year")
				throw new Exception("Only parameter is 'Year'.");
			Year = Int32.Parse(proposedValue);
		}

		public bool CanParameterBeEdited(string paramID)
		{
			if (paramID != "Year")
				throw new Exception("Only parameter is 'Year'.");
			return true;
		}

		public System.Collections.IList ValidateAggregate(Federation aFed, bool isCreate)
		{
			return null;
		}

		public void LoadNamespaces(Federation aFed)
		{
			int month = 1;
			foreach (string each in NamespaceNames)
			{
				CalendarStore store = new CalendarStore(aFed, each, Year, month++);
				aFed.RegisterNamespace(store);
			}
		}

		#endregion
	}
}
  