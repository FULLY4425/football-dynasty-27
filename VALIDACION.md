# Estado de verificación

14 de octubre de 2026. .NET SDK 8.0.301. El ejecutable CoreHarness compiló el código real de `Assets/Scripts/Domain`, cargó los 136 clubes/4.006 jugadores y completó 61 comprobaciones correctamente.

El código de Runtime y Editor depende de Unity y URP y no se compiló con .NET. No se utilizó una imitación de las APIs de Unity para presentarlo como compilado. No hay captura visual ni build de Unity verificados.

Unity Hub 3.22.3.65535 se instaló desde el origen winget con hash del instalador verificado. Unity 6000.3.26f1/de8167e7440c fue seleccionado a partir del catálogo oficial de versiones. El editor no está instalado. La ejecución directa del CLI empaquetado de Hub devolvió acceso denegado; la apertura automática de Hub fue rechazada por la política del entorno. Se requiere abrir Hub manualmente, iniciar sesión, activar la licencia e instalar el editor para continuar.

No se cambió ni eliminó el proyecto Godot. No se migraron los archivos de guardado previos ni se modificaron contratos/carreras del usuario.

Continuación: reconstrucción completa del núcleo con cero errores y cero advertencias. Se verificaron dos temporadas, conservación de fichajes/fondos/once, continuidad del generador aleatorio al cargar y rechazo sin cambios parciales de propiedad duplicada, presupuestos infinitos, once, resultados y torneos inválidos. La interfaz incorpora el botón de siguiente temporada, pero continúa sin ejecutarse en Unity.

Repeticiones: 16 comprobaciones nuevas del dominio; el total es 61, todas aprobadas. Se guardó y reabrió un archivo real de repetición en una carpeta temporal. Estas pruebas no validan reproducción gráfica, cámaras, pausa/restauración del partido ni física Unity.

Comprobación adicional de sintaxis C# 9: los diez archivos de Assets se analizaron con Roslyn sin errores de sintaxis. No equivale a compilar contra las APIs de Unity/URP.

Integración gráfica y móvil: total actual 66 pruebas del núcleo aprobadas. Se añadieron cinco pruebas del cálculo de joystick: zona muerta, diagonal limitada, magnitud parcial, entrada no finita y dirección negativa. Veintiún archivos de Assets pasan el análisis de sintaxis C# 9. Pendientes: resolución de paquetes, compilación Unity/URP/Cinemachine, renderizado de material/normal maps, multitouch, pausa/replay con Brain y rendimiento en PC/Android/iOS.

15 de octubre de 2026: creación de entrenador y selección de equipo integradas en Unity. Total actual: 84 pruebas de dominio aprobadas (18 adicionales), compilación .NET con cero errores/advertencias; análisis sintáctico de 25 archivos C# sin errores. No valida compilación de APIs Unity ni render de Canvas/RenderTexture. Guardado probado con identidad, apariencia, accesorios y club; compatibilidad anterior y conservación de progreso comprobadas.

Tácticas: 101 pruebas de dominio aprobadas; 17 adicionales para posesión, roles, objetivos y guardado. Escenario cinemático de 500 pasos completado. 28 archivos de Assets pasan sintaxis C# 9. La prueba de escena TacticalPlayCheck está preparada pero no ejecutada; no hay verificación de partido Unity ni PhysX.
