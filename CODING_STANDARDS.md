# Coding Standards

This document describes the coding conventions of MutinyIRC. It comes from the existing
`.editorconfig` and from the conventions in the source code. Where the two disagree, or where the
codebase disagrees with itself, [Known Contradictions](#known-contradictions) records the conflict.
Those items are open questions, not settled rules.

`.editorconfig` decides anything a tool can check. This document explains the intent behind those
settings and records the conventions that `.editorconfig` cannot express.
[Known Contradictions](#known-contradictions) covers which settings the build enforces, and how.

## Language and Tooling

- **Target framework:** `net8.0` in every project.
- **Language:** C# (the default language version for .NET 8).
- **Build check:** run `dotnet build MutinyIRC.sln` after you change code. `Directory.Build.props`
  sets `EnforceCodeStyleInBuild`, so the `IDE####` severities in `.editorconfig` apply to the
  build. Style diagnostics are **warnings**. A clean build has 0 errors.
- **Unused code**, enforced at build time:
  - `CS8019` — unnecessary `using` directives (**error**).
  - `CA1823` — unused private fields (**error**).
  - `IDE0051` / `IDE0059` / `IDE0060` — uncalled private code, unused locals, unused parameters
    (warnings).

Remove dead usings, parameters, locals, and private members as you write. Do not leave them for
later. The codebase already carries a backlog of style warnings (see
[Known Contradictions](#known-contradictions)). Do not add to it.

## Files and Encoding

- **Line endings:** use LF for source and text files. `.gitattributes` and `end_of_line = lf` in
  `.editorconfig` enforce this. The `.editorconfig` setting also stops `dotnet format` from
  rewriting files with CRLF on Windows. Project and solution files (`*.csproj`, `*.sln`) use CRLF
  to match Visual Studio. Do not commit files with mixed or platform-native endings.
- **Final newline:** every `.cs` file ends with a newline (`insert_final_newline = true`).
- **One-time setup:** run `git config blame.ignoreRevsFile .git-blame-ignore-revs`. This keeps the
  line-ending renormalization commit out of `git blame` (see `CONTRIBUTING.md`).

## Layout and Formatting

These rules follow the `.editorconfig` (the Microsoft and ReSharper sections):

- **Put braces on a new line** for all constructs (`csharp_new_line_before_open_brace = all`).
- **Modifier order:** `private, protected, internal, async, file, public, override, sealed,
  virtual, static, abstract, readonly, extern, unsafe, volatile, new, required`.
- **Maximum line length: 100** (`resharper_csharp_max_line_length = 100`). This is a ReSharper
  hint. The build does not enforce it.
- **The formatter keeps single-line blocks and expression-bodied members on one line** when they
  already fit (`csharp_preserve_single_line_blocks = true`,
  `resharper_place_expr_property_on_single_line = true`).
- **Use an expression body when the member fits on one line.** Use a block body when the member
  wraps (`csharp_style_expression_bodied_* = when_on_single_line`):

  ```csharp
  public CommandResultInfo Execute() => CommandResultInfo.Success("no-context");
  ```

  This is a preference for new code. The build does **not** enforce it, because the related
  diagnostics (`IDE0021`/`IDE0022`/`IDE0025`/`IDE0027`/`IDE0061`) are silent. The codebase uses
  both styles heavily: about 71 expression-bodied methods against about 229 single-statement
  block-bodied ones. Enforcement in either direction would rewrite hundreds of methods and change
  no behavior. Do not reformat existing members to match.
- **Wrap object and collection initializers one item per line**
  (`resharper_wrap_object_and_collection_initializer_style = chop_always`).
- **Use no trailing comma** in multi-line lists (`resharper_trailing_comma_in_multiline_lists =
  false`).
- **Remove redundant parentheses** (`resharper_parentheses_redundancy_style = remove`). Keep them
  around arithmetic and other binary operators, where explicit grouping helps the reader.

### `var` vs explicit types

The `.editorconfig` prefers **explicit types**:

- `csharp_style_var_for_built_in_types = false` — use `int`, `string`, and the other built-ins, not
  `var`.
- `csharp_style_var_when_type_is_apparent = true` — `var` is acceptable when the right-hand side
  shows the type, for example a constructor call.
- `resharper_object_creation_when_type_evident = explicitly_typed`.

`IDE0008` ("use explicit type instead of var") is **silent**, so this is a preference. Prefer
explicit types. Use `var` only when the type is obvious.

### Braces on control flow

`IDE0011` ("add braces") is **silent**, and ReSharper sets `braces_for_if = not_required`. The
codebase often omits braces on single-statement `if` and `foreach` bodies:

```csharp
if (UserList == null || UserList.Count == 0)
    return base.Name;
```

This is allowed. Keep the body on its own line and indent it. Do not put the statement on the same
line as the `if`.

### Empty strings

Prefer `string.Empty` over `""` (`resharper_empty_string = string_empty`). The source follows this
consistently: fields initialize to `string.Empty`.

## Naming

- **Types, methods, properties, events:** PascalCase.
- **Parameters and locals:** camelCase.
- **Private instance fields:** `_camelCase` with a leading underscore (for example `_channel`,
  `_connection`, `_reconnectTimer`).
- **Private static and const fields:** PascalCase (for example `ReconnectBaseDelay`,
  `ReconnectMaxDelay`), so the underscore rule does not flag constant-like fields.
- **Interfaces:** an `I` prefix (`IConnection`, `IPlugin`, `ICommand`, `ISender`).
- **Event handler methods:** `Source_EventName` (for example `Channel_OnMessage`,
  `Server_Disconnected`).

The `dotnet_naming_rule` entries in `.editorconfig` enforce field naming (diagnostic `IDE1006`) at
**warning** severity. The legacy FlamingIRC socket subsystem is the one exception — see
[Known Contradictions](#known-contradictions).

## Strings

Use **string interpolation**, not `string.Format`:

```csharp
return $"{base.Name} ({UserList.Count})";
```

Use `string.Format` when the format string is not an inline literal, such as a stored constant or a
runtime parameter. There is nothing to interpolate in those cases:

```csharp
public static string MakeColor(string text, MircColor textColor)
{
    return string.Format(TextColorFormat, (int)textColor, text);
}
```

## Immutability

Mark a private field `readonly` (or `static readonly`) when only its declaration or a constructor
assigns it. `IDE0044` is a warning and flags the fields you miss.

## Namespaces and Usings

- **Use file-scoped namespaces** (`csharp_style_namespace_declarations = file_scoped`):
  `namespace X;`, not `namespace X { ... }`. The Style analyzer category enforces this.
- **Put `using` directives outside the namespace** (`csharp_using_directive_placement =
  outside_namespace`) — above the namespace declaration, never inside it.

## Nullable Reference Types

Only `MutinyIRC.UI` sets `<Nullable>enable</Nullable>`. The other projects do not opt in.

- In UI code, annotate reference types (`Server?`, `RelayCommand?`) and handle the null case
  correctly. To resolve a nullable warning, fix the actual null path. Do not mask it with a
  fallback value or with a `!` that hides a real bug. Use `null!` initialization only for fields
  that something assigns before use, such as a DI-injected field or a field set in a lifecycle
  hook. This prevents cascading warnings.
- In the other projects, nullable annotations are not necessary and are usually absent.

## Documentation Comments

- XML doc comments (`/// <summary>`) document the public FlamingIRC and PluginFramework API. Many
  also use `<remarks>`, `<example>`, and `<see cref="..."/>` cross-references.
- MutinyIRC.Common uses XML docs for public members that are not obvious.
- **Put exactly one space after `///`.** Do not indent the prose. Indent nested XML by one level
  (2 spaces) for each level of nesting. Never use tabs:

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
- Documentation voice: explain the *why* before the *how*. Use second person. Keep paragraphs
  tight. Pair each concept with a short, concrete example.

## Architecture Conventions

These rules hold the design together. Do not work around them:

- **Layering:** `FlamingIRC` (protocol) → `MutinyIRC.Common` (domain) → `MutinyIRC.UI` (MVVM).
  Logic shared beyond the UI belongs in `MutinyIRC.Common`, never in `MutinyIRC.UI`.
- **Event-driven flow:** IRC events travel from `Listener` to `Server`/`Channel` to the ViewModels
  through C# events. There is no message bus.
- **FlamingIRC is a public framework.** Do not delete an unused public member only because nothing
  in this repository calls it. Remove a public member only when it has no plausible use case.
- **MVVM:** the UI uses `CommunityToolkit.Mvvm` (`ObservableObject`, `SetProperty`, `RelayCommand`)
  and Ninject for DI. `CompositionRoot` wires both.
- **Plugins and commands:** reflection finds them in the `plugins/` folder through the `[Plugin]`
  attribute. Nothing registers them statically.

## Testing

- **Framework:** NUnit 4 with FakeItEasy for mocking.
- UI tests use `Avalonia.Headless.NUnit`. See the `avalonia-headless-test` skill for the patterns.
- Each test project mirrors the layer it covers: `FlamingIRC.Tests`, `MutinyIRC.Tests`,
  `MutinyIRC.UI.Tests`, `MutinyIRC.PluginFramework.Tests`.
- `InternalsVisibleTo` exposes the internals of each project to its matching test project (for
  example FlamingIRC → FlamingIRC.Tests).

## Source Control

- **Do not commit without asking first.** Suggest a commit message and let the maintainer decide.
- **Commit message prefix:** `[FlamingIRC]` or `[MutinyIRC]`, for the code that changed.
- **Use one line** per commit message, except for significant architectural changes.
- **Add no `Co-Authored-By` trailers.**

---

## Known Contradictions

In these places the `.editorconfig` and the codebase disagree, or the codebase disagrees with
itself. Each item needs a decision.

### 1. FlamingIRC legacy fields do not follow the field naming rule

About 35 private fields in the legacy FlamingIRC socket subsystem (`Dcc/`, `Ctcp/`, `Listener`,
`UserList`, `ServerProperties`) use bare `camelCase` instead of `_camelCase`. The naming rule
therefore stays at `warning` and not `error`. The subsystem is consistent with itself, has few
tests, and uses bare names that collide easily (`buffer`, `socket`, `thread`, `server`, `list`). A
text-based rename would corrupt them. **Decision needed:** rename the fields with a Roslyn rename
that handles shadowing correctly, or keep the legacy subsystem on its own convention permanently.

### 2. GPL license headers are inconsistent

The full GPLv2 header appears on **26 legacy `FlamingIRC` files** (`Sender.cs`, `Listener.cs`,
`Connection.cs`, the `Dcc/` and `Ctcp/` trees, and others). It is **absent** from the newer
FlamingIRC files (`Events/*`, `IConnection.cs`, `ThreadHelper.cs`, `Extensions.cs`) and from
**every other project** (`MutinyIRC.Common`, the UI, and the rest have no header). **Decision
needed:** add the header to all FlamingIRC files, because FlamingIRC has a separate license; remove
it everywhere; or document that only the original files carry it.

### 3. Style warning backlog

Style rules became build-enforced only recently, so the codebase carries **130 pre-existing style
warnings**. The build has 0 errors, and these warnings serve as a worklist. Most of them:

| Rule | Count | What it wants |
|---|---|---|
| `IDE1006` | 67 | Naming rule — this is item 1 above |
| `IDE0370` | 22 | Suppression is unnecessary (stale `#pragma` or attributes) |
| `IDE0060` | 11 | Unused parameter |
| `IDE0200` | 8 | Remove unnecessary lambda expression |

Fix `IDE0060` case by case. Do not fix it in one pass. Almost every current hit is a parameter the
structure needs, and the analyzer cannot see this. The plugin framework dispatches command
overloads **by the type of the first parameter**, so a `context`, `channel`, or `server` parameter
is part of the dispatch contract even when the body ignores it. Suppress or keep those parameters.
Remove a parameter only when nothing depends on it.

Promote a rule to `error` when its count reaches zero. `IDE0052` moved this way already.

`resharper_csharp_max_line_length = 100` applies only in ReSharper and Rider. The build does not
enforce it. `dotnet format` reports `IDE0001` and `IDE0002`; the build does not.