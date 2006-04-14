using System;

namespace FlexWiki
{
  internal enum InterpreterState
  {
    ReadyToParse,
    ParseSuccess,
    ParseFailure,
    EvaluationSuccess,
    EvaluationFailure
  };
}
