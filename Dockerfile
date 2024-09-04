# Base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Build image
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files
COPY ["OrderProcessWebAPI/OrderProcessWebAPI.csproj", "OrderProcessWebAPI/"]
COPY ["OrderProcess.Entities/OrderProcess.Entities.csproj", "OrderProcess.Entities/"]
COPY ["OrderProcess.DataAccess/OrderProcess.DataAccess.csproj", "OrderProcess.DataAccess/"]
COPY ["OrderProcess.Business/OrderProcess.Business.csproj", "OrderProcess.Business/"]

# Restore dependencies
RUN dotnet restore "OrderProcessWebAPI/OrderProcessWebAPI.csproj"

# Copy the rest of the files
COPY . .

# Set the working directory to the web API project
WORKDIR "/src/OrderProcessWebAPI"
RUN dotnet build "OrderProcessWebAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "OrderProcessWebAPI.csproj" -c Release -o /app/publish

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "OrderProcessWebAPI.dll"]
