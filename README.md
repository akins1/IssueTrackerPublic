# Issue Tracker
Issue Tracker is a web application written in ASP.NET Core MVC that lets users create, manage, modify, and archive Tickets to track the bugs/issues of a Project

If you are interested in trying out the demo, this project is deployed to Azure App Service [here](https://issuetrackerakins1.azurewebsites.net/).
Select login and try out one of the demo users!

Link: https://issuetrackerakins1.azurewebsites.net/

## Technologies
- ASP.NET Core MVC
- PostgreSQL
- ASP.NET Identity
- Entity Framework
- Azure App Service with Github Actions
- [AdminLTE template](https://adminlte.io/)

## Features
- ### Users
  - are able to authenticate into the application by registering an account or logging in
  - are assigned roles with different levels of authority (Role-Based Access Control): 
    - Admin
    - Project Manager
    - Developer
- ### Projects
  - contains metadata such as: name, description, date created, deadline priority, status, image etc.
  - can have a single Project Manager assigned to it
  - contains a list of Tickets
  - contains a history of changes to metadata
  - contains a "project team" consisting of the Developers assigned to its Tickets
  - can be created, edited, and archived/restored by Admins and Project Managers
- ### Tickets
  - are housed within Projects
  - contains metadata such as: title, description, date created, creator, developer, type, priority, status, etc.
  - can have a single Developer assigned to it
  - can be created, edited, and archived/restored independent of its Project by Admins, Project Managers, and Developers 
  - contains a history of changes to metadata
  - contains a list of comments that Users make
  - contains attatchments for different document formats (images, PDFs)
- ### Dashboard
  - contains charts displaying general information about Project/Ticket distributions
  - lists all the Users in the system
  
## Pending
- NullReferenceExceptions in AllTickets and AllProjects Views when querying items with no assigned PM/Devs (for PM and Dev roles). 
This bug is fixed, but Github Actions limit was reached, so awaiting next month for reset.
