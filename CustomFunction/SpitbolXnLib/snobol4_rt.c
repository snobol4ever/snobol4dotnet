/*
 * snobol4_rt.c — libsnobol4_rt: SPITBOL external-function runtime helpers.
 *
 * Provides the C-side API that external C libraries use to access SNOBOL4
 * runtime state.  The .NET runtime registers two callbacks once per process
 * load — immediately after dlopen of libsnobol4_rt.so:
 *
 *   1. get_context_fn  — fills in (xndta**, first_call*) for the current call.
 *   2. set_callback_fn — stores a C shutdown callback into the current entry.
 *
 * Registration protocol
 * ---------------------
 * The .NET runtime calls snobol4_rt_register(get_context_fn, set_callback_fn)
 * once after loading libsnobol4_rt.so.  Both pointers are pinned managed
 * delegates held for process lifetime.
 *
 * Exported API (used by fixture C libraries)
 * -------------------------------------------
 *   long* snobol4_xndta(void)               — pointer to xndta[0] for current call
 *   int   snobol4_first_call(void)           — 1 on first-ever call, 0 thereafter
 *   int   snobol4_reload_call(void)          — always 0 on DOTNET (no save/reload)
 *   void  snobol4_register_callback(void*)  — register xncbp shutdown callback
 */

#include <stddef.h>

/* ── callback types ───────────────────────────────────────────────────── */

typedef void (*snobol4_rt_get_context_fn)(void *xndta_out, void *first_call_out);
typedef void (*snobol4_rt_set_callback_fn)(void *callback_fn);

static snobol4_rt_get_context_fn _get_context   = NULL;
static snobol4_rt_set_callback_fn _set_callback = NULL;

/* ── registration (called once by .NET runtime after dlopen) ─────────── */

void snobol4_rt_register(snobol4_rt_get_context_fn get_ctx_fn,
                         snobol4_rt_set_callback_fn set_cb_fn)
{
    _get_context   = get_ctx_fn;
    _set_callback  = set_cb_fn;
}

/* ── internal helper: fetch current context ─────────────────────────── */

static void _fetch(long **xndta_out, int *first_call_out)
{
    if (_get_context)
        _get_context(xndta_out, first_call_out);
    else
    {
        if (xndta_out)      *xndta_out      = NULL;
        if (first_call_out) *first_call_out = 0;
    }
}

/* ── public API ──────────────────────────────────────────────────────── */

long *snobol4_xndta(void)
{
    long *xndta = NULL;
    _fetch(&xndta, NULL);
    return xndta;
}

int snobol4_first_call(void)
{
    int fc = 0;
    _fetch(NULL, &fc);
    return fc;
}

/* Always 0 on DOTNET — no image save/reload mechanism exists yet. */
int snobol4_reload_call(void)
{
    return 0;
}

/*
 * snobol4_register_callback — register a shutdown callback (xncbp).
 *
 * The callback will be invoked when the SNOBOL4 program calls UNLOAD for
 * this function, or when the process exits (whichever comes first).
 * The double-fire guard on the .NET side ensures it runs at most once.
 */
void snobol4_register_callback(void (*callback_fn)(void))
{
    if (_set_callback)
        _set_callback((void *)callback_fn);
}

/* ── allocation helpers (net-ext-create) ────────────────────────────────── */

#include <stdlib.h>
#include <string.h>

void *snobol4_alloc(long n)
{
    if (n <= 0) return NULL;
    void *p = malloc((size_t)n);
    if (p) memset(p, 0, (size_t)n);
    return p;
}

long *snobol4_alloc_longs(long n)
{
    return (long *)snobol4_alloc(n * (long)sizeof(long));
}

void snobol4_free(void *p)
{
    free(p);
}
