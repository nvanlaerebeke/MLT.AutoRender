FROM centos:latest
ENV container docker
ARG SOURCE
MAINTAINER Nico van Laerebeke nvanlaerebeke@gmail.com

#setup base OS
RUN yum install -y epel-release

#install mono
RUN rpm --import "https://keyserver.ubuntu.com/pks/lookup?op=get&search=0x3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF" ; \ 
    curl https://download.mono-project.com/repo/centos7-vs.repo | tee /etc/yum.repos.d/mono-centos7-vs.repo
    
RUN yum install -y mono-core

RUN yum install -y alsa-lib SDL2 libXv libvdpau gtk2 xorg-x11-server-utils xorg-x11-util-macros 
RUN yum install -y libtheora libvorbis jack-audio-connection-kit opencv-core qt libsamplerate \
    SDL sox vid.stab libxml2 frei0r pulseaudio-libs avformat libavdevice ffms2 libexif fftw \ 
    libebur128  gavl  ladspa libsoup liboil qimageblitz qjson recordmydesktop dvgrab \
    dvdauthor xine-ui eigen3 libvpx libavc1394

#install app
COPY "$SOURCE" /tmp/
RUN rpm -ivh /tmp/*.rpm

RUN mkdir /root/AutoRender/
COPY config.ini /root/AutoRender/

#run app
#ENTRYPOINT ["/usr/lib/AutoRender/Server/bin/startupscript.sh", "start"]

COPY run.sh /
ENTRYPOINT [ "/run.sh" ]