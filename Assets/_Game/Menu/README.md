# Menu

`MainMenu.unity` es la escena de entrada del juego.

- `MainMenuController` construye los paneles Jugar, selector de niveles, Opciones, Creditos y Salir.
- `MenuSettingsService` guarda el volumen maestro y el estado de silencio con `PlayerPrefs`.
- El menu permite cargar `Beach`, `Ciudad`, `Desert` y `Forest` desde el selector de niveles.
- `LevelSelection` contiene la vista de pantalla completa, las tarjetas animadas y el arte vectorial reutilizable de cada entorno.
- `Art/Backgrounds/LEVEL SELECTOR` contiene el fondo general `BG- LEVEL SELECTOR` y las imagenes `BGE-*` de cada tarjeta.
- `Loading` carga los niveles de forma asincrona y muestra progreso, porcentaje y consejos sobre `BG-LOADSCREEN`.
- `Pause` se inyecta automaticamente en las escenas de juego y ofrece Reanudar, Opciones y Salir al menu principal.
- `Scripts/UI` contiene iconos vectoriales, paneles redondeados, tipografias, transiciones, hover, parallax y audio de interfaz reutilizable.

El menu no depende de los sistemas de vuelo ni modifica sus scripts.
