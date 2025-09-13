## Design Note
This library is using dotnet, but intentially ignores some dotnet specific features. That way the client can be dotnet, but the server can be something like Node.JS. The goal was to keep it more portable.

Example features would be support for ASP.NET server side validation and attributes. In an official app, I wont have one app in multiple tech stacks, so the code could be more intentially designed.