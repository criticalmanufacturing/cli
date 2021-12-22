FROM rust:1-slim
ARG MDBOOK_VERSION="0.4.13"
LABEL maintainer="Marco Silva <marcosilva@criticalmanufacturing.com>" \
      version=$MDBOOK_VERSION

RUN cargo install mdbook --vers ${MDBOOK_VERSION}

WORKDIR /data
VOLUME ["/data"]