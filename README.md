# aqua-core

| branch | Package | AppVeyor | Travis |
| --- | --- | --- | --- |
| `master` | [![NuGet Badge](https://buildstats.info/nuget/aqua-core?includePreReleases=true)](http://www.nuget.org/packages/aqua-core) [![MyGet Pre Release](http://img.shields.io/myget/aqua/vpre/aqua-core.svg?style=flat-square&label=myget)](https://www.myget.org/feed/aqua/package/nuget/aqua-core) | [![Build status](https://ci.appveyor.com/api/projects/status/98rc3yav530hlw1c/branch/master?svg=true)](https://ci.appveyor.com/project/6bee/aqua-core) | [![Travis build Status](https://travis-ci.org/6bee/aqua-core.svg?branch=master)](https://travis-ci.org/6bee/aqua-core?branch=master) |


Transform any object-graph into a dynamic, composed dictionaries like structure, holding serializable values and type information.


Aqua-core provides a bunch of serializable classes:
* `DynamicObject`
* `TypeInfo`
* `FieldInfo`
* `PropertyInfo`
* `MethodInfo`
* `ConstructorInfo`

Any object graph may be translated into a `DynamicObject` structure and back to it's original type using `DynamicObjectMapper`.