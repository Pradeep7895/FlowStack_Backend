FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FlowStack.AuthService/FlowStack.AuthService.csproj", "FlowStack.AuthService/"]
RUN dotnet restore "FlowStack.AuthService/FlowStack.AuthService.csproj"
COPY . .
WORKDIR "/src/FlowStack.AuthService"
RUN dotnet build "FlowStack.AuthService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FlowStack.AuthService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FlowStack.AuthService.dll"]
