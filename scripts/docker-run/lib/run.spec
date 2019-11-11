%global __os_install_post %{nil}

#turn off compression
%define _source_payload w0.gzdio
%define _binary_payload w0.gzdio

%define debug_package %{nil}
%define _rpmfilename  %%{NAME}-%%{VERSION}-%%{RELEASE}-docker.%%{ARCH}.rpm

Name: AutoRender
Version: %{_version}
Release: %{_revision}%{?dist}
Summary: melt AutoRender server

Group: CrazySoftware
License: MIT
URL: crazytje.com		
Source0: AutoRender-run.tgz
AutoReqProv: no

%description
Sources and Dockerfile to get a running AutoRender docker image


%prep
rm -rf $RPM_BUILD_ROOT

%setup -c

%build

%install
install -d "$RPM_BUILD_ROOT"/usr/lib64/AutoRender/Docker
cp AutoRender-*.rpm "$RPM_BUILD_ROOT"/usr/lib64/AutoRender/Docker
cp Makefile "$RPM_BUILD_ROOT"/usr/lib64/AutoRender/Docker/Makefile
cp Dockerfile "$RPM_BUILD_ROOT"/usr/lib64/AutoRender/Docker/Dockerfile
cp run.sh "$RPM_BUILD_ROOT"/usr/lib64/AutoRender/Docker/run.sh
cp VERSION "$RPM_BUILD_ROOT"/usr/lib64/AutoRender/Docker/VERSION

%files
/usr/lib64/AutoRender/Docker/

%post

%clean
rm -rf $RPM_BUILD_ROOT

%changelog