## ...
- design project creation flow
- add background sync to AppDataCache

## Low Priority
- add paging & filter support to repositories (not needed for WPF)
- review Sqlite repositories to see if they can be refactored
	- connection stuff can be refactored
- error logging for WPF app

## Considerations
- add backend support for using EF
- review unique create results and consider if generic is better
- 

## UI Changes
- window buttons no background, larger, no margin. Use icon instead of text if needed. Color background on hover only. No pressed color. #535346
- background color to be dark. #2F2D28
- UI font Segoe UI
- task font? JetBrains Mono (TBD)




## Notes
### Project Create/Open
Project creation is different based on standalone vs client/server. In standalone mode, all that really matters is the project name and path. Of which, the path is most important. So really just a file dialogue to create a new project would be enough. 
Naming it would then decide the project name. Alternatively, I could do a setup like Visual Studio where the project name is entered, and then the path is chosen but; with the filename field disabled. That way it puts people in the mindset of projectname = filename; without thinking they can set the filename to be something different from their project name. 
In client/server mode, there has to be validation that a filename is okay. And there is no path selection. This is where making the flow be "enter project name" and then after (in standalone mode only) choose your save location is probably best. Because in client/serer mode I can just hide step two. I will call verify for each backend; but only HTTP will actually do a return. 
The only thing I have to deal with is what to do when a directory is chosen but the a file with that filename already exists. That is TBD.

For project opening, in standalone mode its simply just open the db file and that will resolve everything. In client server mode I need to list a the projects; and return it that way.

### Create UI Flow
- popup window show
- chose local or server
- if local
	- enter project name
	- choose directory
		- verify after path selection
	- when valid, create button enabled
- if server
	- enter server connection data
	- if connect
		- enter name
			- verify on changed
		- when valid, create button enabled

### Open UI Flow
- popup window show
- chose local or server
- if local
	- chose file with dialogue
	- open project if valid
- if server
	- enter server connection data
	- if connection
		- choose from dropdown
		- open project enabled when chosen

### ALT IDEA
- Create/Open for project has 4 options, no window to chose server vs local. 
- I want creating a project to be as simple as word almost. Perhaps all projects in a client start off as local standalone to a local directory. 
	- this allows the user to start editing immediatley if they want. But that is highly unlikely. Word docs that is more likely to be wanted. But for a task system, that is far less likely. What is more convinient is a open recent.
- still, the goal to simplify the local vs the server projects needs to occur. I think the best thing to do is instead of multiple grids with back and forth, just one grid. Dropdown or Radial to chose server vs local. If local, show the path. If server, show the server configuration. Basically the simplification is that its on one screen.
	- Another step would be to support uploading a local project to a server.
	- Another step would to just have "create project" be like "create file". It would use a temp name like I described in the first section. And only decide save to local vs server after the fact. When they go to save. For now, just default to saving locally. I know this works for desktops apps, idk about web apps.
	- I like this idea alot as it is the most streamline. Have the "Save As" window be the one where all this stuff is decided. So the flow for create would be as follows.
		- File/Create Project
		- on the backend create a temp project name and save to app data or documents or something
		- immediatley open the project with the temp name and save as a local backend.
		- user can rename the project at any time.
		- if the user wants to save to a specific location instead of the temp location, they do "Save As".
		- if the user wants to save as a server project, that is also in the save as. Either way, that flow can be decided a bit later. To streamline it. But most likely there will be some system to default to local and a "Save To Server" button seperate as save as. We shall see.

		- For my WPF task app, I need to create a project file. I want creating a new project to default to a temporary name and path. I assume appdata is best for the path. And for the filename I will need to do some sort of "NewProjectX" where x is a number that makes it unique. Which means I need to read the files from that path, and check in order what is available. Like NewProject1, NewProject2, etc.. What is the best way to do this? Oh, I also need to make sure this "AppData" folder exists. And I assume I should use roaming?


## Redesign
- to save time