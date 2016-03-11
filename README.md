aqua-core
=========
Capable to transform any object-graph into a dynamic structure of composed dictionaries, holding serializable values, type information, and dynamic-objects.

AppVeyor: [![Build status](https://ci.appveyor.com/api/projects/status/98rc3yav530hlw1c?svg=true)](https://ci.appveyor.com/project/6bee/aqua-core)

#Contents
Aqua-core provides a bunch of serializable classes:
* DynamicObject
* TypeInfo
* FieldInfo
* PropertyInfo
* MethodInfo
* ConstructorInfo

Any object graph may be translated into a DynamicObject structure and back to it's original type using DynamicObjectMapper.