FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ExpenseWeb/ExpenseWeb.csproj", "ExpenseWeb/"]
RUN dotnet restore "ExpenseWeb/ExpenseWeb.csproj"

COPY . .
WORKDIR /src/ExpenseWeb
RUN dotnet publish "ExpenseWeb.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Render injeta a variável PORT automaticamente (default: 10000)
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "ExpenseWeb.dll"]
