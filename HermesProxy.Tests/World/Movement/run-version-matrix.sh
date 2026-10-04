#!/usr/bin/env bash
# Runs the World/Movement tests once per legacy/modern build pair.
#
# LegacyVersion and ModernVersion are static readonly, so a build pair is a process: a plain
# `dotnet test` only ever exercises the default pair (3.3.5a -> 1.14.2). The pairs below cover
# each legacy era (1.12, 2.4.3, 3.3.5a), each modern movement layout (bit-packed flags, 32-bit
# flag words, 3.4.3) and each of the five ObjectUpdateBuilder families.
#
# Run it after any change to the movement codecs, MovementInfo, or a create's movement block.
# A pair with no snapshot yet fails and leaves a .received.txt beside where the .verified.txt
# belongs; read it before renaming it.
#
#   ./run-version-matrix.sh            # Debug
#   ./run-version-matrix.sh Release
set -u
cd "$(dirname "$0")/../../.." || exit 1

config="${1:-Debug}"
dll="HermesProxy.Tests/bin/$config/net10.0/HermesProxy.Tests.dll"

dotnet build HermesProxy.Tests -c "$config" -v q --nologo || exit 1

pairs=(
    "V3_3_5a_12340 V1_14_2_42597"
    "V3_3_5a_12340 V3_4_3_54261"
    "V2_4_3_8606 V2_5_3_41750"
    "V2_4_3_8606 V2_5_2_39570"
    "V1_12_1_5875 V1_14_2_42597"
    "V1_12_1_5875 V1_14_0_40237"
)

failed=0
for pair in "${pairs[@]}"; do
    read -r legacy modern <<< "$pair"
    echo "== legacy $legacy -> modern $modern"
    HERMES_TEST_LEGACY_BUILD="$legacy" HERMES_TEST_MODERN_BUILD="$modern" \
        dotnet "$dll" --filter-namespace "HermesProxy.Tests.World.Movement*" --no-progress \
        | grep -E "^(failed|  total|  failed|  succeeded|Test run summary)|error" \
        || true
    [ "${PIPESTATUS[0]}" -eq 0 ] || failed=$((failed + 1))
done

echo "== $failed of ${#pairs[@]} pairs failed"
exit "$failed"
