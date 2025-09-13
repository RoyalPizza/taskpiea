# Taskiea
Taskiea is my take on a very simple task management tool. It can run as server/client or standalone.

Feature wise, it will remain very simple and "featureless". I value simplicity, and believe that it is the most
effictive way to manage.

# Multi Tech Stack
This project will be built in multiple tech stacks. Thus there will be multiple servers, clients, and libraries. Below are the planned tech stacks.

[Server/Client]
- ASP.NET CORE API, ASP.NET MVC
- ASP.NET CORE API, Blazor
- ASP.NET CORE API, React
- NodeJS, React
- (1), WPF
- (1), Dear ImGUI

*(1) desktop clients should support any backend as data will always be REST*

# V1 Design
Until this app is finished, the design section will only include what is desired for the next targeted version.

## Architecture
- SQL Lite data storage
- Shared library with all core logic
- REST API backend for server/client model

## Core Features
- Create users so they can be linked to tasks. No authentication or login system.
- CRUD tasks with name, description, status, and user
- Popup on click to edit task name, description, and user; but can set status inline.

## Gotchas
- no status flow. Always manually set status
- no subtasks
- no offline support. (client applications will not work as standalone yet)
- no attachments
- no markdown support, plain text only.