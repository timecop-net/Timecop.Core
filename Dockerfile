FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.sln .
COPY src/Timecop.Core/Timecop.Core.csproj ./src/Timecop.Core/Timecop.Core.csproj
COPY test/Timecop.Core.Tests/Timecop.Core.Tests.csproj ./test/Timecop.Core.Tests/Timecop.Core.Tests.csproj
RUN dotnet restore

# copy everything else and build app
COPY ./ ./
WORKDIR /source
RUN dotnet build -c release -o /out/package --no-restore

FROM build as test
ENV TZ="Africa/Johannesburg"

RUN dotnet test

FROM build as pack-and-push
WORKDIR /source

ARG PackageVersion
ARG NuGetApiKey

RUN dotnet pack ./src/Timecop.Core/Timecop.Core.csproj -o /out/package -c Release
RUN dotnet nuget push /out/package/Timecop.Core.$PackageVersion.nupkg -k $NuGetApiKey -s https://api.nuget.org/v3/index.json