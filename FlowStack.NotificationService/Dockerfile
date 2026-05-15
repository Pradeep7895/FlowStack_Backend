FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FlowStack.NotificationService/FlowStack.NotificationService.csproj", "FlowStack.NotificationService/"]
RUN dotnet restore "FlowStack.NotificationService/FlowStack.NotificationService.csproj"
COPY . .
WORKDIR "/src/FlowStack.NotificationService"
RUN dotnet build "FlowStack.NotificationService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FlowStack.NotificationService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FlowStack.NotificationService.dll"]