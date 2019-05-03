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

#install app
COPY "$SOURCE" /tmp/
RUN rpm -ivh /tmp/*.rpm

#run app
ENTRYPOINT ["/usr/lib/AutoRender/Server/bin/startupscript.sh", "start"]