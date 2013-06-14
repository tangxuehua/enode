echo off
echo install
@forfiles /s /m packages.config /c "cmd /c %1\nuget install @file -o %2 -source http://nuget.org/api/v2"
echo on