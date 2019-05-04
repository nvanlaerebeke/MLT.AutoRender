#!/bin/bash
SELF_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" >/dev/null 2>&1 && pwd )"

cd ${SELF_DIR}
mkdir build
tar xvf AutoRender.tgz -C build

cd ${SELF_DIR}/build && make VERSION=$VERSION REVISION=$REVISION run