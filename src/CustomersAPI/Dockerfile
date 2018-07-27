FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 80

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY src/CustomersAPI/CustomersAPI.csproj src/CustomersAPI/
COPY src/CustomersDataModels/CustomersDataModels.csproj src/CustomersDataModels/
COPY src/RequestCorrelation/RequestCorrelation.csproj src/RequestCorrelation/
COPY src/ApplicationInsightsInitializers/ApplicationInsightsInitializers.csproj src/ApplicationInsightsInitializers/
RUN dotnet restore src/CustomersAPI/CustomersAPI.csproj
COPY . .
WORKDIR /src/src/CustomersAPI
RUN dotnet build CustomersAPI.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish CustomersAPI.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "CustomersAPI.dll"]
