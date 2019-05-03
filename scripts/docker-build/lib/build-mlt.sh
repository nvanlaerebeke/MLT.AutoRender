#!/bin/bash
SELF_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"
ROOT=$SELF_DIR/../../../build/
SRC=$ROOT/src
BIN=$ROOT/bin

mkdir -p $SRC
mkdir -p $BIN

cd $SRC
git clone https://github.com/mltframework/mlt.git && cd mlt && git checkout tags/v6.14.0 
cd $SRC
git clone https://git.ffmpeg.org/ffmpeg.git && cd ffmpeg && git checkout n4.1.3 

cd $SRC/mlt && ./configure && make
cd $SRC/ffmpeg && ./configure && make

#Copy melt resources
cp $SRC/mlt/src/melt/melt $BIN
cp -R $SRC/mlt/src/modules $BIN
cp -R $SRC/mlt/profiles $BIN
cp -R $SRC/mlt/presets $BIN

mkdir $BIN/framework/ $BIN/mlt++/
cp $SRC/mlt/src/framework/libmlt* $BIN/framework
cp $SRC/mlt/src/mlt++/libmlt* $BIN/mlt++

#copy ffmpeg resources
cp $SRC/ffmpeg/ffmpeg $BIN
cp $SRC/ffmpeg/ffprobe $BIN