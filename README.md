# aqua-core

|  | `master` | `dnx` |
| --- | --- | --- |
| Package | [![NuGet Badge](https://buildstats.info/nuget/aqua-core?includePreReleases=true)](http://www.nuget.org/packages/aqua-core)  |  |
| AppVeyor | [![Build status](https://ci.appveyor.com/api/projects/status/98rc3yav530hlw1c/branch/master?svg=true)](https://ci.appveyor.com/project/6bee/aqua-core) | [![Build status](https://ci.appveyor.com/api/projects/status/98rc3yav530hlw1c/branch/dnx?svg=true)](https://ci.appveyor.com/project/6bee/aqua-core) |


Capable to transform any object-graph into a dynamic structure of composed dictionaries, holding serializable values, type information, and dynamic-objects.

#Contents
Aqua-core provides a bunch of serializable classes:
* DynamicObject
* TypeInfo
* FieldInfo
* PropertyInfo
* MethodInfo
* ConstructorInfo

Any object graph may be translated into a DynamicObject structure and back to it's original type using DynamicObjectMapper.