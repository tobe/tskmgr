# tskmgr

A simple task manager built in WPF. As a uni project (hence all the comments are in Croatian, but I'm sure you'll find your way through).

![tskmgr](https://raw.github.com/tobe/tskmgr/master/screenshot.png)

## Requirements
### For using
* Windows Vista or greater
* Administrative privileges
* Internet connectivity (maybe?)

### For building
* MSVC++ 14.0 or greater
* LiveCharts (included in the project)
* LiveCharts.Wpf (included in the project)
* MahApps.Metro (included in the project)

## Features
Features are separated on a "per-view" basis

### Processes
* Lists all processes along with some info
* Start new and end 'em
* A statusbar showing resource usage
* Refreshed in background thread, each 5 seconds

### Applications
* Shows apps, refreshes on load
* End apps and bring them to front

### Services
* Lists services along with some info about them
* Can stop them

### Network
* A graph which refreshes every 60 seconds, showing some information about the best interface it can find, such as bandwidth usage and some packet count stuff
* Has its own thread for refreshing things

### Specifications
* Shows your PC's specs

### CPU Graph
* In its own thread, with background data refreshing in another thread
### RAM Graph
* In its own thread, with background data refreshing in another thread

## Why
I needed to create *something* for a Windows class. Task Manager was one of suggested apps, and I figured why not.

## Etc
* Learned how WPF works
* Could have made (better) use of MVVM
* The way the items are refreshed in the process view causes a lot of load on the GUI thread. Sadly, there is no alternative way to fix it.
Perhaps one way would be to compare each item in the process list, check whether all of its attributes changed, and only if they did update the said item.
However, this would require too much work and I wanted to keep things simple.