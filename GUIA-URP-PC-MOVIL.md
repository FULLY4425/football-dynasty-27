# Implementación visual URP para Football Dynasty 27

Proyecto objetivo: Unity 6.3 LTS 6000.3.26f1, URP 17.3.0, Cinemachine 3.1.7 y Unity UI 2.0.0. Los scripts están integrados en el proyecto existente. El editor de Unity aún no está instalado: no hay compilación de las APIs de Unity, captura nueva ni medición de rendimiento. La comprobación de sintaxis y las pruebas del dominio son verificaciones diferentes.

La dirección visual propuesta usa césped con normal mapping, luz principal suave, cámara externa de transmisión y controles móviles discretos. El material generado es una base procedural. Para un acabado fotorrealista hay que sustituir el detalle por texturas PBR de césped de calidad, y también mejorar jugadores, animaciones y estadio. Cambiar el motor o activar normales no produce automáticamente la apariencia de un juego comercial.

## 1. Preparación y ruta de integración

1. Instala y activa el editor indicado. Abre esta carpeta desde Unity Hub.
2. Window → Package Manager: verifica Universal RP 17.3.0, Cinemachine 3.1.7 y Unity UI 2.0.0. El manifest ya solicita esos paquetes.
3. Project Settings → Player → Other Settings: Color Space **Linear**, Active Input Handling **Input Manager (Old)** o **Both**. Esta integración mantiene el Input Manager para conservar los controles existentes y usa StandaloneInputModule. Si se migra todo a Input System, sustituye tanto el lector del teclado como el módulo del EventSystem; no añadas un segundo módulo simultáneo.
4. Ejecuta **Football Dynasty → Preparar gráficos URP**. Crea PC_High, Mobile_60, renderer y material de césped dentro de Assets/Resources/Graphics y Assets/Settings. Los mapas PNG se generan e importan en este paso.
5. Ejecuta **Crear escena de simulación** si aún no hay Football.unity. El arranque conecta calidad, césped, luz, Cinemachine y UI; no añadas copias manuales de esos componentes a este mismo simulador. Los objetos del partido y el Canvas se crean al pulsar Play.

GraphicsSetup solo crea los perfiles/material cuando faltan: conserva tus cambios posteriores de Inspector. Por eso editar sus constantes después de generarlos no actualiza automáticamente los assets existentes.

## 2. URP Asset, renderer y Quality Settings

Usa Universal Renderer en **Forward**, con SRP Batcher habilitado. Para este estadio y una luz principal, empieza con Forward antes de añadir complejidad. Asigna PC_High en Project Settings → Graphics → Default Render Pipeline.

En Project Settings → Quality crea niveles **PC High** y **Mobile 60**. Asigna a cada uno su Render Pipeline Asset. Marca PC High como predeterminado de Windows y Mobile 60 como predeterminado de Android/iOS. Mantén ambos perfiles referenciados en Quality para que la construcción considere sus variantes de shader. El gestor también selecciona el perfil correcto durante la ejecución y usa una copia, preservando el asset.

| Ajuste | PC_High | Mobile_60 |
|---|---:|---:|
| Render Scale | 1,00 | 0,85 inicial; mínimo 0,65 |
| MSAA | 4× | 2× |
| HDR | Activado | Desactivado |
| Main Light Shadows | Activadas | Activadas |
| Resolución de sombras principal | 4096 | 2048 |
| Cascadas | 4 | 2 |
| Distancia de sombras | 110 m | 80 m |
| Soft Shadows | High | Low |
| Sombras de luces adicionales | Desactivadas | Desactivadas |
| Additional Lights | Disabled inicialmente | Disabled |
| LOD Bias durante ejecución | 1,5 | 0,75 |

La distancia móvil de 80 m conserva sombra bajo jugadores visibles con esta cámara alta. Bajar a 25 m puede hacer que desaparezcan las sombras del campo aunque el jugador esté cerca en pantalla. Ajusta distancia y reparto de cascadas mirando una captura de los extremos del campo.

