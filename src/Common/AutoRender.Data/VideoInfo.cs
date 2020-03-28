using System;

namespace AutoRender.Data {

    public class VideoInfo {
        public bool IsValid { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public TimeSpan Duration { get; set; }

        #region Video

        public int Width { get; set; }
        public int Height { get; set; }

        public string VideoCodec { get; set; }

        #endregion Video

        #region Audio

        public string AudioCodec { get; set; }

        #endregion Audio
    }
}