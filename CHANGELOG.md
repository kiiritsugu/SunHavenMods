# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

### Changed
- Document in `UnifiedTotems.csproj` why Krafs.Publicizer and `<Publicize>` are used for game DLL references.
- Add `.gitignore` for build artifacts (`bin/`, `obj/`), IDE files, and local `private.targets`.
- Remove tracked `bin/` and `obj/` build output (contained local Windows user paths).
- Add inline comments in `plugins.cs` and `ItemHandler.cs` describing how to fix item setup (Sprinklers pattern), ID mismatch, and unnecessary patches.

### Fixed
- Add `Krafs.Publicizer` NuGet package so `<Publicize>` entries and `Publicize="true"` references actually work at compile time.
- Publicize `PSS.Database` in addition to `SunHaven.Core`, allowing access to internal members like `Database.ids`.

### Removed
- Incomplete `Database.Instance.ids` debug line in `plugins.cs`.
