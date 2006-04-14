using System;

namespace FlexWiki.Web.Admin
{
  public class Result
  {
    public Level ResultLevel;
    public HtmlStringWriter Writer = new HtmlStringWriter();
    public string Title;

    public Result(string title, Level aLevel)
    {
      ResultLevel = aLevel;
      Title = title;
    }

  };
}
