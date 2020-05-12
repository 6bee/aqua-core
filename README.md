# aqua-core

| branch   | AppVeyor                         | Travis CI                      | Codecov.io         | Codacy                  | CodeFactor             | License                     |
| ---      | ---                              | ---                            | ---                | ---                     | ---                    | ---                         |
| `master` | [![AppVeyor Build Status][1]][2] | [![Travis Build Status][3]][4] | [![codecov][5]][6] | [![Codacy Badge][7]][8] | [![CodeFactor][9]][10] | [![GitHub license][11]][12] |

| package                     | nuget                    | myget                          |
| ---                         | ---                      | ---                            |
| `aqua-core`                 | [![NuGet Badge][13]][14] | [![MyGet Pre Release][17]][18] |
| `aqua-core-newtonsoft-json` | [![NuGet Badge][15]][16] | [![MyGet Pre Release][19]][20] |

Transform any object-graph into a dynamic, composed dictionaries like structure, holding serializable values and type information.

Aqua-core provides a bunch of serializable classes:
*   `DynamicObject`
*   `TypeInfo`
*   `FieldInfo`
*   `PropertyInfo`
*   `MethodInfo`
*   `ConstructorInfo`

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
            Text = "We are excited to announce the release of .NET Core 1.0, ASP.NET Core 1.0 and Entity Framework Core 1.0, available on Windows, OS X and Linux! .NET Core is a cross-platform, open source, and modular .NET platform [...]"
        },
        new Post
        {
            Title = "Happy 15th Birthday .NET!",
            Date = new DateTime(2017, 2, 13),
            Author = "bmassi",
            Text = "Today marks the 15th anniversary since .NET debuted to the world [...]"
        }
    }
}

DynamicObject dynamicObject = new DynamicObjectMapper().MapObject(blog);

Blog restoredBlog = new DynamicObjectMapper().Map(dynamicObject) as Blog;
```

[1]: https://ci.appveyor.com/api/projects/status/98rc3yav530hlw1c/branch/master?svg=true
[2]: https://ci.appveyor.com/project/6bee/aqua-core
[3]: https://travis-ci.org/6bee/aqua-core.svg?branch=master
[4]: https://travis-ci.org/6bee/aqua-core?branch=master
[5]: https://codecov.io/gh/6bee/aqua-core/branch/master/graph/badge.svg
[6]: https://codecov.io/gh/6bee/aqua-core
[7]: https://api.codacy.com/project/badge/Grade/92ef3842d8274d148b0af85aa5ec6acc
[8]: https://www.codacy.com/manual/6bee/aqua-core
[9]: https://www.codefactor.io/repository/github/6bee/aqua-core/badge
[10]: https://www.codefactor.io/repository/github/6bee/aqua-core
[11]: https://img.shields.io/github/license/6bee/aqua-core.svg
[12]: https://github.com/6bee/aqua-core/blob/master/license.txt
[13]: https://buildstats.info/nuget/aqua-core?includePreReleases=true
[14]: https://www.nuget.org/packages/aqua-core
[15]: https://buildstats.info/nuget/aqua-core-newtonsoft-json?includePreReleases=true
[16]: https://www.nuget.org/packages/aqua-core-newtonsoft-json
[17]: https://img.shields.io/myget/aqua/vpre/aqua-core.svg?style=flat-square&label=myget
[18]: https://www.myget.org/feed/aqua/package/nuget/aqua-core
[19]: https://img.shields.io/myget/aqua/vpre/aqua-core-newtonsoft-json.svg?style=flat-square&label=myget
[20]: https://www.myget.org/feed/aqua/package/nuget/aqua-core-newtonsoft-json
