ROOT=`pwd`
PROJECT=AutoRender
SUBMODULES= $(wildcard src/Lib/*)
BUILDDIR=build

CONFIGURATION=Debug
VERSION=$(shell cat VERSION)
REVISION=$(shell svn info | awk -F " " '/^Revision:/{print $$2}')

.PHONY: rpm bin build clean tar rpmbuild submdoules app

rpm: clean tar rpmbuild

bin: clean
	cd ./scripts/ && ./build-mlt.sh

build: clean app

clean:
	echo $(ROOT)
	rm -rf $(ROOT)/build
	rm -rf $(ROOT)/bin
	rm -rf $(ROOT)/src/*/bin
	rm -rf $(ROOT)/src/*/obj
	#rm -rf packages

tar:
	tar -ccvpf $(PROJECT).tgz . --exclude=*.tgz

rpmbuild:
	mkdir -p ~/rpmbuild/{BUILD,RPMS,SOURCES,SPECS,SRPMS}
	echo '%_topdir %(echo ~)/rpmbuild' > ~/.rpmmacros
	
	cp $(PROJECT).tgz ~/rpmbuild/SOURCES/
	cp autorender-server.spec ~/rpmbuild/SPECS/
	
	cd ~ && rpmbuild -ba ~/rpmbuild/SPECS/autorender-server.spec --define="_version $(VERSION)" --define="_revision $(REVISION)"

submodules:
	$(foreach MODULE,$(SUBMODULES), cd $(ROOT)/$(MODULE) && make build ;)

app:
	nuget restore src/Server.sln
	msbuild src/Server.sln /p:Configuration=$(CONFIGURATION) /p:Platform="Any CPU"
	
	#mkdir build/lib
	#mkdir build/bin
	#cp lib/startupscript.sh build/bin/
	#cp lib/autorender-server.service build/lib/
