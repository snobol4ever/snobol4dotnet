#!/usr/bin/env bash
# rungs_entries_equal_ref.sh -- the witness of a rungs row: each named entry of corpus/tests/snobol4/ALL.sno is extracted alone, run on the
# console under -bf in a folder of its own (stdin its own input when it carries one, else nothing), and must print on standard output exactly
# the ref that sbl printed for it.
#
#   scripts/rungs_entries_equal_ref.sh <entry>...      from anywhere; builds Snobol4 (Release) first; one PASS or RED line per entry
#
# Standard output only, never 2>&1: two engines that agree on an error print different diagnostic text for it. A run is bounded at 30 s.
# The harness grades more than this (masks, the declared switches); an entry that needs those reads RED here for a reason the board would
# not give, so a row names only entries whose ref this compares byte for byte.
# Exit: 0 every entry equal, 1 an entry differs, 2 could not measure (no build, no corpus or harness beside the repo, an entry not extractable).
set -u
HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ENGINE="$(cd "$HERE/.." && pwd)"
ROOT="$(cd "$ENGINE/.." && pwd)"
C="$ROOT/corpus/tests/snobol4"
H="$ROOT/harness/scripts/corpus_suite_harness.py"
DLL="$ENGINE/Snobol4/bin/Release/net10.0/Snobol4.dll"
[ "$#" -gt 0 ] || { echo "usage: $0 <entry>..."; exit 2; }
dotnet build "$ENGINE/Snobol4/Snobol4.csproj" -c Release -m:2 -nodeReuse:false -p:ReadyToRunBuild=false -v:q > /dev/null 2>&1 || { echo "REFUSE(2): Snobol4 does not build"; exit 2; }
[ -f "$DLL" ] && [ -f "$C/ALL.sno" ] && [ -f "$C/ALL.ref" ] && [ -f "$H" ] || { echo "REFUSE(2): no build, corpus or harness beside $ENGINE"; exit 2; }
t="$(mktemp -d)"; trap 'rm -rf "${t:?}"' EXIT
bad=0
for e in "$@"; do
    d="$t/$e"; mkdir "$d"
    python3 "$H" extract "$C/ALL.sno" "$C/ALL.ref" "$e" "$d/$e.sno" --out-ref "$d/$e.ref" --out-in "$d/$e.in" > /dev/null 2>&1 \
        || { echo "REFUSE(2): $e cannot be extracted from $C/ALL.sno"; exit 2; }
    in=/dev/null; [ -f "$d/$e.in" ] && in="$d/$e.in"
    (cd "$d" && timeout 30 dotnet "$DLL" -bf "$e.sno" < "$in" 2> /dev/null > "$d/$e.out")
    if cmp -s "$d/$e.out" "$d/$e.ref"; then echo "PASS $e"; else echo "RED $e"; bad=1; fi
done
exit "$bad"
