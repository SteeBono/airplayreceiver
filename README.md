# AirPlay Receiver
Open source implementation of AirPlay 2 Mirroring / Audio protocol in C# .Net Core.  

## Generic

Tested on macOS with iPhone 12 Pro iOS14.  
  
The project is fully functional, but the AAC and ALAC libraries written in C ++ must be built.  
  
## How To

### Build AAC Codec
To download, build and install fdk-aac do the following:  
  
Clone the repository and cd into the folder:  
```
$ git clone https://github.com/mstorsjo/fdk-aac.git
$ cd fdk-aac
```
  
Configure the build and make the library:  
```
$ autoreconf -fi
$ ./configure
$ make
```
  
### Build ALAC Codec
To download, build and install alac do the following:  
  
Clone the repository and cd into the folder:  
```
$ git clone https://github.com/mikebrady/alac.git
$ cd alac
```
  
Download and paste 'GiteKat''s files in 'alac/codec' folder cloned before
```
$ https://github.com/GiteKat/LibALAC/tree/master/LibALAC
```
  
The 'mikebrady''s source code does not contains 'extern' keyword.
We need external linkage so we use 'GiteKat''s source code files.
  
Configure the build and make the library:  
```
$ autoreconf -fi
$ ./configure
$ make
```
  
### Linux
On terminal type 'apt-get install build-essential autoconf automake libtool' to install build tools
Add compiled DLL path into 'appsettings_linux.json' file.  
  
### MacOS
On terminal type 'brew install autoconf automake libtool' to install build tools
Add compiled DLL path into 'appsettings_osx.json' file.  
  
### Windows

Use [this](http://www.gaia-gis.it/gaia-sins/mingw64_how_to.html#env) tutorial to understand how to compile source code on Windows.  
You need MinGW32 or MinGW64 based on arch.  

Add compiled DLL path into 'appsettings_win.json' file.  
  
## Wiki
  
Here you will find an [Article](https://github.com/SteeBono/airplayreceiver/wiki) where I explain how the whole AirPlay 2 protocol works.
  
## Disclamier
  
All the resources in this repository are written using only opensource projects.  
The code and related resources are meant for educational purposes only.  
I do not take any responsibility for the use that will be made of it.    

## Credits

Inspired by others AirPlay open source projects.  
Big ty to OmgHax.c's author 😱. 

## If you want support me 🔥

If you appreciate my work, consider buying me a cup of coffee to keep me recharged 🥲  
  
BTC: 1MT4VAP3WnuNxSciWGAaasN9TxZiUPHtxv  
BCH: qp32ey3x9dc35up9ny3xzhprpwfmd8kclu65x3htl5  
ETH: 0xb2c39868d17eafccaadf516c8a306c240509c0a6  
