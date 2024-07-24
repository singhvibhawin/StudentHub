# Use the official .NET SDK image for .NET 8.0 as the build environment
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

# Set the working directory inside the container
WORKDIR /app

# Copy the .csproj file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application code
COPY . ./

# Build the project
RUN dotnet build -c Release -o out

# Publish the project
RUN dotnet publish -c Release -o out

# Use the official .NET runtime image for .NET 8.0 for the final stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# Set the working directory inside the container
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app/out .

# Set the environment variable for ASP.NET Core
ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:80

# Expose port 80 to the outside world
EXPOSE 80

# Start the application
ENTRYPOINT ["dotnet", "ConnectingDatabase.dll"]