FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY src src/
RUN dotnet restore src/CustomersMVC/CustomersMVC.csproj
RUN dotnet build src/CustomersMVC/CustomersMVC.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish src/CustomersMVC/CustomersMVC.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "CustomersMVC.dll"]
