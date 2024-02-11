#!/bin/sh

if [ $1 = "all" ]; then
    for dir in /sln/Plugins/*
    do
        if [ -d "$dir" ]; then
            name=$(basename ${dir})
            echo "Found $name"
            dotnet publish -c Release /sln/Plugins/$name/$name.csproj -o /out/$name --self-contained -r linux-x64
        fi
    done
else
    dotnet publish -c Release /sln/Plugins/Christofel.$1/Christofel.$1.csproj -o /out/Christofel.$1/ --self-contained -r linux-x64
fi

