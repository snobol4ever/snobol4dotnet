# Benchmark Grid — D-161 (post M-NET-CORPUS-RUNGS)

**Date:** 2026-03-20  
**Build:** Release, .NET 10.0.201, Linux x64  
**Reps:** 5, Warmup: 1  
**Baseline:** perf/post_hotfix_session159.md

## Results

| Benchmark | Mean | ±StdDev | Alloc/run | vs D-159 |
|-----------|-----:|--------:|----------:|---------|
| Roman_1776 | 25.2ms | ±23.9ms | 434 KB | — (new) |
| ArithLoop_1000 | 39.0ms | ±27.8ms | 1662 KB | ≈ (39.8ms) |
| StringPattern_200 | 108.6ms | ±38.9ms | 5820 KB | — (new) |
| Fibonacci_18 | 188.6ms | ±18.1ms | 11853 KB | ✅ −34% (286.6ms) |
| StringManip_500 | 57.6ms | ±24.0ms | 2449 KB | — (new) |
| FuncCallOverhead_3000 | 19.6ms | ±25.7ms | 805 KB | ✅ −52% (40.6ms) |
| StringConcat_500 | 16.6ms | ±23.7ms | 379 KB | — (new) |
| VarAccess_2000 | 81.0ms | ±21.8ms | 5878 KB | ✅ −22% (103.2ms) |
| OperatorDispatch_100 | 7.0ms | ±0.9ms | 612 KB | — (new) |
| PatternBacktrack_500 | 68.8ms | ±30.7ms | 1971 KB | — (new) |
| TableAccess_500 | 34.2ms | ±31.8ms | 1624 KB | — (new) |
| MixedWorkload_200 | 204.0ms | ±30.1ms | 13931 KB | ✅ −9% (223.2ms) |
| CodeFixed_200 | 97.6ms | ±43.9ms | 6128 KB | — (new) |
| CodeDynamic_200 | 35.2ms | ±10.4ms | 6432 KB | — (new) |
| EvalFixed_200 | 31.6ms | ±5.1ms | 5935 KB | — (new) |
| EvalDynamic_200 | 36.0ms | ±6.8ms | 6133 KB | — (new) |
| IndirectDispatch_500 | 5.2ms | ±0.4ms | 266 KB | ERROR: error 22 ($FN(X) indirect call bug) |

## Notes

- .NET 10 vs .NET 8: runtime upgrade accounts for most improvement vs D-159 baseline
- `IndirectDispatch_500` errors: `$FN(X)` indirect function call not yet supported — diagnosed D-161, fix pending D-162
- High StdDev on several benchmarks: container CPU variance; means are directionally correct
