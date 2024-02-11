FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /build

COPY ./src/ ./
RUN dotnet restore

RUN dotnet publish -c Release -o out Core/Christofel.Application/Christofel.Application.csproj

FROM mcr.microsoft.com/dotnet/runtime:8.0
WORKDIR /app
COPY --from=build-env /build/out .
ENTRYPOINT ["dotnet", "Christofel.Application.dll"]
