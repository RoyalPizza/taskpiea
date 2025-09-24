# v1
- remove all triming of lines from codebase. Regex must work with this change.
- auto add settings. on any interaction if some settings do not exist, re add them with default value. Settings section must always exist.
    - note... right now only lastId is a setting. In the future there will be more.
- add color support
    - section headers should be yellow
    - task IDs should be orange
- ensure extension is fine with non required sections being deleted
    - TASKS, TODO, BUGS, USERS
    - basicaly if a section is deleted, that feature wont be supported. But it cannot crash the extension.

# v2
- add suppport for users
    - provide intelisense so that as soon as @ is typed, it shows suggestions from the users list
    - @username needs to be colored lime

# v3
- add TODOS support. This feature needs to thought out. But it will be similar to other extensions. I need to write out tasks here.

# v4
- add BUGS support. This feature needs to thought out. But it will be similar to other extensions. I need to write out tasks here.

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