using System;
namespace AutoRender.Messaging {
    public class VideoInfo : CrazyUtils.Base {
        public string Name = "";
        public bool IsValid = false;
        public TimeSpan Duration;

        public VideoInfo(Lib.VideoInfo pInfo) {
            if (pInfo != null) {
                Name = pInfo.FileName;
                IsValid = pInfo.IsValid;
                Duration = pInfo.Duration;
            }
        }
    }
}