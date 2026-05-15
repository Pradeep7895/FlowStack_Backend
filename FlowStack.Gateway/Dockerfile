FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FlowStack.Gateway/FlowStack.Gateway.csproj", "FlowStack.Gateway/"]
RUN dotnet restore "FlowStack.Gateway/FlowStack.Gateway.csproj"
COPY . .
WORKDIR "/src/FlowStack.Gateway"
RUN dotnet build "FlowStack.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FlowStack.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FlowStack.Gateway.dll"]