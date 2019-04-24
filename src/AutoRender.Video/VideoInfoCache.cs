using AutoRender.Data;
using log4net;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;

namespace AutoRender.Video {

    public class VideoInfoCache {
        private readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private ConcurrentDictionary<string, VideoInfoWrapper> _dicVideoCache = new ConcurrentDictionary<string, VideoInfoWrapper>();

        public VideoInfo Get(string pPath) {
            var objFileInfo = new FileInfo(pPath);

            if (_dicVideoCache.ContainsKey(pPath)) {
                if (_dicVideoCache.TryGetValue(pPath, out VideoInfoWrapper objInfo)) {
                    if (objInfo.FileInfo.Equals(objFileInfo)) {
                        return objInfo.VideoInfo;
                    } else {
                        _dicVideoCache.TryRemove(pPath, out _);
                    }
                }
            }

            var objNewVideoInfo = new VideoInfoWrapper(objFileInfo, new VideoInfoReader(pPath).Read());
            if (_dicVideoCache.ContainsKey(pPath)) {
                _dicVideoCache[pPath] = objNewVideoInfo;
            } else {
                _dicVideoCache.TryAdd(pPath, objNewVideoInfo);
            }

            return _dicVideoCache[pPath].VideoInfo;
        }

        public void Remove(string pPath) {
            if (_dicVideoCache.ContainsKey(pPath)) {
                if (!_dicVideoCache.TryRemove(pPath, out _)) {
                    Log.Error($"Failed to clean up cache for {pPath}");
                }
            }
        }

        private class VideoInfoWrapper {
            public readonly FileInfo FileInfo;
            public readonly VideoInfo VideoInfo;

            public VideoInfoWrapper(FileInfo pFileInfo, VideoInfo pVideoInfo) {
                FileInfo = pFileInfo;
                VideoInfo = pVideoInfo;
            }
        }
    }
}