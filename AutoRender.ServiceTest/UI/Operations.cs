using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRender.ServiceTest {
    public class Operations {
        public General General { get; private set; }
        public Tests Tests { get; private set; }
        public Operations() {
            General = new General();
            Tests = new Tests();
        }
    }
}
