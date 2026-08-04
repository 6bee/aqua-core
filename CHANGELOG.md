# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased vNext][vnext-unreleased]

### Added

- Add `Aqua.MessagePack` project with full MessagePack serialization support (scalar codec, value formatter, aqua type formatters)
- Add `Aqua.Protobuf` project with full Protobuf serialization support (scalar codec, packed array serializer, value mapper, aqua.proto schema)
- Add `Aqua.Dynamic.TypeSafety` static helper with `TypeSafety.AllowAny` (no-op `ITypeSafetyChecker`) and `TypeSafety.AllowList(...)` (opt-in allow-list checker backed by its own type set, throwing `TypeSafetyException`) in reference to [OWASP A8:2017-Insecure Deserialization][CWE-502]
- Add `Aqua.Dynamic.TypeSafetyException` as the dedicated exception thrown by `TypeSafety.AllowList` checkers
- Add `System.Numerics.BigInteger` and `System.Numerics.Complex` to `KnownTypesRegistry` default types (keys: `bigint`, `complex`)

### Changed

- Consolidate `KnownTypesRegistry` from `Aqua.Text.Json` into root namespace `Aqua.KnownTypesRegistry`
- `Aqua.KnownTypesRegistry` generates derived type key for generic types (except `Nullable<T>`) including generic type arguments which is potentially a breaking change.
- Replace `Aqua.protobuf-net` serialization backend with new `Aqua.Protobuf` project
- Update `Aqua.Newtonsoft.Json` project: remove (replace) `KnownTypesRegistry`, update converters and contract resolvers
- Bump _System.Text.Json_ from 10.0.0 to 10.0.9 (netstandard2.0) (concerns _Aqua.Text.Json_)

### Removed

- Remove project `Aqua.protobuf-net` (fully replaced by new `Aqua.Protobuf` project)
- Remove `KnownTypesRegistry` from `Aqua.Newtonsoft.Json` (consolidated into `Aqua.KnownTypesRegistry`)
- Remove abstract types `Aqua.TypeSystem.MemberInfo` and `Aqua.TypeSystem.MethodBaseInfo` from `KnownTypesRegistry` default types (they cannot be concrete deserialization targets)
- Remove obsolete `AssemblyInfo.cs` files across projects

### Deprecated

### Fixed

### Security

## [5.5.1][5.5.1] - 2026-01-19

### Fixed

- Re-add type registrations for DateOnly, TimeOnly, Int128, UInt128

## [5.5.0][5.5.0] - 2026-01-16

### Added

- Add support for `System.Int128`, `System.UInt128`, `System.DateOnly`, `System.TimeOnly`
- Add .NET 10.0 framework target
- Add `Assembly.GetLoadedTypes()` extension method

### Changed

