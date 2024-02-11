#!/usr/bin/env sh

if [[ ! $REMOTE_URI ]]; then
	echo "REMOTE_URI environment variable not set!"
	exit 1
fi

PROJECTS=("Core/Christofel.Common" "Libs/Christofel.CoursesLib" "" "Plugins/Christofel.ReactHandler" "")
CONTEXTS=("ChristofelBaseContext" "CoursesContext" "ApiCacheContext" "ReactHandlerContext" "ManagementContext")

select opt in "${CONTEXTS[@]}" "Quit"; do
    case "$REPLY" in
    1) echo "You picked $opt which is option 1";
       I=1;
       break;;
    2) echo "You picked $opt which is option 2";
       I=2;
       break;;
    3) echo "You picked $opt which is option 3";
       I=3;
       break;;
    4) echo "You picked $opt which is option 4";
       I=4;
       break;;
    5) echo "You picked $opt which is option 5";
       I=5;
       break;;
    $((${#CONTEXTS[@]}+1))) echo "Goodbye!"; break;;
    *) echo "Invalid option. Try another one.";continue;;
    esac
done

PROJECT=${PROJECTS[$I]}
CONTEXT=${CONTEXTS[$I]}

BASE=$(dirname $0)/../..

PROJECT_PATH=$BASE/src/$PROJECT
STARTUP_PROJECT_PATH=$BASE/src/Tools/Christofel.Design
OUT=${CONTEXT}.efbundle
OUT_PATH=bin/$OUT
mkdir -p bin

read -p "Create bundle? (y/n) " yn
case $yn in
	[yY] )
        dotnet ef migrations bundle \
            --self-contained -r linux-x64 \
            --startup-project $STARTUP_PROJECT_PATH \
            --project $PROJECT_PATH \
            --context $CONTEXT \
            --output $OUT_PATH;
          ;;
	[nN] ) ;;
	* ) echo invalid response;
		exit 1;;
esac

#REMOTE_URI=a@b.com
REMOTE_PATH=/tmp/$OUT

scp $OUT_PATH $REMOTE_URI:$REMOTE_PATH
ssh $REMOTE_URI "BUNDLE=$OUT PATH=${REMOTE_PATH} /bin/bash -" < remote-deploy-migrations.sh
