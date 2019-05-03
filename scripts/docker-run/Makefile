PROJECT:="docker-build"
BUILDDIR:=$(shell ../../rpmbuild/)
SOURCE:=""

.PHONY: clean env docker

run:
	echo "Centos 7 RPM"
	docker build -t $(PROJECT):el7-$(VERSION) -f Dockerfile .
	docker run \
		--rm \
		--name $(PROJECT)-$(VERSION) \
		$(PROJECT)-$(VERSION)