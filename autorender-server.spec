%global __os_install_post %{nil}

#turn off compression
%define _source_payload w0.gzdio
%define _binary_payload w0.gzdio

%define debug_package %{nil}
%define _rpmfilename  %%{NAME}-%%{VERSION}-%%{RELEASE}.%%{ARCH}.rpm

Name: AutoRender
Version: %{_version}
Release: %{_revision}%{?dist}
Summary: melt AutoRender server

Group: CrazySoftware
License: MIT
URL: crazytje.com		
Source0: AutoRender.tgz
AutoReqProv: no

BuildRequires: mono-complete
BuildRequires: nuget

Requires: mono-core

%description
Server sotware for MLT Framework
Takes in the mlt xml files (without consumer) and allows the client to render them with a h264 consumer

%prep
rm -rf $RPM_BUILD_ROOT

%setup -c

%build
make ROOT=$RPM_BUILD_ROOT build

%install
install -d "$RPM_BUILD_ROOT"/usr/lib/AutoRender/Server
install -d "$RPM_BUILD_ROOT"/etc/systemd/system/

chmod +x build/bin/*.sh
cp -r build/* "$RPM_BUILD_ROOT"/usr/lib/AutoRender/Server/
cp build/lib/autorender-server.service "$RPM_BUILD_ROOT"/etc/systemd/system/

%files
/usr/lib/AutoRender/Server/
/etc/systemd/system/autorender-server.service

%post
systemctl daemon-reload
systemctl enable autorender-server
systemctl restart autorender-server

%clean
rm -rf $RPM_BUILD_ROOT

%changelog
* Sat Nov 4 2018 Nico van Laerebeke <nico.vanlaerebeke@gmail.com>
- initial rpm release
