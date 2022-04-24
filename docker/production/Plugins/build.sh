#!/bin/sh
docker run -v $PWD:/out/ -v $PWD/../../../src/:/sln mcr.microsoft.com/dotnet/sdk:6.0 /out/builder.sh $1
