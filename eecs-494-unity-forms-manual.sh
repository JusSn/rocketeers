#!/bin/bash

# eecs-494-unity-forms-manual.sh Version 2.1

DP0=$(pwd)
TURNIN=$1
UNITY=/Applications/Unity/Unity.app/Contents/MacOS/Unity
ZIP=/Applications/Keka.app/Contents/Resources/keka7z

ZIPFILE=$TURNIN-public.7z
APP=$TURNIN.app
EXE=$TURNIN.exe
EXEDATA=${TURNIN}_Data
EL=0

if [ "$1" == "" ]; then
  echo
  echo "Usage: sh eecs-494-unity-forms-manual.sh TURNIN_NAME"
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
  if [ $EL -eq 0 ]; then
    echo "$ZIP" a "$ZIPFILE" "$APP" "$EXE" "$EXEDATA"
    "$ZIP" a "$ZIPFILE" "$APP" "$EXE" "$EXEDATA"
    EL=$?
  fi=
fi

echo ""

if [ $EL -eq 0 ]; then
  shasum -a 512 "$ZIPFILE"
else
  echo Something went wrong. See above for details.
fi

exit
