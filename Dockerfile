FROM ubuntu:24.04

# update apt-get
RUN apt -y update

ENV DEBIAN_FRONTEND=noninteractive

# install apt-utils, curl, sudo, wget
RUN apt -q -y install apt-utils curl sudo wget software-properties-common

RUN mkdir /devvol
VOLUME /devvol

# install dependencies
RUN apt -q -y install apt-transport-https build-essential python3 git libglib2.0-dev

# install node

RUN curl -sL https://deb.nodesource.com/setup_22.x | sudo -E bash -
RUN sudo apt -q -y install nodejs

# install dotnet

RUN sudo add-apt-repository ppa:dotnet/backports
RUN sudo apt update
RUN sudo apt -q -y install dotnet-sdk-9.0

RUN npm i -g node-gyp