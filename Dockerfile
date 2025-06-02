# Stage 1: Build the application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Copy csproj and restore as distinct layers
COPY *.sln .
COPY ./*.csproj ./AvaloniaPdbAccounts/
# Copy other projects if they are referenced
# COPY OtherProject/*.csproj ./OtherProject/
RUN dotnet restore AvaloniaPdbAccounts/AvaloniaPdbAccounts.csproj

# Copy everything else and build app
COPY . .
WORKDIR /source/AvaloniaPdbAccounts
# RUN dotnet publish -c Release -o /app --no-restore
RUN dotnet run

# Stage 2: Create the runtime image
FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app

# Copy the published application from the build stage
COPY --from=build /app .

# Set up for X11 forwarding (if your base image doesn't have these)
# RUN apt-get update && apt-get install -y libx11-6 libxext6 libxrender1 libfontconfig1 libice6 libsm6 && rm -rf /var/lib/apt/lists/*

# Set the entry point for the application
ENTRYPOINT ["dotnet", "AvaloniaPdbAccounts.dll"]