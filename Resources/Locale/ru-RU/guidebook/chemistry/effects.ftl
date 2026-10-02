-create-3rd-person =
    { $chance ->
        [1] Создаёт
        *[other] создать
    }

-cause-3rd-person =
    { $chance ->
        [1] Вызывает
        *[other] вызывать
    }

-satiate-3rd-person =
    { $chance ->
        [1] Насыщает
        *[other] насытить
    }

reagent-effect-guidebook-create-entity-reaction-effect =
    { $chance ->
        [1] Создаёт
       *[other] создать
    } { $amount ->
        [1] {$entname}
       *[other] {$amount} предмета типа «{$entname}»
    }

reagent-effect-guidebook-explosion-reaction-effect =
    { $chance ->
        [1] Вызывает
       *[other] вызвать
    } взрыв

reagent-effect-guidebook-emp-reaction-effect =
    { $chance ->
        [1] Вызывает
       *[other] вызвать
    } электромагнитный импульс

reagent-effect-guidebook-flash-reaction-effect =
    { $chance ->
        [1] Вызывает
       *[other] вызвать
    } ослепляющую вспышку

reagent-effect-guidebook-foam-area-reaction-effect =
    { $chance ->
        [1] Создаёт
       *[other] создать
    } большое количество пены

reagent-effect-guidebook-smoke-area-reaction-effect =
    { $chance ->
        [1] Создаёт
       *[other] создать
    } большое количество дыма

reagent-effect-guidebook-satiate-thirst =
    { $chance ->
        [1] Утоляет
       *[other] утолить
    } жажду { $relative ->
        [1] с обычной эффективностью
       *[other] с коэффициентом {NATURALFIXED($relative, 3)} от обычной эффективности
    }

reagent-effect-guidebook-satiate-hunger =
    { $chance ->
        [1] Утоляет
       *[other] утолить
    } голод { $relative ->
        [1] с обычной эффективностью
       *[other] с коэффициентом {NATURALFIXED($relative, 3)} от обычной эффективности
    }

reagent-effect-guidebook-health-change =
    { $chance ->
        [1] { $healsordeals ->
            [heals] Лечит
            [deals] Наносит урон
           *[both] Меняет здоровье на
        }
       *[other] { $healsordeals ->
            [heals] лечить
            [deals] нанести урон
           *[both] изменить здоровье на
        }
    } {$changes}

reagent-effect-guidebook-status-effect =
    { $type ->
        [add] { $chance ->
            [1] Накладывает
           *[other] наложить
        } эффект «{LOC($key)}» минимум на {NATURALFIXED($time, 3)} с с накоплением длительности
        [remove] { $chance ->
            [1] Снимает
           *[other] снять
        } {NATURALFIXED($time, 3)} с действия эффекта «{LOC($key)}»
       *[set] { $chance ->
            [1] Накладывает
           *[other] наложить
        } эффект «{LOC($key)}» минимум на {NATURALFIXED($time, 3)} с без накопления длительности
    }

reagent-effect-guidebook-activate-artifact =
    { $chance ->
        [1] Пытается
       *[other] попытаться
    } активировать артефакт

reagent-effect-guidebook-set-solution-temperature-effect =
    { $chance ->
        [1] Устанавливает
       *[other] установить
    } температуру раствора на отметке {NATURALFIXED($temperature, 2)} К

reagent-effect-guidebook-adjust-solution-temperature-effect =
    { $chance ->
        [1] { $deltasign ->
            [1] Нагревает
           *[-1] Охлаждает
        }
       *[other] { $deltasign ->
            [1] нагреть
           *[-1] охладить
        }
    } раствор { $deltasign ->
        [1] не выше {NATURALFIXED($maxtemp, 2)} К
       *[-1] не ниже {NATURALFIXED($mintemp, 2)} К
    }

reagent-effect-guidebook-adjust-reagent-reagent =
    { $chance ->
        [1] { $deltasign ->
            [1] Добавляет
           *[-1] Удаляет
        }
       *[other] { $deltasign ->
            [1] добавить
           *[-1] удалить
        }
    } {NATURALFIXED($amount, 2)} ед. реагента «{$reagent}» { $deltasign ->
        [1] в раствор
       *[-1] из раствора
    }

reagent-effect-guidebook-adjust-reagent-group =
    { $chance ->
        [1] { $deltasign ->
            [1] Добавляет
           *[-1] Удаляет
        }
       *[other] { $deltasign ->
            [1] добавить
           *[-1] удалить
        }
    } {NATURALFIXED($amount, 2)} ед. реагентов группы «{$group}» { $deltasign ->
        [1] в раствор
       *[-1] из раствора
    }

reagent-effect-guidebook-adjust-temperature =
    { $chance ->
        [1] { $deltasign ->
            [1] Добавляет
           *[-1] Отводит
        }
       *[other] { $deltasign ->
            [1] добавить
           *[-1] отвести
        }
    } {POWERJOULES($amount)} тепла { $deltasign ->
        [1] телу
       *[-1] от тела
    }

reagent-effect-guidebook-chem-cause-disease =
    { $chance ->
        [1] Вызывает
       *[other] вызвать
    } болезнь «{$disease}»

reagent-effect-guidebook-chem-cause-random-disease =
    { $chance ->
        [1] Вызывает
       *[other] вызвать
    } одну из болезней: {$diseases}

reagent-effect-guidebook-jittering =
    { $chance ->
        [1] Вызывает
       *[other] вызвать
    } дрожь

reagent-effect-guidebook-chem-clean-bloodstream =
    { $chance ->
        [1] Очищает
       *[other] очистить
    } кровь от других химических веществ

