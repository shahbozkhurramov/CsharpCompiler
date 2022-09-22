namespace CsharpCompiler.Codes;

public static class DefaultCode
{
    public static string HelloWorldCode = @"using System;

Console.WriteLine(""Hello World!"");";
    
    public static string OOPCode = @"using System;

public class Program
{
    static void Main(string[] args)
    {
        var person = new Person(""Shahboz"", 20);
        Console.WriteLine($""My name is {person.Name}. I am {person.Age} years old."");
    }
}

public class Person
{
    public string Name {get;set;}
    public int Age {get;set;}

    public Person(string name, int age)
    {
        Name = name;
        Age = age;
    }
}";
}