#!/usr/bin/env bash
# build_native.sh — rebuild all native .so libraries from source
# Requires: gcc (or CC env var pointing to another C compiler)
# Platform: Linux x64. For other platforms, adjust -o names and flags.

set -euo pipefail
CC="${CC:-gcc}"
CFLAGS="-O2 -shared -fPIC -Wall"
ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

build() {
    local src="$1" out="$2"
    echo "  $CC $src → $out"
    $CC $CFLAGS -o "$ROOT/$out" "$ROOT/$src"
}

echo "=== Building native libraries ==="
build "CustomFunction/SpitbolCLib/spitbol_math.c"        "CustomFunction/SpitbolCLib/libspitbol_math.so"
build "CustomFunction/SpitbolNoconvLib/spitbol_noconv.c" "CustomFunction/SpitbolNoconvLib/libspitbol_noconv.so"
build "CustomFunction/SpitbolCreateLib/spitbol_create.c" "CustomFunction/SpitbolCreateLib/libspitbol_create.so"
build "CustomFunction/SpitbolXnLib/snobol4_rt.c"         "CustomFunction/SpitbolXnLib/libsnobol4_rt.so"
build "CustomFunction/SpitbolXnLib/snobol4_rt.c"         "CustomFunction/libsnobol4_rt.so"
build "CustomFunction/SpitbolXnLib/spitbol_xn.c"         "CustomFunction/SpitbolXnLib/libspitbol_xn.so"
echo "=== Done ==="
