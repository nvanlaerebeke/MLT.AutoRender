using System;

namespace AutoRender.Data {

    public class VideoInfo {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsValid { get; set; }
        public TimeSpan Duration { get; set; }

        #region Video

        public string Width { get; set; }
        public string Height { get; set; }

        public string VideoCodec { get; set; }

        #endregion Video

        #region Audio

        public string AudioSampleRate { get; set; }
        public string AudioBitRate { get; set; }
        public string AudioCodec { get; set; }

        #endregion Audio
    }
}