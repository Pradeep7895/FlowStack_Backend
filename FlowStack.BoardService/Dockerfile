FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FlowStack.BoardService/FlowStack.BoardService.csproj", "FlowStack.BoardService/"]
RUN dotnet restore "FlowStack.BoardService/FlowStack.BoardService.csproj"
COPY . .
WORKDIR "/src/FlowStack.BoardService"
RUN dotnet build "FlowStack.BoardService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FlowStack.BoardService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FlowStack.BoardService.dll"]