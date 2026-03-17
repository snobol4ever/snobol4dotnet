/*
 * spitbol_xn.c — fixture C library for net-ext-xnblk + net-load-xn tests.
 */

extern long *snobol4_xndta(void);
extern int   snobol4_first_call(void);
extern void  snobol4_register_callback(void (*fn)(void));

/* ── xn_counter ─────────────────────────────────────────────────────── */

long xn_counter(void)
{
    long *xndta = snobol4_xndta();
    if (snobol4_first_call())
        xndta[0] = 0;
    xndta[0]++;
    return xndta[0];
}

/* ── xn_first_call_flag ─────────────────────────────────────────────── */

long xn_first_call_flag(void)
{
    return snobol4_first_call() ? 1 : 0;
}

/* ── xncbp callback machinery ───────────────────────────────────────── */

static long xn_callback_count_val = 0;

static void xn_cleanup(void)
{
    xn_callback_count_val++;
}

long xn_reset_callback_count(void)
{
    xn_callback_count_val = 0;
    return 0;
}

long xn_register_callback(void)
{
    snobol4_register_callback(xn_cleanup);
    return 1;
}

long xn_callback_count(void)
{
    return xn_callback_count_val;
}
