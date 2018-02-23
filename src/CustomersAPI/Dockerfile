# Use a multi-stage Dockerfile to encapsulate both building
# the service and packaging it into a container in an efficient,
# easily-understood set of instructions

# Build and publish
FROM microsoft/aspnetcore-build:2.0.0 AS builder

# Move into a working directory and copy our source files
# into the container
#
# Note that this Dockerfile should be built from the top-level
# solution directory so that other projects (like CustomersDataModels)
# are available in the Docker context when building.
WORKDIR /src
COPY . .

# Publish the app
WORKDIR /src/src/CustomersAPI
RUN dotnet restore
RUN dotnet publish --no-restore -c Release -o /app

# Use a base image that extends microsoft/dotnet:2.0-runtime
# by adding some ASP.NET Core env vars and precompiled ASP.NET Core libraries.
#
# ASP.NET Core apps can just as easily work directly from a microsoft/dotnet base
# image, but the microsoft/aspnetcore images offer some perf benefits.
# https://hub.docker.com/r/microsoft/aspnetcore/
# https://hub.docker.com/r/microsoft/dotnet/
FROM microsoft/aspnetcore:2.0.5 AS final

# Set the working directory in the container
WORKDIR /app

# Expose ports that Kestrel will listen on
EXPOSE 5000

# This copies the published app from the 'publish' container
# that was created in the previous stage.
COPY --from=builder /app .

ENTRYPOINT ["dotnet", "CustomersAPI.dll"]
