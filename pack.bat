@echo off
set configuration=Debug
clean ^
  && dotnet test test\Aqua.Tests                --configuration %configuration% ^
  && dotnet test test\Aqua.Tests.VBNet          --configuration %configuration% ^
  && dotnet pack src\Aqua                       --configuration %configuration% --include-symbols --include-source --output "artifacts" ^
  && dotnet pack src\Aqua.Newtonsoft.Json       --configuration %configuration% --include-symbols --include-source --output "artifacts" ^
  && dotnet pack src\Aqua.protobuf-net          --configuration %configuration% --include-symbols --include-source --output "artifacts"