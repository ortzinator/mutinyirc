namespace MutinyIRC.PluginFramework
{
    using System;

    /// <summary>
    ///   Marks an <c>Execute</c> overload that receives the command's entire argument tail as a
    ///   single un-coerced <see cref="string"/>, instead of the dispatcher splitting it into tokens
    ///   and promoting them to <see cref="MutinyIRC.Common.ChannelInfo"/> / <see cref="char"/>[].
    /// </summary>
    /// <remarks>
    ///   Use this for passthrough commands where a leading <c>-</c> or <c>#</c> token is data, not a
    ///   switch or a channel to promote (e.g. <c>/mode</c>, <c>/quote</c>). The decorated overload
    ///   must have the shape <c>Execute(TContext context, string rest)</c>, where <c>rest</c> is the
    ///   space-joined tail. Do not declare both a raw and a non-raw overload at the same arity on a
    ///   single command — the most-specific-first dispatch does not order them deterministically.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class RawArgumentsAttribute : Attribute
    {
    }
}
