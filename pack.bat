@echo off
set configuration=Debug
set version-suffix="alpha-001"
clean ^
  && dotnet restore ^
  && dotnet build src\Aqua                                            --configuration %configuration% --version-suffix "%version-suffix%" ^
  && dotnet build src\Aqua.Newtonsoft.Json                            --configuration %configuration% --version-suffix "%version-suffix%" ^
  && dotnet build test\Aqua.Tests.TestObjects1                        --configuration %configuration% --version-suffix "%version-suffix%" ^
  && dotnet build test\Aqua.Tests.TestObjects2                        --configuration %configuration% --version-suffix "%version-suffix%" ^
  && dotnet build test\Aqua.Tests                                     --configuration %configuration% --version-suffix "%version-suffix%" ^
  && dotnet test test\Aqua.Tests\Aqua.Tests.csproj                    --configuration %configuration% ^
  && dotnet pack src\Aqua\Aqua.csproj                                 --configuration %configuration% --version-suffix "%version-suffix%" --include-symbols --include-source --include-symbols --include-source --output "artifacts" ^
  && dotnet pack src\Aqua.Newtonsoft.Json\Aqua.Newtonsoft.Json.csproj --configuration %configuration% --version-suffix "%version-suffix%" --include-symbols --include-source --include-symbols --include-source --output "artifacts"