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
SOLR_PORT=8984

echo "SOLR_CONTAINER $SOLR_CONTAINER"
echo "SOLR_CORE $SOLR_CORE"
echo "SOLR_PORT $SOLR_PORT"

echo "creates a solr container  name $SOLR_CONTAINER downloading the solr image"
docker run --name $SOLR_CONTAINER -d -p $SOLR_PORT:8983 -t solr

echo "starts the solr container"
docker start $SOLR_CONTAINER

echo "waits for the solr admin interface to be up and running"
until $(curl --output /dev/null --silent --head --fail http://localhost:$SOLR_PORT/); do
    printf '.'
    sleep 5
done


echo "creates the sol core $SOLR_CORE"
docker exec -it --user=solr $SOLR_CONTAINER bin/solr create_core -c $SOLR_CORE
echo "disables the autocreation of fields the sol core $SOLR_CORE"
docker exec -it --user=solr $SOLR_CONTAINER bin/solr config -c $SOLR_CORE -p 8983 -action set-user-property -property update.autoCreateFields -value false

echo "wait for the solr core $SOLR_CORE to be up"
until $(curl --output /dev/null --silent --head --fail http://localhost:$SOLR_PORT/solr/$SOLR_CORE/select?q=*:*); do
    printf '.'
    sleep 5
done

echo "upload the custom schema and solr config to the $SOLR_CORE in the $SOLR_CONTAINER"
docker cp ./solr/solrconfig.xml $SOLR_CONTAINER:/opt/solr/server/solr/$SOLR_CORE/conf/solrconfig.xml
docker cp ./solr/managed-schema $SOLR_CONTAINER:/opt/solr/server/solr/$SOLR_CORE/conf/managed-schema

# This step increases Solr heap size
# https://github.com/docker-solr/docker-solr/issues/44
# this is not tested in this context. after this command exacutes the docker container needs restart
docker cp ./solr.in.sh $SOLR_CONTAINER:/opt/solr/bin/solr.in.sh

echo "reload the core $SOLR_CORE in the $SOLR_CONTAINER using the $SOLR_PORT"
curl "http://localhost:$SOLR_PORT/solr/admin/cores?action=RELOAD&core=$SOLR_CORE"

echo "wait for the solr core $SOLR_CORE to be up"
until $(curl --output /dev/null --silent --head --fail http://localhost:$SOLR_PORT/solr/$SOLR_CORE/select?q=*:*); do
    printf '.'
    sleep 5
done

echo "copy into the $SOLR_CONTAINER the sample data"
docker cp ./../import_data/sample.csv $SOLR_CONTAINER:/opt/solr/

echo "Load into $SOLR_CORE sample data"
docker exec -it --user=solr $SOLR_CONTAINER bin/post -c $SOLR_CORE sample.csv

RESPONSE=$(curl "http://localhost:$SOLR_PORT/solr/$SOLR_CORE/select?q=*:*&rows=0" | grep 24373)
echo "Check solr reponse to data query: $RESPONSE"


if [[ -n "${RESPONSE/[ ]*\n/}" ]]
then
  echo "The script terminated with success!"
  exit 0
else
  echo "The script failed. Incorrect number of solr documents found"
  exit 1  
fi
