# VR Spy Companion

This program is designed to be used alongside VR Spy by UCLA Physics. Refer to https://github.com/cms-outreach/ispy-analyzers for information regarding how to create a '.ig' file using CMSSW.

## Quick Start

### Command Line Use

VR Spy Companion accepts 1 argument (the path to your '.ig' file) when being called from the command line. An interface will display allowing you to select your desired event.

### GUI Use

GUI coming soon

## Common Issues

### ADB  

VR Spy Companion utilizes the Android Debug Bridge to communicate with Oculus devices. Please make sure your Oculus Quest device is connected with developer mode enabled before running the companion, and have ADB installed with the executable located in the same directory as VR Spy Companion. This should not be an issue unless you are compiling from source.
