#!/bin/bash

VERSION=$(grep "^VERSION=[0-9].[0-9].[0-9]" "../linux/flash-multi" | awk -F = '{ print $2 }')

echo
echo "Version: $VERSION"
echo "Creating archive 'flashmulti-$VERSION.tar.gz'"
tar -czf /tmp/flashmulti-$VERSION.tar.gz --transform s/linux/flashmulti-$VERSION/ ../linux/*
sleep 1s
echo
echo "Package: /tmp/flashmulti-$VERSION.tar.gz"
echo "SHA256:  `(sha256sum /tmp/flashmulti-$VERSION.tar.gz | awk -v N=1 '{print $N}')`"
echo "Size:    `(ls -al /tmp/flashmulti-$VERSION.tar.gz | awk -v N=5 '{print $N}')`"
echo
