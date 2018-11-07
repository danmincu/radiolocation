#!/bin/bash

# documentation
# https://docs.docker.com/install/linux/docker-ce/centos/#upgrade-docker-ce
# https://docs.docker.com/v1.11/engine/reference/commandline/run/

# https://hub.docker.com/r/mysql/mysql-server/


echo "start docker"
systemctl start docker

MYSQL_CONTAINER="mysql"
MYSQL_PORT=3306
MYSQL_INIT_DATABASE="security"
MYSQL_ROOT_PASSWORD="password1!"
MYSQL_USER="user"
MYSQL_USER_PASSWORD="password1!"

echo "1. Creates a mySQL container after downloading the image"
docker run --name=$MYSQL_CONTAINER -d -p $MYSQL_PORT:3306 -t -e MYSQL_DATABASE=$MYSQL_INIT_DATABASE -e MYSQL_USER=$MYSQL_USER -e MYSQL_PASSWORD=$MYSQL_USER_PASSWORD mysql/mysql-server

echo "2. Waits for the $MYSQL_CONTAINER container be up and running"

while true ; do 
  result=$(docker container ls | grep $MYSQL_CONTAINER| grep healthy| tr ' ' '.'| xargs)
  if [ -z $result ]; then
    printf '.'
  else
    echo "$MYSQL_CONTAINER container is up!"
    break
  fi
  sleep 3
done


password=$(docker logs $MYSQL_CONTAINER 2>&1 | grep GENERATED | tr '\r' ' '| cut -d ":" -f2 | xargs)

echo "root password is $password"
echo "reset root password to $MYSQL_ROOT_PASSWORD"
docker exec -i mysql mysql -uroot --connect-expired-password --password=$password <<< "ALTER USER 'root'@'localhost' IDENTIFIED BY '$MYSQL_ROOT_PASSWORD';"

# USE THIS ONLY IF YOU WANT TO ENTER BASH AS ROOT
docker exec -it mysql mysql -uroot --password=$MYSQL_ROOT_PASSWORD

# USE THIS ONLY IF YOU WANT TO ENTER BASH AS $MYSQL_USER
docker exec -it mysql mysql -u$MYSQL_USER --password=$MYSQL_USER_PASSWORD
