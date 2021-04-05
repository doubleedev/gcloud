FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build

# Copy the src+test
WORKDIR /app
COPY . ./
WORKDIR /app
RUN dotnet restore

# build app
WORKDIR /app
RUN dotnet publish -c Release -o out

# copy tests and run
WORKDIR /app
RUN dotnet test

# this will be the final build
FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS runtime
WORKDIR /app
# GCP AppEngine requires that port 8080 is exposed
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/tango-dev/out ./
ENTRYPOINT ["dotnet", "tango-dev.dll"]
