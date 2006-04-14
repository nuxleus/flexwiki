using System;

namespace FlexWiki
{
  public class BackingTopicData : TopicData
  {
    private BackingTopic _back;

    public BackingTopicData(BackingTopic back)
    { 
      _back = back;
    }


    public override string Author
    {
      get
      {
        return _back.LastAuthor;
      }
    }

    public override DateTime LastModificationTime
    {
      get
      {
        return _back.LastModificationTime;
      }
    }

    public override string Name
    {
      get
      {
        return _back.FullName.LocalName;
      }
    }

    public override string Namespace
    {
      get
      {
        return _back.FullName.Namespace;
      }
    }

    public override string Version
    {
      get
      {
        //  TODO -- THe code here is stolen from TopicName.NewVersionStringForUser 
        //  I would have modified that so this encoding only appeared in one place but I
        // couldn't check out the file.

        string u = Author;
        u = u.Replace('\\', '-');
        u = u.Replace('?', '-');
        u = u.Replace('/', '-');
        u = u.Replace(':', '-');
        return LastModificationTime.ToString("yyyy-MM-dd-HH-mm-ss.ffff") + "-" + u;
      }
    }

  }
}
