#!/bin/bash

# Этот скрипт может выполняться на сервере для самостоятельной сборки

cd /var/netcore

echo "Восстановление зависимостей..."
dotnet restore

echo "Сборка проекта..."
dotnet publish -c Release -r linux-x64 --self-contained false -o ./published --no-restore

echo "Остановка сервиса..."
systemctl stop Homie.service || true

echo "Копирование файлов..."
cp -r ./published/* ./
rm -rf ./published

echo "Запуск сервиса..."
systemctl start Homie.service

echo "Готово!"