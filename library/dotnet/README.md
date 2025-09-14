## Design Note
This library is using dotnet, but intentially ignores some dotnet specific features. That way the client can be dotnet, but the server can be something like Node.JS. The goal was to keep it more portable.

Example features would be support for ASP.NET server side validation and attributes. In an official app, I wont have one app in multiple tech stacks, so the code could be more intentially designed.

## Repository Layer
This app is designed around the idea of using a client in either standalone or as client server. In client server models, the model that is given to the client is usually passed back up during crud operations. The backend code must know how to translate this view model into the databse model. 

For terminology purposes, classes will basically represent the View object. If a specific class needs to be made in order to represent a db model, that would be in a seperate namespace. And the service code will have to be very careful about the types because of the duplicate names. 

I am not using EF for Sqlite, so as of now I think that just mapping properties to columns is fine, no need to do model conversion.

Additionally, the reason an interace is declared per type is so that a client can just hold a refernce of the interface type and chose to use either HTTP or Sqlite easily.

A RepositoryManager using a semi "service locator pattern" will be used by the client as DI is typically not possible on most UI frameworks. And their might need to be some specific operations called, like all datalayers having initialize called when a new project is created.