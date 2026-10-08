# Humanity

<p align="center"><img alt="Humanity" width="60%" src="Resources/Textures/Logo/splash.png" /></p>

**Humanity** - Уникальная сборка которая переносит игровой опыт [CIV13](https://github.com/civ13/civ13) в Space Station 14

**[Наш Discord](https://discord.gg/zRPUHmaEE)**

## Сборка и запуск

Нужны **Git**, **Python 3** и **.NET SDK 10**. Команды выполняются из корня репозитория.

```powershell
git submodule update --init --recursive
dotnet build Content.Server -c Release
dotnet build Content.Client -c Release
```

Запуск сервера:

```powershell
dotnet run --project Content.Server -c Release --no-build -- --config-file Resources/ConfigPresets/Civ/production.toml
```

В другом терминале запустите клиент и подключитесь к `localhost`:

```powershell
dotnet run --project Content.Client -c Release --no-build
```

На Windows можно использовать `START_SERVER.bat` и `START_CLIENT.bat`. После изменения исходников сначала повторите сборку.

## Участие в разработке

Об ошибках сообщайте в Discord или задачах репозитория: опишите действия для повторения и приложите журнал при сбое запуска.

Тексты для игроков пишем на русском с учётом контекста. Новые C# файлы размещаем в каталоге `Humanity` соответствующего проекта.

## Лицензии

Код репозитория распространяется по лицензии [MIT](LICENSE.TXT). Это относится к исходному коду SS14 и последующим изменениям.

Большинство ресурсов распространяется по лицензии [CC BY-SA 3.0](https://creativecommons.org/licenses/by-sa/3.0/), если рядом с ресурсом не указано иное. Сведения об авторах и лицензиях находятся в `meta.json` и `attributions.txt`.

Часть ресурсов использует [CC BY-NC-SA 3.0](https://creativecommons.org/licenses/by-nc-sa/3.0/) или другие лицензии, запрещающие коммерческое использование. Для коммерческого использования такие ресурсы потребуется исключить или получить разрешение правообладателей.

Ресурсы из Civ13 распространяются по лицензии [GNU AGPLv3](https://opensource.org/license/agpl-v3). Условия для конкретных файлов указаны в сопроводительных сведениях об авторах и лицензиях.
