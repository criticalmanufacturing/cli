VERSION 0.6
FROM alpine
WORKDIR /app

build:
    FROM python:3.8-slim-bullseye
    WORKDIR /app
    COPY mkdocs/requirements.txt requirements.txt
    RUN pip3 install -r requirements.txt
    RUN apt update && apt install -y python3-pip python3-cffi python3-brotli libpango-1.0-0 libpangoft2-1.0-0
    COPY . .
    RUN mkdocs build
    SAVE ARTIFACT /app/book /dist AS LOCAL dist
    SAVE ARTIFACT /output /output AS LOCAL dist/output

image:
    FROM nginx
    RUN touch /usr/share/nginx/html/.nojekyll # disable GitHub pages parsing
    COPY +build/dist /usr/share/nginx/html
    COPY +build/output /usr/share/nginx/html/output
    EXPOSE 80
    SAVE IMAGE cli/docs:latest