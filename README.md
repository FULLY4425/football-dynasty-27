# Football Dynasty 27 — migración a Unity

Proyecto fuente para **Unity 6.3 LTS, 6000.3.26f1**, con Universal Render Pipeline 17.3.0. Esta versión migra los datos y el motor de carrera a C# y prepara una escena de simulación 11 contra 11. El acabado visual continúa siendo de prototipo: usar Unity no sustituye el trabajo de modelado, animación y dirección artística.

## Estado real de la entrega

Unity Hub 3.22.3 se instaló correctamente en este PC. No se completó la instalación/activación del editor. Su CLI empaquetada no pudo ejecutarse (acceso denegado), y la apertura automática de Hub fue rechazada por la política del entorno. No se realizó una compilación de Unity ni una comprobación visual del proyecto; las capturas de Godot no sirven como prueba de esta migración.

El núcleo de C# compiló con .NET 8 y pasó **101 comprobaciones**. Esto valida la carrera, no el renderizador, APIs de Unity, controles, audio, rendimiento, shaders, ni PhysX.

## Abrir en Unity

1. Abre Unity Hub desde Inicio, inicia sesión con tu Unity ID y activa la licencia correspondiente a tu cuenta.
2. Instala Unity **6000.3.26f1** desde Installs. No hace falta instalar Visual Studio ni módulos móviles para la primera revisión del editor. Para exportar Android se necesitará Android Build Support, SDK/NDK y OpenJDK.
3. Projects → Add → selecciona esta carpeta `football-dynasty-unity`.
4. Espera a que Package Manager instale URP. El script de preparación creará `Assets/Scenes/Football.unity` y la configuración URP. Si no ocurre, usa el menú **Football Dynasty → Crear escena de simulación**.
5. Abre esa escena y pulsa Play. Elige club, prepara el once y pulsa **Jugar próximo partido**.
6. Exportación Windows: menú **Football Dynasty → Exportar Windows**, tras resolver los posibles errores de importación/compilación.

Referencia oficial de instalación y cuenta: https://docs.unity.com/en-us/hub/install-hub-win-mac

## Código preparado

- Datos: 136 clubes y 4.006 jugadores. Se conserva la base 2026/27 de Godot, Messi/Inter Miami, Cristiano/Al Nassr y 12 leyendas históricas. Los atributos, valores y contratos son parámetros del juego. Las leyendas están separadas de las plantillas actuales.
- Carrera: seis ligas, cinco copas nacionales, Champions y dos Liguillas; calendario, clasificación, simulación, once, fichajes al propietario y guardado JSON versionado.
- Unión entre modos: el marcador del próximo partido de liga se entrega a `Career.Advance`, que simula los demás encuentros y avanza una sola jornada. Se verificó esta unión en el núcleo C#; el recorrido completo desde Unity no se ejecutó.
- Física: balón de 430 g y 22 agentes con Rigidbody. Control con aceleración, frenado, sprint y energía, giros, pases y tiros. IA elemental de presión y apoyo; porteros que se desplazan cerca de su línea.
- Campo de 105 × 68 m, porterías de 7,32 × 2,44 m, postes físicos, gradas perimetrales y público con instancing. Materiales, sombras, textura procedural de césped y cámara televisiva.
- Interfaz provisional: elección de club, cartas con seis atributos, once en tres formaciones, mercado paginado con búsqueda, oferta al propietario, clasificación y resultados de torneos. Está escrita con IMGUI como base funcional; no tiene aún el diseño visual terminado. Los controles táctiles de partido se añadieron mediante Canvas; su verificación en dispositivo sigue pendiente.

Controles preparados: WASD/flechas mover; Shift correr; J pase; K/espacio tiro; L cambiar jugador; P pausa. Al finalizar un partido, Guardar resultado y continuar carrera. Salir descarta el partido en curso. Guardado en `Application.persistentDataPath/career-unity-v1.json`, separado de Godot y de la versión web.

## Arquitectura y pruebas

