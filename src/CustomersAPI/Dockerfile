# Use a base image that extends microsoft/dotnet:1.0-runtime
# by adding some ASP.NET Core env vars and precompiled ASP.NET Core libraries.
#
# ASP.NET Core apps can just as easily work directly from a microsoft/dotnet base
# image, but the microsoft/aspnetcore images offer some perf benefits.
# https://hub.docker.com/r/microsoft/aspnetcore/
# https://hub.docker.com/r/microsoft/dotnet/
FROM microsoft/aspnetcore:2.0.0

# The optional source arg is just to allow
# overriding the location that published output
# is copied from.
ARG source

WORKDIR /app

# Expose ports that Kestrel will listen on
EXPOSE 5000

# Notice that, by default, this copies
# from a folder that's populated by docker-compose.ci.build.
# Depending on how you're building, this path may need updated
# (or a source explicitly passed).
COPY ${source:-obj/Docker/publish} .

ENTRYPOINT ["dotnet", "CustomersAPI.dll"]
