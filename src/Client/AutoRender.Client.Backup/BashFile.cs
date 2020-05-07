using System.IO;

namespace AutoRender.Client.Backup {
    internal class BashFile {
        private readonly string From;
        private readonly string To;
        private readonly string LogFile;

        public BashFile(string pFrom, string pTo, string pLogFile) {
            From = Cygwin.GetCygwinPath(pFrom);
            To = Cygwin.GetCygwinPath(pTo);
            LogFile = Cygwin.GetCygwinPath(pLogFile);
        }

        public string Create() {
            var file = Path.GetTempFileName();
            File.WriteAllText(file, GetContent());
            return file;
        }

        private string GetContent() {
            //    --inplace \
            return $@"#!/bin/bash
rsync \
    -vvaWhHA \
    --chmod=Dg+s,ug+w,Fo-w,+X \
    --delete \
    --progress \
    --exclude '$RECYCLE.BIN' \
    --exclude 'System Volume Information' \
    --exclude 'Series/Complete' \
    --exclude 'Series/Dropped' \
    --exclude 'Series/Incomplete' \
    --exclude 'Series/Ongoing' \
    --exclude 'Anime' \
    '{From}' '{To}' \
    | tee '{LogFile}'".Replace(System.Environment.NewLine, "\n");
        }
    }
}
