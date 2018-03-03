FROM microsoft/dotnet:2.1-sdk as build
WORKDIR /src
COPY ./GenericHostApp.csproj .
RUN dotnet restore
COPY . .
RUN dotnet publish -o /out

FROM microsoft/aspnetcore:2.1.0-preview1
WORKDIR /app
COPY --from=build /out .
ENTRYPOINT ["dotnet", "GenericHostApp.dll"]