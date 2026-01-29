FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Copy only the API project file
COPY src/EmployeeManagement.API/EmployeeManagement.API.csproj ./src/EmployeeManagement.API/

# Restore only API dependencies
RUN dotnet restore src/EmployeeManagement.API/EmployeeManagement.API.csproj

# Copy the rest of the source code
COPY src ./src

# Publish API
RUN dotnet publish src/EmployeeManagement.API/EmployeeManagement.API.csproj \
    -c Release \
    -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "EmployeeManagement.API.dll"]