QualityManager fija objetivo 60 FPS y VSync Count 0. En móvil baja Render Scale tras dos ventanas de dos segundos por encima de 19 ms, y recupera lentamente tras cinco ventanas por debajo de 16,8 ms. No cambia calidad en cada fotograma. El Canvas mantiene la resolución de pantalla porque Render Scale afecta al render 3D.

En PC, si eliges VSync Count 1 para evitar tearing, el monitor gobierna la frecuencia y targetFrameRate deja de ser el límite efectivo. En móvil la frecuencia de la pantalla y sus divisores afectan al resultado: un objetivo de 60 no garantiza 60 en una pantalla configurada a 90 Hz. Comprueba el modo de pantalla, frame pacing y rendimiento real.

## 3. Césped nítido con normales y franjas

StadiumTurf crea un plano de **105 × 68 m**, cuatro vértices, dos triángulos, UV 0–1 y tangentes. El collider del suelo ya existe. No se crean miles de hojas ni diez cubos para representar las franjas. Las líneas blancas permanecen encima del plano.

En Assets/Resources/Graphics/StadiumTurf.mat:

1. Shader **Universal Render Pipeline/Lit**, Surface Opaque, Metallic 0, Smoothness aproximadamente 0,12; Receive Shadows activado.
2. Base Map **GrassStripes** y Base Color blanco; diez franjas alternadas distribuidas a lo largo del eje X de 105 m. El patrón no depende de la cámara: si quieres franjas perpendiculares, cambia el mapa o sus UV, no los controles.
3. Surface Inputs → Normal Map **GrassFlatNormal**, para activar la ruta de normal mapping.
4. Detail Inputs → Base Map **GrassDetail**, Normal Map **GrassDetailNormal**, fuerza inicial **0,55**, Tiling **70 × 45,3333**. Esto equivale a un mosaico de detalle cada 1,5 m.
5. No necesitas Detail Mask para cubrir todo el campo. El detalle gris neutro modula las franjas sin sustituir sus dos verdes.
6. Importa normales como **Normal Map**, no como textura de color. Usa Repeat, mipmaps, Trilinear y anisotropía 8. El detalle generado está en espacio lineal; el color base utiliza sRGB.
7. Para el acabado final, reemplaza el detalle procedural por mapas tileables de césped capturado/PBR. Neutraliza el color del mapa de detalle antes de usarlo como multiplicador para conservar las franjas. Prueba 1–2K en PC y 512–1K ASTC 6×6 en móvil. Los mapas normales utilizan el importador de normales para sus canales y compresión.

Ajusta los verdes bajo la iluminación final; evita saturación extrema y reflejos húmedos si quieres un campo seco. No uses normales con fuerza excesiva: pueden producir parpadeo a distancia. Las normales describen la superficie, no cambian la silueta de las hojas; la vista televisiva se beneficia más que una cámara pegada al suelo.

## 4. Lighting y sombras de jugadores

El simulador conserva una sola Directional Light. StadiumLighting configura:

- Rotación **(48, -28, 0)**, intensidad **1,15**, tono ligeramente cálido.
- Shadows **Soft**, Strength **0,8**, Bias **0,03**, Normal Bias **0,25** como punto de partida.
- Ambiente **Trilight**, cielo azul desaturado, horizonte moderado y rebote del suelo verde oscuro.
- La luz se asigna a RenderSettings.sun; el césped recibe sombras y los MeshRenderers de jugadores las proyectan.

Inspecciona pies, balón y campo. Si la sombra flota, baja Normal Bias gradualmente; si aparece acne, ajusta bias y resolución antes de desactivar sombras. El público instanciado no proyecta miles de sombras. Móvil usa aproximadamente la mitad de los espectadores.

