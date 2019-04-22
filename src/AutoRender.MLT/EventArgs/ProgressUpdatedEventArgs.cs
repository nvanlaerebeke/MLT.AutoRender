using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoRender.MLT.EventArgs {
    public class ProgressUpdatedEventArgs: System.EventArgs {
        public int Frame { get; private set; }
        public int Percentage { get; private set; }

        public ProgressUpdatedEventArgs(int pFrame, int pPercentage) {
            Frame = pFrame;
            Percentage = pPercentage;
        }
    }
}
