# RCM-UV - Resources Consumption Metrics tool

This software with the objective of gathering metrics related to hardware usage by single processes and multi-process programs given its name. It registers the percentage of CPU usage, the percentage of GPU usage, and the principal memory (RAM) usage and shows the values in a console in real time. When the process is finished it calculates the mean values and writes a log for future analysis.
It has been designed to be used on Windows systems, it makes use of Windows’ PowerShell commands to gather samples of hardware usage. The compatibility with other OS hasn’t been tested yet.


## Download 
```
git clone https://github.com/ETSE-UV/RCM-UV.git
```

## How to use

### Demo video
Watch a demo video of the tool by clicking on the picture below:

[![Watch the video](https://img.youtube.com/vi/yPMkxQbk0fE/maxresdefault.jpg)](https://www.youtube.com/watch?v=yPMkxQbk0fE)

* After starting the program, we must confirm the name of the process or the program to analyze
![Screenshot 1](https://raw.githubusercontent.com/ETSE-UV/RCM-UV/master/Images/screenshot1.png)

* The program will check if the named process is running to start registering values. If the process hasn’t started yet, a message to start it will be shown to the user
![Screenshot 2](https://raw.githubusercontent.com/ETSE-UV/RCM-UV/master/Images/screenshot2.png)

*	Once the process to study has been started, our software will periodically check the values of hardware usage, show them on the screen. After ending the process, the program will calculate averages values and register them in logs
![Screenshot 3](https://raw.githubusercontent.com/ETSE-UV/RCM-UV/master/Images/screenshot3.png)
