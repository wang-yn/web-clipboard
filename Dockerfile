FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
WORKDIR /src
COPY backend/backend.csproj .
RUN dotnet restore -r linux-musl-x64
COPY backend/ .
RUN dotnet publish -c Release -r linux-musl-x64 -o /app \
    --self-contained true \
    --no-restore \
    -p:PublishSingleFile=true \
    -p:PublishTrimmed=true \
    -p:TrimMode=partial \
    -p:EnableCompressionInSingleFile=true

FROM mcr.microsoft.com/dotnet/runtime-deps:9.0-alpine AS runtime
RUN apk add --no-cache icu-libs
WORKDIR /app
COPY --from=build /app/backend .
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1
ENTRYPOINT ["./backend"]