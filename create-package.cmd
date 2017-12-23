@echo off

ECHO cleaning nuget directory
rem del /q "package\*"
rmdir "tools/nuget/content" /s /q
rmdir "tools/nuget/lib" /s /q
rem FOR /D %%p IN ("tools/nuget/content/*.*") DO rmdir "%%p" /s /q
rem FOR /D %%p IN ("tools/nuget/lib/*.*") DO rmdir "%%p" /s /q

ECHO copying files
rem ROBOCOPY tools\nuget package\tools install.ps1 /NFL /NDL /NJH /NJS /nc /ns /np
ROBOCOPY lib tools/nuget/content/edge/ *.js /NFL /NDL /NJH /NJS /nc /ns /np
ROBOCOPY tools/build/nuget/content/edge/x86 tools/nuget/content/edge/x86 *.* /NFL /NDL /NJH /NJS /nc /ns /np
ROBOCOPY tools/build/nuget/content/edge/x64 tools/nuget/content/edge/x64 *.* /NFL /NDL /NJH /NJS /nc /ns /np

ROBOCOPY tools\build\nuget\lib\net40 tools/nuget/lib/net40 *.dll /NFL /NDL /NJH /NJS /nc /ns /np
ROBOCOPY tools\build\nuget\lib\net45 tools/nuget/lib/net45 *.dll /NFL /NDL /NJH /NJS /nc /ns /np
ROBOCOPY tools\build\nuget\lib\netcoreapp1.1 tools/nuget/lib/netcoreapp1.1 *.dll /NFL /NDL /NJH /NJS /nc /ns /np
ROBOCOPY tools\build\nuget\lib\netcoreapp2.0 tools/nuget/lib/netcoreapp2.0 *.dll /NFL /NDL /NJH /NJS /nc /ns /np


rem ROBOCOPY src\double\Edge.js\bin\Release\net45 package\lib\net45 *.dll /NFL /NDL /NJH /NJS /nc /ns /np
rem ROBOCOPY src\double\Edge.js\bin\Release\netcoreapp1.1 package\lib\netcoreapp1.1 *.dll /NFL /NDL /NJH /NJS /nc /ns /np
rem ROBOCOPY src\double\Edge.js\bin\Release\netcoreapp2.0 package\lib\netcoreapp2.0 *.dll /NFL /NDL /NJH /NJS /nc /ns /np




