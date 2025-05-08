# VR Spy Companion

This program is designed to be used alongside VR Spy by UCLA Physics. Refer to https://github.com/cms-outreach/ispy-analyzers for information regarding how to create a '.ig' file using CMSSW.

This program works by unzipping and parsing the ig file, producing obj (geometry data) and json files, and sending these to the headset via the Android Debug Bridge. These files will be automatically found and used by our event display which you can download here: https://www.meta.com/s/4YdsvzpbH. 

This project is part of the work being done by the UCLA VR Group. You can check out more projects here: https://vr.physics.ucla.edu/index.html

The master branch of this repository is designed to be used by macOS. If you are using Windows, use the Windows branch.

## Quick Start

### Command Line Use 

1) Clone the repository
2) Connect your headset (Quest 2 & Quest 3 are supported) and enable USB Debugging and Developer Mode on the headset
3) Go to bin/Debug/net8.0 and run the following command
```
./VR\ Spy\ Companion path_to_ig_file [options]
```

VR Spy Companion accepts 2 arguments (the path to your '.ig' file and option flags) when being called from the command line. An interface will display allowing you to select your desired run. After execution, the event files will be automatically uploaded to your headset. 

### Options
- 's': Allows you to select a single event from the run
### GUI Use

GUI coming soon

## Common Issues

**Issue:** "Unhandled exception. System.ArgumentNullException: Value cannot be null. (Parameter 'device')"
**Solution:** Make sure that the headset is plugged in and that USB debugging and Developer Mode is enabled on the headset
### ADB  

VR Spy Companion utilizes the Android Debug Bridge to communicate with Oculus devices. Please make sure your Oculus Quest device is connected with developer mode enabled before running the companion. ADB is already installed with the executable and is located in platform-tools. The program uses a hardcoded path to platform-tools/adb so do not move this folder around. 

### ISpy Analyzer & Generation of IG Files
To visualize energies registered by the CMS's Electromagnetic Calorimeter (ECAL) in the Endcap regions (EE) within LEGO plots, users must create .ig files using our modified ISpy Analyzer file (ISpyEERecHit.cc) which has been updated to include calorimeter segmentation data for the EE regions.

To do this, simply replace the file "ISpyEERecHit.cc" at "cd CMSSW_14_0_5/src/ISpy/Analyzers/src/ISpyEERecHit.cc" in CMSSW with the file at https://github.com/nathan-joshua/ispy-analyzers/blob/master/src/ISpyEERecHit.cc and run the remaining commands to generate .ig files as usual.
