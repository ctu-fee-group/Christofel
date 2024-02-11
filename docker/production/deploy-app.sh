#!/usr/bin/env sh
#

source ./.env && export $(cut -d= -f1 < .env)

if [[ ! $TAG ]]; then
	echo "Could not find the TAG variable."
	exit 1
fi

if [[ ! $REMOTE_URI ]]; then
	echo "REMOTE_URI environment variable not set!"
	exit 1
fi

echo "Building image $IMAGE_NAME:$TAG"

read -p "Build? (y/n) " yn

case $yn in
	[yY] )
        docker compose build christofel;
          ;;
	[nN] ) ;;
	* ) echo invalid response;
		exit 1;;
esac

echo "Pushing the image to $REMOTE_URI"

docker save $IMAGE_NAME:$TAG | xz | pv | ssh $REMOTE_URI docker load
