## Design Note
This library is using dotnet, but intentially ignores some dotnet specific features. That way the client can be dotnet, but the server can be something like Node.JS. The goal was to keep it more portable.

Example features would be support for ASP.NET server side validation and attributes. In an official app, I wont have one app in multiple tech stacks, so the code could be more intentially designed.

## Repository Layer
This app is designed around the idea of using a client in either standalone or as client server. In client server models, the model that is given to the client is usually passed back up during crud operations. The backend code must know how to translate this view model into the databse model. 

For terminology purposes, classes will basically represent the View object. If a specific class needs to be made in order to represent a db model, that would be in a seperate namespace. And the service code will have to be very careful about the types because of the duplicate names. 

I am not using EF for Sqlite, so as of now I think that just mapping properties to columns is fine, no need to do model conversion.

Additionally, the reason an interace is declared per type is so that a client can just hold a refernce of the interface type and chose to use either HTTP or Sqlite easily.

A RepositoryManager using a semi "service locator pattern" will be used by the client as DI is typically not possible on most UI frameworks. And their might need to be some specific operations called, like all datalayers having initialize called when a new project is created.

## Design Flaws
- concrete result classes mean that making additional operations a bit odd to add.This really depends more on what unique calls end up being required. Either it will be fine to just create new result classes, or it would have been better to have a generic result. Validation and deletion were really the only two unique ones.
- connection cache is odd. Because I am not doing one database per server, and instead one database per project with sqlite, I need to support multiple connection strings. Which means each type an API call is made, it needs to do a lookup based on project name.
- repository manager works differently based on platform. A standalone is expected to dispose and recreate. A server is expected to retain whole lifetime. HTTP clients have to be disposed, but server clients need no such functionality.