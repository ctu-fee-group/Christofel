#!/usr/bin/env sh

set -euxo pipefail

if [[ ! $REMOTE_URI ]]; then
	echo "REMOTE_URI environment variable not set!"
	exit 1
fi

if [[ $# -gt 0  ]]
then
read -p "Build? (y/n) " yn

case $yn in
	[yY] )
   (cd Plugins && ./build.sh $1)
          ;;
	[nN] ) ;;
	* ) echo invalid response;
		exit 1;;
esac

   if [ $1 = "all" ]
   then
    tar czv ./Plugins | ssh $REMOTE_URI -- tar xzv -C '~/docker/christofel'
   else
    tar czv ./Plugins/Christofel.$1 | ssh $REMOTE_URI -- tar xzv -C '~/docker/christofel'
   fi
   err=$?
   if [[ $err -ne 0 ]]; then
     echo "There was an error when deploying!"
     exit "$err"
   fi
else
  echo "Not enough arguments."
fi
