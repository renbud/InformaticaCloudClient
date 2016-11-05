# InformaticaCloudClient
Command line tool to start and monitor a task or workflow on Informatica Cloud

Keywords (Informatica IOD Task)


Synopsis
========

> InformaticaCloudClient -t MyTaskName -e MyTaskType --run --wait -o MyOutputFileName.csv

-t MyTaskName   (specify the name of the task or workflow)

-e MyTaskType   (specify the type of the task or workflow)

--run           (if specified starts the task)

--wait          (if specified waits for the task to complete before exiting)

-o MyOutputFileName.csv  (if specified outputs the activity monitor and activity log for the task or workflow to the specified file)


-u UserName     (if specified change the UserName stored in InformaticaCloudClient.exe.config)

-p Password     (if specified change the password stored encrypted in InformaticaCloudClient.exe.config)

UserName and Password must be specified once in order to save them in the config file.
The encryption used to store the password is specific to the current windows login.
