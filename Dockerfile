FROM mcr.microsoft.com/dotnet/sdk:7.0-jammy-amd64  AS dotnet70
ENV DEBIAN_FRONTEND=noninteractive
WORKDIR /app
COPY ./out .
RUN apt update
RUN apt install -y libimage-exiftool-perl
RUN rm 'exiftool(-k).exe'
RUN ln -s /usr/bin/exiftool './exiftool(-k).exe'
RUN ./'exiftool(-k).exe' -ver
CMD ["./PhotoOrganizer", "-s=/cloud", "-d=/foto"]
