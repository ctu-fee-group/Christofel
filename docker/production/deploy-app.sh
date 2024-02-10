#!/usr/bin/env sh

source ./.env && export $(cut -d= -f1 < .env)
export IMAGE_TAG=christofel-backend

read -p "Build? (y/n) " yn

case $yn in
	[yY] )
        docker compose build christofel;
          ;;
	[nN] ) ;;
	* ) echo invalid response;
		exit 1;;
esac

docker save $IMAGE_TAG:$TAG | xz | pv | ssh $REMOTE_URI docker load
