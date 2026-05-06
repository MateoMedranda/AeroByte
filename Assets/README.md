# AeroByte Project - Estructura de Assets

Este directorio contiene los recursos y scripts principales para el proyecto **AeroByte** ("Ostras amigos, a todo gas"). A continuación se describe la estructura de carpetas implementada para organizar el juego:

## Estructura de Carpetas

### `_Game/`
Directorio principal del juego. Contiene todos los elementos específicos de la simulación y mecánicas propias del proyecto.

*   **`Art/`**: Recursos visuales estilo "low poly" para mantener un rendimiento optimizado.
    *   `Aircrafts/`: Modelos 3D y materiales de las aeronaves. Permite la personalización visual de los aviones (cambio de material o camuflaje, por ejemplo `Mat_DesertCamo.mat`).
    *   `Environments/`: Modelos y materiales para los diferentes escenarios modulares del juego como el Océano, Ciudad, Bosque y Desierto.

*   **`Audio/`**: Recursos sonoros espaciales e inmersivos (Dynamic Music System).
    *   `Music/`: Pistas musicales dinámicas para diferentes momentos del vuelo, como pistas para crucero (condiciones estables) y pistas de alerta táctica (intercepción o tormentas).
    *   `SFX/`: Efectos de sonido con atenuación direccional y efecto Doppler. Incluye voces sintéticas de alerta (ej. "Pull Up" de un sistema GPWS) y sonidos de motores y turbinas acordes a la velocidad.

*   **`Core/`**: Sistemas fundamentales y bases de la arquitectura de software del juego.
    *   `SaveSystem/`: Manejo de serialización (JSON) de datos del jugador. Controla la economía y las ganancias obtenidas tras vuelos exitosos.
    *   `EventSystem/`: Bus de eventos global para comunicar distintas partes del juego sin acoplamiento excesivo.

*   **`Data/`**: Datos de configuración e información estructurada.
    *   `AircraftStats/`: Contiene la información técnica y de parámetros de las diferentes aeronaves (agilidad, tamaño, etc.) haciendo uso de `ScriptableObject`.

*   **`Features/`**: Módulos de jugabilidad, lógicas y sistemas principales.
    *   `FlightSystem/`: Lógicas físicas de control de la aeronave basadas en pitch, roll, yaw y potencia. 
    *   `WeatherSystem/`: Lógica del clima dinámico. Maneja el concepto de "Clima como Antagonista" mediante la aplicación de fuerzas que simulan tormentas de arena, engelamiento en alas o turbulencias severas.
    *   `MissionSystem/`: Sistemas de objetivos como el despliegue logístico de precisión. Calcula cálculos y aproximaciones para lanzar suministros o balizas.
    *   `UI_System/`: Interfaces de usuario funcionales. Incluye componentes como el Head-Up Display (HUD) dinámico de vuelo para mostrar altitud, velocidad y rumbo, y las pantallas multifunción (MFDs).

*   **`Scenes/`**: Escenas de Unity que conforman los niveles y menús del juego.
    *   Contiene los niveles independientes (Océano, Ciudad, Bosque, Desierto), escenas de pruebas físicas y de menú, así como la escena base de release.

### `Settings/`
*   Contiene configuraciones globales del proyecto de Unity, tales como los perfiles y assets de la tubería de renderizado (Render Pipeline Asset) adecuados para el apartado gráfico low poly del juego.

---
*Documento de Diseño: Versión 1.00 (Grupo 7)*
