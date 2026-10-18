#!/usr/bin/env bash
# test_in_batches.sh -- run TestSnobol4 in batches of 100 tests, each in its own test host.
#
# Lon 2026-10-18: "You better be running tests in batches of 100." A whole-suite `dotnet test` intermittently hangs (once for ten
# minutes, its testhost idle) or crashes its host, and one bad test then costs every result of the run. Here a batch that crashes or
# hangs costs only its own hundred, and it is named.
#
#   scripts/test_in_batches.sh [size]      from anywhere; default size 100
#
# Builds once, lists every test by fully qualified name (vstest /ListFullyQualifiedTests), and runs each batch with a filter of its
# names, in its own process group, bounded at BATCH_SECS (300). A batch that times out or whose host crashes is printed as NOT RUN with
# its test count, never folded into passed or failed. Prints one line per batch, then every failing test by name, then a total line:
#   TOTAL tests=N passed=P failed=F skipped=S not_run=R batches=B incomplete=I
# Exit: 0 every test ran and none failed; 1 a test failed or a batch did not complete; 2 could not measure (no build, no list).
set -u
cd "$(dirname "${BASH_SOURCE[0]}")/.." || exit 2
size="${1:-100}"; secs="${BATCH_SECS:-300}"
proj=TestSnobol4/TestSnobol4.csproj; dll=TestSnobol4/bin/Debug/net10.0/TestSnobol4.dll
t="$(mktemp -d)"; trap 'rm -rf "$t"' EXIT

dotnet build "$proj" -m:4 -v:q > "$t/build" 2>&1 || { tail -5 "$t/build"; echo "REFUSE(2): the test project does not build"; exit 2; }
dotnet vstest "$dll" /ListFullyQualifiedTests /ListTestsTargetPath:"$t/all" > /dev/null 2>&1 && [ -s "$t/all" ] \
    || { echo "REFUSE(2): vstest listed no tests from $dll"; exit 2; }
split -l "$size" -d -a 3 "$t/all" "$t/batch."

# bounded <secs> <out> cmd...: run cmd in its own process group; on the bound, kill the whole group (dotnet test, vstest, testhost)
bounded() {
    local limit="$1" out="$2" pid n=0; shift 2
    setsid "$@" > "$out" 2>&1 & pid=$!
    while kill -0 "$pid" 2> /dev/null; do
        sleep 1; n=$((n + 1))
        if [ "$n" -ge "$limit" ]; then kill -TERM -- "-$pid" 2> /dev/null; sleep 2; kill -KILL -- "-$pid" 2> /dev/null; wait "$pid" 2> /dev/null; return 124; fi
    done
    wait "$pid"
}

tests=0 passed=0 failed=0 skipped=0 notrun=0 batches=0 incomplete=0
: > "$t/failures"
for b in "$t"/batch.*; do
    batches=$((batches + 1)); n=$(wc -l < "$b"); tests=$((tests + n))
    filter="$(sed 's/^/FullyQualifiedName=/' "$b" | paste -sd'|')"
    bounded "$secs" "$t/out" dotnet test "$proj" --no-build -v:q --filter "$filter" --logger "console;verbosity=normal"; rc=$?
    p=$(grep -acE '^\s+Passed ' "$t/out"); f=$(grep -acE '^\s+Failed ' "$t/out"); s=$(grep -acE '^\s+Skipped ' "$t/out")
    if [ "$rc" = 124 ] || grep -aq "Test Run Aborted" "$t/out" || [ $((p + f + s)) -lt "$n" ]; then
        why="crashed"; [ "$rc" = 124 ] && why="hung past ${secs}s"
        echo "batch ${b##*.}: $n tests  NOT RUN ($why; $((p + f + s)) reported)  first: $(head -1 "$b")"
        incomplete=$((incomplete + 1)); notrun=$((notrun + n - p - f - s))
    else
        echo "batch ${b##*.}: $n tests  passed=$p failed=$f skipped=$s"
    fi
    passed=$((passed + p)); failed=$((failed + f)); skipped=$((skipped + s))
    grep -aE '^\s+Failed ' "$t/out" | sed -E 's/^\s+Failed ([^ ]+).*/FAIL \1/' >> "$t/failures"
done
cat "$t/failures"
echo "TOTAL tests=$tests passed=$passed failed=$failed skipped=$skipped not_run=$notrun batches=$batches incomplete=$incomplete"
[ "$failed" = 0 ] && [ "$incomplete" = 0 ] && exit 0
exit 1
