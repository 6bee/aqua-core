# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).


## [Unreleased 5.0.0][unreleased]

### Added
- Added support for [protobuf-net v2][protobuf-net-v2] serialization.
- Added support for _System.Text.Json_ serialization.
- Added _GetMethodEx_ extension methods for method reflection by exact signature.

### Changed
- Migrated to [nullable reference types][nullable-references].
- Reduce [Json.NET][json-net] doc size by substituting type info for common and well known types.

### Deprecated

### Removed

### Fixed
- Fixed issue relating to MemberInfo reflection on _.NET 6.0 WebAssembly_. [#36][issue#36]
- Fixed _FormatNativeTypeAsString_ value formatting to use _InvariantCulture_.
- Fixed type missmatch of numeric values with [Json.NET][json-net] serialization using custom `JsonConverter` for `DynamicObject`.
- Various minor API improvements and bug fixes.

### Security
- Introduced `ITypeSafetyChecker` to enable type checking before instantiation by `DynamicObjectMapper`.
- Introduced `KnownTypesRegistry` to control types for deserialization with [Json.NET][json-net].


[unreleased]: https://github.com/6bee/aqua-core/compare/4.6.5...main

[issue#36]: https://github.com/6bee/aqua-core/issues/36

[json-net]: https://www.newtonsoft.com/json
[nullable-references]: https://docs.microsoft.com/en-us/dotnet/csharp/nullable-references
[protobuf-net-v2]: https://www.nuget.org/packages/protobuf-net/2.4.6
