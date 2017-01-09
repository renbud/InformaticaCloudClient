# InformaticaCloudClient
Command line tool to start, stop and monitor a task or workflow on Informatica Cloud.

Keywords (Informatica, Cloud, REST API, IOD, Task, Scripting)

This tool can be used where you want to invoke or monitor an IOD task by scripting. It gives a simple command line interface that saves developing code to call the Informatica Cloud REST API.


Usage
========
 <br><br> > InformaticaCloudClient -t MyTaskName -e MyTaskType --run --wait -o MyOutputFileName.csv


-t MyTaskName   (specify the name of the task or workflow)

-e MyTaskType   (specify the type of the task or workflow)

--stop          (if specified, stops the task if it is running)

--run           (if specified, starts the task)

--wait          (if specified, waits for the task to complete before exiting)

-o MyOutputFileName.csv  (if specified, outputs the activity monitor and activity log for the task or workflow to the specified file)

-u UserName     (if specified, change the UserName stored in InformaticaCloudClient.exe.config)

-p Password     (if specified, change the password stored encrypted in InformaticaCloudClient.exe.config)

UserName and Password must be specified once in order to save them in the config file.
The encryption used to store the password is specific to the current windows login.


If a combination of --stop, --run and --wait are specified, then they are executed in that order

1. --stop First any running task of that name will be stopped
2. --run  Next the task of that name will be started
3. --wait Finally the process waits for the task to complete before exiting
