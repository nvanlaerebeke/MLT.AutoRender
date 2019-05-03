PROJECT:=AutoRender
ROOT:=$(shell pwd)
SUBMODULES:= $(wildcard src/Lib/*)
BUILDDIR:=$(ROOT)/build
DISTDIR=:$(ROOT)/dist

CONFIGURATION=Debug
VERSION=$(shell cat $(ROOT)/VERSION)
REVISION:=$(shell git rev-parse --short HEAD)

.PHONY: rpm bin build clean tar rpmbuild submdoules app run

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



dist: clean app bin prep
	echo '%_topdir %(echo ~)/rpmbuild' > ~/.rpmmacros
	cd ~ && rpmbuild -ba ~/rpmbuild/SPECS/build.spec --define="_version $(VERSION)" --define="_revision $(REVISION)"

app: submodules
	nuget restore src/Server.sln
	msbuild src/Server.sln /p:Configuration=$(CONFIGURATION) /p:Platform="Any CPU"

	install -d "$(DISTDIR)/bin"
	mv "$(BUILDDIR)/"* "$(DISTDIR)/bin"

submodules:
	$(foreach MODULE,$(SUBMODULES), cd $(ROOT)/$(MODULE) && make build ;)

bin: 
	/bin/bash ./scripts/docker-build/lib/build-mlt.sh
	
	install -d "$(DISTDIR)"
	mv "$(BUILDDIR)/bin" "$(DISTDIR)"

prep: 
	cp "$(ROOT)/scripts/docker-build/
	cd "$(DISTDIR) && tar -ccvpf ~/rpmbuild/SOURCES/AutoRender.tgz .

run:
	make PROJECT=$(PROJECT) ROOT=$(ROOT) VERSION=$(VERSION) SOURCE=$(DISTDIR)/*.rpm -C $(ROOT)/scripts/docker-run run