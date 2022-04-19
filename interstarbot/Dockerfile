#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["interstarbot/interstarbot.csproj", "interstarbot/"]
RUN dotnet restore "interstarbot/interstarbot.csproj"
COPY . .
WORKDIR "/src/interstarbot"
RUN dotnet build "interstarbot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "interstarbot.csproj" -c Release -o /app/publish

FROM ubuntu:latest as ffmpeg
RUN apt clean && apt autoclean && apt update
RUN apt-get -y install wget
RUN apt-get -y install xz-utils
RUN wget https://johnvansickle.com/ffmpeg/builds/ffmpeg-git-amd64-static.tar.xz 
COPY ["interstarbot/sampleaudio.wav", "."]
RUN tar xvf ffmpeg-git-amd64-static.tar.xz

FROM base AS final

ENV MediaIO__SampleAudio=sampleaudio.wav
ENV MediaIO__ProcessedMediaLocation=/app
ENV MediaIO__FFMPEG=ffmpeg

WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=ffmpeg ffmpeg-git-20220302-amd64-static/ffmpeg /usr/local/bin/
COPY --from=ffmpeg ffmpeg-git-20220302-amd64-static/ffprobe /usr/local/bin/
COPY --from=ffmpeg sampleaudio.wav /app/
ENTRYPOINT ["dotnet", "interstarbot.dll"]