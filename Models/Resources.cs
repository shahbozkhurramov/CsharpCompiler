namespace CsharpCompiler.Models;

internal sealed class Resources
{
    public Dictionary<string, string> assembly { get; set; }
    public Dictionary<string, string> pdb { get; set; }
    public Dictionary<string, string> runtime { get; set; }
}