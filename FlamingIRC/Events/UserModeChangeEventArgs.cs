namespace FlamingIRC
{
    using System;

    public class UserModeChangeEventArgs : EventArgs
    {
        /// <summary>
        /// Whether a mode was added or removed.
        /// </summary>
        public ModeAction Action;

        /// <summary>
        /// The mode that was changed.
        /// </summary>
        public UserMode Mode;

        /// <summary>
        /// This user's mode has changed.
        /// </summary>
        /// <param name="action">Whether a mode was added or removed.</param>
        /// <param name="mode">The mode that was changed.</param>
        /// <seealso cref="Listener.OnUserModeChange"/>
        public UserModeChangeEventArgs(ModeAction action, UserMode mode)
        {
            Action = action;
            Mode = mode;
        }
    }
}
