#!/usr/bin/env bash

# This script is used to generate a version string for the current commit.
#
# The format is:
#   <tag>-<commits since tag>.<short sha>
# or (if there is no tag):
#   0.0.0-<commits since beginning>.<short sha>
#
# See https://learn.microsoft.com/en-us/nuget/concepts/package-versioning for
# more information on versioning.

TAG=$(git describe --abbrev=0 --tags 2>/dev/null)

if [ -n "$TAG" ] && git describe --exact-match &>/dev/null; then
    VERSION=$TAG
else
    SHORT_SHA=$(git rev-parse --short HEAD)
    if [ -z "$TAG" ]; then
        COMMITS_SINCE_BEGINNING=$(git rev-list --count HEAD 2>/dev/null)
        VERSION="0.0.0-$COMMITS_SINCE_BEGINNING.$SHORT_SHA"
    else
        COMMITS_SINCE_TAG=$(git rev-list --count HEAD "^$TAG" 2>/dev/null)
        VERSION="$TAG-$COMMITS_SINCE_TAG.$SHORT_SHA"
    fi
fi

echo "$VERSION"
