#!/bin/bash
#
# AutoRender Server     Start/Stop the AutoRender.Serivce process.
#
# chkconfig: 2345 90 60
# description: Server sotware for MLT Framework
#              Takes in the mlt xml files (without consumer) and allows the client to render them with a h264 consumer


NAME=AutoRender.Service
PIDFILE=/var/run/autorender-server.pid
BIN=/usr/lib/AutoRender/Server/
MONO=/usr/bin/mono
RED='\033[0;31m'
GREEN='\033[0;32m'
NC='\033[0m'

cd $BIN/melt && source source-me

start() {
    if [ -f $PIDFILE ]; then
        PID=`cat $PIDFILE`
        COUNT=`ps --no-headers --pid $PID | wc -l`
        if [ $COUNT == 0 ]; then
            rm -f $PIDFILE
        fi
    fi
    if [ ! -f $PIDFILE ]; then
        echo -e "${GREEN}Starting $NAME...${NC}"
        cd $BIN && $MONO AutoRender.Service.exe | tee /var/log/autorender-server.log 2>&1
    else
        echo -e "${RED}Service already running${NC}" 2>&1
        exit 1
    fi
}
stop() {
    if [ -f $PIDFILE ]; then
        echo -e "${RED}Stopping $NAME...${NC}"
        msg=$(kill $(cat $PIDFILE) 2>&1)
        if [ $? -gt 0 ]; then
            echo -e "${RED}failed${NC}"
            echo -e "${RED}$msg${NC}"
        else
            rm $PIDFILE > /dev/null 2>&1
            echo -e "${GREEN}stop succeeded${NC}"
        fi
    else
        echo -e "${RED}$NAME not started${NC}"
    fi
}

restart() {
    stop
    start
}

status() {
    if [ -f $PIDFILE ]; then
        PID=`cat $PIDFILE`
        COUNT=`ps --no-headers --pid $PID | wc -l`
        if [ $COUNT == 0 ]; then
            echo -e "${RED}not running${NC}"
            rm -f $PIDFILE
            exit 1
        else
            echo -e "${GREEN}process running with pid ${PID}${NC}"
        fi
    else
        echo -e "${RED}process not found${NC}"
        exit 1
    fi
}

case $1 in start|stop|restart|status)
        "$1"
        ;;
    *)
        echo -e "${RED}No such operation${NC}"
        exit 1
        ;;
esac
