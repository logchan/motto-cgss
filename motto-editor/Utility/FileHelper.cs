using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace motto_editor.Utility
{
    public static class FileHelper
    {
        public static MemoryStream ReadFileToMemoryStream(string file)
        {
            var ms = new MemoryStream();
            using (var fs = new FileStream(file, FileMode.Open))
            {
                fs.CopyTo(ms);
            }

            ms.Seek(0, SeekOrigin.Begin);
            return ms;
        }
    }
}
