using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace cw5.Services
{
    public class WriteToFileImp : WriteToFile
    {
        public void Write(string file, string text)
        {

            using (StreamWriter outputFile = new StreamWriter(file, true))
            {
                    outputFile.WriteLine(text+"\n");
            }
        }
    }
}
