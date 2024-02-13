#!/bin/bash
bflat build  -dNOALLOC  -Os --stdlib:none   -o NoAlloc --ldflags ~/dev/bflatbin/lib/linux/x64/glibc/libSystem.Native.a
bflat build  -dARENA    -Os --stdlib:none   -o Arena   --ldflags ~/dev/bflatbin/lib/linux/x64/glibc/libSystem.Native.a
bflat build  -dSTANDARD -Os --stdlib:DotNet -o Standard Main.cs Markov_standard.cs --ldflags ~/dev/bflatbin/lib/linux/x64/glibc/libSystem.Native.a

bflat build  -dTEST -Os --stdlib:none -o Test

clang -Os TPOP.c eprintf.c -o TPOP

strip -s NoAlloc
strip -s Arena
strip -s Standard
strip -s TPOP
