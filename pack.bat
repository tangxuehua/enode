echo off
mkdir nupkgFiles
echo pack
forfiles /p .\ /m *.nuspec /c "cmd /c .\tools\nuget pack @file -outputdirectory nupkgFiles"
echo on