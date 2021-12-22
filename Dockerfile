FROM devops/mdbook as builder
COPY book.toml .
COPY src ./src
RUN pwd && ls -lha && mdbook build

FROM nginx:alpine as runtime
COPY --from=builder /data/book /usr/share/nginx/html