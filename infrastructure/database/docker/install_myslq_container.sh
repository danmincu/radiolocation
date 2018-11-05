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
docker run --name=$MYSQL_CONTAINER -d -p $MYSQL_PORT:3306 -t -e MYSQL_DATABASE='cellsites' -e MYSQL_USER='user' -e MYSQL_PASSWORD='jordan2006' mysql/mysql-server

echo "2. Waits for the $MYSQL_CONTAINER container be up and running"

while true ; do 
  result=$(docker container ls | grep $MYSQL_CONTAINER| grep healthy)
  if [ -z $result ]; then
    printf '.'
  else
    echo "Container is up!"
    break
  fi
  sleep 3
done


password=$(docker logs $MYSQL_CONTAINER 2>&1 | grep GENERATED | tr '\r' ' '| cut -d ":" -f2 | xargs)

echo "root password is $password"

docker exec -it $MYSQL_CONTAINER mysql -uroot --password=$password 

#<<< "ALTER USER 'root'@'localhost' IDENTIFIED BY 'jordan2006';"

#docker exec -it $MYSQL_CONTAINER mysql -uroot --password='AJBYP)udeg%Yp0hx0B.EKRYwAbO'
#ALTER USER 'root'@'localhost' IDENTIFIED BY 'jordan2006';