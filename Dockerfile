# Sử dụng image .NET SDK để xây dựng ứng dụng
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /app

# Sao chép file csproj và khôi phục các gói NuGet
COPY *.csproj ./
RUN dotnet restore

# Sao chép tất cả các file và publish ứng dụng
COPY . ./
RUN dotnet publish *.csproj -c Release -o /out

# Sử dụng image .NET Runtime để chạy ứng dụng
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /app
COPY --from=build /out .
EXPOSE 80
ENTRYPOINT ["dotnet", "MvcMovie.dll"]