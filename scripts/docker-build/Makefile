PROJECT:="docker-build"
BUILDDIR:=""
SOURCE:=""

.PHONY: rpm clean env docker

rpm: clean env docker

clean:
	rm -rf $(BUILDDIR)/rpmbuild

env:
	mkdir -p $(BUILDDIR)/rpmbuild/{BUILD,RPMS,SOURCES,SPECS,SRPMS}
	/bin/cp -f $(ROOT)/scripts/docker-build/lib/build.spec $(BUILDDIR)/rpmbuild/SPECS/

docker:
	echo "Centos 7 RPM"
	docker build -t rpmbuild:el7-$(PROJECT)-rpmbuild -f Dockerfile .
	docker run \
		--rm \
		-v "$(BUILDDIR)/rpmbuild/":/root/rpmbuild/ \
		-v "$(SOURCE)":/opt/AutoRender/$(shell basename $(SOURCE)) \
		-e VERSION="$(VERSION)" \
		-e REVISION="$(REVISION)" \
		--name rpmbuild-$(PROJECT) \
		rpmbuild:el7-$(PROJECT)-rpmbuild

connect:
	docker exec -ti `docker ps | grep rpmbuild:el7 | awk '{print $$1}'` /bin/bash

stop:
	docker stop `docker ps | grep rpmbuild:el7 | awk '{print $$1}'`
