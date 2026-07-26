# Coding Standards

This document describes the coding conventions used in MutinyIRC. It is derived from the
existing `.editorconfig` and from the conventions actually present in the source tree. Where the
two disagree, or where the codebase is internally inconsistent, that is called out explicitly in
[Known Contradictions](#known-contradictions) at the end — those are open questions to resolve,
not settled rules.

`.editorconfig` is the source of truth for anything a tool can check. This document explains the
intent behind those settings and captures the conventions that `.editorconfig` does not encode.
Which of its settings are actually enforced, and by what, is covered under
[Known Contradictions](#known-contradictions).

## Language and Tooling

- **Target framework:** `net8.0` across every project.
- **Language:** C# (default language version for .NET 8).
- **Build check:** run `dotnet build MutinyIRC.sln` after changes. `EnforceCodeStyleInBuild` is set
  in `Directory.Build.props`, so the `IDE####` severities declared in `.editorconfig` apply to the
  build. Style diagnostics are **warnings**; a clean build is 0 errors.
- **Unused code**, enforced at build time:
  - `CS8019` — unnecessary `using` directives (**error**).
  - `CA1823` — unused private fields (**error**).
  - `IDE0051` / `IDE0059` / `IDE0060` — uncalled private code, unused locals, unused parameters
    (warnings).

Remove dead usings, parameters, locals, and private members as you go rather than leaving them for
later. There is a standing backlog of pre-existing style warnings (see
[Known Contradictions](#known-contradictions)) — don't add to it.

## Files and Encoding

- **Line endings:** LF for source and text files, enforced via `.gitattributes` and
  `end_of_line = lf` in `.editorconfig` (the latter keeps `dotnet format` from rewriting files
  with CRLF on Windows). Project and solution files (`*.csproj`, `*.sln`) use CRLF per Visual
  Studio convention. Do not commit files with mixed or platform-native endings.
- **Final newline:** every `.cs` file ends with a newline (`insert_final_newline = true`).
- **One-time setup:** configure `git config blame.ignoreRevsFile .git-blame-ignore-revs` so the
  line-ending renormalization commit doesn't pollute `git blame` (see `CONTRIBUTING.md`).

## Layout and Formatting

These follow the `.editorconfig` (Microsoft + ReSharper sections):

- **Braces on a new line** for all constructs (`csharp_new_line_before_open_brace = all`).
- **Modifier order:** `private, protected, internal, async, file, public, override, sealed,
  virtual, static, abstract, readonly, extern, unsafe, volatile, new, required`.
- **Max line length: 100** (`resharper_csharp_max_line_length = 100`). Note this is a ReSharper
  hint, not a build-enforced rule.
- **Single-line blocks and expression-bodied members are preserved** on one line where they
  already fit (`csharp_preserve_single_line_blocks = true`,
  `resharper_place_expr_property_on_single_line = true`).
- **Expression bodies where the member fits on one line**, block bodies for anything that wraps
  (`csharp_style_expression_bodied_* = when_on_single_line`):

  ```csharp
  public CommandResultInfo Execute() => CommandResultInfo.Success("no-context");
  ```

  This is a preference for new code, **not** enforced — the corresponding diagnostics
  (`IDE0021`/`IDE0022`/`IDE0025`/`IDE0027`/`IDE0061`) are silenced. The tree holds both styles in
  quantity (~71 expression-bodied methods against ~229 single-statement block-bodied ones), so
  enforcing either direction would rewrite hundreds of methods for no behavioral gain. Don't
  reformat existing members to match.
- **Object and collection initializers** wrap one item per line
  (`resharper_wrap_object_and_collection_initializer_style = chop_always`).
- **No trailing comma** in multi-line lists (`resharper_trailing_comma_in_multiline_lists =
  false`).
- **Redundant parentheses are removed** (`resharper_parentheses_redundancy_style = remove`),
  except arithmetic/other binary operators, which prefer explicit grouping for clarity.

### `var` vs explicit types

The `.editorconfig` leans toward **explicit types**:

- `csharp_style_var_for_built_in_types = false` — use `int`, `string`, etc., not `var`, for
  built-ins.
- `csharp_style_var_when_type_is_apparent = true` — `var` is acceptable when the type is obvious
  from the right-hand side (e.g. a constructor call).
- `resharper_object_creation_when_type_evident = explicitly_typed`.

`IDE0008` ("use explicit type instead of var") is **silenced**, so this is a preference, not
enforced. Prefer explicit types; use `var` only when the type is plainly apparent.

### Braces on control flow

`IDE0011` ("add braces") is **silenced** and ReSharper is set to `braces_for_if = not_required`.
The codebase routinely omits braces on single-statement `if`/`foreach` bodies:

```csharp
if (UserList == null || UserList.Count == 0)
    return base.Name;
```

This is allowed. Keep the body on its own line, indented — do not place the statement on the same
line as the `if`.

### Empty strings

Prefer `string.Empty` over `""` (`resharper_empty_string = string_empty`). This is followed
consistently in the source (fields initialize to `string.Empty`).

## Naming

- **Types, methods, properties, events:** PascalCase.
- **Parameters and locals:** camelCase.
- **Private instance fields:** `_camelCase` with a leading underscore (e.g. `_channel`,
  `_connection`, `_reconnectTimer`).
- **Private static / const fields:** PascalCase (e.g. `ReconnectBaseDelay`, `ReconnectMaxDelay`),
  so constant-like fields are not flagged for the underscore convention.
- **Interfaces:** `I`-prefixed (`IConnection`, `IPlugin`, `ICommand`, `ISender`).
- **Event handler methods:** `Source_EventName` (e.g. `Channel_OnMessage`, `Server_Disconnected`).

Field naming is enforced by `dotnet_naming_rule` entries in `.editorconfig` (diagnostic
`IDE1006`) at **warning** severity. The legacy FlamingIRC socket subsystem is the one exception —
see [Known Contradictions](#known-contradictions).

## Strings

Use **string interpolation**, not `string.Format`:

```csharp
return $"{base.Name} ({UserList.Count})";
```

`string.Format` remains correct where the format string is not an inline literal — a stored
constant or a runtime parameter — because there is nothing to interpolate:

```csharp
public static string MakeColor(string text, MircColor textColor)
{
    return string.Format(TextColorFormat, (int)textColor, text);
}
```

## Immutability

Mark private fields `readonly` (or `static readonly`) whenever they are assigned only in their
declaration or a constructor. `IDE0044` is set to `warning` and will flag the ones you miss.

## Namespaces and Usings

- **File-scoped namespaces** (`csharp_style_namespace_declarations = file_scoped`): `namespace X;`,
  not `namespace X { ... }`. Enforced through the Style analyzer category.
- **`using` directives go outside the namespace** (`csharp_using_directive_placement =
  outside_namespace`) — above the namespace declaration, never inside it.

## Nullable Reference Types

`<Nullable>enable</Nullable>` is set **only in `MutinyIRC.UI`**. The other projects do not opt in.

- In UI code, annotate reference types (`Server?`, `RelayCommand?`) and handle the null case
  correctly. When resolving a nullable warning, fix the actual null path — do not mask it with a
  fallback value or a `!` that hides a real bug. Prefer `null!` initialization only for fields
  that are genuinely assigned before use (e.g. DI-injected or set in a lifecycle hook), to avoid
  cascading warnings.
- In non-UI projects, nullable annotations are not required and generally absent.

## Documentation Comments

- Public FlamingIRC and PluginFramework API surface is documented with XML doc comments
  (`/// <summary>`), often with `<remarks>`, `<example>`, and `<see cref="..."/>` cross-references.
- MutinyIRC.Common uses XML docs for non-obvious public members.
- **Exactly one space after `///`**, with no extra indentation on prose. Indent nested XML with
  spaces, one level (2 spaces) per level of nesting — never tabs:

  ```csharp
  /// <summary>
  /// A PONG message is a reply to a server PING message.
  /// </summary>
  /// <remarks>
  /// Possible Errors
  /// <list type="bullet">
  ///   <item><description>ERR_NOORIGIN</description></item>
  ///   <item><description>ERR_NOSUCHSERVER</description></item>
  /// </list>
  /// </remarks>
  ```
- Documentation voice: explain the *why* before the *how*, use second person, keep paragraphs
  tight, and pair concepts with short concrete examples.

## Architecture Conventions

These are load-bearing design rules:

- **Layering:** `FlamingIRC` (protocol) → `MutinyIRC.Common` (domain) → `MutinyIRC.UI` (MVVM).
  Logic shared beyond the UI belongs in `MutinyIRC.Common`, never in `MutinyIRC.UI`.
- **Event-driven flow:** IRC events propagate `Listener` → `Server`/`Channel` → ViewModels via C#
  events, not a message bus.
- **FlamingIRC is a public framework.** Do not delete unused public members merely because nothing
  in this repo calls them; remove a public member only when it has no plausible use case.
- **MVVM:** UI uses `CommunityToolkit.Mvvm` (`ObservableObject`, `SetProperty`, `RelayCommand`)
  and Ninject for DI, wired in `CompositionRoot`.
- **Plugins/commands** are discovered by reflection from the `plugins/` folder via the `[Plugin]`
  attribute — they are not registered statically.

## Testing

- **Framework:** NUnit 4 with FakeItEasy for mocking.
- UI tests use `Avalonia.Headless.NUnit`; see the `avalonia-headless-test` skill for patterns.
- Test projects mirror the layer they cover: `FlamingIRC.Tests`, `MutinyIRC.Tests`,
  `MutinyIRC.UI.Tests`, `MutinyIRC.PluginFramework.Tests`.
- `InternalsVisibleTo` exposes internals to the matching test project (e.g. FlamingIRC →
  FlamingIRC.Tests).

## Source Control

- **Do not commit without asking first.** Suggest a commit message and let the maintainer decide.
- **Commit message prefix:** `[FlamingIRC]` or `[MutinyIRC]` depending on which code changed.
- **One line** per commit message, except for significant architectural changes.
- **No `Co-Authored-By` trailers.**

---

## Known Contradictions

These are the places where the `.editorconfig` and the codebase disagree, or where the codebase is
inconsistent with itself. Each needs a decision.

### 1. FlamingIRC legacy fields do not follow the field naming rule

~35 private fields in the legacy FlamingIRC socket subsystem (`Dcc/`, `Ctcp/`, `Listener`,
`UserList`, `ServerProperties`) use bare `camelCase` instead of `_camelCase`. This is why the
naming rule sits at `warning` rather than `error`. The subsystem is internally consistent, has
thin test coverage, and uses collision-prone bare names (`buffer`, `socket`, `thread`, `server`,
`list`) that a text-based rename would corrupt. **Decision needed:** migrate them via a
Roslyn-aware rename (safe against shadowing), or leave the legacy subsystem on its own convention
permanently.

### 2. GPL license headers are inconsistent

The full GPLv2 header appears on **26 legacy `FlamingIRC` files** (`Sender.cs`, `Listener.cs`,
`Connection.cs`, the `Dcc/` and `Ctcp/` trees, etc.) but is **absent** from newer FlamingIRC files
(`Events/*`, `IConnection.cs`, `ThreadHelper.cs`, `Extensions.cs`) and from **every other
project** (`MutinyIRC.Common`, UI, etc. have no header). **Decision needed:** either add the
header to all FlamingIRC files (it is a separately-licensed framework), drop it entirely, or
document that only original-provenance files carry it.

### 3. Style warning backlog

Style rules became build-enforced only recently, so the tree carries **130 pre-existing style
warnings**. The build is green (0 errors) and these are visible as a worklist. The bulk:

| Rule | Count | What it wants |
|---|---|---|
| `IDE1006` | 67 | Naming rule — this is item 1 above |
| `IDE0370` | 22 | Suppression is unnecessary (stale `#pragma`/attributes) |
| `IDE0060` | 11 | Unused parameter |
| `IDE0200` | 8 | Remove unnecessary lambda expression |

`IDE0060` needs judgement rather than a sweep. Nearly all current hits are structurally required
parameters that the analyzer cannot see are load-bearing: the plugin framework dispatches command
overloads **by first-parameter type**, so a `context`/`channel`/`server` parameter is part of the
dispatch contract even when the body ignores it. Suppress or keep those; only remove a parameter
that is genuinely free.

Promote individual rules to `error` as their counts reach zero — `IDE0052` has already been
promoted this way.

Note that `resharper_csharp_max_line_length = 100` remains ReSharper/Rider-only — it is not
enforced by the build. `IDE0001` and `IDE0002` likewise report under `dotnet format` but not at
build time.