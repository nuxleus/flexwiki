using System;

namespace FlexWiki.Web.Services.WireTypes
{
  public class WikiVersion
  {
    private int major; 
    private int minor;
    private int build;
    private int revision; 

    public int Major
    {
      get { return major; }
      set { major = value; }
    }

    public int Minor
    {
      get { return minor; }
      set { minor = value; }
    }

    public int Build
    {
      get { return build; }
      set { build = value; }
    }

    public int Revision
    {
      get { return revision; }
      set { revision = value; }
    }
  }
}
