# Menu

`MainMenu.unity` es la escena de entrada del juego.

- `MainMenuController` construye los paneles Jugar, Opciones, Creditos y Salir.
- `MenuSettingsService` guarda el volumen maestro y el estado de silencio con `PlayerPrefs`.
- El menu carga `1. Mapa_Costa` al pulsar Jugar.
- `Scripts/UI` contiene iconos vectoriales, paneles redondeados, tipografias, transiciones, hover, parallax y audio de interfaz reutilizable.

El menu no depende de los sistemas de vuelo ni modifica sus scripts.
