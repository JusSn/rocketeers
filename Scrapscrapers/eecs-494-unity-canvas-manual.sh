#!/bin/bash

# eecs-494-unity-canvas-manual.sh Version 2.1

DP0=$(pwd)
TURNIN=$1
UNITY=/Applications/Unity/Unity.app/Contents/MacOS/Unity
ZIP=/Applications/Keka.app/Contents/Resources/keka7z

ZIPFILE=$TURNIN-private.7z
APP=$TURNIN.app
EXE=$TURNIN.exe
EXEDATA=${TURNIN}_Data
REPODIR=/tmp/${TURNIN}_Repo
EL=0

if [ "$1" == "" ]; then
  echo
  echo "Usage: sh eecs-494-unity-canvas-manual.sh TURNIN_NAME"
  echo
  exit 1
fi

for file in $(ls "$ZIPFILE" 2> /dev/null); do rm $file; done

if [ ! -d "$APP" ]; then
  echo "$APP not found."
  EL=1
fi
if [ ! -f "$EXE" ]; then
  echo "$EXE not found."
  EL=1
fi
if [ ! -d "$EXEDATA" ]; then
  echo "$EXEDATA not found."
  EL=1
fi

if [ $EL -ne 0 ]; then
  echo Build missing.
else
  for dir in $(ls -d "$REPODIR" 2> /dev/null); do rm -r $dir; done

  pushd /tmp/

  echo git clone "$DP0" "$REPODIR"
  git clone "$DP0" "$REPODIR"
  EL=$?

  for dir in $(ls -d "$REPODIR/.git" 2> /dev/null); do rm -rf $dir; done

  popd

  if [ $EL -eq 0 ]; then
    echo "$ZIP" a "$ZIPFILE" "$APP" "$EXE" "$EXEDATA" "$REPODIR"
    "$ZIP" a "$ZIPFILE" "$APP" "$EXE" "$EXEDATA" "$REPODIR"
    EL=$?
  fi

  for dir in $(ls -d "$REPODIR" 2> /dev/null); do rm -r $dir; done
fi

echo ""

if [ $EL -eq 0 ]; then
  shasum -a 512 "$ZIPFILE"
else
  echo Something went wrong. See above for details.
fi

exit
