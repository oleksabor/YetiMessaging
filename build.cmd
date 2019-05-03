set outpath=..\..\bin
set releasenotes="server can use object factory, LibLog logging, refactoring"
set description="broadcast crossprocess message library"
set version="1.1.0"
set tags="communication"
set csproj=src\YetiMessaging\YetiMessaging.csproj 

msbuild /t:pack %csproj%  /p:configuration=Release /p:Version=%version% /p:PackageTags=%tags% /p:PackageReleaseNotes=%releasenotes% /p:PackageDescription=%description% /p:PackageOutputPath=%outpath%
