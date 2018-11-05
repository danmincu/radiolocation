#!/bin/bash

# documentation
# https://docs.docker.com/install/linux/docker-ce/centos/#upgrade-docker-ce
# https://docs.docker.com/v1.11/engine/reference/commandline/run/
# https://hub.docker.com/_/solr/

echo "start docker"
systemctl start docker

#create a solr container called mcs_solr
SOLR_CONTAINER="mcs_solr"
SOLR_CORE="mcellsites"

echo "SOLR_CONTAINER $SOLR_CONTAINER"
echo "SOLR_CORE $SOLR_CORE"

echo "remove the docker container $SOLR_CONTAINER"
docker container kill $SOLR_CONTAINER
docker container rm $SOLR_CONTAINER