- Bump _System.Text.Json_ from 8.0.5 to 10.0.0 (netstandard2.0) (concerns _Aqua.Text.Json_)
- Bump _Newtonsoft.Json_ from 13.0.3 to 13.0.4 (concerns _Aqua.Newtonsoft.Json_)
- Bump _protobuf-net_ from 2.4.8 to 2.4.9 (concerns _Aqua.protobuf-net_)
- Type resolution prioritize publicly visible types in case of multiple matching candidates [#36][issue#36]
- Declare members of `DynamicObject`, `PropertySet`, and `Property` virtual
- Deprecate aqua-core-text-json and integrate code into aqua-core package

### Removed

- Removed .NET 6.0 framework target
- Removed support for partially trusted code (code access security (CAS) and security-transparent code)

### Fixed

- Resolve overloaded methods with generic arguments [#49][issue#49]

### Security

- Force version 4.3.1 of _System.Text.RegularExpressions_ due to known vulnerability.

## [5.4.2][5.4.2] - 2024-11-19

### Security

- Bump _System.Text.Json_ from 8.0.4 to 8.0.5 ([CVE-2024-43485][CVE-2024-43485])

## [5.4.1][5.4.1] - 2024-07-10

### Security

- Bump _System.Text.Json_ from 8.0.3 to 8.0.4 ([CVE-2024-30105][CVE-2024-30105])

## [5.4.0][5.4.0] - 2024-06-04

### Added

- Add .NET 8.0 framework target

### Changed

- Bump _System.Text.Json_ from 7.0.3 to 8.0.3 (concerns _Aqua.Text.Json_)
- Include type hierarchy in `Implements` type checking

### Removed

- Binary serialization removed for .NET 8.0 and later ([SYSLIB0050: Formatter-based serialization is obsolete][syslib0050])
- Dependency on _System.Text.Json_ removed for .NET 6.0 and later as provided by target framework [#43][issue#43]

## [5.3.0][5.3.0] - 2023-09-14

### Changed

- Bump _Newtonsoft.Json_ from 13.0.1 to 13.0.3 (concerns _Aqua.Newtonsoft.Json_)
- Bump _protobuf-net_ from 2.4.7 to 2.4.8 (concerns _Aqua.protobuf-net_)
- Bump _System.Text.Json_ from 6.0.6 to 7.0.3 (concerns _Aqua.Text.Json_)
- Remove `JsonSerializerOptions.IgnoreReadOnlyProperties` setting (concerns _Aqua.Text.Json_)
- Add `TypeResolver.AllowEmitType` property too optionally suppress automatic type emitting
- Add `ITypeResolver.TryResolveType(...)` extension method
- Support dynamic object mapping to custom collection types with `Add` method

## [5.2.0][5.2.0] - 2022-11-11

### Added

- Protected virtual methods in DynamicObjectMapper to allow override object creation and object initialization individually.

### Removed

- Removed methods previously marked as obsolete.

## [5.1.0][5.1.0] - 2022-09-01

### Changed

- Targeting .NET 6.0
- Updated dependency _System.Text.Json 6.0.5_

### Fixed

- Fixed issue relating to json and protobuf deserialization of dynamic object for empty type. [#39][issue#39]

## [5.0.0][5.0.0] - 2021-09-29

### Added

- Added support for [protobuf-net v2][protobuf-net-v2] serialization.
- Added support for _System.Text.Json_ serialization.
- Added _GetMethodEx_ extension methods for method reflection by exact signature.
- Added support for `System.Half`

### Changed

- Migrated to [nullable reference types][nullable-references].
- Reduce [Json.NET][json-net] doc size by substituting type info for common and well known types.

### Fixed

- Fixed issue relating to MemberInfo reflection on _.NET 6.0 WebAssembly_. [#36][issue#36]
- Fixed _FormatNativeTypeAsString_ value formatting to use _InvariantCulture_.
- Fixed type missmatch of numeric values with [Json.NET][json-net] serialization using custom `JsonConverter` for `DynamicObject`.
- Various minor API improvements and bug fixes.

### Security

- Introduced `ITypeSafetyChecker` to enable type checking before instantiation by `DynamicObjectMapper`.

[vnext-unreleased]: https://github.com/6bee/aqua-core/compare/v5.5.1...main
[5.5.1]: https://github.com/6bee/aqua-core/compare/v5.5.0...v5.5.1
[5.5.0]: https://github.com/6bee/aqua-core/compare/v5.4.2...v5.5.0
[5.4.2]: https://github.com/6bee/aqua-core/compare/v5.4.1...v5.4.2
[5.4.1]: https://github.com/6bee/aqua-core/compare/v5.4.0...v5.4.1
[5.4.0]: https://github.com/6bee/aqua-core/compare/5.3.0...v5.4.0
[5.3.0]: https://github.com/6bee/aqua-core/compare/5.2.0...5.3.0
[5.2.0]: https://github.com/6bee/aqua-core/compare/5.1.0...5.2.0
[5.1.0]: https://github.com/6bee/aqua-core/compare/5.0.0...5.1.0
[5.0.0]: https://github.com/6bee/aqua-core/compare/4.6.5...5.0.0

[issue#36]: https://github.com/6bee/aqua-core/issues/36
[issue#39]: https://github.com/6bee/aqua-core/issues/39
[issue#43]: https://github.com/6bee/aqua-core/issues/43
[issue#49]: https://github.com/6bee/aqua-core/issues/49

[json-net]: https://www.newtonsoft.com/json
[nullable-references]: https://docs.microsoft.com/en-us/dotnet/csharp/nullable-references
[protobuf-net-v2]: https://www.nuget.org/packages/protobuf-net/2.4.6
[syslib0050]: https://learn.microsoft.com/en-us/dotnet/fundamentals/syslib-diagnostics/syslib0050
[CVE-2024-30105]: https://github.com/advisories/GHSA-hh2w-p6rv-4g7w
[CVE-2024-43485]: https://github.com/advisories/GHSA-8g4q-xg66-9fp4
[CWE-502]: https://cwe.mitre.org/data/definitions/502.html
