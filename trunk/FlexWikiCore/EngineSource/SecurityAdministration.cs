using System;
using System.Xml.Serialization; 

namespace FlexWiki
{
  [XmlRoot("security")]
	public class SecurityConfiguration
	{
    private AuthorizationConfiguration _authorizationConfiguration;
    private string _version; 

    [XmlElement("authorization")]
    public AuthorizationConfiguration AuthorizationConfiguration
    {
      get { return _authorizationConfiguration; }
      set { _authorizationConfiguration = value; }
    }

    [XmlAttribute("version")]
    public string Version
    {
      get { return _version; }
      set { _version = value; }
    }
	}
}
