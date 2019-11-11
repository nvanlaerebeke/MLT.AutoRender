PROJECT:="AutoRender"
ROOT:=../../
RPMBUILDDIR:=../../rpmbuild

.PHONY: clean env docker

rpm: env tgz rpmbuild

env:
	cp "$(ROOT)/scripts/docker-run/lib/Dockerfile" "$(RPMBUILDDIR)"
	cp "$(ROOT)/scripts/docker-run/lib/Makefile" "$(RPMBUILDDIR)"
	cp "$(ROOT)/scripts/docker-run/lib/run.sh" "$(RPMBUILDDIR)"
	cp "$(ROOT)/VERSION" "$(RPMBUILDDIR)"
	
tgz:
	cd "$(RPMBUILDDIR)" && tar -ccvpf AutoRender-run.tgz --exclude=*.tgz .

rpmbuild:
	cp "$(RPMBUILDDIR)/AutoRender-run.tgz" ~/rpmbuild/SOURCES
	cp "$(ROOT)/scripts/docker-run/lib/run.spec" ~/rpmbuild/SPECS

	cd ~/rpmbuild && rpmbuild -ba SPECS/run.spec --define="_version $(VERSION)" --define="_revision $(REVISION)"