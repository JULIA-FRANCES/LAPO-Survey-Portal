FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["SurveyPortal.Api/SurveyPortal.Api.csproj", "SurveyPortal.Api/"]
RUN dotnet restore "SurveyPortal.Api/SurveyPortal.Api.csproj"

COPY . .
WORKDIR "/src/SurveyPortal.Api"
RUN dotnet publish "SurveyPortal.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_HOSTBUILDER__RELOADCONFIGONCHANGE=false
EXPOSE 10000
ENTRYPOINT ["dotnet", "SurveyPortal.Api.dll"]
