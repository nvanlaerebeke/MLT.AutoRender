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
            return $@"#!/bin/bash

cd ~

rsync \
    -vvaWhHA \
    --chmod=Dg+s,ug+w,Fo-w,+X \
    --delete \
    --progress \
    --inplace \
    --exclude '$RECYCLE.BIN' \
    --exclude 'System Volume Information' \
    --exclude 'Anime' \
    '{From}' '{To}' \
    | tee '{LogFile}'".Replace(System.Environment.NewLine, "\n");
        }
    }
}
