#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/azure-functions/dotnet:4 AS base
RUN apt-get update && apt-get install -y libgdiplus
RUN apt-get update; apt-get install -y fontconfig fonts-liberation
COPY FunctionAppSvgToPngContainer/fonts/ /usr/share/fonts/truetype
RUN fc-cache -f -v
# use "fc-list" in terminal to check what fonts got installed

# refresh system font cache
WORKDIR /home/site/wwwroot
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["FunctionAppSvgToPngContainer/FunctionAppSvgToPngContainer.csproj", "FunctionAppSvgToPngContainer/"]
RUN dotnet restore "FunctionAppSvgToPngContainer/FunctionAppSvgToPngContainer.csproj"
COPY . .
WORKDIR "/src/FunctionAppSvgToPngContainer"
RUN dotnet build "FunctionAppSvgToPngContainer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FunctionAppSvgToPngContainer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /home/site/wwwroot
COPY --from=publish /app/publish .
ENV AzureWebJobsScriptRoot=/home/site/wwwroot \
    AzureFunctionsJobHost__Logging__Console__IsEnabled=true