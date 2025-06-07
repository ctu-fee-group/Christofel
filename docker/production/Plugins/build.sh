#!/bin/sh
docker run -v $PWD:/out/ -v $PWD/../../../src/:/sln mcr.microsoft.com/dotnet/sdk:8.0 /out/builder.sh $1
