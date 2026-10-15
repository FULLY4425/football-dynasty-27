# Inicio de carrera: entrenador y club

El proyecto Unity incorpora un asistente de cuatro pasos, inspirado en la distribución y los tonos azul/verde de las capturas de referencia. Es una interfaz Canvas propia; no se han extraído personajes ni recursos de esas imágenes.

## Flujo implementado

1. **Personaje:** Alex, Mateo o Samira. Cada modelo tiene tono de piel, cabello y rasgos visuales distintos. El personaje se elige de forma independiente del género y nombre del entrenador.
2. **Atuendo:** deportivo, americana o chaleco; cuatro colores superiores, tres pantalones, dos tipos de calzado, gafas y corbata. La vista previa 3D cambia al seleccionar una opción.
3. **Información:** género, nombre, apellido, apelativo opcional, país/región y fecha de nacimiento. Se validan campos, longitudes, fecha real y edad de 18 a 100 años al inicio de la temporada, el 1 de julio.
4. **Club:** cambia de liga con las flechas y elige un equipo de su lista. Hay 114 clubes seleccionables en seis ligas. La ficha muestra nombre, país, iniciales, camiseta esquemática con colores del equipo, presupuesto y plantilla. El botón Comenzar carrera guarda entrenador y club y abre el menú de carrera.

Puedes volver atrás sin perder los valores introducidos. Los menús utilizan referencia 1920×1080 y Safe Area; se han preparado para escritorio y móvil en orientación horizontal. Las etiquetas y fondos decorativos no interceptan los toques.

## Perfil y guardado

CoachProfile y managedClub son campos adicionales del guardado existente `career-unity-v1.json`. Los guardados anteriores sin entrenador conservan su progreso y abren el asistente una vez para completar el perfil. Al volver a abrir un guardado con entrenador se recuperan nombre, aspecto y club y se entra en la carrera. El perfil continúa al comenzar otra temporada.

El asistente no simula jornadas ni reinicia fondos o fichajes. Un guardado con perfil inválido se rechaza antes de aplicar cambios a la carrera. La finalización de la interfaz espera a que el archivo se haya guardado correctamente.

## Reseñas y datos del equipo

Se incluyeron siete reseñas breves con fundación y ciudad contrastadas en fuentes de los propios clubes: Real Madrid, Barcelona, Bayern, Manchester United, Juventus, PSG y América. El resto dispone de ficha e identidad básica del catálogo; su historia ampliada se señala como pendiente. No se inventaron fechas, títulos, instalaciones ni estadísticas de afición para rellenar el diseño de referencia.

Los monogramas y las camisetas esquemáticas son gráficos del menú, no escudos oficiales ni equipaciones reproducidas. Presupuestos, valoraciones y atributos son parámetros del juego, no datos financieros oficiales.

Fuentes de cada reseña: `Assets/Scripts/Domain/ClubIdentity.cs`. Se conservan las URLs de los clubes junto a los textos. Las selecciones externas de Messi/Cristiano y los clubes de leyendas siguen en el mercado, pero no aparecen como opciones de las seis ligas.

## Componentes

- `Domain/CoachProfile.cs`: perfil, apariencia, validación y copias independientes.
- `Domain/ClubIdentity.cs`: reseñas originales y fuentes verificadas.
- `Runtime/Presentation/CoachOnboarding.cs`: Canvas, navegación, formularios y selector de clubes.
- `Runtime/Presentation/CoachAvatarPreview.cs`: modelos originales procedurales, cámara separada y RenderTexture con relación 2:3. La capa 30 se reserva para esta vista previa.
- `Career`, `CareerSave` y `FootballSimulator`: persistencia, arranque y entrada al club seleccionado.

Los modelos son provisionales hechos con geometría básica; no tienen todavía el detalle humano ni la ropa con simulación de tela de un juego comercial. Se pueden sustituir por prefabs propios con rig manteniendo el perfil y el asistente.

## Verificación

84 pruebas del núcleo aprobadas. Incluyen perfil, ropa, accesorios y club a través de guardado JSON; datos inválidos sin modificar la partida; compatibilidad de guardados antiguos; rechazo de clubes externos; continuidad entre temporadas y fichas para los 114 clubes jugables. Los 25 archivos de Assets pasan análisis de sintaxis C# 9.

**Pendiente en Unity:** importar y compilar, probar Canvas en diferentes pantallas, teclado móvil, navegación Atrás/Continuar, render de los avatares, iluminación de la cámara auxiliar y comprobación visual. El editor sigue sin estar instalado; esta entrega es código fuente preparado, no un ejecutable ni una captura verificada de Unity.
