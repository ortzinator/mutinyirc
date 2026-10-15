using System;
using System.Collections.Generic;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnUserModeRequest"/>.
/// </summary>
public sealed class UserModeRequestEventArgs : EventArgs
{
    /// <summary>
    /// All the modes that are set on your user.
    /// </summary>
    public IReadOnlyList<UserMode> Modes { get; }

    public UserModeRequestEventArgs(IEnumerable<UserMode> modes)
        => Modes = new List<UserMode>(modes).AsReadOnly();
}
