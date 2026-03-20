namespace Snobol4.Common;

// ---------------------------------------------------------------------------
// OpCode — the SNOBOL4.NET threaded instruction set
//
// Every opcode maps to exactly one Executive method call.
// Operands are pre-resolved integer indices — no string lookups at runtime.
// Static gotos are pre-resolved to instruction indices.
// ---------------------------------------------------------------------------
internal enum OpCode : byte
{
    // --- Stack push ---
    PushVar     = 1,   // Operand: VariableSlot index
    PushConst   = 2,   // Operand: ConstantPool index
    PushExpr    = 3,   // Operand: ParseExpression index (star function)

    // --- Function call ---
    CallFunc         = 10,  // Operand A: FunctionSlot index, Operand B: arg count
    CallFuncIndirect = 11,  // Operand: arg count; function name is on stack below args (via $VAR)

    // --- Binary operators ---
    OpAdd       = 20,  // __+
    OpSubtract  = 21,  // __-
    OpMultiply  = 22,  // __*
    OpDivide    = 23,  // __/
    OpPower     = 24,  // __^
    OpConcat    = 25,  // ___ (space concatenation)
    OpAlt       = 26,  // __| (pattern alternation)
    OpPeriod    = 27,  // __. (conditional assignment)
    OpDollar    = 28,  // __$ (immediate assignment)
    OpQuestion  = 29,  // __? (pattern match)
    OpAt        = 30,  // __@
    OpAmpersand = 31,  // __&
    OpPercent   = 32,  // __%
    OpHash      = 33,  // __#
    OpTilde     = 34,  // __~

    // --- Unary operators ---
    OpUnaryMinus    = 40,  // _-
    OpUnaryPlus     = 41,  // _+
    OpIndirection   = 42,  // _$
    OpKeyword       = 43,  // _&
    OpName          = 44,  // _.
    OpNegation      = 45,  // _~
    OpInterrogation = 46,  // _? (zero-arg)
    OpUnaryAt       = 47,  // _@
    OpUnaryPercent  = 48,  // _%
    OpUnaryHash     = 49,  // _#
    OpUnarySlash    = 50,  // _/
    OpUnaryOpsyn    = 51,  // opsyn-defined unary: IntOperand = constants index of "_X" key

    // --- Assignment / indexing ---
    BinaryEquals    = 55,  // x._BinaryEquals()
    IndexCollection = 56,  // x.IndexCollection()

    // --- Statement boundary ---
    Init     = 60,  // Operand: source line number
    Finalize = 61,  // x.FinalizeStatement() + ErrorJump check

    // --- Choice (comma operator) ---
    ChoiceStart = 62,  // COMMA_CHOICE: if Failure, pop + clear
    ChoiceEnd   = 63,  // Operand: closing brace count

    // --- Control flow ---
    Jump            = 70,  // Unconditional. Operand: instruction index
    JumpOnSuccess   = 71,  // Jump if NOT failure
    JumpOnFailure   = 72,  // Jump if failure (also clears Failure)
    Halt            = 73,  // End of program
    GotoIndirect     = 74,  // Pop symbol, look up in LabelTable
    GotoIndirectCode = 75,  // Pop symbol, look up via CodeVar

    // --- Unconditional goto helpers ---
    SaveFailure      = 76,  // Push x.Failure onto local save
    RestoreFailure   = 77,  // Pop saved Failure back
    SetFailure       = 78,  // x.Failure = true
    ClearFailure     = 79,  // x.Failure = false (used before failure-goto eval)
    CheckGotoFailure = 80,  // LogRuntimeException(20) if x.Failure

    // --- MSIL-compiled expression delegate ---
    CallMsil = 90,  // Operand: index into Builder.MsilDelegates
}

// ---------------------------------------------------------------------------
// Instruction — 12-byte value type, lives in a dense array
// ---------------------------------------------------------------------------
internal readonly struct Instruction
{
    internal readonly OpCode Op;
    internal readonly int    IntOperand;   // primary operand
    internal readonly int    IntOperand2;  // secondary (CallFunc arg count)

    internal Instruction(OpCode op)
    {
        Op = op; IntOperand = 0; IntOperand2 = 0;
    }

    internal Instruction(OpCode op, int operand)
    {
        Op = op; IntOperand = operand; IntOperand2 = 0;
    }

    internal Instruction(OpCode op, int operand, int operand2)
    {
        Op = op; IntOperand = operand; IntOperand2 = operand2;
    }

    public override string ToString() =>
        IntOperand2 != 0 ? $"{Op}({IntOperand},{IntOperand2})" :
        IntOperand  != 0 ? $"{Op}({IntOperand})" :
                           $"{Op}";
}
