# Ditto
Ditto will do one way replication, from source to destination. In other words, the source will never be modified by Ditto, only the destination. It will replicate additions, modifications and deletions.

## Basic Usage
In it's simplest mode it can be called from the command line without requiring a JSON file but giving it a source a destination folder like so:

```
Ditto.exe C:\SourceDir\ D:\DestinationDir\
```

## JSON Usage
More complex jobs can be defined using JSON. The JSON file is passed to Ditto like so:

```
Ditto.exe ExampleJob.json
```

### Sources
Each job requires at least one source but you can have as many as you like. Each source requires the following options:

* **name:** Only used if it generates a report
* **source:** The source folder
* **destination:** The destination folder. This isn't the path to the folder. See destination property for details
* **folder_exceptions:** A list of folders to skip. This is just the name of the folder, not the path
* **file_exceptions:** A list of files to skip.

It should be formatted like so:

```json
{
    "name": "Source 1",
    "source": "C:\\source1",
    "destination": "source 1",
    "folder_exceptions": ["DfsrPrivate"],
    "file_exceptions": ["Thumbs.db", "thumbs.db", "Desktop.ini", "desktop.ini"]
}
```

### Settings
Several settings can be defined. They are mostly for sending a report via email once the job finishes. The settings are as follows:

* **destination:** The destination path for all sources job. Each source will have its own folder in the destination path.
* **subject:** The subject line of the report sent out.
* **smtp:** The SMTP server used to send the report.
* **identity:** A file called identity can be placed in the destination. If this setting is set to true, Ditto will attempt to read the first line from the file. A line is inserted into the report stating *Destination identified as:* followed by the line read from the file. Useful if backing up to USB drives that are swapped daily, weekly, monthly etc.
* **job_name:** The name of the job.
* **sender:** The sender of the report.
* **recipients:** An array of email recipient address that will receive the report.

```json
{
    "destination": "D:\\Destination",
    "subject": "Ditto",
    "smtp": "127.0.0.1",
    "identify": false,
    "job_name": "Ditto",
    "sender": "sender@domain.tld",
    "recipients": [
        "recipient@domain.tld"
    ]
}
```


## Full Example
Below is an example of all the options available in Ditto in one file.

```json
{
    "sources": [
        {
            "name": "Source 1",
            "source": "C:\\source1",
            "destination": "source 1",
            "folder_exceptions": ["DfsrPrivate"],
            "file_exceptions": ["Thumbs.db", "thumbs.db", "Desktop.ini", "desktop.ini"]
        },
        {
            "name": "Source 2",
            "source": "C:\\source 2",
            "destination": "source 2",
            "folder_exceptions": [],
            "file_exceptions": []
        }
    ],
    "destination": "D:\\Destination",
    "subject": "Ditto",
    "smtp": "127.0.0.1",
    "identify": false,
    "job_name": "Ditto",
    "sender": "sender@domain.tld",
    "recipients": [
        "recipient@domain.tld"
    ]
}
```
