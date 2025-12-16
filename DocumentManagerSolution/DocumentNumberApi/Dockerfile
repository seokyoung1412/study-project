FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DocumentNumberApi/DocumentNumberApi.csproj", "DocumentNumberApi/"]
RUN dotnet restore "DocumentNumberApi/DocumentNumberApi.csproj"
COPY . .
WORKDIR "/src/DocumentNumberApi"
RUN dotnet build "DocumentNumberApi.csproj" -c Release -o /app/build

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/build .
ENTRYPOINT ["dotnet", "DocumentNumberApi.dll"]