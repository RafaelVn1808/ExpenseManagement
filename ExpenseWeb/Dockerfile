FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ExpenseWeb/ExpenseWeb.csproj", "ExpenseWeb/"]
RUN dotnet restore "ExpenseWeb/ExpenseWeb.csproj"

COPY . .
WORKDIR /src/ExpenseWeb
RUN dotnet publish "ExpenseWeb.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8081
EXPOSE 8081

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ExpenseWeb.dll"]
