FROM mcr.microsoft.com/dotnet/sdk:7.0
WORKDIR /build

COPY ./src/ ./
RUN dotnet restore
RUN dotnet build

RUN dotnet tool install --global dotnet-ef

COPY ./docker/production/migrator/migrate.sh ./

CMD ["sh", "./migrate.sh"]

