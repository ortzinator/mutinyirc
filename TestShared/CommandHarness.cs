using System;
using System.Collections.Generic;
using System.Linq;
using MutinyIRC.Common;
using MutinyIRC.PluginFramework;

namespace MutinyIRC.TestShared;

/// <summary>
/// Registers commands with a real <see cref="PluginManager"/> and dispatches through it, so a
/// test exercises the argument coercion and overload selection, not only the command body.
/// </summary>
/// <remarks>
/// More than one test project compiles this file through a linked <c>Compile</c> item. It is the
/// only place that writes to the internal <c>PluginManager._commands</c> dictionary.
/// </remarks>
internal sealed class CommandHarness
{
    private readonly PluginManager _manager = new PluginManager();

    /// <summary>Makes a harness that holds only the command <typeparamref name="T"/>.</summary>
    public static CommandHarness For<T>(string name) where T : ICommand
        => new CommandHarness().Register<T>(name);

    /// <summary>Registers <typeparamref name="T"/> under <paramref name="name"/>.</summary>
    public CommandHarness Register<T>(string name) where T : ICommand
    {
        Type type = typeof(T);
        _manager._commands.Add(type.FullName,
            new CommandInfo(type.Assembly.Location, type.FullName, name, typeof(ICommand)));
        return this;
    }

    /// <summary>
    /// Dispatches the one registered command. Use the overload that takes a name when the
    /// harness holds more than one command.
    /// </summary>
    public CommandResultInfo Dispatch(MessageContext context, params object[] args)
    {
        if (_manager._commands.Count != 1)
            throw new InvalidOperationException(
                $"The harness holds {_manager._commands.Count} commands. Give the command name.");

        return Dispatch(_manager._commands.Values.Single().CommandName, context, args);
    }

    /// <summary>Dispatches the command that is registered under <paramref name="name"/>.</summary>
    public CommandResultInfo Dispatch(string name, MessageContext context, params object[] args)
        => Execute(new CommandExecutionInfo
        {
            Name = name,
            Context = context,
            ParameterList = new List<object>(args)
        });

    /// <summary>Executes <paramref name="info"/> as the caller built it.</summary>
    public CommandResultInfo Execute(CommandExecutionInfo info) => _manager.ExecuteCommand(info);
}
