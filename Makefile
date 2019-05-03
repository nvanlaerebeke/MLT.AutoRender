PROJECT:=AutoRender
ROOT:=$(shell pwd)
SUBMODULES:=$(wildcard src/Lib/*)
BUILDDIR:=$(ROOT)/build
DISTDIR:=$(ROOT)/dist
RPMBUILDDIR:=$(ROOT)/rpmbuild

CONFIGURATION=Debug
VERSION=$(shell cat $(ROOT)/VERSION)
REVISION:=$(shell git rev-parse --short HEAD)

.PHONY: rpm bin build clean tar rpmbuild submdoules app run runrpm

#
# SECTION 1: starts the rpm build process - provides clean sources to the docker-build
#
rpm: clean tgz
	make PROJECT=$(PROJECT) ROOT=$(ROOT) BUILDDIR=$(BUILDDIR) VERSION=$(VERSION) REVISION=$(REVISION) SOURCE=$(BUILDDIR)/$(PROJECT).tgz -C $(ROOT)/scripts/docker-build rpm

clean:
	rm -rf $(BUILDDIR)
	rm -rf $(DISTDIR)
	rm -rf $(ROOT)/src/*/bin
	rm -rf $(ROOT)/src/*/obj
	rm -rf $(ROOT)/src/packages

tgz:
	install -d $(BUILDDIR)
	tar -ccvpf build/$(PROJECT).tgz --exclude=.git* --exclude=*.tgz --exclude=build . 


#
# SECTION 2: Builds the sources .NET + melt/ffmpeg binaries into dist directory
#            and builds an rpm that installs the binaries + startup script into
#            a docker image by providing a dockerfile and rpm that contains 
#            the above  binaries
#
run: dist
	make PROJECT=$(PROJECT) ROOT=$(ROOT) VERSION=$(VERSION) SOURCE=$(DISTDIR)/*.rpm -C $(ROOT)/scripts/docker-run rpm

dist: clean app bin prep
	echo '%_topdir %(echo ~)/rpmbuild' > ~/.rpmmacros
	cd ~ && rpmbuild -ba SPECS/build.spec --define="_version $(VERSION)" --define="_revision $(REVISION)"

	install -d "$(RPMBUILDDIR)"
	mv RPMS/*.rpm "$(RPMBUILDDIR)"

app: submodules
	nuget restore src/Server.sln
	msbuild src/Server.sln /p:Configuration=$(CONFIGURATION) /p:Platform="Any CPU"

	install -d "$(DISTDIR)/bin"
	mv "$(BUILDDIR)/"* "$(DISTDIR)/bin"

submodules:
	$(foreach MODULE,$(SUBMODULES), cd $(ROOT)/$(MODULE) && make build ;)

bin: 
	/bin/bash ./scripts/docker-build/lib/build-mlt.sh
	
	install -d "$(DISTDIR)/bin"
	cp -R "$(BUILDDIR)/bin/*" "$(DISTDIR)/bin/"