# Mulf_Differer
A diff tool to create result html from multiple files diffs.

## Getting Started
Install zip file from here. [Install Mulf_Differer](https://github.com/Saknowman/Mulf_Differer/releases/download/ver.1.0.0.0/Mulf_Differer.zip)  
Unzip it.  
Run Differer.exe

## DEMO
![mulf_differer_demo](https://user-images.githubusercontent.com/49089191/69853688-d5548000-12ca-11ea-9700-73743411f208.PNG)

Prepare target_files.txt.

```txt:./target_files.txt
./sample1.txt
./sample2.txt
./sample3.txt
```

Run Differer.exe

```bash
Target files: ./target_files.txt
Output: result.html
```

Programm compare diffs between

- sample1 and sample2
- sample1 and sample3
- sample2 and sample3

Open ./result.html on browser.
