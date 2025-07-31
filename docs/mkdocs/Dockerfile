FROM python:3.8-slim-bullseye AS mkdocs
WORKDIR /app
RUN apt update && apt install -y python3-pip python3-cffi python3-brotli libpango-1.0-0 libpangoft2-1.0-0
COPY mkdocs/requirements.txt requirements.txt
RUN pip3 install -r requirements.txt

FROM mkdocs AS builder
COPY . .
RUN mkdocs build

FROM nginx
COPY --from=builder /app/book /usr/share/nginx/html
EXPOSE 80