Cuando el estadio sea un prefab colocado en el editor, activa Contribute GI en las gradas, prepara UV de lightmap, light probes para jugadores y reflection probes limitadas. Hornea iluminación indirecta de arquitectura y conserva la sombra directa de la luz principal en tiempo real. El estadio actual se genera en Play: no se puede hornear su geometría antes de crearla en una escena de edición.

Opcional en PC: Global Volume con ACES o Neutral, contraste +5 y Bloom muy moderado. La cámara habilita postprocesado en PC. Empieza sin SSAO y sin Motion Blur. Mobile 60 desactiva HDR y postprocesado: conserva claridad mediante material e iluminación. No uses el Bloom para ocultar materiales de mala calidad.

## 5. Cinemachine: cámara externa de transmisión

El script BroadcastCamera usa Cinemachine 3, namespace Unity.Cinemachine. No mezcles ejemplos de CinemachineVirtualCamera de la versión 2.

Configuración equivalente en Inspector:

1. Main Camera con **Cinemachine Brain**; etiqueta MainCamera. Una sola cámara renderiza el partido.
2. Objeto **Cinemachine Camera** con Tracking Target/Follow y Look At apuntando a **Broadcast focus**.
3. Position Control **Follow**; Binding Mode **World Space**, Follow Offset **(0, 24, 36)**, Position Damping **(0,35; 0,2; 0,35)**.
4. Rotation Control **Hard Look At**. El enfoque combina principalmente balón y un 18 % del jugador controlado, con límites cerca de las gradas.
5. Lens: Perspective, Field of View **50°** durante juego y transición a **45°** al terminar; rango elegido 45–55°, Near Clip 0,1 y Far Clip 350.
6. Usa interpolación en Rigidbody y el objetivo visual interpolado para reducir temblor. Mantén la cámara fuera del código de movimiento físico.

Es una vista externa de transmisión, orientada al campo. Para una vista detrás del jugador necesitarías otro rig y otro encuadre, pero no es la cámara principal solicitada para retransmisión deportiva.

La integración elimina el antiguo LateUpdate que movía la cámara. MatchReplay desactiva el Brain al entrar en repetición y lo restaura al salir; así sus tres cámaras no compiten con Cinemachine. Las cámaras de repetición cercanas conservan su configuración propia.

## 6. Canvas, joystick y botones A/B/C

MobileMatchUI crea:

- Canvas **Screen Space – Overlay**, Sorting Order 20, GraphicRaycaster.
- CanvasScaler **Scale With Screen Size**, Reference Resolution **1920 × 1080**, Match Width or Height **0,5**.
- Panel Safe Area que sigue Screen.safeArea al cambiar resolución/orientación.
- Joystick abajo a la izquierda, diámetro 270, handle 105, margen de posición (200,200), alpha de fondo 0,38.
- Tres botones circulares a la derecha, diámetro 130, alpha 0,48: **A tiro**, **B pase**, **C cambio de jugador**.

VirtualJoystick usa coordenadas del RectTransform y el dedo capturado, no Input.mousePosition. Normaliza el radio, aplica zona muerta radial de 0,12 y entrega Vector2 analógico. TouchActionButton conserva una pulsación pendiente hasta que el partido la consume; otro dedo puede continuar arrastrando el joystick. Las etiquetas y el handle tienen Raycast Target desactivado.

MatchInput convierte movimiento a ejes de cámara proyectados sobre el suelo. La magnitud del joystick conserva velocidad parcial y la diagonal no aumenta la velocidad máxima. Llevar el joystick más allá del 85 % activa sprint en móvil. PC conserva WASD/flechas, Shift, J, K/Espacio y L.

El Canvas se oculta automáticamente en PC, menús, final del partido y durante repetición. En Editor puedes activar **Preview In Editor** en MobileMatchUI durante Play para verificar composición con el ratón; multitouch se verifica en dispositivo. Pérdida de foco, pausa de aplicación y ocultación resetean controles para evitar entradas atascadas.

