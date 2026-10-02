
roles-antag-survivor-name = Выживший

roles-antag-survivor-objective = Текущая задача: Выжить

survivor-round-end-dead-count =
    { $deadCount ->
        [one] [color=red]{ $deadCount }[/color] выживший умер.
        *[other] [color=red]{ $deadCount }[/color] выживших умерло.
    }

survivor-round-end-alive-count =
    { $aliveCount ->
        [one] [color=yellow]{ $aliveCount }[/color] выживший остался на станции.
        *[other] [color=yellow]{ $aliveCount }[/color] выживших осталось на станции.
    }

survivor-round-end-alive-on-shuttle-count =
    { $aliveCount ->
        [one] [color=green]{ $aliveCount }[/color] выживший выбрался живым.
        *[other] [color=green]{ $aliveCount }[/color] выживших выбралось живыми.
    }


objective-issuer-swf = [color=turquoise]Федерация космических волшебников[/color]

wizard-title = Волшебник

wizard-description = На станции присутствует волшебник! Никогда не знаешь, что они могут натворить.

roles-antag-wizard-name = Волшебник

roles-antag-wizard-objective = Преподайте им урок, который они никогда не забудут.

wizard-round-end-name = волшебник

survivor-role-greeting = Вы — выживший.
    Главное — добраться до Центрального командования живым.
    Соберите столько оружия, сколько понадобится для выживания.
    Не доверяйте никому.
wizard-role-greeting = ВЫ — ВОЛШЕБНИК!
    Отношения Федерации космических волшебников с NanoTrasen накалились.
    Федерация выбрала вас для визита на станцию.
    Покажите экипажу, на что способны ваши чары.
    Действуйте по своему усмотрению, но помните: Федерация хочет, чтобы вы вернулись живым.
