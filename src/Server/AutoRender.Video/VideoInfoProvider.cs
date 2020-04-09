using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoRender.Data;
using CrazyUtils.Extension;

namespace AutoRender.Video {

    public class VideoInfoProvider {
        private readonly ConcurrentDictionary<string, FileInfo> FileInfoCache = new ConcurrentDictionary<string, FileInfo>();
        private readonly ConcurrentDictionary<string, VideoInfo> VideoCache = new ConcurrentDictionary<string, VideoInfo>();
        private readonly NReco.VideoInfo.FFProbe FFProbe;

        private static readonly Mutex Lock = new Mutex();

        public VideoInfoProvider() {
            FFProbe = new NReco.VideoInfo.FFProbe();
        }

        public Task<VideoInfo> GetAsync(string strPath) {
            return Task.Run<VideoInfo>(() => { return Get(strPath); });
        }

        public VideoInfo Get(string pPath) {
            try {
                var o = GetFromCache(pPath);
                if (o == null) {
                    o = GetFromFile(pPath);
                    if (o != null) {
                        AddToCache(pPath, o);
                    }
                }
                return o;
            } catch (Exception) { }
            return null;
        }

        private VideoInfo GetFromCache(string pPath) {
            if (File.Exists(pPath)) {
                if (FileInfoCache.ContainsKey(Path.GetFullPath(pPath).ToLower())) {
                    var i = new FileInfo(pPath);
                    if (FileInfoCache.TryGetValue(i.FullName.ToLower(), out var o)) {
                        if (
                            i.CreationTimeUtc.Equals(o.CreationTimeUtc) &&
                            i.LastWriteTimeUtc.Equals(o.LastWriteTimeUtc) &&
                            i.Length.Equals(o.Length)
                        ) {
                            if (VideoCache.TryGetValue(i.FullName.ToLower(), out var v)) {
                                return v;
                            }
                        } else {
                            _ = FileInfoCache.TryRemove(i.FullName, out _);
                            _ = VideoCache.TryRemove(i.FullName, out _);
                        }
                    }
                }
            }
            return null;
        }

        private VideoInfo GetFromFile(string pPath) {
            //Only allow getting info from 1 file at a time
            lock (Lock) {
                var i = FFProbe.GetMediaInfo(pPath);
                var a = i.Streams.Where(s => s.CodecType == "audio").FirstOrDefault();
                var v = i.Streams.Where(s => s.CodecType == "video").FirstOrDefault();

                if (v == null) return null;

                return new VideoInfo() {
                    AudioCodec = (a != null) ? a.CodecName : null,
                    Duration = i.Duration,
                    Height = v.Height,
                    IsValid = true,
                    Name = new FileInfo(pPath).Name,
                    Path = pPath,
                    VideoCodec = GetVideoCodec(v.CodecName),
                    Width = v.Width
                };
            }
        }

        private void AddToCache(string pPath, VideoInfo o) {
            if (File.Exists(pPath)) {
                var i = new FileInfo(pPath);
                _ = FileInfoCache.TryAdd(i.FullName.ToLower(), i);
                _ = VideoCache.TryAdd(i.FullName.ToLower(), o);
            }
        }

        private string GetVideoCodec(string pCodec) {
            return pCodec.ReplaceAll(
                new Dictionary<string, string>() {
                    { "h264", "libx264" },
                    { "h265", "libx265" }
                }
            );
        }

        public void Clear() {
            FileInfoCache.Clear();
            VideoCache.Clear();
        }
    }
}