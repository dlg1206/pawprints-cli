<h1>
    <img alt="rit-paw" src="Resources/rit-logo.png" width="150">
    PawPrints CLI
</h1>

> A Schedule Maker CLI Tool for Rochester Institute of Technology Students

## Features
* Generate Detailed Schedules Within Seconds
* Set Rules to Filter Out Classes
* Use Buffers to Prevent Conflicts
* Get Results in Basic, Full, or JSON reports
* Templates to Simplify Use

## Quickstart Guide
1. Go to **https://api.rit.edu** and request a key
2. Download correct binary from the **[releases page](https://github.com/dlg1206/RIT-Schedule-Maker/releases)**
3. Rename to `pawprints`
4. ``./pawprints genconfig -k <key>``

## Before Installation
An RIT API key is also required, which students can request **[here](https://api.rit.edu/)**.

## Installation
PawPrints CLI can be installed one of two ways: Download Binary or Build Locally

### Method 1: Binary
1. Download the latest binary from the **[releases page](https://github.com/dlg1206/RIT-Schedule-Maker/releases)**

2. Use ``./pawprints <args>``

### Method 2: Local Install
1. Clone this repo: ``git clone https://github.com/dlg1206/RIT-Schedule-Maker.git && cd RIT-Schedule-Maker/RITScheduleMaker``

2. Use ``dotnet run <args>``

## Usage
PawPrints CLI has two main commands: ``walk`` and ``genconfig``

### walk
This command requests course information using the RIT API then generates a series of potential schedules

### Basic Usage
```bash
dotnet run walk -k <key> -cf <path/to/config>
dotnet run walk -k <key> -sd <startDate> -ed <endDate> -c <courses> <optional arguments>
```

#### Required Arguments
* ``-k <key value> | --key <key value>``: RITAuthorization key, required to access the RIT API

**And ONE of the Following Set of Arguments:**
* ``-cf <path/to/config> | --configFile <path/to/config>``: Config YAML File to Use

**OR**
* ``-sd <MM/DD/YEAR> | --startDate <MM/DD/YEAR>``  : Starting Date to Search for Classes
* ``-ed <MM/DD/YEAR> | --endDate <MM/DD/YEAR>``: Ending Date to Search for Classes
* ``-c <COURSE-NUMBER COURSE-NUMBER ...>  | --courses <COURSE-NUMBER COURSE-NUMBER ...>``: Space Seperated List of Courses to Search for

#### Optional Arguments
* ``-st <time> | --startTime <time>``: Classes Must Start After This Time (24hr)
* ``-et <time> | --endTime <time>``: Classes Must End After This Time (24hr)
* ``-f <format> | --format <format>``: Format for Output; default = basic (basic, full, json)
* ``-o <path> | --output <path>``: Path to Output File
* ``-d  | --debug``: Turn on Debug Mode
* ``-s  | --silent``: Turn on Silent Mode

> _Note: Using a Config File will ignore any Command Line Arguments_
#### Example Usage
```bash
dotnet run walk -k 1973f96d944ea41d94950355261e61231236d3d76e -cf myconfig.yml
dotnet run walk -k 1973f96d944ea41d94950355261e61231236d3d76 -sd 8/13/19 -ed 12/6/19 -c MATH-120 CSCI-320 HIST-301
dotnet run walk -k 1973f96d944ea41d94950355261e61231236d3d76 -sd 8/13/19 -ed 12/6/19 -st 10:00 -et 17:00 -f full -o out.txt -c MATH-120 CSCI-320 HIST-301
```

### genconfig
This is the inbuilt CLI tool to generate basic configuration files.
#### Basic Usage
```bash
dotnet run genconfig <args>
```
Running without arguments will just run the genconfig tool.
#### Optional Arguments
* ``-k <key value> | --key <key value>``: RITAuthorization key; Will Automatically ``walk`` with after generating the configuration file
* ``-d | --debug``: Turn on Debug Mode
* ``-s | --silent``: Turn on Silent Mode
#### Example Usage
```bash
dotnet run genconfig
dotnet run genconfig -k 1973f96d944ea41d94950355261e61231236d3d76e
```

## Configuration File
Config files are yaml files used to create schedules with a complex set of conditions. A blank Configuration File has been provided **[here](https://github.com/dlg1206/RIT-Schedule-Maker/blob/master/Resources/ConfigurationFile.yml)**.

### Required Fields
Same as the CLI args.

* ``startDate <MM/DD/YEAR>``  : Starting Date to Search for Classes
* ``endDate <MM/DD/YEAR>``: Ending Date to Search for Classes
* ``courses``: List of Courses to Search for

 _Example Minimum Config File_
 ```yaml
startDate: 8/13/19
endDate: 12/6/19
courses:
  - MATH-120
  - CSCI-320
  - HIST-301
 ``` 

### Basic Additional Fields
These fields are not required by further refine the search
* `name`: string; name of the schedule
* `format`: string; Format for Output; default = basic (basic, full, json)
* `output`: Path to Output File

### Advanced Additional Fields: Rules
Rules are special conditions that applied to the courses as an additional filter. All of these are options are listed underneath the `rules` field and are all optional.
* ``noClassOn``: list of days; Days which no classes ( excludes buffers ) are allowed
  > Accepted Day Arguments are: **Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday**
* ``noClassBefore``: 12hr (8:00 AM) / 24hr (8:00); Earliest time a class is allowed
* ``noClassAfter``: 12hr (5:00 PM) / 24hr (17:00); Latest time a class is allowed
* ``allowOnline``: bool; Allow the scheduling of Online Classes
* ``layover``: int; Minimum time required between classes

_Example Rule Usage_
```yaml
rules:
  noClassOn: 
    - Monday
    - Friday
  noClassBefore: 8:00 AM
  noClassAfter: 5:00 PM
  allowOnline: false
  layover: 20
``` 

### Advanced Additional Fields: Buffers
Buffers are blocks of time where no classes can be scheduled. This is useful for scheduling around club meetings, work, etc. The ``buffers`` field have unlimited buffers. All the fields are required in the buffer.
* ``name``: string; name of the buffer
* ``startTime``: 12hr (8:00 AM) / 24hr (8:00); Start time of the buffer
* ``endTime``: 12hr (5:00 PM) / 24hr (17:00); End time of the buffer
* ``days``: list of days; Days which the buffers occur on
> Accepted Day Arguments are: **Sunday, Monday, Tuesday, Wednesday, Thursday, Friday, Saturday**

 _Example Buffer Usage_
 ```yaml
 buffers:
   - name: Foo Club
     startTime: 3:00 PM
     endTime: 4:30 PM
     days:
       - Wednesday
   - name: Work
     startTime: 11:00
     endTime: 16:00
     days: 
       - Tuesday
       - Thursday
```

## Roadmap
- [ ] html support
- [ ] Redirect Excess Schedules to File
- [ ] Compare Schedules Function
- [ ] Advanced ``genconfig`` to include Rules and Buffers
- [ ] Validate configurations created by ``genconfig`` at runtime
- [ ] Use Specific Sections when Indicated
