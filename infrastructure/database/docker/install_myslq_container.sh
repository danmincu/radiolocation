#!/bin/bash

# documentation
# https://docs.docker.com/install/linux/docker-ce/centos/#upgrade-docker-ce
# https://docs.docker.com/v1.11/engine/reference/commandline/run/

# https://hub.docker.com/r/mysql/mysql-server/


echo "start docker"
systemctl start docker

MYSQL_CONTAINER="mysql"
MYSQL_PORT=3306


echo "1. Creates a mySQL container after downloading the image"
docker run --name=$MYSQL_CONTAINER -d -p $MYSQL_PORT:3306 -t mysql/mysql-server

echo "2. Waits for the $MYSQL_CONTAINER container be up and running"
until $(docker container ls | grep $MYSQL_CONTAINER| grep healthy); do
    printf '.'
    sleep 5
done
