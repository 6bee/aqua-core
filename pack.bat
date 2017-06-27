@echo off
set configuration=Debug
set version-suffix="004"
clean ^
  && dotnet restore ^
  && dotnet build src\Aqua --configuration %configuration% ^
  && dotnet build src\Aqua.Newtonsoft.Json --configuration %configuration% ^
  && dotnet build test\Aqua.Tests.TestObjects1 --configuration %configuration% ^
  && dotnet build test\Aqua.Tests.TestObjects2 --configuration %configuration% ^
  && dotnet build test\Aqua.Tests --configuration %configuration% ^
  && dotnet test test\Aqua.Tests\Aqua.Tests.csproj --configuration %configuration% ^
  && dotnet pack src\Aqua\Aqua.csproj --output "..\..\artifacts" --configuration %configuration% --include-symbols --version-suffix %version-suffix% ^
  && dotnet pack src\Aqua.Newtonsoft.Json\Aqua.Newtonsoft.Json.csproj --output "..\..\artifacts" --configuration %configuration% --include-symbols --version-suffix %version-suffix%