

MYSQL_ROOT_PASSWORD="password1!"


echo "1. Creates a superuser"
docker exec -i mysql mysql -uroot --password=$MYSQL_ROOT_PASSWORD <<< "CREATE USER 'superuser'@'%' IDENTIFIED BY '$MYSQL_ROOT_PASSWORD';"
docker exec -i mysql mysql -uroot --password=$MYSQL_ROOT_PASSWORD <<< "GRANT ALL PRIVILEGES ON *.* TO 'superuser'@'%';"
echo "2. Creates a locations database"
docker exec -i mysql mysql -uroot --password=$MYSQL_ROOT_PASSWORD <<< "CREATE DATABASE locations;"
echo "3. Creates a user called <locuser> with $MYSQL_ROOT_PASSWORD password"
docker exec -i mysql mysql -uroot --password=$MYSQL_ROOT_PASSWORD <<< "CREATE USER 'locuser'@'%' IDENTIFIED BY '$MYSQL_ROOT_PASSWORD';"
docker exec -i mysql mysql -uroot --password=$MYSQL_ROOT_PASSWORD <<< "USE locations;GRANT ALL PRIVILEGES ON locations.* TO 'locuser'@'%';"
echo "4. Flush privileges"
docker exec -i mysql mysql -uroot --password=$MYSQL_ROOT_PASSWORD <<< "FLUSH PRIVILEGES;"