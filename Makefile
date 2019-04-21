ROOT=`pwd`
PROJECT=AutoRender
BUILDDIR=build
CONFIGURATION=Debug
VERSION=$(shell cat VERSION)
REVISION=$(shell svn info | awk -F " " '/^Revision:/{print $$2}')

rpm: clean tar rpmbuild

build: clean app

clean:
	echo $(ROOT)
	rm -rf $(ROOT)/build
	rm -rf $(ROOT)/*/bin/
	rm -rf $(ROOT)/*/obj/
	#rm -rf packages

tar:
	tar -ccvpf $(PROJECT).tgz . --exclude=*.tgz

rpmbuild:
	mkdir -p ~/rpmbuild/{BUILD,RPMS,SOURCES,SPECS,SRPMS}
	echo '%_topdir %(echo ~)/rpmbuild' > ~/.rpmmacros
	
	cp $(PROJECT).tgz ~/rpmbuild/SOURCES/
	cp autorender-server.spec ~/rpmbuild/SPECS/
	
	cd ~ && rpmbuild -ba ~/rpmbuild/SPECS/autorender-server.spec --define="_version $(VERSION)" --define="_revision $(REVISION)"

app:
	nuget restore AutoRenderServer.sln
	xbuild AutoRenderServer.sln /p:Configuration=$(CONFIGURATION) /p:Platform="Any CPU"
	
	mkdir build/lib
	mkdir build/bin
	cp lib/startupscript.sh build/bin/
	cp lib/autorender-server.service build/lib/
