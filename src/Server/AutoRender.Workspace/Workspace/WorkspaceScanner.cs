using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoRender.Data;
using AutoRender.MLT;
using AutoRender.Video;

namespace AutoRender.Workspace {

    internal class WorkspaceScanner {
        private readonly string NewPath;
        private readonly string ProjectDir;
        private readonly string FinalDir;
        private readonly VideoInfoProvider VideoInfoProvider;

        public WorkspaceScanner(string pNewPath, string pPojectDir, string pFinalDir, VideoInfoProvider pVideoInfoProvider) {
            NewPath = pNewPath;
            ProjectDir = pPojectDir;
            FinalDir = pFinalDir;
            VideoInfoProvider = pVideoInfoProvider;
        }

        /// <summary>
        /// Scans the workspace directory for what the content is
        /// </summary>
        /// <returns></returns>
        public List<WorkspaceItem> Scan() {
            return GetMatches(
                Directory.GetFiles(NewPath, "*.mp4").OrderBy(p => p),
                GetFinal(FinalDir),
                Directory.GetFiles(ProjectDir, "*.mlt").OrderBy(p => p)
            );
        }

        /// <summary>
        /// Links the new, final and projects together
        /// </summary>
        /// <param name="pNew"></param>
        /// <param name="pFinal"></param>
        /// <param name="pProjects"></param>
        /// <returns></returns>
        private List<WorkspaceItem> GetMatches(IEnumerable<string> pNew, IEnumerable<string> pFinal, IEnumerable<string> pProjects) {
            var matches = new List<WorkspaceItem>();
            var lstNew = new List<VideoInfo>();
            var lstFinal = new List<VideoInfo>();
            var lstProjects = new List<MLTProject>();

            pNew.ToList().ForEach(n => {
                var i = VideoInfoProvider.Get(n);
                if (i != null) { lstNew.Add(i); }
            });
            pFinal.ToList().ForEach(f => {
                var i = VideoInfoProvider.Get(f);
                if (f != null) { lstFinal.Add(i); }
            });
            pProjects.ToList().ForEach(p => {
                var mp = new MLTProject(p, VideoInfoProvider);
                if (mp != null) { lstProjects.Add(mp); }
            });

            //The project links the new and final files together
            //Without it a link to the final file cannot be made
            //
            //Note: It can, but then the name needs to be calculated here based on the video info
            //      If needed this can be added, but if it is not a problem, leave as is
            new List<MLTProject>(lstProjects).ForEach(p => {
                _ = lstProjects.Remove(p);

                var final = lstFinal.Where(f => f.Path.Equals(p.TargetPath)).FirstOrDefault();
                if (final != null) {
                    _ = lstFinal.Remove(final);
                }
                var newfile = lstNew.Where(n => n.Path.Equals(p.SourcePath)).FirstOrDefault();
                if (newfile != null) {
                    _ = lstNew.Remove(newfile);
                }
                matches.Add(new WorkspaceItem(p, newfile, final));
            });
            lstNew.ForEach(n => matches.Add(new WorkspaceItem(null, n, null)));
            lstFinal.ForEach(f => matches.Add(new WorkspaceItem(null, null, f)));
            return matches;
        }

        private IEnumerable<string> GetFinal(string pPath) {
            var lstFinal = new List<string>();
            Directory.GetDirectories(FinalDir).ToList().ForEach(d =>
                lstFinal.AddRange(Directory.GetFiles(d, "*.mp4").ToList())
            );
            return lstFinal;
        }
    }
}