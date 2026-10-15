using System;

namespace FlamingIRC;

/// <summary>
/// The data for <see cref="Listener.OnUserModeChange"/>. A mode of this client's user changed.
/// </summary>
public sealed class UserModeChangeEventArgs : EventArgs
{
    /// <summary>
    /// Tells if the change adds or removes the mode.
    /// </summary>
    public ModeAction Action { get; }

    /// <summary>
    /// The mode that changed.
    /// </summary>
    public UserMode Mode { get; }

    public UserModeChangeEventArgs(ModeAction action, UserMode mode)
    {
        Action = action;
        Mode = mode;
    }
}
