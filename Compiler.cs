using CsharpCompiler.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System.Collections.Immutable;
using System.Net.Http.Json;
using System.Reflection;

namespace CsharpCompiler;

public static class Compiler
{
    private static Task InitializationTask;
    private static List<MetadataReference> References;

    public static void InitializeMetadataReferences(HttpClient client)
    {
        async Task InitializeInternal()
        {
            
            var response = await client.GetFromJsonAsync<BlazorBoot>("_framework/blazor.boot.json");
            var assemblies = await Task.WhenAll(response.resources.assembly.Keys.Select(x => client.GetAsync( "_framework/" + x)));

            var references = new List<MetadataReference>(assemblies.Length);
            foreach (var asm in assemblies)
            {
                using (var task = await asm.Content.ReadAsStreamAsync())
                {
                    references.Add(MetadataReference.CreateFromStream(task));
                }
            }

            References = references;
        }
        InitializationTask = InitializeInternal();
    }

    public static Task WhenReady(Func<Task> action)
    {
        if (InitializationTask.Status != TaskStatus.RanToCompletion)
        {
            return InitializationTask.ContinueWith(x => action());
        }
        else
        {
            return action();
        }
    }

    public static (bool success, Assembly asm) LoadSource(string source)
    {
        var compilation = CSharpCompilation.Create("DynamicCode")
            .WithOptions(new CSharpCompilationOptions(OutputKind.ConsoleApplication))
            .AddReferences(References)
            .AddSyntaxTrees(CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.Preview)));

        ImmutableArray<Diagnostic> diagnostics = compilation.GetDiagnostics();

        bool error = false;
        foreach (Diagnostic diag in diagnostics)
        {
            switch (diag.Severity)
            {
                case DiagnosticSeverity.Info:
                    Console.WriteLine(diag.ToString());
                    break;
                case DiagnosticSeverity.Warning:
                    Console.WriteLine(diag.ToString());
                    break;
                case DiagnosticSeverity.Error:
                    error = true;
                    Console.WriteLine(diag.ToString());
                    break;
            }
        }

        if (error)
            return (false, null);

        using (var outputAssembly = new MemoryStream())
        {
            compilation.Emit(outputAssembly);

            return (true, Assembly.Load(outputAssembly.ToArray()));
        }
    }
}