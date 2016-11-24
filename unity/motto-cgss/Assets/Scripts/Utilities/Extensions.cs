using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace Utilities
{
    public static class Extensions
    {
        [StringFormatMethod("line")]
        public static void AppendLine(this StringBuilder sb, string line, params object[] args)
        {
            sb.AppendLine(String.Format(line, args));
        }
    }
}
