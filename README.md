ConfigureOptions: Este script permite configurar opciones del juego a través de menús desplegables, como la dimensión del laberinto, la cantidad de jugadores y la cantidad de piezas, y proporciona un botón para iniciar el juego.

Characters: Define una clase para crear personajes, con atributos como imagen, facción, habilidad, descripción de la habilidad, rango y tiempo de habilidad, así como posiciones y efectos. Incluye un método para restablecer los valores originales de los personajes.

CharactersDisplay: Este script muestra la imagen del personaje en la interfaz de usuario, asignando la imagen del personaje al componente UI al iniciar la escena.

Factions: ScriptableObject que define facciones en el juego, cada una con su propia imagen y una lista de personajes asociados.

Player: Define a los jugadores del juego, con un identificador único y una lista de personajes (piezas) que selecciona cada jugador.

SelectCharacter: Gestiona la selección de personajes por parte de los jugadores, mostrando facciones y personajes, y permitiendo a los jugadores seleccionar personajes para su lista. También maneja la transición entre turnos y escenas.

AbilityActivator: Gestiona la activación de habilidades especiales de los personajes, verificando si la habilidad está lista, aplicando efectos de habilidades y gestionando el estado de enfriamiento.

GameData: Almacena información global sobre el juego, como el jugador ganador, y se mantiene entre escenas como un singleton.

GameManager: Coordina varias partes del juego, incluyendo la configuración de jugadores y personajes, la detección de movimientos, la actualización de la interfaz de usuario, y la transición de turnos. Verifica si algún jugador ha ganado el juego.

MazeGenerator: Genera el laberinto y coloca los elementos dentro de él, incluyendo muros, caminos, trampas y el centro del laberinto. Utiliza un algoritmo de búsqueda en profundidad para generar el laberinto.

Trap: ScriptableObject que define las trampas, con atributos como el tipo de trampa y la imagen de la trampa.

TrapType: Enumeración que define los diferentes tipos de trampas disponibles (Update, Ice, Hole, Teleport, Lava).

TrapDisplay: Gestiona la visualización y los efectos de las trampas en el laberinto, detectando colisiones con personajes y activando los efectos de las trampas.

TrapManager: Gestiona las trampas en el juego y aplica sus efectos a los personajes, como teletransportación, efecto de hielo, agujero y lava.

WinnerDisplay: Muestra el ID del jugador ganador y las imágenes de sus personajes en la interfaz de usuario cuando la escena comienza.

Espero que este resumen sea útil. ¿Hay algo más en lo que pueda ayudarte?

