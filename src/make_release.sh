#!/bin/bash

VERSION=$(grep "^VERSION=[0-9].[0-9].[0-9]" "../linux/flash-multi" | awk -F = '{ print $2 }')

echo
echo "Version: $VERSION"
echo "Creating archive 'flash-multi-$VERSION.tar.gz'"
tar -czf /tmp/flash-multi-$VERSION.tar.gz --transform s/linux/flash-multi-$VERSION/ ../linux/*
sleep 1s

SHA=`(sha256sum /tmp/flash-multi-$VERSION.tar.gz | awk -v N=1 '{print $N}')`

echo
echo "Package: flash-multi-$VERSION.tar.gz"
echo "SHA256:  ${SHA^^}"
echo "Size:    `(ls -al /tmp/flash-multi-$VERSION.tar.gz | awk -v N=5 '{print $N}')`"
echo
