# GUÍA DE USO DEL SISTEMA DE MINIJUEGOS
## Sistema completo de progreso y flujo entre escenas

---

## 📋 ÍNDICE DE SCRIPTS

### Scripts Principales (Singletons)
1. **MinigameProgressManager** - Gestor de progreso global
2. **PrefabManagerSingleton** - Gestor de objetos seleccionados
3. **ActualizadorIngredientes** - Actualiza visibilidad en nivel principal

### Scripts de Minijuegos
4. **UtensilioMinijuego** - Minijuego que cambia de escena
5. **UtensilioMinijuegoTimer** - Minijuego con timer en la misma escena
6. **MinijuegoFinalizador** - Helper para finalizar minijuegos en otras escenas

### Scripts de Gestión
7. **IngredienteCondicional** - Control de visibilidad de ingredientes
8. **SpritePlayerMovement** - Sistema de arrastre de objetos
9. **LateralScroll** - Scroll lateral de la escena

---

## 🚀 CONFIGURACIÓN INICIAL (Paso a paso)

### PASO 1: Configurar Escena Principal

1. **Crear GameObject con MinigameProgressManager**
   ```
   - Crear GameObject vacío → "GameProgressManager"
   - Añadir script: MinigameProgressManager
   - En Inspector:
     * Marcar "Guardar Progreso": TRUE
     * Lista de Minijuegos → Añadir nombres de escenas:
       - "MinijuegoCortar"
       - "MinijuegoMezclar"
       - "MinijuegoHornear"
       (Los nombres deben coincidir EXACTAMENTE con los nombres de escenas)
   ```

2. **Crear GameObject con PrefabManagerSingleton**
   ```
   - Crear GameObject vacío → "PrefabManager"
   - Añadir script: PrefabManagerSingleton
   - No requiere configuración adicional
   ```

3. **Crear GameObject con ActualizadorIngredientes**
   ```
   - Crear GameObject vacío → "IngredienteUpdater"
   - Añadir script: ActualizadorIngredientes
   - No requiere configuración adicional
   ```

---

### PASO 2: Configurar Ingredientes

1. **Crear Tag "Ingrediente"**
   ```
   - Edit → Project Settings → Tags and Layers
   - Añadir nuevo Tag: "Ingrediente"
   ```

2. **Preparar GameObjects de ingredientes**
   ```
   - Seleccionar cada ingrediente en la jerarquía
   - En Inspector → Tag → Seleccionar "Ingrediente"
   - Asegurarse de que tengan Collider (para detección)
   ```

3. **Configurar grupos condicionales**
   ```
   - Crear GameObject vacío → "Ingredientes_Grupo1_Antes"
   - Añadir script: IngredienteCondicional
   - Configurar:
     * minijuegoRequerido: "MinijuegoCortar" (nombre exacto)
     * mostrarAntesDeCompletar: TRUE
     * mostrarDespuesDeCompletar: FALSE
     * Ingredientes (lista): Arrastrar ingredientes iniciales
   
   - Crear GameObject vacío → "Ingredientes_Grupo1_Despues"
   - Añadir script: IngredienteCondicional
   - Configurar:
     * minijuegoRequerido: "MinijuegoCortar"
     * mostrarAntesDeCompletar: FALSE
     * mostrarDespuesDeCompletar: TRUE
     * Ingredientes (lista): Arrastrar ingredientes nuevos
   ```

---

### PASO 3A: Configurar Utensilio con Cambio de Escena

```
UTENSILIO → Añadir script: UtensilioMinijuego

Inspector:
- Configuración del Minijuego:
  * escenaMinijuego: "MinijuegoCortar"
  
- Ingredientes Requeridos:
  * Tamaño: 3 (ejemplo)
  * Elemento 0: Arrastrar ingrediente "Tomate"
  * Elemento 1: Arrastrar ingrediente "Queso"
  * Elemento 2: Arrastrar ingrediente "Pan"
  
- Feedback Visual:
  * colorCorrecto: Verde (0.3, 1, 0.3, 0.5)
  * colorIncorrecto: Rojo (1, 0.3, 0.3, 0.5)
  
- Configuración de Detección:
  * radioDeteccion: 2
  * tagIngrediente: "Ingrediente"
  * ignorarTag: FALSE
  
- Audio (opcional):
  * Arrastrar AudioClips si deseas
```

---

### PASO 3B: Configurar Utensilio con Timer (mismo escena)

```
UTENSILIO → Añadir script: UtensilioMinijuegoTimer

Inspector:
- Configuración del Minijuego:
  * nombreMinijuego: "MinijuegoHornear" (DEBE estar en lista del ProgressManager)
  
- Ingredientes:
  * ingredienteRequerido: Arrastrar "Masa"
  * ingredienteSiGanas: Arrastrar "Pan"
  * ingredienteSiPierdes: Arrastrar "MasaQuemada"
  
- Configuración del Timer:
  * tiempoEspera: 10 (segundos)
  * margenExito: 3 (segundos de ventana para extraer)
  
- Configuración de Detección:
  * radioDeteccion: 2
  * tagIngrediente: "Ingrediente"
  
- UI del Timer:
  * timerOffset: (0, 0, 2) → delante del utensilio
  * fuenteTimer: Arrastrar fuente (opcional)
  
- Feedback Visual:
  * Configurar colores a gusto
```

---

### PASO 4: Configurar Escenas de Minijuegos

