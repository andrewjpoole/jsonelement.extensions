cd C:\dev\jsonelement.extensions

coverlet ".\AJP.JsonElementExtensions.UnitTests\bin\Debug\net5.0\AJP.JsonElementExtensions.UnitTests.dll" --target "dotnet" --targetargs "test AJP.JsonElementExtensions.UnitTests\AJP.JsonElementExtensions.UnitTests.csproj --no-build" --format cobertura

del "./coverageReport/*.*?"

reportgenerator -reports:.\coverage.cobertura.xml -targetdir:./coverageReport

start C:\dev\jsonelement.extensions\coverageReport\index.html