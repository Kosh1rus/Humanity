objectives-round-end-result = { $count ->
    [one] Был один { $agent }.
    [few] Было { $count } { $agent }.
    *[other] Было { $count } { $agent }.
}

objectives-player-user-named = [color=White]{$name}[/color] ([color=gray]{$user}[/color])
objectives-player-named = [color=White]{$name}[/color]
objectives-no-objectives = {$custody}{$title} — {$agent}.
objectives-objective-success = {$objective} | [color={$markupColor}]Выполнено![/color]
objectives-objective-fail = {$objective} | [color={$markupColor}]Не выполнено![/color] ({$progress}%)

objectives-round-end-result-in-custody = { $custody } из { $count } { $agent } были арестованы.

objectives-with-objectives = { $custody }{ $title } – { $agent } со следующими целями:

objectives-in-custody = [bold][color=red]| АРЕСТОВАН | [/color][/bold]
