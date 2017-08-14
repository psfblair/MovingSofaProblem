#!/bin/sh

TARGET_DIR="$1"
TARGET_FILE="$2"

MY_DIR=`dirname $0`
LOCAL_PLUGINS_DIR="$MY_DIR/../../Assets/Plugins"

cp -v "$TARGET_FILE" "$LOCAL_PLUGINS_DIR"

REMOTE_PLUGINS_DIR="$MY_DIR/../../../remote/Documents/holodevelop/MovingSofaProblem/Assets/Plugins"

if [ -d "$REMOTE_PLUGINS_DIR" ]
then
    cp -v "$TARGET_FILE" "$REMOTE_PLUGINS_DIR"
else
    echo "Remote plugin directory not mounted."
fi