reagent-effect-guidebook-cure-disease =
    { $chance ->
        [1] Лечит
       *[other] вылечить
    } болезни

reagent-effect-guidebook-cure-eye-damage =
    { $chance ->
        [1] { $deltasign ->
            [1] Повреждает
           *[-1] Лечит
        }
       *[other] { $deltasign ->
            [1] повредить
           *[-1] вылечить
        }
    } глаза

reagent-effect-guidebook-chem-vomit =
    { $chance ->
        [1] Вызывает
       *[other] вызвать
    } рвоту

reagent-effect-guidebook-create-gas =
    { $chance ->
        [1] Создаёт
       *[other] создать
    } {$moles} моль газа «{$gas}»

reagent-effect-guidebook-drunk =
    { $chance ->
        [1] Вызывает
       *[other] вызвать
    } опьянение

reagent-effect-guidebook-electrocute =
    { $chance ->
        [1] Поражает
       *[other] поразить
    } организм током на {NATURALFIXED($time, 3)} с

reagent-effect-guidebook-extinguish-reaction =
    { $chance ->
        [1] Тушит
       *[other] потушить
    } огонь

reagent-effect-guidebook-flammable-reaction =
    { $chance ->
        [1] Повышает
       *[other] повысить
    } горючесть

reagent-effect-guidebook-ignite =
    { $chance ->
        [1] Поджигает
       *[other] поджечь
    } организм

reagent-effect-guidebook-make-sentient =
    { $chance ->
        [1] Наделяет
       *[other] наделить
    } существо разумом

reagent-effect-guidebook-make-polymorph =
    { $chance ->
        [1] Превращает
       *[other] превратить
    } существо в {$entityname}

reagent-effect-guidebook-modify-bleed-amount =
    { $chance ->
        [1] { $deltasign ->
            [1] Усиливает
           *[-1] Ослабляет
        }
       *[other] { $deltasign ->
            [1] усилить
           *[-1] ослабить
        }
    } кровотечение

reagent-effect-guidebook-modify-blood-level =
    { $chance ->
        [1] { $deltasign ->
            [1] Повышает
           *[-1] Понижает
        }
       *[other] { $deltasign ->
            [1] повысить
           *[-1] понизить
        }
    } уровень крови

reagent-effect-guidebook-paralyze =
    { $chance ->
        [1] Парализует
       *[other] парализовать
    } существо минимум на {NATURALFIXED($time, 3)} с

reagent-effect-guidebook-movespeed-modifier =
    { $chance ->
        [1] Изменяет
       *[other] изменить
    } скорость передвижения в {NATURALFIXED($walkspeed, 3)} раза минимум на {NATURALFIXED($time, 3)} с

reagent-effect-guidebook-reset-narcolepsy =
    { $chance ->
        [1] Временно подавляет
       *[other] временно подавить
    } нарколепсию

reagent-effect-guidebook-wash-cream-pie-reaction =
    { $chance ->
        [1] Смывает
       *[other] смыть
    } крем от пирога с лица

reagent-effect-guidebook-cure-zombie-infection =
    { $chance ->
        [1] Лечит
       *[other] вылечить
    } зомби-инфекцию

reagent-effect-guidebook-cause-zombie-infection =
    { $chance ->
        [1] Заражает
       *[other] заразить
    } зомби-инфекцией

reagent-effect-guidebook-innoculate-zombie-infection =
    { $chance ->
        [1] Лечит зомби-инфекцию и обеспечивает иммунитет к повторному заражению
       *[other] вылечить зомби-инфекцию и обеспечить иммунитет к повторному заражению
    }

reagent-effect-guidebook-reduce-rotting =
    { $chance ->
        [1] Восстанавливает
       *[other] восстановить
    } {NATURALFIXED($time, 3)} с разложения тканей

reagent-effect-guidebook-area-reaction =
    { $chance ->
        [1] Создаёт
       *[other] создать
    } облако дыма или пены на {NATURALFIXED($duration, 3)} с

reagent-effect-guidebook-add-to-solution-reaction =
    { $chance ->
        [1] Переносит
       *[other] перенести
    } нанесённые на предмет вещества в его внутренний резервуар

reagent-effect-guidebook-plant-attribute =
    { $chance ->
        [1] Меняет
       *[other] изменить
    } показатель «{$attribute}» на [color={$colorName}]{$amount}[/color]

reagent-effect-guidebook-plant-cryoxadone =
    { $chance ->
        [1] Омолаживает
       *[other] омолодить
    } растение с учётом его возраста и времени роста

reagent-effect-guidebook-plant-phalanximine =
    { $chance ->
        [1] Восстанавливает
       *[other] восстановить
    } жизнеспособность растения, утраченную из-за мутации

reagent-effect-guidebook-plant-diethylamine =
    { $chance ->
        [1] Увеличивает
       *[other] увеличить
    } срок жизни и/или базовое здоровье растения; шанс каждого эффекта — 10%

reagent-effect-guidebook-plant-robust-harvest =
    { $chance ->
        [1] Повышает
       *[other] повысить
    } силу растения на {$increase}, но не выше {$limit}. При достижении {$seedlesstreshold} растение теряет семена. Попытка превысить {$limit} может снизить урожайность с вероятностью 10%

reagent-effect-guidebook-plant-seeds-add =
    { $chance ->
        [1] Возвращает
       *[other] вернуть
    } растению способность давать семена

reagent-effect-guidebook-plant-seeds-remove =
    { $chance ->
        [1] Лишает
       *[other] лишить
    } растение семян