1. **En la escena del minijuego (ej: MinijuegoCortar.unity)**
   ```
   - Crear GameObject vacío → "MinijuegoController"
   - Añadir script: MinijuegoFinalizador
   - Configurar:
     * escenaNivelPrincipal: "NivelPrincipal" (nombre de tu escena principal)
     * delayAntesDeVolver: 2
   ```

2. **Al finalizar el minijuego en tu código**
   ```csharp
   // Si el jugador GANA:
   MinijuegoFinalizador finalizador = FindObjectOfType<MinijuegoFinalizador>();
   int puntos = 85; // Tu sistema de puntuación
   finalizador.CompletarMinijuego(puntos);
   
   // Si el jugador PIERDE:
   finalizador.FallarMinijuego();
   ```

3. **Añadir escenas a Build Settings**
   ```
   - File → Build Settings
   - Arrastrar todas las escenas (principal + minijuegos)
   ```

---

## 🎮 FLUJO DE JUEGO COMPLETO

### Ejemplo 1: Minijuego con Cambio de Escena

```
1. Jugador arrastra "Tomate" al utensilio → Feedback verde
2. Jugador arrastra "Queso" al utensilio → Feedback verde
3. Jugador arrastra "Pan" al utensilio → Feedback verde
4. Todos los ingredientes colocados → Carga escena "MinijuegoCortar"
5. En el minijuego, el jugador juega
6. Al terminar:
   - Si gana: MinijuegoFinalizador.CompletarMinijuego(puntos)
   - Si pierde: MinijuegoFinalizador.FallarMinijuego()
7. Vuelve a escena principal
8. ActualizadorIngredientes actualiza visibilidad automáticamente
9. Si ganó: aparecen ingredientes nuevos, desaparecen los viejos
   Si perdió: ingredientes se quedan igual
```

### Ejemplo 2: Minijuego con Timer (misma escena)

```
1. Jugador arrastra "Masa" al "Horno"
2. Timer de 10 segundos aparece delante del horno
3. Jugador puede:
   a) Click en timer antes de tiempo → Masa vuelve, timer reinicia
   b) Esperar a que llegue a 0 → Timer muestra "¡LISTO!" (3 seg ventana)
   c) Click durante ventana → ¡GANA! Aparece "Pan"
   d) No hacer click → ¡PIERDE! Aparece "MasaQuemada"
4. Mientras tanto, puede ir a otros minijuegos
5. Timer sigue corriendo en tiempo real
6. Al volver, timer muestra tiempo correcto
```

---

## 🔧 MÉTODOS ÚTILES DEL PROGRESSMANAGER

```csharp
// Consultar estado
bool completado = MinigameProgressManager.Instance.EstaCompletado("MinijuegoCortar");
int puntos = MinigameProgressManager.Instance.ObtenerPuntuacion("MinijuegoCortar");
int intentos = MinigameProgressManager.Instance.ObtenerIntentos("MinijuegoCortar");

// Reiniciar progreso
MinigameProgressManager.Instance.ReiniciarMinijuego("MinijuegoCortar");
MinigameProgressManager.Instance.ReiniciarTodoElProgreso(); // BORRA TODO

// Estadísticas globales
int total = MinigameProgressManager.Instance.ObtenerPuntuacionTotal();
int completados = MinigameProgressManager.Instance.ContarMinijuegosCompletados();
```

---

## ❗ CHECKLIST DE PROBLEMAS COMUNES

- [ ] EventSystem existe en la escena (se crea automático con MainMenuController)
- [ ] Todos los ingredientes tienen Tag "Ingrediente"
- [ ] Ingredientes tienen Collider para detección
- [ ] Nombres de minijuegos coinciden EXACTAMENTE en:
  - MinigameProgressManager (lista)
  - UtensilioMinijuego (escenaMinijuego)
  - UtensilioMinijuegoTimer (nombreMinijuego)
  - IngredienteCondicional (minijuegoRequerido)
- [ ] Escenas añadidas a Build Settings
- [ ] Cámara tiene tag "MainCamera"
- [ ] PrefabManagerSingleton existe en escena

---

## 📁 ESTRUCTURA RECOMENDADA

```
Escena Principal (NivelPrincipal.unity)
├── GameProgressManager (MinigameProgressManager)
├── PrefabManager (PrefabManagerSingleton)
├── IngredienteUpdater (ActualizadorIngredientes)
├── Utensilios/
│   ├── Horno (UtensilioMinijuegoTimer)
│   ├── Sarten (UtensilioMinijuego)
│   └── Cuchillo (UtensilioMinijuego)
└── Ingredientes/
    ├── GrupoInicial (IngredienteCondicional)
    │   ├── Tomate
    │   └── Queso
    └── GrupoNivel2 (IngredienteCondicional)
        ├── Pan
        └── Lechuga

Escena Minijuego (MinijuegoCortar.unity)
└── MinijuegoController (MinijuegoFinalizador)
```

---

## 💡 TIPS Y TRUCOS

1. **Debug rápido**: En Inspector de MinigameProgressManager, usa ContextMenu:
   - Click derecho → "Reiniciar Todo El Progreso"

2. **Ver lista de minijuegos**: Ejecuta el juego, ve a IngredienteCondicional
   - Campo "Ayuda" muestra todos los minijuegos disponibles

3. **Gizmos**: Selecciona utensilio en Scene view
   - Círculo amarillo = Radio de detección
   - Cubo amarillo = Posición del timer

4. **Timers persistentes**: Los timers usan tiempo REAL del sistema
   - Funcionan incluso si cierras el juego
   - PlayerPrefs guarda el progreso

---

Creado para ElGustoEsMio
Sistema de Minijuegos v1.0
