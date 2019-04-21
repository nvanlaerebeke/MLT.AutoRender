using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRender.ServiceTest {
    class Program {
        static void Main(string[] args) {
            System.Threading.Thread.CurrentThread.Name = "Main";
            Controller.Start();
        }
    }
}
