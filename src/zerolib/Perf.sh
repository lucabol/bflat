hyperfine --export-markdown linux.md --warmup 5  "./NoAlloc kjbible.txt" "./Standard kjbible.txt" "./Arena kjbible.txt" "./TPOP kjbible.txt"
ls -lh TPOP NoAlloc Arena Standard
