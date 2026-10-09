# syntax=docker/dockerfile:1
FROM python:3.13-slim-trixie AS mkdocs
WORKDIR /docs
COPY requirements.txt .
RUN pip install --no-cache-dir -r requirements.txt
COPY mkdocs.yml .
COPY src/ src/
EXPOSE 8000
CMD ["mkdocs", "serve", "--dev-addr=0.0.0.0:8000"]

FROM mkdocs AS builder
RUN mkdocs build --strict

# Export the static site without creating or starting a container.
FROM scratch AS export
COPY --from=builder /docs/site/ /

FROM nginx:stable-alpine AS web
COPY --from=builder /docs/site/ /usr/share/nginx/html/
EXPOSE 80
