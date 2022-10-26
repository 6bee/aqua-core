# Changelog
All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).


## [Unreleased vNext][vnext-unreleased]

### Added

### Changed

### Deprecated

### Removed

### Fixed

### Security


## [Unreleased 5.1.1][5.1.1-unreleased]

### Added
- Protected virtual methods in DynamicObjectMapper to allow override object creation and object initialization individually.

### Changed

### Deprecated

### Removed

### Fixed

### Security


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


[vnext-unreleased]: https://github.com/6bee/aqua-core/compare/5.1.0...main
[5.1.1-unreleased]: https://github.com/6bee/aqua-core/compare/5.1.0...main
[5.1.0]: https://github.com/6bee/aqua-core/compare/5.0.0...5.1.0
[5.0.0]: https://github.com/6bee/aqua-core/compare/4.6.5...5.0.0

[issue#36]: https://github.com/6bee/aqua-core/issues/36
[issue#39]: https://github.com/6bee/aqua-core/issues/39

[json-net]: https://www.newtonsoft.com/json
[nullable-references]: https://docs.microsoft.com/en-us/dotnet/csharp/nullable-references
[protobuf-net-v2]: https://www.nuget.org/packages/protobuf-net/2.4.6
