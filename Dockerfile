# Builds the commit at HEAD, not the working tree: the context is .git alone, so nothing untracked
# can reach a layer and GitVersion sees a clean checkout. Commit first, and build from a full clone
# with BuildKit: GitVersion needs the history, and $BUILDPLATFORM needs BuildKit.
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0-noble AS build
ARG TARGETARCH
WORKDIR /src
COPY .git .git
RUN git reset --hard HEAD
RUN arch=$([ "$TARGETARCH" = "amd64" ] && echo x64 || echo "$TARGETARCH") \
 && dotnet publish HermesProxy --configuration Release --runtime "linux-$arch" \
      -p:UsePublishBuildSettings=true --output /app \
 && mkdir -p /state/Logs /state/AccountData /state/PacketsLog

FROM mcr.microsoft.com/dotnet/runtime-deps:10.0-noble-chiseled
WORKDIR /app
COPY --from=build /app .
COPY --from=build --chown=$APP_UID:$APP_UID /state .
# A container can only be reached through its own interfaces, whatever ExternalAddress says. An
# image is updated by pulling a tag, so the startup check has nothing to tell it.
ENV HERMES_ProxyNetworkOptions__BindAddress=0.0.0.0 \
    HERMES_DiagnosticsOptions__EnableVersionCheck=false
EXPOSE 1119 8081 8084 8086
VOLUME ["/app/Logs", "/app/AccountData", "/app/PacketsLog"]
USER $APP_UID
ENTRYPOINT ["./HermesProxy"]
