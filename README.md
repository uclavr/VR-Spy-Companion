# VR Spy Companion

This program is designed to be used alongside VR Spy by UCLA Physics. Refer to https://github.com/cms-outreach/ispy-analyzers for information regarding how to create a '.ig' file using CMSSW.

This program works by unzipping and parsing the ig file, producing obj (geometry data) and json files, and sending these to the headset via the Android Debug Bridge. These files will be automatically found and used by our event display which you can download here: https://www.meta.com/s/4YdsvzpbH. You can also look at the source code if you are interested https://github.com/andxsu/CMS-Event-Display. 

This project is part of the work being done by the UCLA VR Group. You can check out more projects here: https://vr.physics.ucla.edu/index.html

The master branch of this repository is designed to be used by macOS. If you are using Windows, use the Windows branch.

## Quick Start

### Command Line Use 

1) Clone the repository
2) Connect your headset (Quest 2 & Quest 3 are supported) and enable USB Debugging and Developer Mode on the headset
3) Go to bin/Debug/net6.0 and run the following command
```
"VR Spy Companion.exe" path_to_ig_file
```

VR Spy Companion accepts 1 argument (the path to your '.ig' file) when being called from the command line. An interface will display allowing you to select your desired event.

### GUI Use

GUI coming soon

## Common Issues

**Issue:** "Unhandled exception. System.ArgumentNullException: Value cannot be null. (Parameter 'device')"
**Solution:** Make sure that the headset is plugged in and that USB debugging and Developer Mode is enabled on the headset
### ADB  

VR Spy Companion utilizes the Android Debug Bridge to communicate with Oculus devices. Please make sure your Oculus Quest device is connected with developer mode enabled before running the companion. ADB is already installed with the executable and is located in platform-tools. The program uses a hardcoded path to platform-tools/adb so do not move this folder around.  
