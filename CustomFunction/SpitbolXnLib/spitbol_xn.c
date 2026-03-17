/*
 * spitbol_xn.c — fixture C library for net-ext-xnblk tests.
 *
 * Demonstrates XNBLK persistent opaque state: the function uses
 * snobol4_xndta() to access its private 32-long storage block and
 * snobol4_first_call() to detect the first invocation.
 *
 * Exported functions
 * ------------------
 *   long xn_counter(void)
 *       On first call: initialises xndta[0] = 0.
 *       Every call: increments xndta[0] and returns the new value.
 *       Proves that state persists across successive SNOBOL4 calls.
 *
 *   long xn_first_call_flag(void)
 *       Returns 1 on the very first call, 0 on every subsequent call.
 *       Directly exposes snobol4_first_call() for test assertion.
 *
 * Link against libsnobol4_rt.so to resolve snobol4_xndta / snobol4_first_call.
 */

/* Forward declarations — resolved from libsnobol4_rt.so at link time. */
extern long *snobol4_xndta(void);
extern int   snobol4_first_call(void);

long xn_counter(void)
{
    long *xndta = snobol4_xndta();
    if (snobol4_first_call())
        xndta[0] = 0;           /* initialise on first call */
    xndta[0]++;
    return xndta[0];
}

long xn_first_call_flag(void)
{
    return snobol4_first_call() ? 1 : 0;
}
