# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore dependencies
COPY ["MotorcycleRepairShop.csproj", "./"]
RUN dotnet restore "MotorcycleRepairShop.csproj"

# Copy the rest of the code and build
COPY . .
RUN dotnet build "MotorcycleRepairShop.csproj" -c Release -o /app/build

# Stage 2: Publish
FROM build AS publish
RUN dotnet publish "MotorcycleRepairShop.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 3: Final Production Image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "MotorcycleRepairShop.dll"]
