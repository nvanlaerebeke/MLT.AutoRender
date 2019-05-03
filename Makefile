PROJECT:=AutoRender
ROOT:=$(shell pwd)
SUBMODULES:=$(wildcard src/Lib/*)
BUILDDIR:=$(ROOT)/build
DISTDIR:=$(ROOT)/dist
RPMBUILDDIR:=$(ROOT)/rpmbuild
RPMDIR:=$(ROOT)/rpm

CONFIGURATION=Debug
VERSION=$(shell cat $(ROOT)/VERSION)
REVISION:=$(shell git rev-parse --short HEAD)

.PHONY: rpm bin build clean tar rpmbuild submdoules app run runrpm

#
# SECTION 1: starts the rpm build process - provides clean sources to the docker-build
#
rpm: clean tgz
	make PROJECT=$(PROJECT) ROOT=$(ROOT) BUILDDIR=$(BUILDDIR) VERSION=$(VERSION) REVISION=$(REVISION) SOURCE=$(BUILDDIR)/$(PROJECT).tgz -C $(ROOT)/scripts/docker-build rpm

	install -d "$(DISTDIR)"
	/bin/cp -f "$(BUILDDIR)"/rpmbuild/RPMS/*.rpm "$(DISTDIR)"

clean:
	rm -rf "$(BUILDDIR)"
	rm -rf "$(DISTDIR)"
	rm -rf "$(ROOT)"/src/*/bin
	rm -rf "$(ROOT)"/src/*/obj
	rm -rf "$(ROOT)/src/packages"

tgz:
	install -d "$(BUILDDIR)"
	tar -ccvpf "build/$(PROJECT).tgz" --exclude=.git* --exclude=*.tgz --exclude=build . 


#
# SECTION 2: Builds the sources .NET + melt/ffmpeg binaries into dist directory
#            and builds an rpm that installs the binaries + startup script into
#            a docker image by providing a dockerfile and rpm that contains 
#            the above  binaries
#
run: distrpm
	install -d "$(RPMDIR)"
	make PROJECT="$(PROJECT)" ROOT="$(ROOT)" VERSION="$(VERSION)" RPMBUILDDIR="$(RPMBUILDDIR)" -C "$(ROOT)/scripts/docker-run" rpm

distrpm: distsource
	/bin/cp -f "$(ROOT)/scripts/docker-build/lib/build.spec" ~/rpmbuild/SPECS/
	/bin/cp -f "$(DISTDIR)/AutoRender.tgz" ~/rpmbuild/SOURCES/

	echo '%_topdir %(echo ~)/rpmbuild' > ~/.rpmmacros
	cd ~/rpmbuild && rpmbuild -ba SPECS/build.spec --define="_version $(VERSION)" --define="_revision $(REVISION)"
	/bin/cp -f ~/rpmbuild/RPMS/"$(PROJECT)-$(VERSION)-$(REVISION)"*.rpm "$(RPMBUILDDIR)"

distsource: dist
	/bin/cp -f "$(ROOT)/scripts/docker-build/lib/autorender" "$(DISTDIR)"
	/bin/cp -f "$(ROOT)/scripts/docker-build/lib/autorender.service" "$(DISTDIR)"

	cd "$(DISTDIR)" && tar -ccvpf AutoRender.tgz --exclude=*.tgz .

dist: clean app bin
	install -d "$(RPMBUILDDIR)"
	cp RPMS/*.rpm "$(RPMBUILDDIR)"

app: submodules
	nuget restore src/Server.sln
	msbuild src/Server.sln /p:Configuration=$(CONFIGURATION) /p:Platform="Any CPU"

	install -d "$(DISTDIR)/bin"
	cp -R "$(BUILDDIR)/"* "$(DISTDIR)/bin"

submodules:
	$(foreach MODULE,$(SUBMODULES), cd $(ROOT)/$(MODULE) && make build ;)

bin: 
	/bin/bash ./scripts/docker-build/lib/build-mlt.sh
	
	install -d "$(DISTDIR)/bin"
	cp -R "$(BUILDDIR)/bin/"* "$(DISTDIR)/bin/"