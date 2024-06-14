# aqua-core

[![GitHub license][lic-badge]][lic-link]
[![Github Workflow][pub-badge]][pub-link]

| branch | AppVeyor                         | Travis CI                      | Codecov.io         | Codacy                  | CodeFactor             |
| ---    | ---                              | ---                            | ---                | ---                     | ---                    |
| `main` | [![AppVeyor Build Status][1]][2] | [![Travis Build Status][3]][4] | [![codecov][5]][6] | [![Codacy Badge][7]][8] | [![CodeFactor][9]][10] |

| package                     | nuget                    | myget                          |
| ---                         | ---                      | ---                            |
| `aqua-core`                 | [![NuGet Badge][13]][14] | [![MyGet Pre Release][15]][16] |
| `aqua-core-newtonsoft-json` | [![NuGet Badge][17]][18] | [![MyGet Pre Release][19]][20] |
| `aqua-core-protobuf-net`    | [![NuGet Badge][21]][22] | [![MyGet Pre Release][23]][24] |
| `aqua-core-text-json`       | [![NuGet Badge][25]][26] | [![MyGet Pre Release][27]][28] |

Transform any object-graph into a dynamic, composed dictionaries like structure, holding serializable values and type information.

Aqua-core provides a bunch of serializable classes:

- `DynamicObject`
- `TypeInfo`
- `FieldInfo`
- `PropertyInfo`
- `MethodInfo`
- `ConstructorInfo`

Any object graph may be translated into a `DynamicObject` structure and back to it's original type using `DynamicObjectMapper`.

## Sample

Mapping an object graph into a `DynamicObject` and then back to it's original type

```C#
Blog blog = new Blog
{
    Title = ".NET Blog",
    Description = "A first-hand look from the .NET engineering teams",
    Posts = new[]
    {
        new Post
        {
            Title = "Announcing .NET Core 1.0",
            Date = new DateTime(2016, 6, 27),
            Author = "rlander"
            Text = "We are excited to announce the release of .NET Core 1.0, ASP.NET Core 1.0 and " +
               "Entity Framework Core 1.0, available on Windows, OS X and Linux! " +
               ".NET Core is a cross-platform, open source, and modular .NET platform [...]",
        },
        new Post
        {
            Title = "Happy 15th Birthday .NET!",
            Date = new DateTime(2017, 2, 13),
            Author = "bmassi",
            Text = "Today marks the 15th anniversary since .NET debuted to the world [...]",
        }
    }
}

DynamicObject dynamicObject = new DynamicObjectMapper().MapObject(blog);

Blog restoredBlog = new DynamicObjectMapper().Map(dynamicObject) as Blog;
```

[1]: https://ci.appveyor.com/api/projects/status/98rc3yav530hlw1c/branch/main?svg=true
[2]: https://ci.appveyor.com/project/6bee/aqua-core

[3]: https://api.travis-ci.com/6bee/aqua-core.svg?branch=main
[4]: https://travis-ci.com/github/6bee/aqua-core?branch=main

[5]: https://codecov.io/gh/6bee/aqua-core/branch/main/graph/badge.svg
[6]: https://codecov.io/gh/6bee/aqua-core

[7]: https://app.codacy.com/project/badge/Grade/b6c426b5f19140d8a793f06d73984005
[8]: https://app.codacy.com/gh/6bee/aqua-core/dashboard

[9]: https://www.codefactor.io/repository/github/6bee/aqua-core/badge
[10]: https://www.codefactor.io/repository/github/6bee/aqua-core

[13]: https://buildstats.info/nuget/aqua-core
[14]: https://www.nuget.org/packages/aqua-core
[15]: https://img.shields.io/myget/aqua/vpre/aqua-core.svg?style=flat-square&label=myget
[16]: https://www.myget.org/feed/aqua/package/nuget/aqua-core

[17]: https://buildstats.info/nuget/aqua-core-newtonsoft-json
[18]: https://www.nuget.org/packages/aqua-core-newtonsoft-json
[19]: https://img.shields.io/myget/aqua/vpre/aqua-core-newtonsoft-json.svg?style=flat-square&label=myget
[20]: https://www.myget.org/feed/aqua/package/nuget/aqua-core-newtonsoft-json

[21]: https://buildstats.info/nuget/aqua-core-protobuf-net
[22]: https://www.nuget.org/packages/aqua-core-protobuf-net
[23]: https://img.shields.io/myget/aqua/vpre/aqua-core-protobuf-net.svg?style=flat-square&label=myget
[24]: https://www.myget.org/feed/aqua/package/nuget/aqua-core-protobuf-net

[25]: https://buildstats.info/nuget/aqua-core-text-json
[26]: https://www.nuget.org/packages/aqua-core-text-json
[27]: https://img.shields.io/myget/aqua/vpre/aqua-core-text-json.svg?style=flat-square&label=myget
[28]: https://www.myget.org/feed/aqua/package/nuget/aqua-core-text-json

[lic-badge]: https://img.shields.io/github/license/6bee/aqua-core.svg
[lic-link]: https://github.com/6bee/aqua-core/blob/main/license.txt

[pub-badge]: https://github.com/6bee/aqua-core/actions/workflows/publish.yml/badge.svg
[pub-link]: https://github.com/6bee/aqua-core/actions/workflows/publish.yml
