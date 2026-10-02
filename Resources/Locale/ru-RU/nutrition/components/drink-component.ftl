drink-component-on-use-is-empty = {CAPITALIZE(THE($owner))} пуст!
drink-component-on-examine-is-empty = [color=gray]Пусто[/color]
drink-component-on-examine-is-opened = [color=yellow]Открыто[/color]
drink-component-on-examine-is-sealed = Пломба цела.
drink-component-on-examine-is-unsealed = Пломба сорвана.
drink-component-on-examine-is-full = Полная ёмкость
drink-component-on-examine-is-mostly-full = Почти полная ёмкость
drink-component-on-examine-is-half-full = Наполовину полная ёмкость
drink-component-on-examine-is-half-empty = Наполовину пустая ёмкость
drink-component-on-examine-is-mostly-empty = Почти пустая ёмкость
drink-component-on-examine-exact-volume = Содержит {$amount} ед.
drink-component-try-use-drink-not-open = Сначала откройте «{$owner}»!
drink-component-try-use-drink-is-empty = {CAPITALIZE(THE($entity))} пуст!
drink-component-try-use-drink-cannot-drink = Вы не можете ничего пить!
drink-component-try-use-drink-had-enough = Вы больше не можете пить!
drink-component-try-use-drink-cannot-drink-other = Этот персонаж не может ничего пить!
drink-component-try-use-drink-had-enough-other = Этот персонаж больше не может пить!
drink-component-try-use-drink-success-slurp = Хлюп
drink-component-try-use-drink-success-slurp-taste = Хлюп. {$flavors}
drink-component-force-feed = {CAPITALIZE(THE($user))} пытается насильно вас напоить!
drink-component-force-feed-success = { GENDER($user) ->
    [female] {CAPITALIZE(THE($user))} насильно вас напоила! {$flavors}
    [epicene] {CAPITALIZE(THE($user))} насильно вас напоили! {$flavors}
    *[other] {CAPITALIZE(THE($user))} насильно вас напоил! {$flavors}
}
drink-component-force-feed-success-user = Вы напоили {THE($target)}.
drink-system-verb-drink = Выпить
