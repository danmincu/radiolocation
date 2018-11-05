#!/bin/bash

# documentation
# https://docs.docker.com/install/linux/docker-ce/centos/#upgrade-docker-ce
# https://docs.docker.com/v1.11/engine/reference/commandline/run/
# https://hub.docker.com/_/solr/

echo "start docker"
systemctl start docker

#create a solr container called mcs_solr
MYSQL_CONTAINER="mysql"
MYSQL_PORT=3306

echo "remove the docker container $SOLR_CONTAINER"
docker container kill $MYSQL_CONTAINER
docker container rm $MYSQL_CONTAINER