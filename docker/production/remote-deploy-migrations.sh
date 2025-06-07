#!/usr/bin/env sh

cd ~/docker/christofel
docker compose cp $PATH database:/tmp/$BUNDLE
docker compose exec database /tmp/$BUNDLE
docker compose exec database /bin/rm /tmp/$BUNDLE
rm /tmp/$BUNDLE
