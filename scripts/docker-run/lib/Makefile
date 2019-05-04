PROJECT:=AutoRender
PROJECT_LOWER:=$(shell echo $(PROJECT) | tr A-Z a-z)
SOURCE:=$(wildcard AutoRender-*.el7.x86_64.rpm)
VERSION:=$(shell cat VERSION)

.PHONY: run connect

run:
	echo "Centos 7 RPM"
	docker build --build-arg "SOURCE=$(SOURCE)" -t "$(PROJECT_LOWER)-$(VERSION)" -f Dockerfile .
	docker run \
		--rm \
		--name $(PROJECT_LOWER)-$(VERSION) \
		$(PROJECT_LOWER)-$(VERSION)

connect:
	docker exec -ti `docker ps | grep $(PROJECT_LOWER)-$(VERSION) | awk '{print $$1}'` /bin/bash
