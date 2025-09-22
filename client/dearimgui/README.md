## Build Notes
- This assumes SDL2_DIR is a variable on your PC to the SDL headers and libs

## v1
- double click to edit tasks (unlimted popup windows)
- main menu item to add new user and task
- save to csv support (plus auto save)
- open project & close project features

## v2
- switch to cmake and add better 10x support
- switch to rad debugger instead of visual studio
- full refactor to seperate code into files
- Sqlite file backend
- binary file backend
- settings file

## Considerations
- TOML file backend
- JSON file backend
- UI audio effects

## --------------------------------------------
## --------------------------------------------

## Usecase of Task Management Software

TODO

## --------------------------------------------
## --------------------------------------------

## Design Notes
- inline vs poup editing
	- user table is inline because its only a couple columns
	- task table is a popup because the assumption is that tasks would need more UI space for things like attachments and comments.
	- inline on a table that is scolled and clicked frequently could have the issue where items are interacted with and edited accidently. We dont want people to accidently edit an item.

	- trello and clickup both have "quick action" buttons on hover. Like an edit card or rename. Which will turn the row into a text box. So for the task table, that is likely best. As it keeps the UI light (no edit buffers) and no accidental edits. But you can edit with one extra click, inline, without going into a popup.

	- an alt design would to have an "edit mode" and "view mode" that users can choose. That way if they are doing mass edits, they can go for it. 

	- for users and any future tables like that, inline edit is fine. Its a page that no one goes to unless they are there to edit people. But in general, items that are unlikely to be "mass edits" should not have to worry about much design. Inline edit would suffice.

	- the key here is that the design of simplicty > popularity/commonality/"features". Sometimes its just better to let apps be an excel document, and users will be sad if they fat finger. But dont put safety rails up 24/7 on the just incase. Instead, give them full power and then provide even better features (like a change tracker) so they can figure out why things happened.

- task popup

- change tracker
	- some software have a concept of logging messages or activity noting x user did x action or modified x field to x value. etc... This is very useful for reporting and diagnostics. 
		- I have seen, in person, people blame software saying it must have changed the value; when a feature like this doesnt exist. 
		- I think that this is an excellent feature, the only thing is it needs to be somewhat "dumb data" and bloat free. We dont want activity data to slow down our applicatoin. Therefore, it should be given low priority.
		- In an app implementing an architecture like what is outline below; it would be better to log the activity; but not cache it. Instead, that should be pulled upon request.
	- change tracker data must remain fairly simple, but can contain embeded information for future feature development.
		- for example, saying x user modified x field to x value on x task. You could embedd things like "userId" so that way its easy to make a hyper link if desired. Same for task. Something like [#123]Bob. 
		- Something like embedded data is a nice to have, but not nescessary. Simply storing a message with names is fine.
		- Clickup and Atlassin does implement this.

- subtasks
	- subtasks were something that did not exist in clickup "properly" in the version we first used. I felt its impact immediatley. However, there is something desireable about keeping the application operating in its KISS design philolsophy. 
	- Typically, forcing users to follow a flow they dont want is not ideal for commercial software. "Let them do what they want, don't force them to only have one assignee per task" - an example.
	- At the same time, over using a task system and not just doing the task is part of the design goal. Keep it simple, only add tasks for whats needed. There is no need for a full time "task manager".
	- this brings up an interesting thought of "what are the important parts of a task manager". (see section about that)

## --------------------------------------------
## --------------------------------------------

## Prioritized Client Architecture
An architecture for applications that contain minimal validation, prioritize client responsiveness, and are unlikely to have frequent server conflicts.

Note: Timestamps in UTC to avoid conflicts.

### Flow
- cache all data on client
	- store a single sync timestamp
- have all UI operations perform against the local cache, no server calls (even CRUD)
- have periodic sync with server (cache <-> server)
	- any item in our cache with an timestamp newer than our server sync time, send to the server for update.
	- request updates from the server for any object with a timestamp newer than our "sync timestamp"

### Potential Downsides
- prioritizes client validation over server validation. Which will not work for systems where users are modifying same data.
	- arguably, this is mostly always untrue. Typically data is broken down into smaller pieces.
	- typically multiple users will use a site, but very infrequently work on the exact same "item". 
	- Only apps like "building an HMI" where graphics are layed out, does this apply.
- could have a race condition where two people are editing the same object and last one wins
	- unfortunate, but should rarely happen
- could have a scenario where client validates but server doesnt.
	- client would just update with validation errors after a (client <-> server) sync
- the code for syncing with the server has to be solid, but a simple "latest timestamp wins" should be fine. 

### Potential Upsides
- client validation is faster and easier to run.
	- no network calls for validation needed
		- For example, validate on each keystroke instead of programming a delay timer to verify after no changes or lost of focus.
		- because you would never send an HTTP request every keystroke
	- Data just binds to its source and modifies it, no need to go through extra crud layers.
		- (only applies to systems where data is not overly relational. which is what this architecture is meant for.)
- entire application can run in offline mode much easier, because it operates off its cache