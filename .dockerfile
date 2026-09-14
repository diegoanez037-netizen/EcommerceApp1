FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY ["EcommerceApp/EcommerceApp.csproj", "EcommerceApp/"]

RUN dotnet restore "EcommerceApp/EcommerceApp.csproj"

COPY . .

WORKDIR "/src/EcommerceApp"

RUN dotnet publish "EcommerceApp.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:10000

EXPOSE 10000

ENTRYPOINT ["dotnet", "EcommerceApp.dll"]