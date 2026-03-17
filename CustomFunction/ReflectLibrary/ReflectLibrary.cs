namespace ReflectFunction;

/// <summary>
/// Single-method class: auto-prototype discovers the one public instance method.
/// LOAD('ReflectLibrary.dll', 'ReflectFunction.Doubler') → DOUBLE(n) callable.
/// </summary>
public class Doubler
{
    public long Double(long n) => n * 2;
}

/// <summary>
/// Single static method: auto-prototype discovers it without instantiation.
/// LOAD('ReflectLibrary.dll', 'ReflectFunction.Greeter') → GREET(s) callable.
/// </summary>
public class Greeter
{
    public static string Greet(string name) => "Hello, " + name + "!";
}

/// <summary>
/// Explicit binding target: has two public methods — auto-prototype must fail
/// without ::MethodName; explicit ::Square succeeds.
/// LOAD('ReflectLibrary.dll', 'ReflectFunction.Calculator::Square') → SQUARE(n).
/// LOAD('ReflectLibrary.dll', 'ReflectFunction.Calculator::Cube')   → CUBE(n).
/// </summary>
public class Calculator
{
    public double Square(double x) => x * x;
    public double Cube(double x)   => x * x * x;
}

/// <summary>
/// Mixed-type method: (string, long) → string.
/// LOAD('ReflectLibrary.dll', 'ReflectFunction.Formatter') → FORMAT(s,n) callable.
/// </summary>
public class Formatter
{
    public string Format(string label, long count) => label + "=" + count;
}

/// <summary>
/// Step 5: async method returning Task&lt;long&gt;.
/// LOAD('ReflectLibrary.dll', 'ReflectFunction.AsyncDoubler') → ASYNCDOUBLE(n).
/// SNOBOL4 call blocks transparently on GetAwaiter().GetResult().
/// </summary>
public class AsyncDoubler
{
    public async Task<long> AsyncDouble(long n)
    {
        await Task.Yield();   // ensure the method is genuinely async
        return n * 2;
    }
}

/// <summary>
/// Step 5: async method returning Task&lt;string&gt;.
/// LOAD('ReflectLibrary.dll', 'ReflectFunction.AsyncGreeter') → ASYNCGREET(s).
/// </summary>
public class AsyncGreeter
{
    public static async Task<string> AsyncGreet(string name)
    {
        await Task.Yield();
        return "Hello async, " + name + "!";
    }
}

/// <summary>
/// Step 5: non-generic Task return (void async) — mapped to null → empty string.
/// LOAD('ReflectLibrary.dll', 'ReflectFunction.AsyncVoidWorker') → ASYNCVOID(s).
/// </summary>
public class AsyncVoidWorker
{
    // Stores result in a field so the test can verify execution occurred
    public static string LastSeen = "";

    public static async Task AsyncVoid(string input)
    {
        await Task.Yield();
        LastSeen = "saw:" + input;
    }
}
