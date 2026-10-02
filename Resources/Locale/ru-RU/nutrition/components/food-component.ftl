food-you-need-to-hold-utensil = Чтобы это съесть, вам нужен подходящий прибор: {$utensil}.
food-nom = Ням. {$flavors}
food-swallow = Вы проглатываете «{$food}». {$flavors}
food-has-used-storage = Сначала выньте предмет из «{$food}».
food-system-remove-mask = Сначала снимите «{$entity}».
food-system-you-cannot-eat-any-more = Вы больше не можете есть!
food-system-you-cannot-eat-any-more-other = {CAPITALIZE(SUBJECT($target))} больше не может есть!
food-system-try-use-food-is-empty = В «{CAPITALIZE(THE($entity))}» больше ничего нет!
food-system-wrong-utensil = Этот прибор не подходит для «{THE($food)}»: {$utensil}.
food-system-cant-digest = Вы не можете переварить «{THE($entity)}»!
food-system-cant-digest-other = {CAPITALIZE(SUBJECT($target))} не может переварить «{THE($entity)}»!
food-system-verb-eat = Съесть
food-system-force-feed = {CAPITALIZE(THE($user))} пытается насильно вас накормить!
food-system-force-feed-success = { GENDER($user) ->
    [female] {CAPITALIZE(THE($user))} насильно вас накормила! {$flavors}
    [epicene] {CAPITALIZE(THE($user))} насильно вас накормили! {$flavors}
    *[other] {CAPITALIZE(THE($user))} насильно вас накормил! {$flavors}
}
food-system-force-feed-success-user = Вы накормили {THE($target)}.
