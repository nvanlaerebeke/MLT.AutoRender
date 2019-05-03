#!/bin/bash
SELF_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

cd ${SELF_DIR}
mkdir build
tar xvf AutoRender.tgz -C build

while [ TRUE ]
do
  sleep 10
done

cd ${SELF_DIR}/build && make VERSION=$VERSION REVISION=$REVISION run

cd ~/rpmbuild/
rpmbuild -ba SPECS/build.spec --define="_version $VERSION" --define="_revision $REVISION"