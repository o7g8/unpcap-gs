#!/bin/sh

dotnet publish -r linux-x64 -p:PublishSingleFile=true -c Release --self-contained true