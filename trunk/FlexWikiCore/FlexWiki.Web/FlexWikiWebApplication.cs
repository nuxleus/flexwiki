using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace FlexWiki.Web
{
    public class FlexWikiWebApplication : IWikiApplication
    {
        private readonly string _configPath;
        private FederationConfiguration _federationConfiguration;
        private readonly LinkMaker _linkMaker;
        private readonly OutputFormat _outputFormat; 
        private readonly ITimeProvider _timeProvider = new DefaultTimeProvider();


        public FlexWikiWebApplication(string configPath, LinkMaker linkMaker)
            :
            this(configPath, linkMaker, OutputFormat.HTML)
        {
        }

        public FlexWikiWebApplication(string configPath, LinkMaker linkMaker,
            OutputFormat outputFormat)
        {
            _configPath = configPath;
            _linkMaker = linkMaker;
            _outputFormat = outputFormat; 
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
            get { return _linkMaker; }
        }

        public OutputFormat OutputFormat
        {
            get { return _outputFormat; }
        }

        public ITimeProvider TimeProvider
        {
            get { return _timeProvider; }
        }

        public void AppendToLog(string logfile, string message)
        {
            string logpath = Path.Combine(Path.GetDirectoryName(_configPath), logfile);

            using (StreamWriter streamWriter = File.AppendText(logpath))
            {
                streamWriter.WriteLine(message);
            }
        }

        public void WriteFederationConfiguration()
        {
            throw new NotImplementedException("Not yet implemented.");
        }



    }
}
