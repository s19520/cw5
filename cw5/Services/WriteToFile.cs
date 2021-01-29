using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cw5.Services
{
    public interface WriteToFile
    {
        public void Write(string file, string text);
    }
}
