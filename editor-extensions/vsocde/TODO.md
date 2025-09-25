# v1
- 

# v2
- add TODOS support. This feature needs to thought out. But it will be similar to other extensions. I need to write out tasks here.
- add BUGS support. This feature needs to thought out. But it will be similar to other extensions. I need to write out tasks here.
- use varied colors for the highlighting
- auto add settings. on any interaction if some settings do not exist, re add them with default value. Settings section must always exist.
- bring back auto task Id generation on pressing return. But dont rescan the TODOs.

# FINAL VERSION
- move to its own repo and update package.json
- update readme to be an actual guide

# Things to think about
- how does a person know how to create a new blank taskp project? Just documentation or a software supported solution?
- descriptoins/comments
    - origionally I was going to let tab - be treated as a description for whatever. Now, I dont like that idea.
    - Because then you could not do "sub tasks" if you so desired. Find another way to support descriptions/comments.
- decide if it would be better to put task ID up front instead of at the end
- learn what it would take to seperate the parser code from extension.js. extension.js would become like my main.c.
    - I do not want to do this if it requires "build steps" or other garbage. I want to keep things simple

so I want to add a new BIG system. Basically, its the TODO section. I want to scan for TODO, FIXME, etc... in the code to 
auto add under the "TODO" section.
- TODO, FIXME, etc.. are keywords that need to be a configurable setting under settings. maybe a comma delimted value?
- need to support exclude folders and files. Maybe just another setting with comma seperated values?
- the todo section is meant ot always match the code. the user is not supposed to edit it. so basically overwritting those lines is fine.
- need to store meta data (filename and line number) on the TODOs so we can jump to them.
- I have not figured this one out, but we cannot do this on every return key. Need to figure out when and how. Maybe only on file/project open?

## Potential Refactor
### Current Issues
- right now the extension struggles with the concept of multiple taskp files. Ideally, there would be one instance of the extension per instance of VSCode. That instance would then do any scaning of directories and such that it needs to do. However, the downside is that the
configurations for what paths to include/exclude are per taskp file. So while technically I could crawl through each and try to figure out 
what needs to be scanned off that (specifically the TODOs), something about it feels off. If possible, it might be simpler to simpler hold
one instance of a parser per taskp file. Or heck, no cache at all! Simply reprocess as needed! The main issue right now is not the task, user,
and setting parsing though. The issue is that scanning the code base TODOs is expensive. And needs to be done as needed. And doing that every
return key press is not ideal. As of right now, I see no other way to achive my goal then to have this concept of a "Project Scanner" that will scan the project for stuff based on the configuration provided from a taskp file.
    - Possible Design
        - New instance of TaskpieaScanner when vscode launches. Scanner does nothing for now.
        - New instance of TaskpieaParser every "event" triggered on the document.
        - When the parser goes to parse, it will ask the scanner for data by passing the taskp filename and its settings (keywords and include/exclude)
        - Parser will look at its cache. 
            - If it does not have anything, it will start the scan and return when finished.
            - If it does have data, it will return that cache data.
        - Scanning the workspace will only occur on vscode open.
            - On save would be to frequent. I save alot during edits. On key press is also to much. On taskp file open is also, debatable. The only method I am comfortable is once on startup. These TODOs are for convinience, not meant to be a constant in sync item. I could however supply a refresh command that will tell the parsers and scanners to execute fully. If the user wants to force refresh.
        - This design seems solid enough to me. Better to implement this and test on a very large project to see how long things take. Ideally we would use grep instead of the method I have been using, to find the keywords. GREP specifically for the scanner, not the parser.
- double document scan
    - right now the parser scans the entire taskp document twice. We need to do one scan only. This means that data is both parsed and output at the same time. The downside is that some data would have to be retroactivley updates; so maybe it is not possible.
    - the main issue is task id generation not being guaranteed to be unique. Unless you have already scan the whole file, it is possible you generate a random ID that matches one already in the file. And so if you are on line 3, and you need to generate an ID, when it gets to line 28, it will say "duplicate" and make a new one. We dont want tasks changing IDs like that. Therefore, I actually see no better way to do this.
    - technically, what I could do is; is just make sure that we always skip past todo section. That makes it a very rapid "go next" style solution so that we are only parsing stuff that mattters.
    - decision, leave for now; fix only if performance is bad.