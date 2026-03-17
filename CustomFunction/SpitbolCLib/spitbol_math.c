/*
 * spitbol_math.c — minimal C shared library for net-load-spitbol tests.
 * Exports three functions callable via LOAD('FNAME(...)Tr', 'libspitbol_math.so'):
 *
 *   long   spl_add(long a, long b)        → integer result
 *   double spl_scale(double x, double f)  → real result
 *   void   spl_reverse(const char *s, char *out, long max) → string result via out-param
 *
 * Convention: each function sets a thread-local result buffer and returns a type tag:
 *   0 = string, 1 = integer, 2 = real, -1 = failure
 * For simplicity in this test library, functions return the value directly via
 * the simpler "return value in register" C ABI that the DOTNET wrapper will use.
 */

#include <string.h>
#include <stdlib.h>

/* Simple integer add */
long spl_add(long a, long b) {
    return a + b;
}

/* Real scale */
double spl_scale(double x, double factor) {
    return x * factor;
}

/* String reverse: writes into caller-supplied buf, returns length */
long spl_reverse(const char *s, char *out, long maxlen) {
    long len = (long)strlen(s);
    if (len >= maxlen) len = maxlen - 1;
    for (long i = 0; i < len; i++)
        out[i] = s[len - 1 - i];
    out[len] = '\0';
    return len;
}

/* String length: takes one string arg, returns integer */
long spl_strlen(const char *s) {
    return (long)strlen(s);
}

/* Negate a real */
double spl_negate(double x) {
    return -x;
}
