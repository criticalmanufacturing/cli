# Base image
FROM mcr.microsoft.com/devcontainers/base:ubuntu-24.04
ARG UID=1000

# Install dependencies
RUN apt-get update && \
    apt-get install -y iputils-ping git wget curl gnupg2 software-properties-common apt-transport-https

# Install .NET 6 and .NET 8 SDKs
RUN wget https://packages.microsoft.com/config/ubuntu/24.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    rm packages-microsoft-prod.deb && \
    add-apt-repository ppa:dotnet/backports && \
    apt-get update && \
    apt-get install -y dotnet-sdk-8.0 dotnet-sdk-6.0

# Install Node.js 12
RUN NODE_VERSION=12.22.12 && \
    curl -LJo /tmp/node-v${NODE_VERSION}-linux-x64.tar.xz https://nodejs.org/dist/latest-v12.x/node-v${NODE_VERSION}-linux-x64.tar.xz && \
    tar xvf /tmp/node-v${NODE_VERSION}-linux-x64.tar.xz -C /tmp && \
    bash -c "cp -r /tmp/node-v${NODE_VERSION}-linux-x64/{bin,include,lib,share} /usr/"

# Create a user to use the container
RUN mkdir -p /etc/sudoers.d && \
    # useradd -u ${UID} -m -s /bin/bash vscode && \ # part of devcontainer base image
    echo "vscode ALL=(ALL) NOPASSWD:ALL" >> /etc/sudoers.d/nopasswd && \
    chmod 0440 /etc/sudoers.d/nopasswd

# Set the user to vscode
USER vscode

RUN echo "alias 'cmf=/src/cmf-cli/bin/Debug/cmf'" >> /home/vscode/.bashrc

# Set the working directory
WORKDIR /src

# Run bash by default
CMD [ "bash" ]
