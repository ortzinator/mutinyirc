using System;
using System.Collections;

namespace FlamingIRC;

/// <summary>
/// Encapsulates the collection of properties sent by the IRC server
/// after registration.
/// </summary>
/// <remarks>See the server_properties.pdf file for a list of comon properties.</remarks>
/// <example><code>
/// //A Connection always has one. It is empty until the server sends a '005' reply,
/// //which it does right after registration, and a server need not send one at all.
/// //Instances are only retrieved from a Connection and not instantiated directly.
/// ServerProperties properties = connection.ServerProperties;
/// //An absent property reads as an empty string, so no null test is needed.
/// Console.Writeline("NICKLEN is" + properties["NICKLEN"] );
/// //Only a handful of properties will ever be available.
/// </code></example>
public sealed class ServerProperties
{
    // The server sends every token uppercase, but callers ask for them in the case they read
    // best ("Network"). Comparison is ordinal because these are protocol tokens, not prose.
    private readonly Hashtable properties;

    /// <summary>
    /// Instances should only be created by the Connection class.
    /// </summary>
    internal ServerProperties() => properties = new Hashtable(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Read-only indexer for the various server
    /// property strings.
    /// </summary>
    /// <returns>The string sent by the server or <see cref="string.Empty"/> if not present..</returns>
    public string this[string key]
    {
        get
        {
            if (properties[key] != null)
            {
                return (string)properties[key];
            }
            else
            {
                return string.Empty;
            }
        }
    }

    /// <summary>
    /// Add a property retrieved from the IRC server. A server may split its properties over
    /// several '005' replies and repeat a token, so a later value replaces an earlier one.
    /// </summary>
    internal void SetProperty(string key, string propertyValue) => properties[key] = propertyValue;

    /// <summary>
    /// Get a read-only enumeration of all the elements
    /// in this object.
    /// </summary>
    /// <returns>An IDictionaryEnumerator type enumeration.</returns>
    /// <example><code>
    /// //To loop over all the values
    /// foreach( DictionaryEntry entry in connection.ServerProperties )
    /// {
    /// Console.WriteLine("Key:" + entry.Key + " Value:" + entry.Value );
    /// }
    /// </code></example>
    public IDictionaryEnumerator GetEnumerator() => properties.GetEnumerator();
    /// <summary>
    /// Test if this instance contains a given key.
    /// </summary>
    /// <param name="key">The server properties key to test.</param>
    /// <returns>True if it is present.</returns>
    public bool ContainsKey(string key) => properties[key] != null;

}
