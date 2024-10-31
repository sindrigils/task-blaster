# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution file
COPY ../TaskBlaster.TaskManagement.sln ./

# Copy only the necessary project folders
COPY TaskBlaster.TaskManagement.Notifications/ TaskBlaster.TaskManagement.Notifications/
COPY TaskBlaster.TaskManagement.DAL/ TaskBlaster.TaskManagement.DAL/
COPY TaskBlaster.TaskManagement.Models/ TaskBlaster.TaskManagement.Models/

# Restore dependencies
WORKDIR /src/TaskBlaster.TaskManagement.Notifications
RUN dotnet restore

# Build the project
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Set environment variables and expose port
ENV ASPNETCORE_URLS=http://0.0.0.0:80
EXPOSE 80

# Run the application
ENTRYPOINT ["dotnet", "TaskBlaster.TaskManagement.Notifications.dll"]
