
comp-mind-ghosting-prevented = Вы не можете стать призраком в данный момент.


comp-mind-examined-dead = { CAPITALIZE(SUBJECT($ent)) } { GENDER($ent) ->
    [male] мёртв
    [female] мертва
    [epicene] мертвы
    *[neuter] мертво
}

comp-mind-examined-ssd = { CAPITALIZE(SUBJECT($ent)) } рассеяно смотрит в пустоту и ни на что не реагирует. { CAPITALIZE(SUBJECT($ent)) } может скоро придти в себя.

comp-mind-examined-dead-and-ssd = { CAPITALIZE(POSS-ADJ($ent)) } душа бездействует и может скоро вернуться.
comp-mind-examined-catatonic = {CAPITALIZE(SUBJECT($ent))} пребывает в глубокой кататонии. Похоже, жизнь в дальнем космосе оказалась слишком тяжёлой. Выздоровление маловероятно.
comp-mind-examined-dead-and-irrecoverable = Душа покинула тело персонажа {THE($ent)}. Выздоровление маловероятно.
