using System;

namespace FlexWiki.Web
{
  internal class FormValues
  {
    private string _contact; 
    private string _description; 
    private string _namespace; 
    private string _title; 

    public string Contact
    {
      get { return _contact; }
      set { _contact = value; }
    }

    public string Description
    {
      get { return _description; }
      set { _description = value; }
    }

    public string Namespace
    {
      get { return _namespace; }
      set { _namespace = value; }
    }

    public string Title
    {
      get { return _title; }
      set { _title = value; }
    }
  }
}
