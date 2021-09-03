# SkyCommands
Command and API service

Handles external Http requests and potentially forwards them to other services.

Additional configuration can be found in `appsettings.json`.
You can overwrite it via Enviroment variables. 

> **Note**: the keys represent the JSON path of a value and `:` has to be replaced with `__`. eg `TOPICS:FLIP_CONSUME` becomes `TOPICS__FLIP_CONSUME`
