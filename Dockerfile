# Giai đoạn 1: Chạy ứng dụng (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 1000

# Giai đoạn 2: Build SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy file .csproj để khôi phục (restore) thư viện
COPY ["AiNutritionTracking.API/AiNutritionTracking.API.csproj", "AiNutritionTracking.API/"]
RUN dotnet restore "./AiNutritionTracking.API/AiNutritionTracking.API.csproj"

# Copy toàn bộ mã nguồn vào container
COPY . .
WORKDIR "/src/AiNutritionTracking.API"
RUN dotnet build "./AiNutritionTracking.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Giai đoạn 3: Publish
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./AiNutritionTracking.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Giai đoạn 4: Final (Gói gọn để chạy)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:1000
ENTRYPOINT ["dotnet", "AiNutritionTracking.API.dll"]