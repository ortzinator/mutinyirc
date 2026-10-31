using System;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MutinyIRC.Common;

namespace MutinyIRC.UI.ViewModels.Prototype;

/// <summary>
/// PROTOTYPE, throw away. Answers "Edit and move UX for an entry point row" (wayfinder ticket #22).
/// Three variants of edit and move in the Networks dialog, switched by the floating bar or by the
/// MUTINY_PROTOTYPE_VARIANT environment variable (A, B or C):
///   A  Inline row: the row turns into fields; a "⋯" menu holds Edit and Move to.
///   B  Flyout: a pencil opens a small form off the row, with a network picker for a move.
///   C  Edit sheet: the row opens a full detail page in the right column.
/// The host-clash refusal is a stub that only shows where a refusal appears; the real rule is
/// the clash ticket's decision. Edits change the dialog's copy only. Cancel discards them.
/// </summary>
public sealed partial class EntryPointEditPrototype : ObservableObject
{
    private static readonly string[] Variants = { "A", "B", "C" };
    private static readonly string[] Names = { "Inline row", "Flyout editor", "Edit sheet" };

    // After the arrays: the constructor reads them.
    public static EntryPointEditPrototype Instance { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsA), nameof(IsB), nameof(IsC), nameof(VariantLabel))]
    private int _index;

    [ObservableProperty] private string _lastAction = "No edits yet";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSheetOpen))]
    private ServerSettingsViewModel? _sheetRow;

    public SettingsViewModel? Settings { get; private set; }

#if DEBUG
    public static bool IsEnabled => true;
#else
    public static bool IsEnabled => false;
#endif

    private EntryPointEditPrototype()
    {
        int start = Array.IndexOf(Variants, Environment.GetEnvironmentVariable("MUTINY_PROTOTYPE_VARIANT")?.ToUpperInvariant());
        _index = start < 0 ? 0 : start;
    }

    public bool IsA => Index == 0;
    public bool IsB => Index == 1;
    public bool IsC => Index == 2;
    public bool IsSheetOpen => SheetRow is not null;
    public string VariantLabel => $"{Variants[Index]} · {Names[Index]}";

    public void Attach(SettingsViewModel settings)
    {
        Settings = settings;
        settings.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(SettingsViewModel.SelectedNetwork))
                CancelAll();
        };
    }

    [RelayCommand]
    private void Next() => Switch(1);

    [RelayCommand]
    private void Previous() => Switch(-1);

    private void Switch(int delta)
    {
        CancelAll();
        Index = (Index + delta + Variants.Length) % Variants.Length;
    }

    private void CancelAll()
    {
        SheetRow = null;
        if (Settings is null) return;
        foreach (var row in Settings.Networks.SelectMany(n => n.Servers))
            row.CancelDraft();
    }

    public NetworkSettingsViewModel? OwnerOf(ServerSettingsViewModel row)
        => Settings?.Networks.FirstOrDefault(n => n.Servers.Contains(row));

    public void OpenSheet(ServerSettingsViewModel row)
    {
        row.BeginDraft();
        SheetRow = row;
    }

    public void CloseSheet() => SheetRow = null;

    /// <summary>
    /// Checks the draft and applies it. Returns false and sets the row's refusal when the draft
    /// cannot be applied. A change of network in the draft is a move.
    /// </summary>
    public bool Apply(ServerSettingsViewModel row)
    {
        string host = row.DraftHost.Trim();
        if (host.Length == 0)
            return Refuse(row, "Type a host name.");

        int port;
        if (string.IsNullOrWhiteSpace(row.DraftPort))
            port = row.DraftSsl ? IrcPorts.DefaultTlsPort : IrcPorts.DefaultPlainPort;
        else if (!int.TryParse(row.DraftPort.Trim(), out port) || !IrcPorts.IsValidPort(port))
            return Refuse(row, "A port is a number from 1 to 65535.");

        // STUB: the real clash rule is the clash ticket's decision.
        var clash = Settings?.Networks
            .SelectMany(n => n.Servers.Select(s => (Network: n, Row: s)))
            .FirstOrDefault(x => x.Row != row && string.Equals(x.Row.Url, host, StringComparison.OrdinalIgnoreCase));
        if (clash?.Row is not null)
            return Refuse(row, $"{host} is already listed under {clash.Value.Network.Name}.");

        var from = OwnerOf(row);
        string before = $"{row.Url}:{row.Port} {(row.Ssl ? "TLS" : "plain")}";
        row.Url = host;
        row.Port = port;
        row.Ssl = row.DraftSsl;
        string after = $"{row.Url}:{row.Port} {(row.Ssl ? "TLS" : "plain")}";

        string action = before == after ? "No change" : $"Edited {before} → {after}";
        if (row.DraftNetwork is not null && from is not null && row.DraftNetwork != from)
        {
            Move(row, from, row.DraftNetwork);
            action = (before == after ? "" : action + "; ") + $"moved to {row.DraftNetwork.Name}";
        }

        LastAction = action + (row.IsPinned ? " (pin kept)" : "");
        row.CancelDraft();
        return true;
    }

    public void MoveTo(ServerSettingsViewModel row, NetworkSettingsViewModel target)
    {
        var from = OwnerOf(row);
        if (from is null || from == target) return;
        Move(row, from, target);
        LastAction = $"Moved {row.Url} from {from.Name} to {target.Name}" + (row.IsPinned ? " (pin kept)" : "");
    }

    private static void Move(ServerSettingsViewModel row, NetworkSettingsViewModel from, NetworkSettingsViewModel to)
    {
        from.Servers.Remove(row);
        to.Servers.Add(row);
        from.RenumberServers();
        to.RenumberServers();
    }

    private static bool Refuse(ServerSettingsViewModel row, string message)
    {
        row.Refusal = message;
        return false;
    }
}
