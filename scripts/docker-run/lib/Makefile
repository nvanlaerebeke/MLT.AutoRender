PROJECT:="AutoRender"
VERSION:=$(shell cat VERSION)

.PHONY: run connect

run:
	echo "Centos 7 RPM"
	docker build -t $(PROJECT):el7-$(VERSION) -f Dockerfile .
	docker run \
		--rm \
		--name $(PROJECT)-$(VERSION) \
		$(PROJECT)-$(VERSION)

connect:
	docker exec -ti `docker ps | grep $(PROJECT):el7-$(VERSION) | awk '{print $$1}'` /bin/bash
