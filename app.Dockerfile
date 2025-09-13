FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

COPY Data/Models/ /app/Data/Models/

COPY ./Cut-Roll-Users/src/Cut-Roll-Users.Api/*.csproj ./Cut-Roll-Users/src/Cut-Roll-Users.Api/
COPY ./Cut-Roll-Users/src/Cut-Roll-Users.Infrastructure/*.csproj ./Cut-Roll-Users/src/Cut-Roll-Users.Infrastructure/
COPY ./Cut-Roll-Users/src/Cut-Roll-Users.Core/*.csproj ./Cut-Roll-Users/src/Cut-Roll-Users.Core/

COPY . .

RUN dotnet publish Cut-Roll-Users/src/Cut-Roll-Users.Api/Cut-Roll-Users.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .
COPY --from=build /app/Data/Models/ /app/Data/Models/

ENTRYPOINT [ "dotnet", "Cut-Roll-Users.Api.dll" ]