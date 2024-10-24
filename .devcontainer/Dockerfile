# Base image
FROM ubuntu:22.04

# Install dependencies
RUN apt-get update && \
    apt-get install -y git wget curl gnupg2 software-properties-common apt-transport-https

# Install .NET 8 SDK
RUN wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb && \
    dpkg -i packages-microsoft-prod.deb && \
    rm packages-microsoft-prod.deb && \
    apt-get update && \
    apt-get install -y dotnet-sdk-8.0

# Install Node.js 20
RUN curl -fsSL https://deb.nodesource.com/setup_20.x | bash - && \
    apt-get install -y nodejs

# Create a user to use the container
RUN mkdir -p /etc/sudoers.d && \
    useradd -m -s /bin/bash vscode && \
    echo "vscode ALL=(ALL) NOPASSWD:ALL" >> /etc/sudoers.d/nopasswd && \
    chmod 0440 /etc/sudoers.d/nopasswd

# Set the user to vscode
USER vscode

# Set the working directory
WORKDIR /workspace

# Run bash by default
CMD [ "bash" ]
