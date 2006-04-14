using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

using FlexWiki;

namespace FlexWiki.BeL.Debugger
{
    class BehaviorDebuggerApplication : IWikiApplication
    {
        private readonly string _configPath;
        private FederationConfiguration _federationConfiguration;
        private readonly ITimeProvider _timeProvider = new DefaultTimeProvider();


        public BehaviorDebuggerApplication(string configPath)
        {
            _configPath = configPath;
        }


        public FederationConfiguration FederationConfiguration
        {
            get
            {
                if (_federationConfiguration == null)
                {
                    XmlSerializer ser = new XmlSerializer(typeof(FederationConfiguration));
                    using (FileStream fileStream = new FileStream(_configPath, FileMode.Open,
                        FileAccess.Read, FileShare.Read))
                    {

                        _federationConfiguration = (FederationConfiguration)ser.Deserialize(fileStream);
                    }
                }

                return _federationConfiguration;
            }
        }

        public LinkMaker LinkMaker
        {
            get { return null;  }
        }

        public OutputFormat OutputFormat
        {
            get { return OutputFormat.Testing; }
        }

        public ITimeProvider TimeProvider
        {
            get { return _timeProvider; }
        }

        public void AppendToLog(string logfile, string message)
        {
            throw new NotImplementedException();
        }

        public void WriteFederationConfiguration()
        {
            throw new NotImplementedException(); 
        }



    }
}
