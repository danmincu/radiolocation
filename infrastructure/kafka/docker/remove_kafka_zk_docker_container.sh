#!/bin/bash

# documentation
# https://docs.docker.com/compose/gettingstarted/

# https://hub.docker.com/r/wurstmeister/kafka/


echo "start docker"
systemctl start docker

#create a solr container called mcs_solr
KAFKA_CONTAINER="kafka"
ZK_CONTAINER="zookeeper"

docker-compose stop
