using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki
{
    public interface IWikiApplication
    {
        FederationConfiguration FederationConfiguration { get; }
        LinkMaker LinkMaker { get; }
        OutputFormat OutputFormat { get; }
        ITimeProvider TimeProvider { get; }

        void AppendToLog(string logfile, string message);
        void WriteFederationConfiguration();

    }
}
