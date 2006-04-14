using System;
using System.Collections.Generic;
using System.Text;

namespace FlexWiki
{
    [Flags]
    public enum ExposedMethodFlags
    {
        /// <summary>
        /// true if the object is willing to be fully responsible for evaluating all arguments directly as ParseTreeNodes
        /// </summary>
        IsCustomArgumentProcessor = 4,
        AllowsVariableArguments = 8,
        NeedContext = 16,

        Default = 0
    }
}
