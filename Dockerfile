FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build

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
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS runtime

# RUN apt-get update
WORKDIR /app
# GCP AppEngine requires that port 8080 is exposed
# EXPOSE 8080
# ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/out ./
ENTRYPOINT ["dotnet", "API.dll"]
