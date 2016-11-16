#!/bin/bash
TARGET=./build/Resources/Libs/
mkdir -p $TARGET

echo 'Copy: ./src/ScenegraphLocationDelegate/out/SceneDistributorScenegraphLocationDelegatePlugins.so to ' $TARGET
cp ./src/ScenegraphLocationDelegate/out/SceneDistributorScenegraphLocationDelegatePlugins.so $TARGET

echo 'Copy: ./src/RendererInfo/out/SceneDistributorInfoPlugin.so to ' $TARGET
cp ./src/RendererInfo/out/SceneDistributorInfoPlugin.so $TARGET

echo 'Copy: ./src/Render/out/SceneDistributorPlugin.so to ' $TARGET
cp ./src/Render/out/SceneDistributorPlugin.so $TARGET



