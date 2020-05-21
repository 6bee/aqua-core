@echo off
set configuration=Debug
clean ^
  && dotnet restore ^
  && dotnet test test\Aqua.Tests                --configuration %configuration% ^
  && dotnet pack src\Aqua                       --configuration %configuration% --include-symbols --include-source --output "artifacts" ^
  && dotnet pack src\Aqua.Newtonsoft.Json       --configuration %configuration% --include-symbols --include-source --output "artifacts" ^
  && dotnet pack src\Aqua.protobuf-net          --configuration %configuration% --include-symbols --include-source --output "artifacts"