cmd-ban-desc = Банит кого-либо

cmd-ban-hint = <имя/ID игрока>
cmd-ban-hint-reason = <причина>
cmd-banlistF-hint = <имя/ID игрока>
cmd-ban_exemption_update-arg-player = <игрок>
cmd-ban_exemption_update-arg-flag = <флаг>
cmd-ban_exemption_get-arg-player = <игрок>
ban-panel-ip = IP
ban-panel-hwid = HWID
server-ban-string = {$admin} выдал серверный бан с уровнем серьёзности {$severity} до {$expires} для [{$name}, {$ip}, {$hwid}]. Причина: {$reason}

cmd-ban-help = Использование: ban <name or user ID> <reason> [продолжительность в минутах, без указания или 0 для пермабана]

cmd-ban-player = Не удалось найти игрока с таким именем.

cmd-ban-invalid-minutes = { $minutes } не является допустимым количеством минут!

cmd-ban-invalid-severity = { $severity } не является допустимой тяжестью!

cmd-ban-invalid-arguments = Недопустимое число аргументов

cmd-ban-hint-duration = [продолжительность]

cmd-ban-hint-severity = [тяжесть]

cmd-ban-hint-duration-1 = Навсегда

cmd-ban-hint-duration-2 = 1 день

cmd-ban-hint-duration-3 = 3 дня

cmd-ban-hint-duration-4 = 1 неделя

cmd-ban-hint-duration-5 = 2 недели

cmd-ban-hint-duration-6 = 1 месяц


cmd-banpanel-desc = Открыть панель банов

cmd-banpanel-help = Использование: banpanel [имя или guid игрока]

cmd-banpanel-server = Это не может быть использовано через консоль сервера

cmd-banpanel-player-err = Указанный игрок не может быть найден


cmd-banlist-desc = Список активных банов пользователя.

cmd-banlist-help = Использование: banlist <name or user ID>

cmd-banlist-empty = Нет активных банов у пользователя { $user }

cmd-ban_exemption_update-desc = Установить исключение на типы банов игрока.

cmd-ban_exemption_update-help = Использование: ban_exemption_update <player> <flag> [<flag> [...]]
    Укажите несколько флагов, чтобы дать игроку исключение из нескольких типов банов.
    Чтобы удалить все исключения, выполните эту команду и укажите единственным флагом "None".

cmd-ban_exemption_update-nargs = Ожидается хотя бы 2 аргумента

cmd-ban_exemption_update-locate = Не удалось найти игрока '{ $player }'.

cmd-ban_exemption_update-invalid-flag = Недопустимый флаг '{ $flag }'.

cmd-ban_exemption_update-success = Обновлены флаги исключений банов для '{ $player }' ({ $uid }).

cmd-ban_exemption_get-desc = Показать исключения банов для определённого игрока.

cmd-ban_exemption_get-help = Использование: ban_exemption_get <player>

cmd-ban_exemption_get-nargs = Ожидается ровно 1 аргумент

cmd-ban_exemption_get-none = Пользователь не имеет исключений от банов.

cmd-ban_exemption_get-show = Пользователь исключён из банов со следующими флагами: { $flags }.

ban-panel-title = Панель банов

ban-panel-player = Игрок

ban-panel-reason = Причина

ban-panel-last-conn = Использовать IP и HWID с последнего подключения?

ban-panel-submit = Забанить

ban-panel-confirm = Уверены?

ban-panel-tabs-basic = Основная инфа

ban-panel-tabs-reason = Причина

ban-panel-tabs-players = Список игроков

ban-panel-tabs-role = Инфо о банах ролей

ban-panel-no-data = Укажите пользователя, IP или HWID чтобы забанить

ban-panel-invalid-ip = Не удаётся спарсить IP адрес. Попробуйте ещё раз

ban-panel-select = Выбрать тип

ban-panel-server = Серверный бан

ban-panel-role = Бан роли

ban-panel-minutes = Минут

ban-panel-hours = Часов

ban-panel-days = Дней

ban-panel-weeks = Недель

ban-panel-months = Месяцев

ban-panel-years = Лет

ban-panel-permanent = Навсегда

ban-panel-ip-hwid-tooltip = Оставьте пустым и установите флажок ниже, чтобы использовать данные последнего подключения

ban-panel-severity = Тяжесть:

ban-panel-erase = Стереть сообщения в чате и игрока из раунда

server-ban-string-no-pii = { $admin } установил серверный бан { $severity } тяжести, который истечёт { $expires } у { $name } с причиной: { $reason }

server-ban-string-never = никогда


ban-kick-reason = Вы были забанены
