dotnet restore && dotnet build src\Aqua test\Aqua.Tests && dotnet test test\Aqua.Tests && dotnet pack src\Aqua --output artifacts --configuration Release --version-suffix 029
pause