`Assets/Scripts/Domain`: modelos y reglas C# independientes de Unity. `Runtime`: adaptadores de escena, física, público y UI. `Assets/Editor/ProjectSetup.cs`: configuración automática y exportación. `Assets/Resources/database-2026.json`: catálogo. `Tests/CoreHarness`: ejecutable de comprobación sin paquetes externos.

```powershell
dotnet run --project Tests/CoreHarness/CoreHarness.csproj
```

Comprueba seis calendarios sin duplicados, tres formaciones, rechazo de un once duplicado, compras de Messi/Cristiano/Zidane, propiedad, fondos y primas, ocho campeones, 189 encuentros de Champions, balance 4+4, clasificaciones completas, guardado y marcador jugado. También comprueba una segunda temporada completa, continuidad determinista tras cargar y rechazo de guardados inválidos sin alterar la partida. La resolución de paquetes URP y la escena se deben verificar en Unity.

## Pendiente para un simulador más realista

Primero: instalar/activar editor, compilar, resolver incompatibilidades y probar visualmente el partido. Después: modelos humanos con rig, animaciones de carrera/pases/tiros/porteros, sincronización de pies y balón, colisiones y tackles, entrenador/IA táctica, árbitro, faltas, fueras de juego, saques reglamentarios, córners, descanso y cambios de lado, sustituciones, audio, narración y acabado/validación de los controles móviles. El balón que sale por la banda se devuelve al campo: todavía es una regla arcade. La simulación no reproduce reglamentos ni calendarios oficiales completos.

Las copas solo usan los clubes disponibles de primera división; Liguilla sin play-in; sorteo continental sin restricciones de país. Se pueden encadenar temporadas con el botón Iniciar siguiente temporada tras finalizar las 40 jornadas. Se conservan propietarios, fondos y once; las competiciones se reinician. Los participantes continentales permanecen en el catálogo inicial: clasificación continental futura, premios, envejecimiento y vencimiento de contratos siguen pendientes. Cantera, renovaciones detalladas y todos los módulos de Godot/web aún no están portados. Los guardados previos se conservan en sus proyectos y no se importan automáticamente.

Los modelos generados siguen siendo sencillos; no hay mocap ni recursos gráficos extraídos de FC o DLS. El proyecto no es todavía un ejecutable Unity listo para jugar ni un simulador terminado.

## Repeticiones y realismo

Se añadieron repetición de los últimos diez segundos, último gol, cámara lenta, barra de tiempo, tres cámaras y biblioteca de repeticiones guardadas. También se prepararon vuelo con giro/resistencia, dispersión por habilidad/fatiga, despeje básico del portero y balón texturizado. Consulta `REPETICIONES.md` para uso, arquitectura y límites. Estos componentes de Unity están pendientes de compilación y comprobación visual en el editor.

## Configuración gráfica PC/móvil

Consulta `GUIA-URP-PC-MOVIL.md`. El proyecto añade perfiles URP PC_High/Mobile_60, césped de dos triángulos con mapas de detalle y normales, cámara Cinemachine 3.1.7 y Canvas táctil 1920×1080 con joystick y A/B/C. Los assets se crean desde Preparar gráficos URP en el editor. Falta instalarlo, compilar contra Unity y verificar imagen/rendimiento real. La base de césped procedural no se presenta como material fotorrealista final.

## Inicio de carrera: entrenador y equipo

Se añadió un asistente Canvas de cuatro pasos con tres personajes originales, atuendos y accesorios, datos del entrenador y selección de liga/club. Conserva el perfil y el equipo en el guardado. Consulta `CREAR-ENTRENADOR.md` para funcionamiento, fuentes históricas y límites de la vista previa 3D. Los avatares son provisionales; el flujo aún necesita prueba dentro del editor.

## Dynasty IQ · Inteligencia táctica

Roles configurables, posesión compartida y máquina de estados conectados al movimiento sin balón. El extremo entra por dentro desde la banda opuesta y el creador ofrece apoyo corto. Consulta `TACTICAS.md` para uso, código, pruebas y la comprobación preparada en Play. Total del núcleo: 101 pruebas aprobadas; la ejecución de Unity sigue pendiente.
