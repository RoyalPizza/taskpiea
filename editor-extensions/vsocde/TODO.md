# v1
- auto add settings. on any interaction if some settings do not exist, re add them with default value. Settings section must always exist.
    - note... right now only lastId is a setting. In the future there will be more.
- ensure extension is fine with non required sections being deleted
    - TASKS, TODO, BUGS, USERS
    - basicaly if a section is deleted, that feature wont be supported. But it cannot crash the extension.

# v2
- add suppport for users
    - @username needs to be colored lime
- use varied colors for the highlighting
- add support for descriptions/comments
- use a different ID genration system
    - the issue is that the IDs only increment by 1, and they are so similar they do not pop
    - find a way to keep it to 5 character hex, keep the IDs simple, but generate more unique entries

# v3
- add TODOS support. This feature needs to thought out. But it will be similar to other extensions. I need to write out tasks here.
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