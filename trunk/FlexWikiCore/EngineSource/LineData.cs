using System;

namespace FlexWiki
{
  public class LineData
  {
    string _text;
    int _left;
    int _right;
    LineType _type;

    public LineData(string text, int left, int right, LineType type)
    {
      _text = text;
      _left = left;
      _right = right;
      _type = type;
    }
    public string Text { get { return _text; } set { _text = value; } }
    public int Left { get { return _left; } set { _left = value; } }
    public int Right { get { return _right; } set { _right = value; } }
    public LineType Type { get { return _type; } set { _type = value; } }
  }
}
