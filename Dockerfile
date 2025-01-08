FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY libgwmapi/ ./libgwmapi/
COPY ora2mqtt/ ./ora2mqtt/
ARG TARGETARCH
RUN if [ "$TARGETARCH" = "amd64" ]; then \
    RID=linux-musl-x64 ; \
    elif [ "$TARGETARCH" = "arm64" ]; then \
    RID=linux-musl-arm64 ; \
    elif [ "$TARGETARCH" = "arm" ]; then \
    RID=linux-musl-arm ; \
    fi \
    && dotnet publish -c Release -o out -r $RID --sc ora2mqtt/ora2mqtt.csproj
COPY openssl.cnf ./out/

FROM mcr.microsoft.com/dotnet/runtime-deps:8.0-alpine
WORKDIR /app
COPY --from=build-env /app/out .
COPY libgwmapi/Resources/gwm_root.pem /etc/ssl/certs/.
ENV OPENSSL_CONF=/app/openssl.cnf

ENTRYPOINT ["/app/ora2mqtt", "-c", "/config/ora2mqtt.yml"]