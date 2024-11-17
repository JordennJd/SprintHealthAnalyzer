# Этап 1: Сборка
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
ENV ASPNETCORE_URLS=http://+:5000 
# Копируем все файлы из текущей директории на хосте в контейнер
COPY . .

# Восстанавливаем зависимости
RUN dotnet restore "HealthSprintAnalyzer.WebApi/HealthSprintAnalyzer.WebApi.csproj"

# Компиляция всех проектов
RUN dotnet build "HealthSprintAnalyzer.WebApi/HealthSprintAnalyzer.WebApi.csproj" -c Release -o /app/build

# Этап 2: Публикация
FROM build AS publish
RUN dotnet publish "HealthSprintAnalyzer.WebApi/HealthSprintAnalyzer.WebApi.csproj" -c Release -o /app/publish

# Этап 3: Запуск
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final

WORKDIR /app

COPY --from=publish /app/publish .

EXPOSE 5000

ENTRYPOINT ["dotnet", "HealthSprintAnalyzer.WebApi.dll"]
