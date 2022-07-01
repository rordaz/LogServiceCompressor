# LogFilesServiceCompressor

Windows Service that zip logs files on schedule, created zip packages per month. Optionally uploads zip files to Azure Blob.

### How it works

Stand alone windows service that compress on schedule (commonly daily) logs folder adding it to a zip file, after compression the files are deleted (this is configurable, enabled by default). 

On the first of every month a new zip is created with the following name pattern: LogZipFile_YYYY_MM.zip, thus a Zip file containing all the logs created in a month will be stored in one zip file. 

The compression achieved is 70%, new log file is reduced from 10MB can be compressed to 3MB.

### Installation & Setup

**Install Service**

Copy of the content of the installation zip file to C:\LogFilesServiceCompressor\

Execute the install.bat as administrator, the install file can be found at **C:\LogFilesServiceCompressor\install.bat**

**Setup**

+ Open the config file found at: C:\LogFilesServiceCompressor\\**LogFilesServiceCompressor.exe.config**

The followings keys are required to be configured:

+ **SourceLogFiles:** Log Files's Folder Location 
+ **ZipFileDestination:** Location to Save Zip Files created by the compressor
+ **BufferTimeDays:** Set equal to **0 (zero)** if you need to keep only the current day uncompressed or unzip, set iqual to **1 (one)** if you need an additional day of logs without compression. 
+ **ScheduleStartTime:** Time the compressor will run, recomended 1 minute after midnight (00:01)

Leave the below keys with their **default** values

+ **RemoveOriginalFiles: true** Remove Original Log Files after Compression have finished
+ **CompressSpan: DAILY** How often will the compression will execute, daily is recomended since it takes less time to the task.
+ **CompressSpanFactor: 1** Number of Days to be zipped 
+ 

```xml
<appSettings>
    <!--Log Files Location -->
    <add key="SourceLogFiles" value="C:\[LOG_FILES_LOCATION]" />
    <!--Location to Save ZipFiles-->
    <add key="ZipFileDestination" value="C:\[ZIP_FOLDER_LOCATION]\LogZipFile_YYYY_MM.zip" />
    <!--Use Date Modified to Filter Logs : false by default-->
    <add key="UseDateModified" value="false" />
    <!--Remove Original Log Files after Compression-->
    <add key="RemoveOriginalFiles" value="true" />
    <!--Limit Resource to Single Core-->
    <add key="LimitResourceToSingleCore" value="true" />
    <!-- CompressSpan  DAILY | WEEKLY | MONTHLY -->
    <add key="CompressSpan" value="DAILY" />
    <!-- Multiplier of the CompressSpanFactor -->
    <!--Number of Days to be zip = CompressSpanFactor x CompressSpan -->
    <add key="CompressSpanFactor" value="1" />
    <!--1 Day-->
    <!-- days without compression -->
    <add key="BufferTimeDays" value="0" />
    <!-- Time the compressor will run-->
    <add key="ScheduleStartTime" value="00:01" />
    <!--SERVICE SETTINGS-->
    <!--Instance Name-->
    <!--<add key="WindowsServiceInstanceName" value="" />-->
    <!--Service Name-->
    <add key="WindowsServiceName" value="LogArchiver" />
    <!--Display Name-->
    <add key="WindowsServiceDisplayName" value="Log Files Compressor" />
    <!--Instance Description-->
    <add key="WindowsServiceDescription" value="Log Archiving" />
    <add key="ClientSettingsProvider.ServiceUri" value="" />
  </appSettings>
```
### Troubleshooting

The compressor service creates logs files every time it executes, the logs can be found at **C:\LogFilesServiceCompressor\log\logFileServiceCompressor_log.txt**

