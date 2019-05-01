using System;

namespace AutoRender.MLT.StdHandlers {
    public class Progress : StdHandler {
        public event EventHandler progressUpdated;
        public int _intPrevPercentage = 0;

        public override void Handle(string pLine) {
            if (pLine.StartsWith("Current Frame:", StringComparison.CurrentCulture)) {
                var arrParts = pLine.Split(',');
                if (arrParts.Length > 0) {
                    int? intFrame = null;
                    int? intPercentage = null;

                    foreach (string strPart in arrParts) {
                        var arrKvp = strPart.Split(':');
                        if (arrKvp.Length == 2) {
                            var strKey = arrKvp[0].Trim();
                            switch (strKey) {
                                case "Current Frame":
                                    int intTmpFrame;
                                    if(int.TryParse(arrKvp[1].Trim(), out intTmpFrame)) {
                                        intFrame = intTmpFrame;
                                    }
                                    break;
                                case "percentage":
                                    int intTmpPercentage;
                                    if (int.TryParse(arrKvp[1].Trim(), out intTmpPercentage)) {
                                        intPercentage = intTmpPercentage;
                                    }
                                    break;
                            }
                        }
                    }
                    if (intFrame != null && intPercentage != null && _intPrevPercentage != intPercentage) {
                        progressUpdated?.Invoke(this, new EventArgs.ProgressUpdatedEventArgs((int)intFrame, (int)intPercentage));
                    }
                }
            }
        }
    }
}
