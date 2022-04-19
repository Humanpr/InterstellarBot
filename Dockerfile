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
RUN wget https://file-examples.com/storage/fe9c293a7d625b5089ed6cf/2017/11/file_example_WAV_2MG.wav -o sampleaudio.wav
RUN tar xvf ffmpeg-git-amd64-static.tar.xz

FROM base AS final
ARG TweeterKeys__ACCESS_TOKEN
ARG TweeterKeys__ACCESS_TOKEN_SECRET
ARG TweeterKeys__API_KEY
ARG TweeterKeys__API_KEY_SECRET
ARG TweeterKeys__BEARER

ENV TweeterKeys__ACCESS_TOKEN=$TweeterKeys__ACCESS_TOKEN
ENV TweeterKeys__ACCESS_TOKEN_SECRET=$TweeterKeys__ACCESS_TOKEN_SECRET
ENV TweeterKeys__API_KEY=$TweeterKeys__API_KEY
ENV TweeterKeys__API_KEY_SECRET=$TweeterKeys__API_KEY_SECRET
ENV TweeterKeys__BEARER=$TweeterKeys__BEARER

ENV MediaIO__SampleAudio=/app/sampleaudio.wav
ENV MediaIO__ProcessedMediaLocation=/app/processed/

WORKDIR /app
COPY --from=publish /app/publish .
COPY --from=ffmpeg ffmpeg-git-20220302-amd64-static/ffmpeg /usr/local/bin/
COPY --from=ffmpeg ffmpeg-git-20220302-amd64-static/ffprobe /usr/local/bin/
COPY --from=ffmpeg sampleaudio.wav /app/
ENTRYPOINT ["dotnet", "interstarbot.dll"]