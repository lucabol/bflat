#!/bin/bash
bflat build -Os --stdlib:none --ldflags ~/dev/bflatbin/lib/linux/x64/glibc/libSystem.Native.a -o Main
strip -s Main

bflat build  -dTEST -Os --stdlib:none -o Test --ldflags ~/dev/bflatbin/lib/linux/x64/glibc/libSystem.Native.a

clang -Os TPOP.c eprintf.c -o TPOP
strip -s TPOP
