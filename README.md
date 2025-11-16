# Algoritmos de Ordenamiento en C#

Este proyecto contiene implementaciones de distintos algoritmos de ordenamiento en C#, con fines principalmente educativos.  
La idea es poder comparar su funcionamiento, complejidad temporal y espacial, así como entender en qué casos conviene usar cada uno.

> Nota: Siéntete libre de adaptar este README según el contenido real de tu repositorio (por ejemplo, agregando o quitando algoritmos, clases o proyectos).

---

## Contenido del repositorio

Dependiendo de cómo esté organizado tu código, aquí puedes listar los algoritmos implementados. Por ejemplo:

- **Burbuja (Bubble Sort)**
- **Selección (Selection Sort)**
- **Inserción (Insertion Sort)**
- **Merge Sort**
- **Quick Sort**
- **Heap Sort**
- **Shell Sort**
- Otros que desees agregar...

Cada algoritmo puede estar implementado en su propia clase o archivo, de forma que sea fácil de leer y comparar.

---

## Objetivos del proyecto

- Servir como **material de estudio** para entender cómo funcionan los algoritmos de ordenamiento.
- Permitir **comparar tiempos de ejecución** entre diferentes métodos.
- Mostrar ejemplos de **implementaciones en C#**, con código limpio y comentado.
- Servir como base para **experimentos** (por ejemplo, cambiar el tipo de datos, tamaños de listas, orden inicial, etc.).

---

## Requisitos

- **.NET SDK** (por ejemplo, .NET 6 o .NET 8)  
  Puedes verificar la versión instalada con:

```bash
dotnet --version
```

---

## Cómo ejecutar el proyecto

1. Clona este repositorio:

```bash
git clone https://github.com/ErvingMiranda/AlgoritmosOrdenamiento.git
```

2. Entra en la carpeta del proyecto:

```bash
cd AlgoritmosOrdenamiento
```

3. Restaura dependencias (si aplica) y compila:

```bash
dotnet build
```

4. Ejecuta el proyecto (ajusta el nombre del proyecto si es necesario):

```bash
dotnet run
```

Si el proyecto está organizado como solución (`.sln`) con varios proyectos, puedes ejecutar uno en específico:

```bash
dotnet run --project ruta/al/proyecto.csproj
```

---

## Estructura sugerida del código

Una posible estructura (ajústala a lo que ya tengas):

```text
AlgoritmosOrdenamiento/
├─ Algoritmos/
│  ├─ BubbleSort.cs
│  ├─ SelectionSort.cs
│  ├─ InsertionSort.cs
│  ├─ MergeSort.cs
│  ├─ QuickSort.cs
│  └─ ...
├─ Utils/
│  └─ ArrayGenerator.cs
├─ Program.cs
└─ README.md
```

- `Algoritmos/` contiene las implementaciones de los distintos algoritmos.
- `Utils/` puede contener generadores de datos, utilidades para medir tiempos, etc.
- `Program.cs` sirve como punto de entrada para probar los algoritmos.

---

## Ejemplo de uso (conceptual)

En tu `Program.cs` podrías tener algo como:

```csharp
using AlgoritmosOrdenamiento.Algoritmos;
using AlgoritmosOrdenamiento.Utils;

class Program
{
    static void Main(string[] args)
    {
        int[] datos = ArrayGenerator.GenerarAleatorio(1000, 0, 10000);

        int[] copiaBubble = (int[])datos.Clone();
        int[] copiaQuick = (int[])datos.Clone();

        Console.WriteLine("Ordenando con Bubble Sort...");
        BubbleSort.Ordenar(copiaBubble);

        Console.WriteLine("Ordenando con Quick Sort...");
        QuickSort.Ordenar(copiaQuick);

        // Aquí podrías medir tiempos, imprimir resultados, etc.
    }
}
```

> Ajusta los nombres de espacios de nombres (`namespaces`) y clases según tu implementación real.

---

## Complejidades (resumen)

Tabla rápida de complejidad temporal promedio y en el peor caso:

| Algoritmo       | Mejor caso   | Promedio      | Peor caso      | Estable | En memoria (in-place) |
|-----------------|--------------|---------------|----------------|---------|------------------------|
| Bubble Sort     | $O(n)$       | $O(n^2)$      | $O(n^2)$       | Sí      | Sí                     |
| Insertion Sort  | $O(n)$       | $O(n^2)$      | $O(n^2)$       | Sí      | Sí                     |
| Selection Sort  | $O(n^2)$     | $O(n^2)$      | $O(n^2)$       | No      | Sí                     |
| Merge Sort      | $O(n \log n)$| $O(n \log n)$ | $O(n \log n)$  | Sí      | No (usa memoria extra) |
| Quick Sort      | $O(n \log n)$| $O(n \log n)$ | $O(n^2)$       | No      | Sí                     |
| Heap Sort       | $O(n \log n)$| $O(n \log n)$ | $O(n \log n)$  | No      | Sí                     |

Esta tabla es puramente informativa y puedes adaptarla según los algoritmos realmente implementados.

---

## Contribuciones

Si quieres mejorar este proyecto:

1. Haz un fork del repositorio.
2. Crea una rama para tu cambio:

```bash
git checkout -b mejora-algoritmo-x
```

3. Realiza tus cambios y haz commit:

```bash
git commit -am "Mejora implementación de Quick Sort"
```

4. Sube tu rama y abre un Pull Request.

---

## Licencia

Especifica aquí la licencia de tu proyecto (por ejemplo, MIT, Apache 2.0, etc.).  
Si aún no has elegido una, una opción común es MIT:

```text
Este proyecto está licenciado bajo los términos de la licencia MIT.
```

---

## Autor

- [Erving Miranda](https://github.com/ErvingMiranda)
- [Osman Cerpas](https://github.com/Jatca08)
- [Mery López](https://github.com/nohemy24)
- [Fernando Zapata](https://github.com/Faza202)
