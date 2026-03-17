namespace FSharpOptionLibrary

// ── Discriminated union used for coercion tests ───────────────────────────────
//
// Shape is a simple DU with one-field and two-field cases.
// The coercion layer in CallReflectFunction maps:
//   Circle r          → StringVar  "Circle 3.5"
//   Rectangle w h     → StringVar  "Rectangle 2 4"
//   (tag-only case)   → StringVar  "<TagName>"

type Shape =
    | Circle    of radius : float
    | Rectangle of width : float * height : float
    | Point                             // zero-field case

// ── Result-like DU ────────────────────────────────────────────────────────────
// OkVal/ErrVal mirrors a Result type without depending on FSharp.Core Result
// (which is itself a DU but we want a user-defined DU in the test).
type Outcome =
    | OkVal  of value : string
    | ErrVal of message : string

// ─────────────────────────────────────────────────────────────────────────────
// IntOption — methods returning int option
//   ParseInt(s)  → Some(n) if s is a valid integer, None otherwise
//   SafeDiv(a,b) → Some(a/b) if b≠0, None otherwise
// ─────────────────────────────────────────────────────────────────────────────
type IntOption() =
    member _.ParseInt(s: string) : int option =
        match System.Int64.TryParse(s) with
        | true, n -> Some(int n)
        | _       -> None

    member _.SafeDiv(a: int64, b: int64) : int64 option =
        if b = 0L then None else Some(a / b)

// ─────────────────────────────────────────────────────────────────────────────
// StringOption — methods returning string option
//   FirstWord(s)  → Some(first-word) if s is non-empty, None otherwise
//   LookupColor(k) → Some(hex) for known colors, None otherwise
// ─────────────────────────────────────────────────────────────────────────────
type StringOption() =
    member _.FirstWord(s: string) : string option =
        let trimmed = s.Trim()
        if trimmed.Length = 0 then None
        else
            let parts = trimmed.Split([|' '; '\t'|], System.StringSplitOptions.RemoveEmptyEntries)
            Some parts[0]

    member _.LookupColor(key: string) : string option =
        match key.ToLowerInvariant() with
        | "red"   -> Some "#FF0000"
        | "green" -> Some "#00FF00"
        | "blue"  -> Some "#0000FF"
        | _       -> None

// ─────────────────────────────────────────────────────────────────────────────
// ShapeFactory — methods returning the Shape DU
//   MakeCircle(r)    → Circle r
//   MakeRect(w,h)    → Rectangle(w,h)
//   MakePoint()      → Point
// ─────────────────────────────────────────────────────────────────────────────
type ShapeFactory() =
    member _.MakeCircle(r: double)              : Shape = Circle r
    member _.MakeRect(w: double, h: double)     : Shape = Rectangle(w, h)
    member _.MakePoint()                        : Shape = Point

// ─────────────────────────────────────────────────────────────────────────────
// OutcomeFactory — methods returning the Outcome DU
//   Succeed(s)  → OkVal s
//   Fail(s)     → ErrVal s
// ─────────────────────────────────────────────────────────────────────────────
type OutcomeFactory() =
    member _.Succeed(s: string) : Outcome = OkVal s
    member _.Fail(s: string)    : Outcome = ErrVal s
