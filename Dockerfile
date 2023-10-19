FROM alpine:latest

WORKDIR /app
COPY ./PhotoOrganizer/bin/Release/netcoreapp7.0/ .

RUN apk add dotnet7-runtime exiftool
RUN rm 'exiftool(-k).exe'
RUN ln -s /usr/bin/exiftool './exiftool(-k).exe'
RUN ./'exiftool(-k).exe' -ver
ENTRYPOINT ["/bin/sh"]
