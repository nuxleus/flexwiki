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

namespace FlexWiki.CalendarProvider
{
    public class CalendarNamespaceProvider : INamespaceProvider
    {
        private ArrayList _monthNames;
        private ArrayList _namespaceNames;
        private int _year;

        public CalendarNamespaceProvider()
        {
        }


        public string Description
        {
            get
            {
                return "Demo - provider a namespace for each month in a year";
            }
        }
        public string OwnerMailingAddress
        {
            get
            {
                return null;
            }
        }

        public IList ParameterDescriptors
        {
            get
            {
                ArrayList answer = new ArrayList();
                answer.Add(new NamespaceProviderParameterDescriptor("Year", "Year", "The year for which you want namespaces and topics", "1964", true));
                return answer;
            }
        }

        public int Year
        {
            get { return _year; }
            set { _year = value; }
        }


        private IList MonthNames
        {
            get
            {
                if (_monthNames != null)
                {
                    return _monthNames;
                }
                _monthNames = new ArrayList();
                for (int m = 1; m <= 12; m++)
                {
                    DateTime t = new DateTime(DateTime.Now.Year, m, 1);
                    _monthNames.Add(t.ToString("MMMM"));
                }
                _monthNames = ArrayList.ReadOnly(_monthNames);
                return _monthNames;
            }
        }

        private IList NamespaceNames
        {
            get
            {
                if (_namespaceNames != null)
                {
                    return _namespaceNames;
                }
                _namespaceNames = new ArrayList();
                foreach (string month in MonthNames)
                {
                    _namespaceNames.Add(month + "-" + Year);
                }
                return _namespaceNames;
            }
        }


        public bool CanParameterBeEdited(string paramID)
        {
            if (paramID != "Year")
                throw new Exception("Only parameter is 'Year'.");
            return true;
        }

        public IList CreateNamespaces(Federation aFed)
        {
            int month = 1;
            ArrayList answer = new ArrayList();
            foreach (string each in NamespaceNames)
            {
                CalendarStore store = new CalendarStore(Year, month);
                NamespaceManager manager = aFed.RegisterNamespace(store, each);
                answer.Add(manager.Namespace);
            }
            return ArrayList.ReadOnly(answer);
        }

        public string GetParameter(string paramID)
        {
            if (paramID == "Year")
                return "" + Year;
            throw new Exception("Only parameter is 'Year'.");
        }

        public void LoadNamespaces(Federation aFed)
        {
            int month = 1;
            foreach (string each in NamespaceNames)
            {
                CalendarStore store = new CalendarStore(Year, month++);
                NamespaceManager manager = aFed.RegisterNamespace(store, each);
            }
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

        public void UpdateNamespaces(Federation aFed)
        {
            // just kill the old ones and reload new ones
            foreach (string each in NamespaceNames)
                aFed.UnregisterNamespace(aFed.NamespaceManagerForNamespace(each));
            LoadNamespaces(aFed);
        }

        public IList ValidateAggregate(Federation aFed, bool isCreate)
        {
            return null;
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

    }
}
