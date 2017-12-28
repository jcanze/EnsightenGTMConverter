# EnsightenGTMConverter
This converter acts as an executable that converts Ensighten exports to GTM container imports in JSON format.  This was set up to support Ensighten and GTM accounts with several and complex spaces and corresponding containers.   
A space relates to a container, although there can be several spaces represented in a container. A GTM tag relates to an Ensighten tag. A trigger relates to a condition. A variable relates to a data definition. 
Please note this only currently works on Windows and is in Beta.

## Required Setup
* Create a directory in C:\ named tmp, with a directory names `ensighten-gtm`. Create directories named `config`, `ensighten`, and `gtm`. If you wish to change any of these directories, this can be done by editing `App.config`.
* Set up your config file. in `C:\tmp\ensighten-gtm\config`. This is required to know how containers will relate to spaces, and is the most manual piece of the process Set it up in the following format:   
```
{
	"AccountID": 987654321,
	"Directories": [
		{
			"Name": "dir1",
			"Containers": [
				{
					"Name": "gtmcontainer_1",
					"ContainerID": "GTM-1234",
					"SpaceID": ["1234"] 
				}
			]
		},
		{
			"Name": "dir2",
			"Containers": [
				{
					"Name": "gtmcontainer_2",
					"ContainerID": "GTM-12345",
					"SpaceID": ["12345"]
				},
				{
					"Name": "gtmcontainer_3",
					"ContainerID": "GTM-123456",
					"SpaceID": ["123456", "1234567"]
				}
			]
		}
	]
}
```
* Export all spaces, tags, containers, and data definitions in Ensighten to Excel format. Place each account's exports in `C:\tmp\ensighten-gtm\ensighten\` in a directory that can be named after the Ensighten account. However, it's only important that this directory name matches the config's `Directories.Name` value.
* Run the executable file or run the solution from Visual Studio.   
   
Please note that this currently will not transform any variable data, so it will reference tmParam, and it is required to still have `Bootstrap.js` included in the page. 