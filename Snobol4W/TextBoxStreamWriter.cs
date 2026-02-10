namespace Snobol4W;

internal class TextBoxStreamWriter : StringWriter
{
    private readonly RichTextBox _textBoxOutput;
    internal StreamWriter Writer;
    internal MemoryStream Mem;

    internal TextBoxStreamWriter(RichTextBox output)
    {
        _textBoxOutput = output;
        Mem = new();
        Writer = new(Mem) { AutoFlush = true };
    }

    public override void Write(char value)
    {
        base.Write(value);
        _textBoxOutput.AppendText(value.ToString());
        Writer.Write(value);
    }

    public override void Write(string? value)
    {
        base.Write(value);
        _textBoxOutput.AppendText(value);
        Writer.Write(value);
    }

    public override void WriteLine(string? value)
    {
        base.Write(value);
        _textBoxOutput.AppendText(value + Environment.NewLine);
        Writer.Write(value);
    }
}
