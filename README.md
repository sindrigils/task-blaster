# Azure
I deployed the project to Azure here is the url for both API's and the WebApp (I have also send an invitation to you, for the repository). I also did the Devops part so feel free to check that out.

API - https://task-blaster-api.azurewebsites.net/
NotificationsAPI - https://task-blaster-notifications.azurewebsites.net/
WebApp - https://taskblasterwebapp-bmdrhegdfhfad7bt.northeurope-01.azurewebsites.net/

# Locally/Docker (Just in case you want to also run this locally)
However if you are going to run this locally just know that the Auth0 wont work, since the callback/logout urls are pointed to Azure.
In both appsettings, I have set the ConnectionsStings for both Docker and Azure (the current one set is the Docker one).

You probably have a better way then this but this is how I updated the database with migrations and ran the population script.

- Compose up the project with docker compose up --build
- Go into both appsettings.json and change the where the Host points to in the connection string it should be changed to "Host=localhost"
- do the dotnet ef database update command and also execute the population_script
- change back to what the host pointed at before changing into "localhost" and now it works :) - found this out here https://github.com/instructure/lti_tool_provider_example/issues/4#issuecomment-473415087


# Useless Info
When you assign a user (on a task that is assign with another user) it will overwrite that user and send him an You got unassign email and send a You got assign email to the new user.
Just letting you know incase you get surprised if you receive two emails when you test this, I did it like this since it makes the most sense to me and would probably be like this in the real world.