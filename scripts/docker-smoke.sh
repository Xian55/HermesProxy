#!/usr/bin/env bash
# Proves an image runs, before docker-image.yml publishes it. No legacy server needed.
#
# Ports are probed from a second container, on the proxy container's own address: a published port
# proves nothing, since docker-proxy accepts on it whether or not the proxy listens.
set -euo pipefail

image="${1:?usage: docker-smoke.sh <image>}"
ports="1119 8081 8084 8086"
name="hermes-smoke-$$"
config="$(mktemp -d)"

# Git Bash would rewrite the container paths. Its own paths mean nothing to docker.exe.
export MSYS_NO_PATHCONV=1
host_path() { if command -v cygpath >/dev/null; then cygpath -w "$1"; else echo "$1"; fi; }

cleanup() {
    docker rm -f -v "$name" "$name-refused" >/dev/null 2>&1 || true
    rm -rf "$config"
}
trap cleanup EXIT

fail() {
    echo "FAIL: $*" >&2
    docker logs "$name" 2>&1 | tail -30 >&2 || true
    exit 1
}

# <description> <text the output must name> <docker run arguments>
expect_refused() {
    local what="$1" text="$2" rc=0 output
    shift 2
    output="$(timeout 60 docker run --rm --name "$name-refused" "$@" "$image" 2>&1)" || rc=$?
    [ "$rc" = 1 ] || { echo "$output" >&2; fail "$what: exit code $rc, expected 1"; }
    grep -q "$text" <<<"$output" || { echo "$output" >&2; fail "$what: output does not name $text"; }
    echo "OK: $what"
}

expect_refused "wildcard ExternalAddress refused" ExternalAddress \
    -e HERMES_ProxyNetworkOptions__ExternalAddress=0.0.0.0
expect_refused "failed listener exits non-zero" 203.0.113.1 \
    -e HERMES_ProxyNetworkOptions__BindAddress=203.0.113.1

# The enum defaults prove their names survived trimming.
help="$(timeout 60 docker run --rm "$image" --help 2>&1)"
grep -q 'ClientBuild (default: V2_5_2_40892)' <<<"$help" || fail "--help lost the client build's name"
grep -q 'PacketLevel (default: Warning)' <<<"$help" || fail "--help lost the log level's name"
echo "OK: --help lists the settings"

# One key from an env file and one from the environment, stdin open as under `docker run -i`.
echo 'HERMES_ClientOptions__ClientBuild=V3_4_3_54261' > "$config/realm.env"
docker create -i --name "$name" \
    --env-file "$(host_path "$config/realm.env")" \
    -e HERMES_LegacyServerOptions__Build=V3_3_5a_12340 \
    "$image" >/dev/null

# A capture and a log past retention, and a file just as old that the proxy did not write.
touch -d '40 days ago' "$config/legacy_12340_20200101_000000_1.pkt" "$config/hermes-20200101_000000.log" "$config/notes.txt"
(
    cd "$config"
    docker cp -q legacy_12340_20200101_000000_1.pkt "$name:/app/PacketsLog/"
    docker cp -q notes.txt "$name:/app/PacketsLog/"
    docker cp -q hermes-20200101_000000.log "$name:/app/Logs/"
    docker cp -q notes.txt "$name:/app/Logs/"
)
docker start "$name" >/dev/null

probe="for port in $ports; do nc -z -w 5 \$0 \$port || exit 1; done"
for attempt in $(seq 1 60); do
    [ "$(docker inspect -f '{{.State.Running}}' "$name")" = true ] || fail "exited during startup"
    address="$(docker inspect -f '{{range .NetworkSettings.Networks}}{{.IPAddress}}{{end}}' "$name")"
    if docker run --rm busybox:1.37 sh -c "$probe" "$address" >/dev/null 2>&1; then
        echo "OK: all ports accept connections on the container's address"
        break
    fi
    [ "$attempt" != 60 ] || fail "ports did not open within 60 attempts"
    sleep 1
done

logs="$(docker logs "$name" 2>&1)"
grep -q 'Modern (Client) Build: V3_4_3_54261' <<<"$logs" || fail "the env file did not apply"
grep -q 'Legacy (Server) Build: V3_3_5a_12340' <<<"$logs" || fail "the environment did not apply"
grep -q 'told to connect to itself' <<<"$logs" || fail "no warning about a loopback ExternalAddress"
if grep -q 'Version .*dirty' <<<"$logs"; then fail "version reports a dirty tree"; fi
echo "OK: env file and environment applied, loopback warned about, version clean"

started=$SECONDS
docker stop "$name" >/dev/null
[ $((SECONDS - started)) -lt 5 ] || fail "docker stop took $((SECONDS - started)) s with stdin open"
exit_code="$(docker inspect -f '{{.State.ExitCode}}' "$name")"
[ "$exit_code" = 0 ] || fail "exit code $exit_code after docker stop"
echo "OK: stopped in time with exit code 0"

for directory in Logs AccountData PacketsLog; do
    listing="$(docker cp "$name:/app/$directory" - | tar -tv --numeric-owner)"
    grep -q ' 1654/1654 ' <<<"$listing" || fail "/app/$directory does not belong to the proxy's user"
done
listing="$(docker cp "$name:/app/Logs" - | tar -t)"
grep -q 'hermes-' <<<"$listing" || fail "no log file written as non-root"
echo "OK: state directories belong to the proxy's user, log written"

logs="$(docker logs "$name" 2>&1)"

# <folder> <the old file> <its kind, as the log says it>
expect_swept() {
    local listing
    listing="$(docker cp "$name:/app/$1" - | tar -t)"
    grep -q "Removed 1 $3 older than 30 days" <<<"$logs" || fail "no sweep of $3 in the log"
    if grep -q "$2" <<<"$listing"; then fail "$2 is still there"; fi
    grep -q 'notes.txt' <<<"$listing" || fail "the sweep of $3 removed a file the proxy did not write"
}
expect_swept PacketsLog legacy_12340_20200101_000000_1.pkt "packet captures"
expect_swept Logs hermes-20200101_000000.log "log files"
echo "OK: retention removed the old capture and log, and nothing else"
