FROM centos:latest
ENV container docker
MAINTAINER Nico van Laerebeke nico.vanlaerebeke@gmail.com

#setup base OS
RUN yum install -y epel-release
RUN  yum groupinstall -y "Development Tools" 

#install mono
RUN rpm --import "https://keyserver.ubuntu.com/pks/lookup?op=get&search=0x3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF" ; \ 
    curl https://download.mono-project.com/repo/centos7-vs.repo | tee /etc/yum.repos.d/mono-centos7-vs.repo
    
RUN yum install -y mono-devel msbuild nuget

#install libs required to build MLT
RUN yum install -y xorg-x11-util-macros which wget nasm make yasm cmake libtheora-devel libvorbis-devel gtk2-devel jack-audio-connection-kit-devel opencv-core qt-devel \
    libsamplerate-devel SDL-devel SDL2-devel sox-devel vid.stab-devel libxml2-devel frei0r-devel  pulseaudio-libs-devel \
    avformat libavdevice ffms2-devel.x86_64 frei0r-devel.x86_64 gtk2-devel libexif-devel fftw-devel libebur128-devel \
    SDL-devel SDL2-devel sox-devel libxml2-devel gavl-devel libsamplerate-devel ladspa-devel jack-audio-connection-kit-devel \
    qt-devel libtheora-devel libvorbis-devel libvdpau-devel libsoup-devel liboil-devel python-devel alsa-lib qimageblitz-devel \
    qjson-devel recordmydesktop dvgrab dvdauthor xine-ui eigen3 libvpx-devel libavc1394-devel

#add build script 
RUN [ "mkdir", "/opt/AutoRender/"]
COPY ./lib/build.sh /opt/AutoRender/

#start build
ENTRYPOINT ["/opt/AutoRender/build.sh"]