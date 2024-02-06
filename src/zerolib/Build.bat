bflat build -Os --stdlib:none -o Main.exe
bflat build  -dTEST -Os --stdlib:none -o Test.exe
clang -Os TPOP.c eprintf.c -o TPOP.exe