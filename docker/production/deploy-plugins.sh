#!/usr/bin/env sh

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
    scp -C -r ./Plugins $REMOTE_URI:~/docker/christofel/
   else
    scp -C -r ./Plugins/Christofel.$1 $REMOTE_URI:~/docker/christofel/Plugins/
   fi
else
  echo "Not enough arguments."
fi
