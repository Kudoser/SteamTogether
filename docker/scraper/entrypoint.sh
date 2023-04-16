#!/bin/bash

set -euo pipefail

log() {
    echo "ENTRYPOINT: $*"
}

while ! curl -s "${BOT_HEALTHCHECK_URL:?}" > /dev/null; do
    log "Waiting for the bot to start..."
    sleep 1
done

log "Bot is up, starting scraper..."

/app/SteamTogether.Scraper "$@"
