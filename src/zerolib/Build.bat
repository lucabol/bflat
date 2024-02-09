bflat build  -dNOALLOC -Os --stdlib:none -o NoAlloc.exe
bflat build  -dARENA -Os --stdlib:none -o Arena.exe
bflat build  -dTEST -Os --stdlib:none -o Test.exe
clang -Os TPOP.c eprintf.c -o TPOP.exe