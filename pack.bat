@echo off
clean ^
  && dotnet restore ^
  && dotnet build src\Aqua ^
  && dotnet build src\Aqua.Newtonsoft.Json ^
  && dotnet build test\Aqua.Tests.TestObjects1 ^
  && dotnet build test\Aqua.Tests.TestObjects2 ^
  && dotnet build test\Aqua.Tests ^
  && dotnet test test\Aqua.Tests\Aqua.Tests.csproj ^
  && dotnet pack src\Aqua\Aqua.csproj --output "..\..\artifacts" --configuration Debug --include-symbols --version-suffix 059 ^
  && dotnet pack src\Aqua.Newtonsoft.Json\Aqua.Newtonsoft.Json.csproj --output "..\..\artifacts" --configuration Debug --include-symbols --version-suffix 059