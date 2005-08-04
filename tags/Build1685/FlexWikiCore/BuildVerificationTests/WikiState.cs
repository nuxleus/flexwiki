using System;

namespace FlexWiki.BuildVerificationTests
{
  internal class WikiState
  {
    private string configPath; 

    internal WikiState(string configPath)
    {
      this.configPath = configPath; 
    }

    internal string ConfigPath
    {
      get { return configPath; }
    }
  }
}
