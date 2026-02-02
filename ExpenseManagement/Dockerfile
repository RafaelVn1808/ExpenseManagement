FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ExpenseManagement/ExpenseApi.csproj", "ExpenseManagement/"]
RUN dotnet restore "ExpenseManagement/ExpenseApi.csproj"

COPY . .
WORKDIR /src/ExpenseManagement
RUN dotnet publish "ExpenseApi.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Render injeta a variável PORT automaticamente (default: 10000)
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
EXPOSE 8080

COPY --from=build /app/publish .
RUN mkdir -p /app/wwwroot/uploads/expenses

ENTRYPOINT ["dotnet", "ExpenseApi.dll"]
