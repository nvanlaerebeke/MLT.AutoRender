# MLT.AutoRender

## Introduction

Auto render MLT projects on CentOS with a Windows Client frontend interface

## Client build

For now the client needs to be build using visual studio 2017+
Make sure to check out the git submodules and compile those first.

## Server build

There are several ways to run the server:
1. On CentOS 7 inside a docker image, Dockerfile and dependencies provided by AutoRender-Docker rpm 
2. Manually using the Dockerfile, requires copying over the Dockerfile and dependencies. Dockerfile with start a CentOS7 container
3. Installing the AutoRender rpm containing the binaries on a CentOS 7 machine
4. Compiling the 

The server rpm provides a Dockerfile so that the AutoRender server process
can be run inside a container.

The build scripts are written to be run on Centos 7

To start the build

```bash
make rpm
```

