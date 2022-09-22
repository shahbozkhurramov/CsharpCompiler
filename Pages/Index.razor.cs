using System.Diagnostics;
using System.Reflection;
using CsharpCompiler.Codes;
using Microsoft.AspNetCore.Components;
using CsharpCompiler.Editors;

namespace CsharpCompiler.Pages;

public partial class Index
{
    public string Output = string.Empty;
    internal string Code = DefaultCode.HelloWorldCode;

    [Inject] 
    private HttpClient Client { get; set; }
    [Inject] 
    private Monaco Monaco { get; set; }

    protected override Task OnInitializedAsync()
    {
        Compiler.InitializeMetadataReferences(Client);
        return base.OnInitializedAsync();
    }

    protected override void OnAfterRender(bool firstRender)
    {
        base.OnAfterRender(firstRender);
        if (firstRender)
        {
            Monaco.Initialize("container", Code, "csharp");
            Run();
        }
    }

    public Task Run()
    {
        return Compiler.WhenReady(RunInternal);
    }

    async Task RunInternal()
    {
        Output = string.Empty;

        var sw = Stopwatch.StartNew();
        // var currentIn = Console.In;

        var currentOut = Console.Out;

        using var writer = new StringWriter();
        Console.SetOut(writer);

        Exception exception = null;
        try
        {
            var (success, asm) = Compiler.LoadSource(Monaco.GetCode("container"));
            if (success)
            {
                var entry = asm.EntryPoint;
                if (entry.Name == "<Main>") // sync wrapper over async Task Main
                    entry = entry.DeclaringType.GetMethod("Main", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static); // reflect for the async Task Main
                
                var hasArgs = entry.GetParameters().Length > 0;
                var result = entry.Invoke(null, hasArgs ? new object[] { new string[0] } : null);
                if (result is Task task)
                    await task;
            }
        }
        catch (Exception ex)
        {
            exception = ex;
        }

        Output += writer.ToString();

        if (exception != null)
            Output += "\r\n" + exception.ToString();

        Console.SetOut(currentOut);

        sw.Stop();
        Output +="\nDone in " + sw.ElapsedMilliseconds + " ms...";
    }

    void SetCode(string code)
    {
        Code = code;
        Monaco.SetCode("container", code);
    }

    void ClearCode()
    {
        SetCode(string.Empty);
    }
}