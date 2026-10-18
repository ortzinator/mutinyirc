using System.Collections.Generic;

namespace MutinyIRC.PluginFramework.Tests.Fixtures;

/// <summary>
/// Declares <c>void</c> overloads of two arities, as most built-in commands do. Each call
/// records which overload ran, so a test can show that one command runs one overload.
/// </summary>
[Plugin("void")]
public class VoidCommand : ICommand
{
    public static List<string> Calls { get; } = new List<string>();

    public void Execute(TestMessageContext _, string a)
    {
        Calls.Add("one:" + a);
    }

    public void Execute(TestMessageContext _, string a, string b)
    {
        Calls.Add("two:" + a + "|" + b);
    }
}
