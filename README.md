# Panopticon Audit History Search

An XrmToolBox plugin for searching Dataverse audit history across many tables at once.

> **0.1.0.1 - alpha.** Feature-complete against the v1 scope and verified for build, packaging, plugin load and cache/search behaviour, but not yet exercised end to end against a live Dataverse org. Expect rough edges, and treat the cache format as unstable.

Dataverse makes audit data hard to interrogate. The Audit Summary view only sorts by Changed Date, its Record filter does not work, and Microsoft does not support exporting audit logs at all - the SDK is the only way out. Answering "who set this field to X, and when?" across a table normally means hand-writing FetchXML and a detail-fetch loop.

Panopticon syncs audit metadata into a local SQLite cache, then searches it instantly, pulling old and new values on demand.

## Features

- **Multi-table search** - select any number of audit-enabled tables and get one merged result grid.
- **Filter by changed field** - "show me every change to `statecode`" resolved locally, without a single value-fetch call.
- **Preflight cost estimate** - see row count, disk size and expected duration before syncing, with automatic sampling when the range is too large to count exactly.
- **Record timeline** - double-click any row for that record's full change history.
- **CSV export** - metadata-only, or with old/new values behind a cost gate. Opens straight into Excel.
- **Resumable sync** - cancel any time; completed monthly windows are kept and skipped on the next run.
- **Service protection aware** - honours `Retry-After` on 429 and backs off rather than failing.

## How it works

Dataverse exposes audit data through two very different doors, and the split drives the whole design:

| | Audit table query | `RetrieveAuditDetails` |
|---|---|---|
| Returns | who, when, which table, which record, which fields changed | old and new values |
| Cost | 5,000 rows per request | one request **per audit row** |
| Practical throughput | ~1-5 min per million rows | ~1-2k rows per minute |

So Panopticon caches the first tier and never bulk-caches the second. The `attributemask` column - a CSV of `AttributeMetadata.ColumnNumber` - is exploded into an indexed table at sync time, which is what makes field-level filtering an index seek instead of a table scan.

### Cache

One SQLite database per environment, under:

```
%APPDATA%\MscrmTools\XrmToolBox\Panopticon\<organization>.db
```

Roughly 400 bytes per audit row, so ~400 MB per million rows. Its size is shown in the toolbar and **Purge cache** deletes it.

Panopticon defaults to **the last 30 days** and refuses an open-ended range. Widening past 90 days prompts you to estimate first; past 365 days requires explicit confirmation.

## Requirements

- Windows, [XrmToolBox](https://www.xrmtoolbox.com/), .NET Framework 4.8
- Dataverse privileges: **View Audit Summary** (`prvReadAuditSummary`) and **View Audit History** (`prvReadRecordAuditHistory`). Panopticon probes for both on connect and tells you which one is missing.

No native SQLite binary is deployed - the plugin binds to `winsqlite3.dll`, which ships with Windows.

## Installation

Not published to the XrmToolBox Tool Library yet, so build and deploy it yourself:

```bash
git clone https://github.com/HurleySk/PanopticonAuditHistorySearch.git
cd PanopticonAuditHistorySearch
dotnet build --configuration Release
.\deploy.ps1 -Force
```

`deploy.ps1` closes XrmToolBox, builds, copies the plugin to `%APPDATA%\MscrmTools\XrmToolBox\Plugins`, clears the manifest cache so the tool is rescanned, and relaunches.

## Usage

1. Connect to an environment. Panopticon checks audit access and loads the audit-enabled tables.
2. Check the tables you care about. The range starts at the last 30 days.
3. **Estimate cost** if you widened the range, then **Sync audit data**.
4. Filter by user, event, operation, record name, or changed field, and hit **Search cache**.
5. Select a row to load its old and new values; double-click for the record's full timeline.
6. **Export CSV** when you have what you need.

Re-syncing the same scope is cheap - completed monthly windows are skipped. Tick **Force refresh** to re-pull them.

## Known limits

These are Dataverse constraints, not plugin bugs:

- Audit data is **not available through the TDS/SQL endpoint**, so SQL 4 CDS cannot query it directly.
- The audit table joins only to `systemuser`, so record and table names are resolved client-side, lazily, for visible rows.
- Values over 5 KB are truncated by the platform. Panopticon flags truncated values rather than presenting them as complete.
- Exact row counts fail above the 50,000 aggregate limit; the estimate falls back to sampling and says so.
- `attributemask` is documented as internal. Panopticon validates the mapping against a real audit row on connect and disables field filtering with a clear message if it does not line up.
- Results are capped at 250,000 rows in the grid. The full match count is still reported.

## Packaging

```bash
dotnet pack --configuration Release
```

The main DLL packs to `Plugins/`; SQLite dependencies pack to `Plugins/Dependencies/`. Both the NuGet package and `deploy.ps1` must keep dependencies in that subfolder: XrmToolBox treats every DLL at the root of `Plugins/` as a candidate tool, so a dependency left there is reported under "Tools not loaded" at startup, and the Tool Store validator separately rejects it for not matching the package version.

XrmToolBox applies no binding redirects to plugin assemblies, so `SQLitePCLRaw.bundle_winsqlite3` is pinned to the exact version `Microsoft.Data.Sqlite` references (2.1.6.2060 for 8.0.10). A newer bundle builds and passes tests - the test host generates redirects - then fails at runtime inside XrmToolBox with a `FileNotFoundException` on `SQLitePCLRaw.core`. Bump both together or not at all.

## License

MIT - see [LICENSE](LICENSE).

## Author

**Samuel Hurley** - [HurleySk](https://github.com/HurleySk)

Built on [XrmToolBox](https://www.xrmtoolbox.com/) by Tanguy Touzard.