Player Settings → Resolution and Presentation: Landscape, permite Landscape Left/Right y desactiva Portrait si no se va a diseñar ese formato. El escritorio no necesita simular una pantalla táctil para jugar.

## 7. Scripts entregados y responsabilidades

Todos están en Assets/Scripts/Runtime/Presentation salvo los indicados:

- **QualityManager.cs**: perfiles, límite de FPS, copia del asset y ajuste gradual de resolución móvil.
- **VirtualJoystick.cs** y **TouchActionButton.cs**: entrada táctil y propiedad de cada dedo.
- **MobileMatchUI.cs** y **SafeAreaPanel.cs**: Canvas, escala, botones y zona segura.
- **MatchInput.cs**: acciones comunes para PC/móvil, movimiento relativo a cámara.
- **BroadcastCamera.cs**: rig real de Cinemachine 3 e integración con replay.
- **StadiumTurf.cs** y **StadiumLighting.cs**: campo, material, luz y ambiente.
- **Assets/Editor/GraphicsSetup.cs**: creación de perfiles, material e importación de mapas.
- **Assets/Scripts/Domain/ControlMath.cs**: cálculo analógico independiente de Unity, probado con .NET.

FootballSimulator conecta todo al arrancar. FootballAgent recibe Vector3 ya calculado; su búsqueda frecuente de perseguidor usa un recorrido sin ordenar/crear listas. Los cambios no sustituyen los modelos humanos ni la IA táctica completa.

## 8. Verificación y criterio de aceptación

Antes de considerar terminada la transformación:

1. Importar paquetes y resolver todos los errores de compilación reales en Unity.
2. Captura 1920×1080: diez franjas nítidas, detalle estable, pies apoyados con sombra y FOV 50. Mirar ambas porterías y un tiro rápido.
3. Android/iOS: comprobar Safe Area, dos dedos a la vez, diagonales, soltar dedo fuera del pad, interrupción de aplicación y botones A/B/C.
4. Entrar en replay desde pausa y desde juego activo; salir sin cambio de balón, marcador o estado de pausa. Confirmar que Cinemachine no mueve la cámara de replay.
5. Build PC y al menos un Android/iPhone real con Development Build y Profiler. Medir CPU/GPU y percentiles de tiempo de frame tras 10–15 minutos para incluir calentamiento.
6. Objetivo 60 FPS: presupuesto total aproximado 16,67 ms, sin picos de GC durante control/cámara. Si limita CPU, reducir resolución no resolverá el problema; investigar agentes, física y grabación. Si limita GPU, revisar sombras, detalle y público antes de añadir efectos.
7. Comprobar que el ajuste de Render Scale no oscila y no empeora la nitidez del HUD; incluir capturas comparables PC/móvil.

Verificado ahora: **66 comprobaciones del dominio**, compilación .NET sin errores y revisión de sintaxis C# 9 de **21 archivos** sin errores. No se ha comprobado aún compilación contra Unity/Cinemachine/URP, resultado visual, multitouch ni 60 FPS.

## Documentación oficial consultada

- URP/Lit y mapas de detalle: https://docs.unity3d.com/6000.0/Documentation/Manual/urp/lit-shader.html
- URP Asset: https://docs.unity.cn/Packages/com.unity.render-pipelines.universal@17.0/manual/universalrp-asset.html
- Cinemachine Camera 3.1: https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/api/Unity.Cinemachine.CinemachineCamera.html
- Cinemachine Follow: https://docs.unity3d.com/Packages/com.unity.cinemachine@3.1/api/Unity.Cinemachine.CinemachineFollow.html
- CanvasScaler: https://docs.unity3d.com/Packages/com.unity.ugui@2.0/manual/script-CanvasScaler.html
- Objetivo FPS: https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Application-targetFrameRate.html
- Safe Area: https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Screen-safeArea.html
