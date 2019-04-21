using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRender.Lib.Melt.StdHandlers {
    public abstract class StdHandler {
        public abstract void Handle(string pLine);
    }
}