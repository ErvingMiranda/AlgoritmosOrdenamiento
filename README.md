# Algoritmos de Ordenamiento (WinForms, C#)

Aplicación Windows Forms (.NET 8) para visualizar y comparar algoritmos de ordenamiento con fines educativos. Permite generar datos aleatorios, ver una animación simultánea de los algoritmos y comparar tiempos de ejecución en milisegundos, segundos y ticks.

— Equipo: NullException · Tarea para la clase de Programación Estructurada —

---

## Qué hay en este repositorio

- Interfaz WinForms en `BusquedaYOrdenamientoDemo` con el formulario principal `Form1` y una ventana de ayuda `HelpForm`.
- Algoritmos implementados y comparados:
  - Burbuja (Bubble Sort)
  - Inserción (Insertion Sort)
  - QuickSort
- Visualización en tiempo real con resaltado de comparaciones/intercambios y control de velocidad.
- Modo rápido sin animación para mediciones instantáneas.
- Tabla de resultados con ganador resaltado y columnas: Algoritmo, ms, s, ticks.

Archivos principales del proyecto:

- `AlgoritmosOrdenamiento.sln` (solución)
- `AlgoritmosOrdenamiento.csproj` (WinForms, `net8.0-windows`)
- `Form1.cs`, `Form1.Designer.cs`, `Form1.resx` (UI principal y lógica)
- `HelpForm.cs` (ayuda/guía de uso y glosario)
- `Program.cs` (punto de entrada)

Namespace: `BusquedaYOrdenamientoDemo`.

---

## Requisitos

- Windows 10/11 (WinForms, destino `net8.0-windows`).
- .NET 8 SDK.

Verifica la versión instalada:

```bash
dotnet --version
```

---

## Cómo ejecutar

Opción A — Visual Studio (recomendado en Windows):
- Abre `AlgoritmosOrdenamiento.sln` y presiona Ejecutar (F5).

Opción B — CLI de .NET (en Windows):
```bash
dotnet build
dotnet run
```

Nota: Al ser WinForms, la ejecución requiere Windows.

---

## Uso rápido

- Generar datos: elige la cantidad y pulsa “Generar”.
- Comparar algoritmos: pulsa “Comparar Algoritmos”.
  - Visualizar carrera: animación simultánea de Burbuja, Inserción y QuickSort con control de velocidad.
  - Resultados rápidos: medición instantánea sin animación y visualización del estado final en los paneles.
- Cancelar: detiene la animación en curso.
- F11: alterna pantalla completa.
- “Ayuda”: abre `HelpForm` con recomendaciones y glosario.

Sugerencia: la visualización es más fluida con conjuntos de hasta ~500 elementos.

---

## Complejidad (implementados)

- Burbuja: mejor $O(n)$, promedio/peor $O(n^2)$; estable; in-place.
- Inserción: mejor $O(n)$, promedio/peor $O(n^2)$; estable; in-place.
- QuickSort: promedio $O(n\log n)$, peor $O(n^2)$; no estable; in-place.

---

## Tecnologías

- C# 12, .NET 8, Windows Forms.
- Renderizado de barras con `System.Drawing` y control de FPS/operaciones para animación.

---

## Equipo y créditos

- Equipo: NullException.
- Curso: Programación Estructurada.
- Autores:
  - [Erving Miranda](https://github.com/ErvingMiranda)
  - [Osman Cerpas](https://github.com/Jatca08)
  - [Mery López](https://github.com/nohemy24)
  - [Fernando Zapata](https://github.com/Faza202)

---

## Licencia

Sin licencia específica declarada en el repositorio.
