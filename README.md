# NobelLaureatesBE

## Description
Project Goal : A project to display Nobel Laureate information

## How to run
- Set **NobelLaureatesBE.API** project as the startup project
- Restore Nuget packages
- Set proper connection string in **appSettings.json** file
- Point nuget package manager console to **NobelLaureatesBE.Repositories** project
- Add-Migration InitialCreate
- Update-Database
- Start the project

## Dependencies
- Microsoft.AspNetCore.Authentication.JwtBearer
- Microsoft.EntityFrameworkCore.SqlServer
- Microsoft.EntityFrameworkCore
- Microsoft.EntityFrameworkCore.Tools
- BCrypt.Net-Next
- Microsoft.Extensions.Http

## Project structure
![image](https://github.com/user-attachments/assets/cfb1f8ba-58ed-47b7-b0ff-13fcd752e5b2)
  
## Unit tests
![image](https://github.com/user-attachments/assets/b83a10dd-4f5a-461d-ae0c-eb0513696381)

