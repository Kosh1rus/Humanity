reagent-effect-condition-guidebook-total-damage =
    { $max ->
        [2147483648] суммарный урон цели не меньше {NATURALFIXED($min, 2)}
       *[other] { $min ->
            [0] суммарный урон цели не больше {NATURALFIXED($max, 2)}
           *[other] суммарный урон цели составляет от {NATURALFIXED($min, 2)} до {NATURALFIXED($max, 2)}
        }
    }

reagent-effect-condition-guidebook-total-hunger =
    { $max ->
        [2147483648] уровень голода цели не меньше {NATURALFIXED($min, 2)}
       *[other] { $min ->
            [0] уровень голода цели не больше {NATURALFIXED($max, 2)}
           *[other] уровень голода цели составляет от {NATURALFIXED($min, 2)} до {NATURALFIXED($max, 2)}
        }
    }

reagent-effect-condition-guidebook-reagent-threshold =
    { $max ->
        [2147483648] реагента «{$reagent}» не меньше {NATURALFIXED($min, 2)} ед.
       *[other] { $min ->
            [0] реагента «{$reagent}» не больше {NATURALFIXED($max, 2)} ед.
           *[other] реагента «{$reagent}» от {NATURALFIXED($min, 2)} до {NATURALFIXED($max, 2)} ед.
        }
    }

reagent-effect-condition-guidebook-mob-state-condition = цель находится в состоянии «{$state}»
reagent-effect-condition-guidebook-job-condition = должность цели — {$job}

reagent-effect-condition-guidebook-solution-temperature =
    температура раствора { $max ->
        [2147483648] не ниже {NATURALFIXED($min, 2)} К
       *[other] { $min ->
            [0] не выше {NATURALFIXED($max, 2)} К
           *[other] от {NATURALFIXED($min, 2)} до {NATURALFIXED($max, 2)} К
        }
    }

reagent-effect-condition-guidebook-body-temperature =
    температура тела { $max ->
        [2147483648] не ниже {NATURALFIXED($min, 2)} К
       *[other] { $min ->
            [0] не выше {NATURALFIXED($max, 2)} К
           *[other] от {NATURALFIXED($min, 2)} до {NATURALFIXED($max, 2)} К
        }
    }

reagent-effect-condition-guidebook-organ-type =
    орган, перерабатывающий вещество, { $shouldhave ->
        [true] относится
       *[false] не относится
    } к типу «{$name}»

reagent-effect-condition-guidebook-has-tag =
    у цели { $invert ->
        [true] нет
       *[false] есть
    } метки «{$tag}»

reagent-effect-condition-guidebook-this-reagent = этот реагент
