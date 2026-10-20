FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["AllergyScan.WebAPI/AllergyScan.WebAPI.csproj", "AllergyScan.WebAPI/"]
COPY ["AllergyScan.BLL/AllergyScan.BLL.csproj", "AllergyScan.BLL/"]
COPY ["AllergyScan.DAL/AllergyScan.DAL.csproj", "AllergyScan.DAL/"]

RUN dotnet restore "AllergyScan.WebAPI/AllergyScan.WebAPI.csproj"

COPY . .
WORKDIR /src/AllergyScan.WebAPI
RUN dotnet publish "AllergyScan.WebAPI.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app
ENV ASPNETCORE_HTTP_PORTS=8080
ENV DOTNET_EnableDiagnostics=0
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AllergyScan.WebAPI.dll"]
