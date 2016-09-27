#!/bin/bash
TARGET=../ResourcesFMX/Libs/

echo 'Copy: ./src/ScenegraphLocationDelegate/out/OpenRLScenegraphLocationDelegatePlugins.so to ' $TARGET
cp ./src/ScenegraphLocationDelegate/out/OpenRLScenegraphLocationDelegatePlugins.so $TARGET

echo 'Copy: ./src/RendererInfo/out/SceneDistributorInfoPlugin.so to ' $TARGET
cp ./src/RendererInfo/out/SceneDistributorInfoPlugin.so $TARGET

echo 'Copy: ./src/Render/out/SceneDistributorPlugin.so to ' $TARGET
cp ./src/Render/out/SceneDistributorPlugin.so $TARGET



