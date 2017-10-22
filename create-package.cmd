@echo off

ECHO cleaning package directory
del /q "package\*"
FOR /D %%p IN ("package\*.*") DO rmdir "%%p" /s /q

ECHO copying files
ROBOCOPY tools\nuget package\tools install.ps1 /NFL /NDL /NJH /NJS /nc /ns /np
ROBOCOPY lib package/content/edge/ *.js /NFL /NDL /NJH /NJS /nc /ns /np
ROBOCOPY tools/build/nuget/content/edge/x86 package/content/edge/x86 *.* /NFL /NDL /NJH /NJS /nc /ns /np
ROBOCOPY tools/build/nuget/content/edge/x64 package/content/edge/x64 *.* /NFL /NDL /NJH /NJS /nc /ns /np

ROBOCOPY src\double\Edge.js\bin\Release\net45 package\lib\net45 *.dll /NFL /NDL /NJH /NJS /nc /ns /np
ROBOCOPY src\double\Edge.js\bin\Release\netcoreapp1.1 package\lib\netcoreapp1.1 *.dll /NFL /NDL /NJH /NJS /nc /ns /np




