@echo off
set configuration=Debug
clean ^
  && dotnet restore ^
  && dotnet build src\Aqua                                            --configuration %configuration% ^
  && dotnet build src\Aqua.Newtonsoft.Json                            --configuration %configuration% ^
  && dotnet build test\Aqua.Tests.TestObjects1                        --configuration %configuration% ^
  && dotnet build test\Aqua.Tests.TestObjects2                        --configuration %configuration% ^
  && dotnet build test\Aqua.Tests                                     --configuration %configuration% ^
  && dotnet test test\Aqua.Tests\Aqua.Tests.csproj                    --configuration %configuration% ^
  && dotnet pack src\Aqua\Aqua.csproj                                 --configuration %configuration% --include-symbols --include-source --output "artifacts" ^
  && dotnet pack src\Aqua.Newtonsoft.Json\Aqua.Newtonsoft.Json.csproj --configuration %configuration% --include-symbols --include-source --output "artifacts"