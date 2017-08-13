using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VstsAdminTools.Commands
{
    public abstract class CommandBase
    {

        public int Run()
        {
            return RunInternal();
        }

        public abstract int RunInternal()
     

    }
}
