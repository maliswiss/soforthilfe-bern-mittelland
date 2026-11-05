# Schnittstelle als Container-Image.
# Build: SDK-Image, zuerst die Projektdateien (Cache für die Abhängigkeiten), danach der Quelltext.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /quelle
COPY global.json Directory.Build.props Soforthilfe.slnx ./
COPY src/Soforthilfe.Api/Soforthilfe.Api.csproj src/Soforthilfe.Api/
COPY src/Soforthilfe.Dienste/Soforthilfe.Dienste.csproj src/Soforthilfe.Dienste/
COPY src/Soforthilfe.Datenzugriff/Soforthilfe.Datenzugriff.csproj src/Soforthilfe.Datenzugriff/
RUN dotnet restore src/Soforthilfe.Api/Soforthilfe.Api.csproj
COPY src/ src/
RUN dotnet publish src/Soforthilfe.Api/Soforthilfe.Api.csproj -c Release -o /ausgabe --no-restore

# Laufzeit: nur ASP.NET Core, ohne SDK, als Benutzer ohne Administratorrechte.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS laufzeit
WORKDIR /app
COPY --from=build /ausgabe .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "Soforthilfe.Api.dll"]
