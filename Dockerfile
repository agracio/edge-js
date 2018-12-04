FROM ubuntu:bionic

# update apt-get
RUN apt-get -y update > /dev/null
RUN apt-get -y upgrade > /dev/null

# install curl, sudo, wget
RUN apt-get install -y curl sudo wget > /dev/null

RUN mkdir /devvol
VOLUME /devvol

RUN apt-get -y update > /dev/null

# install dependencies
RUN apt-get install -y apt-transport-https build-essential libgconf-2-4 python git libglib2.0-dev > /dev/null

# install node

RUN curl -sL https://deb.nodesource.com/setup_8.x | sudo -E bash -
RUN apt-get install -y nodejs > /dev/null

# install net core

RUN wget -qO- https://packages.microsoft.com/keys/microsoft.asc | gpg --dearmor > microsoft.asc.gpg
RUN sudo mv microsoft.asc.gpg /etc/apt/trusted.gpg.d/
RUN wget -q https://packages.microsoft.com/config/ubuntu/18.04/prod.list 
RUN sudo mv prod.list /etc/apt/sources.list.d/microsoft-prod.list
RUN sudo chown root:root /etc/apt/trusted.gpg.d/microsoft.asc.gpg
RUN sudo chown root:root /etc/apt/sources.list.d/microsoft-prod.list

RUN apt-get -y update > /dev/null
RUN apt-get install -y dotnet-sdk-2.1  > /dev/null

RUN npm i -g node-gyp