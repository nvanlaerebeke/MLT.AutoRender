PROJECT:="AutoRender"
ROOT:=../../
RPMBUILDDIR:=$(shell ../../rpmbuld)

.PHONY: clean env docker

rpm: env tgz rpmbuild

env:
	cp "$(ROOT)/scripts/docker-run/lib/Dockerfile" $(RPMBUILDDIR)
	cp "$(ROOT)/scripts/docker-run/lib/Makefile" $(RPMBUILDDIR)

tgz:
	cd "$(RPMBUILDDIR)" && tar -ccvpf AutoRender-docker.tgz .

rpmbuild:
	cp "$(RPMBUILDDIR)/AutoRender-run.tgz" ~/rpmbuild/SOURCES
	cp "$(ROOT)/scripts/docker-run/lib/run.spec" ~/rpmbuild/SPECS

	cd ~/rpmbuild && rpmbuild -ba SPECS/run.spec --define="_version $(VERSION)" --define="_revision $(REVISION)"

run:
	echo "Centos 7 RPM"
	docker build -t $(PROJECT):el7-$(VERSION) -f Dockerfile .
	docker run \
		--rm \
		--name $(PROJECT)-$(VERSION) \
		$(PROJECT)-$(VERSION)