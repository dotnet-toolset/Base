using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Base.IO
{
    public interface IWritable
    {
        Stream OpenWriter();
    }
}
