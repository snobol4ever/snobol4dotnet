using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;
using System.Reflection;
using System.Runtime;
using System.Runtime.Loader;
using System.Text;

namespace Snobol4.Common;

/// <summary>
/// https://stackoverflow.com/questions/14479074/c-sharp-reflection-load-assembly-and-invoke-a-method-if-it-exists
/// https://github.com/munibrbutt/articles-code/blob/main/Dynamically%20loading%20and%20running%20CSharp%20code/ConsoleAppReadCode/Program.cs
/// https://stackoverflow.com/questions/50649795/how-to-debug-dll-generated-from-roslyn-compilation
/// </summary>
public partial class Builder
{
    #region Members

    private static readonly MetadataReference[] _references =
    [
        MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
        MetadataReference.CreateFromFile(typeof(AssemblyTargetedPatchBandAttribute).Assembly.Location),
        MetadataReference.CreateFromFile(Assembly.Load("System.Runtime").Location),
        MetadataReference.CreateFromFile(Assembly.Load("System.Collections").Location),
        MetadataReference.CreateFromFile(Assembly.Load("Snobol4.Common").Location)
    ];

    #endregion

    #region public Methods

    public Assembly Compile(AssemblyLoadContext loadContext, string filePath, string code)
    {
        return CreateAssembly(loadContext, code, filePath);
    }

    #endregion

    #region Private Methods

    private Assembly CreateAssembly(AssemblyLoadContext loadContext, string code, string filePath)
    {
        var encoding = Encoding.UTF8;
        var fileName = Path.GetFileName(filePath);
        var assemblyName = Path.ChangeExtension(fileName, "dll");
        var symbolsName = Path.ChangeExtension(fileName, "pdb");
        var buffer = encoding.GetBytes(code);
        var sourceText = SourceText.From(buffer, buffer.Length, encoding, canBeEmbedded: true);
        var syntaxTree = CSharpSyntaxTree.ParseText(sourceText, new CSharpParseOptions(), path: fileName);
        var syntaxRootNode = (CSharpSyntaxNode)syntaxTree.GetRoot();
        Debug.Assert(syntaxRootNode != null, nameof(syntaxRootNode) + " != null");
        var encoded = CSharpSyntaxTree.Create(syntaxRootNode, null, fileName, encoding);
        var optimizationLevel = GenerateDebugSymbols ? OptimizationLevel.Debug : OptimizationLevel.Release;
        var compilation = CSharpCompilation.Create(
            assemblyName,
            syntaxTrees: [encoded],
            references: _references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
                .WithOptimizationLevel(optimizationLevel)
                .WithPlatform(Platform.AnyCpu));

        using var assemblyStream = new MemoryStream();
        using var symbolsStream = new MemoryStream();

        var embeddedTexts = new List<EmbeddedText>
        {
            EmbeddedText.FromSource(fileName, sourceText)
        };

        var emitOptions = new EmitOptions(
            debugInformationFormat: DebugInformationFormat.PortablePdb,
            pdbFilePath: symbolsName);
        var result = compilation.Emit(
            peStream: assemblyStream,
            pdbStream: symbolsStream,
            embeddedTexts: embeddedTexts,
            options: emitOptions);

        if (result.Success)
        {
            if (WriteDll)
            {
                using Stream fileStream = File.Open(assemblyName, FileMode.Create);
                assemblyStream.Position = 0;
                assemblyStream.CopyTo(fileStream);
            }
            assemblyStream.Position = 0;
            symbolsStream.Position = 0;
            var dll = loadContext.LoadFromStream(assemblyStream, symbolsStream);
            return dll;
        }

        var failures = result.Diagnostics.Where(diagnostic =>
            diagnostic.IsWarningAsError ||
            diagnostic.Severity == DiagnosticSeverity.Error);

        var errors = failures.Aggregate("", (current, error) => current + (error + Environment.NewLine));
        throw new ApplicationException(errors);
    }

    #endregion
}