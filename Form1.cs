using System; // Tipos base, eventos, IDisposable, etc.
using System.Collections.Generic; // Uso de List<T> y otras colecciones genéricas
using System.Diagnostics; // Stopwatch para medir rendimiento y Debug.WriteLine para trazas
using System.Drawing; // Colores, fuentes y rectángulos para dibujo
using System.Globalization; // Formateo cultural (números con separadores, etc.)
using System.Linq; // LINQ: FirstOrDefault, OrderBy, etc. para consultas en memoria
using System.Threading; // CancellationToken para cancelar animaciones
using System.Threading.Tasks; // Programación asíncrona con Task/await
using System.Windows.Forms; // Controles WinForms, eventos y mensajería

namespace BusquedaYOrdenamientoDemo
{
 /// <summary>
 /// Formulario principal que implementa:
 /// - Generación de datos aleatorios
 /// - Visualización animada de Burbuja, Inserción y QuickSort
 /// - Comparación de tiempos (modo rápido y modo visual)
 /// - Renderizado de resultados en una ListView
 /// </summary>
 public partial class Form1 : Form
 {
 #region Campos - Estado visualización
 // Copias de datos por algoritmo para que cada uno trabaje sobre su propio arreglo
 private int[]? datosBurbuja;
 private int[]? datosInsercion;
 private int[]? datosQuick;
 // Arreglos de índices a resaltar durante la animación (comparaciones, swaps)
 private int[]? resaltosBurbuja;
 private int[]? resaltosInsercion;
 private int[]? resaltosQuick;
 #endregion

 #region Campos - Pantalla completa
 private bool estaPantallaCompleta; // Estado actual del modo pantalla completa (F11)
 private Rectangle prevBounds; // Tamaño y posición anterior
 private FormWindowState prevWindowState; // WindowState anterior (Normal/Maximized)
 private FormBorderStyle prevBorderStyle; // Borde anterior (None/Fixed)
 #endregion

 #region Campos - Datos y utilidades
 private int[] datosOriginales = Array.Empty<int>(); // Datos base generados por el usuario
 private readonly Random rng = new Random(); // Generador para números aleatorios

 // Nombres visibles para asociar resultados con algoritmos
 private const string ALG_BURBUJA = "Burbuja";
 private const string ALG_INSERCION = "Inserción";
 private const string ALG_QUICKSORT = "QuickSort";

 private int quickVisualCounter; // Reservado para límites de reporte visual (no usado actualmente)
 #endregion

 #region Inicialización
 public Form1()
 {
 InitializeComponent();
 // Reduce flickering al dibujar en paneles
 DoubleBuffered = true;
 // Permite manejar teclas a nivel de formulario antes que los controles hijos
 KeyPreview = true;
 KeyDown += Formulario_KeyDown;
 }

 protected override void OnLoad(EventArgs e)
 {
 base.OnLoad(e);
 AjustarSplitterInicial();
 // Dejar que el usuario decida pantalla completa (F11); no forzar al cargar
 AjustarColumnasResultados(listaResultados, EventArgs.Empty);
 }
 #endregion

 #region Pantalla completa y teclado
 private void Formulario_KeyDown(object? remitente, KeyEventArgs eventoTeclado)
 {
 // F11 alterna entre pantalla completa y modo normal. Esc sale de pantalla completa
 if (eventoTeclado.KeyCode == Keys.F11)
 {
 CambiarModoPantallaCompleta(!estaPantallaCompleta);
 eventoTeclado.Handled = true;
 }
 else if (eventoTeclado.KeyCode == Keys.Escape && estaPantallaCompleta)
 {
 CambiarModoPantallaCompleta(false);
 eventoTeclado.Handled = true;
 }
 }

 /// <summary>
 /// Cambia el estado de pantalla completa guardando/restaurando geometría y estilo.
 /// </summary>
 private void CambiarModoPantallaCompleta(bool activar)
 {
 if (activar == estaPantallaCompleta) return;

 if (activar)
 {
 prevBounds = Bounds;
 prevWindowState = WindowState;
 prevBorderStyle = FormBorderStyle;
 FormBorderStyle = FormBorderStyle.None;
 WindowState = FormWindowState.Maximized;
 estaPantallaCompleta = true;
 }
 else
 {
 WindowState = prevWindowState == FormWindowState.Minimized ? FormWindowState.Normal : prevWindowState;
 FormBorderStyle = prevBorderStyle;
 Bounds = prevBounds;
 estaPantallaCompleta = false;
 }
 }
 #endregion

 #region Ajustes de interfaz
 /// <summary>
 /// Calcula un SplitterDistance inicial estable y respeta tamaños mínimos de cada panel.
 /// </summary>
 private void AjustarSplitterInicial()
 {
 if (splitPrincipal == null) return;

 int anchoMinimoPanelIzquierdo =250;
 int anchoMinimoPanelDerecho =400;
 splitPrincipal.Panel1MinSize = anchoMinimoPanelIzquierdo;
 splitPrincipal.Panel2MinSize = anchoMinimoPanelDerecho;

 int anchoDisponible = splitPrincipal.ClientSize.Width;
 if (anchoMinimoPanelIzquierdo + anchoMinimoPanelDerecho > anchoDisponible)
 {
 // Evita invalidar; ajusta dinámicamente el mínimo del panel derecho
 splitPrincipal.Panel2MinSize = Math.Max(0, anchoDisponible - anchoMinimoPanelIzquierdo);
 }

 int anchoMaximoPermitido = anchoDisponible - splitPrincipal.Panel2MinSize;
 int distanciaPreferida =280; // Valor empírico que deja buen espacio a la derecha
 int distanciaAplicada = Math.Clamp(distanciaPreferida, splitPrincipal.Panel1MinSize,
 Math.Max(splitPrincipal.Panel1MinSize, anchoMaximoPermitido));

 if (distanciaAplicada >= splitPrincipal.Panel1MinSize &&
 distanciaAplicada <= anchoDisponible - splitPrincipal.Panel2MinSize)
 {
 splitPrincipal.SplitterDistance = distanciaAplicada;
 }
 }
 #endregion

 #region Dibujo de visualización
 /// <summary>
 /// Dibuja las barras del arreglo de un algoritmo y resalta índices relevantes.
 /// Se invoca en el evento Paint de cada panel de dibujo.
 /// </summary>
 private void PanelAlgoritmo_Pintar(object? remitente, PaintEventArgs argumentosPintado)
 {
 if (remitente is not Panel panelDestino) return;

 int[]? datosParaDibujar;
 int[]? indicesResaltados;

 // Selecciona la fuente de datos según el panel que dispara el pintado
 if (panelDestino == panelBurbuja)
 {
 datosParaDibujar = datosBurbuja;
 indicesResaltados = resaltosBurbuja;
 }
 else if (panelDestino == panelInsercion)
 {
 datosParaDibujar = datosInsercion;
 indicesResaltados = resaltosInsercion;
 }
 else
 {
 datosParaDibujar = datosQuick;
 indicesResaltados = resaltosQuick;
 }

 if (datosParaDibujar == null || datosParaDibujar.Length ==0) return;

 var lienzo = argumentosPintado.Graphics;
 lienzo.Clear(Color.Black);

 int anchoPanel = panelDestino.ClientSize.Width;
 int altoPanel = panelDestino.ClientSize.Height;
 if (anchoPanel <=0 || altoPanel <=0) return;

 float anchoBarra = (float)anchoPanel / datosParaDibujar.Length;
 // Calcula el valor máximo (O(n)) para escalar la altura de las barras
 int valorMaximo =0;
 for (int indice =0; indice < datosParaDibujar.Length; indice++)
 {
 if (datosParaDibujar[indice] > valorMaximo)
 {
 valorMaximo = datosParaDibujar[indice];
 }
 }

 if (valorMaximo ==0) return;

 var resaltos = indicesResaltados ?? Array.Empty<int>();
 var pincelNormal = Brushes.White;
 using var pincelResaltado = new SolidBrush(Color.FromArgb(200,50,50));

 // Pinta cada barra; resalta si su índice aparece en 'resaltos'
 for (int indice =0; indice < datosParaDibujar.Length; indice++)
 {
 float alturaBarra = ((float)datosParaDibujar[indice] / valorMaximo) * altoPanel;
 bool esResaltado = Array.IndexOf(resaltos, indice) >=0; // O(k) con k= nº de resaltos (muy pequeño)
 var pincelActual = esResaltado ? pincelResaltado : pincelNormal;
 lienzo.FillRectangle(pincelActual, indice * anchoBarra, altoPanel - alturaBarra, anchoBarra, alturaBarra);
 }
 }
 #endregion

 #region Habilitar/deshabilitar controles
 /// <summary>
 /// Centraliza la habilitación de controles según el estado (ejecutando/idle).
 /// </summary>
 private void ActualizarEstadoControles(bool habilitarControles)
 {
 botonGenerar.Enabled = habilitarControles;
 numericCantidad.Enabled = habilitarControles;
 trackBarVelocidad.Enabled = habilitarControles;
 botonCompararAlgoritmos.Enabled = habilitarControles && datosOriginales.Length >0;
 botonAyuda.Enabled = true;
 botonCancelar.Enabled = !habilitarControles;
 }
 #endregion

 #region Eventos de botones
 /// <summary>
 /// Genera números aleatorios, refresca la muestra y prepara las copias por algoritmo.
 /// </summary>
 private void BotonGenerar_Click(object remitente, EventArgs argumentos)
 {
 int cantidadDatos = (int)numericCantidad.Value;
 datosOriginales = new int[cantidadDatos];
 for (int indice =0; indice < cantidadDatos; indice++)
 {
 datosOriginales[indice] = rng.Next(1,1001); // Enteros en [1,1000]
 }

 // Pinta una muestra acotada para no sobrecargar la UI
 listaMuestraDatos.BeginUpdate();
 listaMuestraDatos.Items.Clear();
 int cantidadAMostrar = Math.Min(50, cantidadDatos);
 for (int indice =0; indice < cantidadAMostrar; indice++)
 {
 listaMuestraDatos.Items.Add(datosOriginales[indice]);
 }
 listaMuestraDatos.EndUpdate();

 botonCompararAlgoritmos.Enabled = true;
 listaResultados.Items.Clear();
 etiquetaEstado.Text = $"Datos generados: {cantidadDatos} números aleatorios.";

 // Copias por algoritmo para visualización independiente
 datosBurbuja = (int[])datosOriginales.Clone();
 datosInsercion = (int[])datosOriginales.Clone();
 datosQuick = (int[])datosOriginales.Clone();
 resaltosBurbuja = resaltosInsercion = resaltosQuick = Array.Empty<int>();
 panelBurbuja.Invalidate();
 panelInsercion.Invalidate();
 panelQuick.Invalidate();
 }

 /// <summary>
 /// Pregunta el modo (visual o rápido) y delega la ejecución.
 /// </summary>
 private void BotonCompararAlgoritmos_Click(object? remitente, EventArgs argumentos)
 {
 if (datosOriginales == null || datosOriginales.Length ==0)
 {
 MessageBox.Show("Primero genera los datos.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
 return;
 }

 // Ofrece ambas opciones siempre; el modo visual puede cancelarse si fuese pesado
 var resultado = MessageBox.Show("¿Desea visualizar la carrera en tiempo real?\n\n" +
 " Sí: Ver la animación de los tres algoritmos compitiendo.\n" +
 " No: Obtendrás los resultados instantáneos y se mostrará el estado final ordenado en los paneles.",
 "Modo de Comparación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

 if (resultado == DialogResult.Yes)
 {
 BotonVisualizarCarrera_Click(remitente, argumentos);
 }
 else
 {
 EjecutarModoRapido(true);
 }
 }

 /// <summary>
 /// Ejecuta la visualización simultánea: corre los3 algoritmos en paralelo,
 /// reporta progreso a los paneles y luego muestra tiempos finales medidos sin animación.
 /// </summary>
 private async void BotonVisualizarCarrera_Click(object? remitente, EventArgs argumentos)
 {
 ctsVisualizacion?.Cancel();
 ctsVisualizacion = new CancellationTokenSource();
 var tokenVisualizacion = ctsVisualizacion.Token;

 // Restablece los datos y resaltos por algoritmo
 datosBurbuja = (int[])datosOriginales.Clone();
 datosInsercion = (int[])datosOriginales.Clone();
 datosQuick = (int[])datosOriginales.Clone();
 resaltosBurbuja = Array.Empty<int>();
 resaltosInsercion = Array.Empty<int>();
 resaltosQuick = Array.Empty<int>();

 // Garantiza que el panel de visualizaciones esté en el panel superior del split secundario
 var splitSecundario = splitPrincipal.Panel2.Controls.Count >0
 ? splitPrincipal.Panel2.Controls[0] as SplitContainer
 : null;
 if (splitSecundario != null && !splitSecundario.Panel1.Controls.Contains(panelVisualizaciones))
 {
 splitSecundario.Panel1.Controls.Clear();
 splitSecundario.Panel1.Controls.Add(panelVisualizaciones);
 }
 panelVisualizaciones.Visible = true;

 etiquetaEstado.Text = "Visualizando simultáneamente...";
 listaResultados.Items.Clear();
 ActualizarEstadoControles(false);
 botonCancelar.Enabled = true;
 // Mantener la barra de velocidad habilitada para control en tiempo real
 trackBarVelocidad.Enabled = true;

 // Throttle para regular FPS y cantidad de operaciones reportadas por frame
 var throttleBurbuja = new FrameThrottler(trackBarVelocidad.Value);
 var throttleInsercion = new FrameThrottler(trackBarVelocidad.Value);
 var throttleQuick = new FrameThrottler(trackBarVelocidad.Value);
 EventHandler? velocidadHandler = (s, e) =>
 {
 throttleBurbuja.SetSpeed(trackBarVelocidad.Value);
 throttleInsercion.SetSpeed(trackBarVelocidad.Value);
 throttleQuick.SetSpeed(trackBarVelocidad.Value);
 };
 trackBarVelocidad.ValueChanged += velocidadHandler;

 try
 {
 // Proxies de progreso que actualizan el estado y solicitan repintado de cada panel
 var progressBurbuja = new Progress<(int[] estado, int[] resaltos)>(p =>
 {
 datosBurbuja = (int[])p.estado.Clone();
 resaltosBurbuja = p.resaltos;
 panelBurbuja.Invalidate();
 });
 var progressInsercion = new Progress<(int[] estado, int[] resaltos)>(p =>
 {
 datosInsercion = (int[])p.estado.Clone();
 resaltosInsercion = p.resaltos;
 panelInsercion.Invalidate();
 });
 var progressQuick = new Progress<(int[] estado, int[] resaltos)>(p =>
 {
 datosQuick = (int[])p.estado.Clone();
 resaltosQuick = p.resaltos;
 panelQuick.Invalidate();
 });

 // Lanza las3 tareas de ordenamiento visual en paralelo
 var tareaBurbuja = EjecutarAlgoritmoVisual(ALG_BURBUJA,
 (arr, prog, tok) => OrdenarPorBurbujaVisual(arr, prog, tok, throttleBurbuja),
 datosBurbuja!, progressBurbuja, tokenVisualizacion);
 var tareaInsercion = EjecutarAlgoritmoVisual(ALG_INSERCION,
 (arr, prog, tok) => OrdenarPorInsercionVisual(arr, prog, tok, throttleInsercion),
 datosInsercion!, progressInsercion, tokenVisualizacion);
 var tareaQuickSort = EjecutarAlgoritmoVisual(ALG_QUICKSORT,
 (arr, prog, tok) => OrdenarPorQuickSortVisual(arr, prog, tok, throttleQuick),
 datosQuick!, progressQuick, tokenVisualizacion);

 await Task.WhenAll(tareaBurbuja, tareaInsercion, tareaQuickSort);

 // Tras finalizar (si no fue cancelado), mide tiempos reales sin throttling/animación
 if (!tokenVisualizacion.IsCancellationRequested)
 {
 var resultadosReales = new List<(string nombre, TimeSpan tiempo, long ticks)>
 {
 MedirAlgoritmo(ALG_BURBUJA, OrdenarPorBurbuja),
 MedirAlgoritmo(ALG_INSERCION, OrdenarPorInsercion),
 MedirAlgoritmo(ALG_QUICKSORT, OrdenarPorQuickSort)
 };

 RenderizarResultados(resultadosReales, true);
 }
 else
 {
 etiquetaEstado.Text = "Visualización cancelada.";
 }
 }
 catch (OperationCanceledException)
 {
 etiquetaEstado.Text = "Visualización cancelada.";
 }
 finally
 {
 trackBarVelocidad.ValueChanged -= velocidadHandler;
 ActualizarEstadoControles(true);
 botonCancelar.Enabled = false;
 ctsVisualizacion = null;
 }
 }

 private void BotonCancelar_Click(object? remitente, EventArgs argumentos)
 {
 ctsVisualizacion?.Cancel();
 etiquetaEstado.Text = "Cancelando...";
 }

 private void BotonAyuda_Click(object? remitente, EventArgs argumentos)
 {
 using var ventanaAyuda = new HelpForm();
 ventanaAyuda.ShowDialog(this);
 }
 #endregion

 #region Comparación y renderizado de resultados
 /// <summary>
 /// Ejecuta los algoritmos sin animación, muestra el estado final en los paneles
 /// y renderiza la tabla de resultados.
 /// </summary>
 private void EjecutarModoRapido(bool actualizarVisual)
 {
 if (datosOriginales == null || datosOriginales.Length ==0) return;

 Cursor = Cursors.WaitCursor;
 ActualizarEstadoControles(false);
 List<(string nombre, TimeSpan tiempo, long ticks)> resultados = new();
 try
 {
 // Medir sobre copias para poder mostrar el estado final ordenado por algoritmo
 var rBurbuja = MedirAlgoritmoConCopia(ALG_BURBUJA, OrdenarPorBurbuja);
 var rInsercion = MedirAlgoritmoConCopia(ALG_INSERCION, OrdenarPorInsercion);
 var rQuick = MedirAlgoritmoConCopia(ALG_QUICKSORT, OrdenarPorQuickSort);
 resultados.Add((rBurbuja.nombre, rBurbuja.tiempo, rBurbuja.ticks));
 resultados.Add((rInsercion.nombre, rInsercion.tiempo, rInsercion.ticks));
 resultados.Add((rQuick.nombre, rQuick.tiempo, rQuick.ticks));

 if (actualizarVisual)
 {
 // Muestra en los paneles el resultado final de cada algoritmo
 datosBurbuja = rBurbuja.arregloOrdenado;
 datosInsercion = rInsercion.arregloOrdenado;
 datosQuick = rQuick.arregloOrdenado;
 resaltosBurbuja = resaltosInsercion = resaltosQuick = Array.Empty<int>();
 panelBurbuja.Invalidate();
 panelInsercion.Invalidate();
 panelQuick.Invalidate();
 etiquetaEstado.Text = "Visualización instantánea: arreglos finalizados.";
 }
 }
 finally
 {
 Cursor = Cursors.Default;
 ActualizarEstadoControles(true);
 }

 RenderizarResultados(resultados, true);
 }

 /// <summary>
 /// Mide un algoritmo sobre una copia y devuelve además el arreglo ordenado.
 /// </summary>
 private (string nombre, TimeSpan tiempo, long ticks, int[] arregloOrdenado) MedirAlgoritmoConCopia(string nombre, Action<int[]> algoritmo)
 {
 int[] copiaDatos = (int[])datosOriginales.Clone();
 var cronometro = Stopwatch.StartNew();
 algoritmo(copiaDatos);
 cronometro.Stop();
 return (nombre, cronometro.Elapsed, cronometro.ElapsedTicks, copiaDatos);
 }

 /// <summary>
 /// Actualiza la ListView con los resultados. Usa LINQ para:
 /// - Normalizar y buscar resultados existentes (FirstOrDefault)
 /// - Encontrar al ganador por tiempo (OrderBy)
 /// </summary>
 private void RenderizarResultados(List<(string nombre, TimeSpan tiempo, long ticks)> resultados, bool mostrarMensaje)
 {
 // Local function: normaliza strings para evitar fallos de coincidencia por espacios/caso
 static string Normalizar(string? s) => string.IsNullOrWhiteSpace(s) ? string.Empty : s.Trim();

 // Busca un resultado por nombre; si no existe, mide y agrega
 (string nombre, TimeSpan tiempo, long ticks) BuscarOMedir(string esperado, Action<int[]> algoritmo)
 {
 string clave = Normalizar(esperado);
 // LINQ: FirstOrDefault para encontrar un item cuyo nombre normalizado coincida (OrdinalIgnoreCase)
 var encontrado = resultados.FirstOrDefault(r =>
 string.Equals(Normalizar(r.nombre), clave, StringComparison.OrdinalIgnoreCase));

 if (!string.IsNullOrWhiteSpace(encontrado.nombre))
 {
 return (esperado, encontrado.tiempo, encontrado.ticks);
 }

 var medido = MedirAlgoritmo(esperado, algoritmo);
 resultados.Add(medido);
 return medido;
 }

 var resultadoQuickSort = BuscarOMedir(ALG_QUICKSORT, OrdenarPorQuickSort);
 var resultadoInsercion = BuscarOMedir(ALG_INSERCION, OrdenarPorInsercion);
 var resultadoBurbuja = BuscarOMedir(ALG_BURBUJA, OrdenarPorBurbuja);

 // Coloca QuickSort al final como workaround visual
 var resultadosGarantizados = new[] { resultadoInsercion, resultadoBurbuja, resultadoQuickSort };

 listaResultados.BeginUpdate();
 try
 {
 listaResultados.Sorting = SortOrder.None;
 if (listaResultados.Groups.Count >0)
 {
 listaResultados.ShowGroups = false;
 listaResultados.Groups.Clear();
 }

 listaResultados.Items.Clear();
 int filaIndex =0;
 foreach (var resultado in resultadosGarantizados)
 {
 AgregarFilaResultado(resultado);
 // Alterna color de fondo para legibilidad
 if (filaIndex < listaResultados.Items.Count)
 {
 var item = listaResultados.Items[filaIndex];
 if (filaIndex %2 ==1)
 {
 item.BackColor = Color.FromArgb(245,248,252);
 }
 }
 filaIndex++;
 }
 }
 finally
 {
 listaResultados.EndUpdate();
 try
 {
 if (listaResultados.Items.Count >0)
 {
 listaResultados.TopItem = listaResultados.Items[0];
 }
 }
 catch { /* ignorar si TopItem no está disponible aún */ }
 listaResultados.Refresh();
 listaResultados.Invalidate();
 listaResultados.Update();
 }

 // LINQ: OrderBy para encontrar el resultado con menor tiempo
 var resultadoGanador = resultadosGarantizados.OrderBy(r => r.tiempo).First();

 foreach (ListViewItem fila in listaResultados.Items)
 {
 if (string.Equals(fila.Tag as string, resultadoGanador.nombre, StringComparison.Ordinal))
 {
 fila.BackColor = Color.FromArgb(230,255,230);
 fila.ForeColor = Color.FromArgb(20,60,20);
 fila.Font = new Font(fila.Font, FontStyle.Bold);
 fila.EnsureVisible();
 }
 }

 etiquetaEstado.Text =
 $"Ganador: {resultadoGanador.nombre} con {resultadoGanador.tiempo.TotalMilliseconds:N3} ms";

 if (mostrarMensaje)
 {
 MessageBox.Show($"Algoritmo ganador: {resultadoGanador.nombre}\n" +
 $"Tiempo: {resultadoGanador.tiempo.TotalMilliseconds:N3} ms" +
 (resultadoGanador.tiempo.TotalMilliseconds >=1000
 ? $" ({resultadoGanador.tiempo.TotalSeconds:N3} s)"
 : string.Empty) +
 "\n\nRevisa la tabla de resultados para ver el detalle de cada algoritmo.",
 "Resultados de la Comparación", MessageBoxButtons.OK, MessageBoxIcon.Information);
 }

 // Reajustar columnas tras cargar resultados
 AjustarColumnasResultados(listaResultados, EventArgs.Empty);
 }

 /// <summary>
 /// Agrega una fila al ListView formateando tiempos en ms, s y ticks.
 /// </summary>
 private void AgregarFilaResultado((string nombre, TimeSpan tiempo, long ticks) resultado)
 {
 if (string.IsNullOrWhiteSpace(resultado.nombre)) return;

 var fila = new ListViewItem(resultado.nombre) { Tag = resultado.nombre };
 var cultura = CultureInfo.CurrentCulture; // Muestra números según configuración regional

 // Formatea valores sub-segundo con mayor precisión para tiempos muy pequeños
 string textoMilisegundos = resultado.tiempo.TotalMilliseconds >=1
 ? resultado.tiempo.TotalMilliseconds.ToString("N3", cultura)
 : FormatearTiempoSubSegundo(resultado.tiempo.TotalMilliseconds, cultura);

 string textoSegundos = resultado.tiempo.TotalSeconds >=1
 ? resultado.tiempo.TotalSeconds.ToString("N3", cultura)
 : FormatearTiempoSubSegundo(resultado.tiempo.TotalSeconds, cultura);

 fila.SubItems.Add(textoMilisegundos);
 fila.SubItems.Add(textoSegundos);
 fila.SubItems.Add(resultado.ticks.ToString("N0", cultura));
 listaResultados.Items.Add(fila);

 // Trazas en Debug para verificar llegada de resultados (útil durante desarrollo)
 Debug.WriteLine($"[AgregarFilaResultado] Algoritmo={resultado.nombre} | ms={textoMilisegundos} | s={textoSegundos} | ticks={resultado.ticks:N0}");
 }

 /// <summary>
 /// Da formato compacto a valores sub-segundo, con un umbral para mostrar "< x".
 /// </summary>
 private static string FormatearTiempoSubSegundo(double valor, CultureInfo cultura)
 {
 if (valor <=0) return "0";

 const double umbral =0.000001;
 if (valor < umbral) return "<" + umbral.ToString("0.######", cultura);

 return valor.ToString("0.######", cultura);
 }

 /// <summary>
 /// Mide el tiempo de ejecución de un algoritmo sobre una copia de los datos.
 /// </summary>
 private (string nombre, TimeSpan tiempo, long ticks) MedirAlgoritmo(string nombre, Action<int[]> algoritmo)
 {
 int[] copiaDatos = (int[])datosOriginales.Clone();
 var cronometro = Stopwatch.StartNew();
 algoritmo(copiaDatos);
 cronometro.Stop();
 return (nombre, cronometro.Elapsed, cronometro.ElapsedTicks);
 }
 #endregion

 #region Algoritmos no visuales (modo rápido)
 /// <summary>
 /// Ordenamiento Burbuja (O(n^2)): itera realizando intercambios adyacentes hasta no haber cambios.
 /// </summary>
 private void OrdenarPorBurbuja(int[] arreglo)
 {
 int elementosPendientes = arreglo.Length;
 bool huboIntercambio;
 do
 {
 huboIntercambio = false;
 for (int indice =0; indice < elementosPendientes -1; indice++)
 {
 if (arreglo[indice] > arreglo[indice +1])
 {
 (arreglo[indice], arreglo[indice +1]) = (arreglo[indice +1], arreglo[indice]);
 huboIntercambio = true;
 }
 }
 elementosPendientes--;
 } while (huboIntercambio);
 }

 /// <summary>
 /// Inserción (O(n^2)): inserta cada elemento en su posición dentro de la parte ya ordenada.
 /// </summary>
 private void OrdenarPorInsercion(int[] arreglo)
 {
 for (int indice =1; indice < arreglo.Length; indice++)
 {
 int valorActual = arreglo[indice];
 int posicion = indice -1;
 while (posicion >=0 && arreglo[posicion] > valorActual)
 {
 arreglo[posicion +1] = arreglo[posicion];
 posicion--;
 }
 arreglo[posicion +1] = valorActual;
 }
 }

 /// <summary>
 /// QuickSort promedio O(n log n): particiona alrededor de un pivote y ordena recursivamente.
 /// </summary>
 private void OrdenarPorQuickSort(int[] arreglo)
 {
 QuickSortRecursivoBasico(arreglo,0, arreglo.Length -1);
 }

 /// <summary>
 /// Implementación recursiva in-place de QuickSort usando pivote medio.
 /// </summary>
 private void QuickSortRecursivoBasico(int[] arreglo, int izquierda, int derecha)
 {
 if (izquierda >= derecha) return;

 int indiceIzquierdo = izquierda;
 int indiceDerecho = derecha;
 int pivote = arreglo[(izquierda + derecha) /2];

 while (indiceIzquierdo <= indiceDerecho)
 {
 while (arreglo[indiceIzquierdo] < pivote) indiceIzquierdo++;
 while (arreglo[indiceDerecho] > pivote) indiceDerecho--;

 if (indiceIzquierdo <= indiceDerecho)
 {
 (arreglo[indiceIzquierdo], arreglo[indiceDerecho]) = (arreglo[indiceDerecho], arreglo[indiceIzquierdo]);
 indiceIzquierdo++;
 indiceDerecho--;
 }
 }

 if (izquierda < indiceDerecho)
 {
 QuickSortRecursivoBasico(arreglo, izquierda, indiceDerecho);
 }

 if (indiceIzquierdo < derecha)
 {
 QuickSortRecursivoBasico(arreglo, indiceIzquierdo, derecha);
 }
 }
 #endregion

 #region Helper visual: throttle por fotogramas
 /// <summary>
 /// Regula la frecuencia de actualización de la UI: combina límite por tiempo (ms por frame)
 /// y por operaciones por frame para suavizar animaciones sin saturar el hilo de UI.
 /// </summary>
 private sealed class FrameThrottler
 {
 private readonly Stopwatch _frameSw = new Stopwatch();
 private double _targetFrameMs; // ms por frame objetivo según velocidad
 private int _opsPerFrame; // operaciones máximas a reportar por frame
 private int _ops; // contador incremental de operaciones

 public FrameThrottler(int velocidad)
 {
 _frameSw.Start();
 SetSpeed(velocidad);
 }

 // velocidad:1..100 => fps aprox2..60 y operaciones por frame1..5000
 public void SetSpeed(int velocidad)
 {
 velocidad = Math.Clamp(velocidad,1,100);
 double norm = (velocidad -1) /99.0; //0..1
 double fps =2.0 + (60.0 -2.0) * norm;
 _targetFrameMs =1000.0 / fps;
 _opsPerFrame = Math.Max(1, (int)Math.Round(1 + (5000 -1) * norm));
 }

 /// <summary>
 /// Incrementa el contador de operaciones y espera si corresponde para mantener el ritmo.
 /// Devuelve true cuando es buen momento para renderizar (reportar progreso y repintar).
 /// </summary>
 public async Task<bool> TickAsync(CancellationToken token)
 {
 _ops++;
 bool timeUp = _frameSw.Elapsed.TotalMilliseconds >= _targetFrameMs;
 bool opsUp = _ops >= _opsPerFrame;
 if (timeUp || opsUp)
 {
 var remaining = _targetFrameMs - _frameSw.Elapsed.TotalMilliseconds;
 if (remaining >0)
 {
 await Task.Delay((int)remaining, token);
 }
 _frameSw.Restart();
 _ops =0;
 return true; // indica que es buen momento para renderizar
 }
 return false;
 }
 }
 #endregion

 #region Algoritmos visuales (async con animación)
 /// <summary>
 /// Burbuja con reportes de progreso y throttle para animación.
 /// </summary>
 private async Task OrdenarPorBurbujaVisual(int[] arreglo, IProgress<(int[] estado, int[] resaltos)> progreso,
 CancellationToken token, FrameThrottler throttle)
 {
 int elementosPendientes = arreglo.Length;
 bool huboIntercambio; // declarar fuera del bloque do-while

 do
 {
 huboIntercambio = false;
 for (int indice =0; indice < elementosPendientes -1; indice++)
 {
 if (token.IsCancellationRequested) return;
 int a = indice;
 int b = indice +1;
 int[] resaltos = new[] { a, b }; // resaltar comparación por claridad
 if (arreglo[a] > arreglo[b])
 {
 (arreglo[a], arreglo[b]) = (arreglo[b], arreglo[a]);
 huboIntercambio = true;
 }
 if (await throttle.TickAsync(token))
 {
 progreso.Report((arreglo, resaltos));
 }
 }
 elementosPendientes--;
 // Siempre renderizar al final de la pasada
 progreso.Report((arreglo, Array.Empty<int>()));
 } while (huboIntercambio);

 progreso.Report((arreglo, Array.Empty<int>()));
 }

 /// <summary>
 /// Inserción con animación: desplaza elementos mayores y coloca el valor actual.
 /// </summary>
 private async Task OrdenarPorInsercionVisual(int[] arreglo, IProgress<(int[] estado, int[] resaltos)> progreso,
 CancellationToken token, FrameThrottler throttle)
 {
 for (int indice =1; indice < arreglo.Length; indice++)
 {
 if (token.IsCancellationRequested) return;
 int valorActual = arreglo[indice];
 int posicion = indice -1;
 // Mostrar la comparación y desplazamientos paso a paso
 while (posicion >=0 && arreglo[posicion] > valorActual)
 {
 if (token.IsCancellationRequested) return;
 arreglo[posicion +1] = arreglo[posicion];
 int[] resaltos = new[] { Math.Max(0, posicion), posicion +1 };
 if (await throttle.TickAsync(token))
 {
 progreso.Report((arreglo, resaltos));
 }
 posicion--;
 }
 arreglo[posicion +1] = valorActual;
 if (await throttle.TickAsync(token))
 {
 progreso.Report((arreglo, new[] { posicion +1 }));
 }
 }

 progreso.Report((arreglo, Array.Empty<int>()));
 }

 /// <summary>
 /// QuickSort visual: particiona, reporta swaps y recurre con control de ritmo.
 /// </summary>
 private async Task OrdenarPorQuickSortVisual(int[] arreglo, IProgress<(int[] estado, int[] resaltos)> progreso,
 CancellationToken token, FrameThrottler throttle)
 {
 await QuickSortRecursivoVisualAsync(arreglo,0, arreglo.Length -1, progreso, token, throttle);
 progreso.Report((arreglo, Array.Empty<int>()));
 }

 /// <summary>
 /// Núcleo recursivo de QuickSort para animación con resaltos de índices.
 /// </summary>
 private async Task QuickSortRecursivoVisualAsync(int[] arreglo, int izquierda, int derecha,
 IProgress<(int[] estado, int[] resaltos)> progreso, CancellationToken token, FrameThrottler throttle)
 {
 if (izquierda >= derecha) return;
 int i = izquierda;
 int j = derecha;
 int pivote = arreglo[(izquierda + derecha) /2];
 int[] resaltos = Array.Empty<int>();
 while (i <= j)
 {
 if (token.IsCancellationRequested) return;
 while (arreglo[i] < pivote) { i++; if (await throttle.TickAsync(token)) progreso.Report((arreglo, new[] { i })); }
 while (arreglo[j] > pivote) { j--; if (await throttle.TickAsync(token)) progreso.Report((arreglo, new[] { j })); }
 if (i <= j)
 {
 (arreglo[i], arreglo[j]) = (arreglo[j], arreglo[i]);
 resaltos = new[] { i, j };
 if (await throttle.TickAsync(token))
 {
 progreso.Report((arreglo, resaltos));
 }
 i++; j--;
 }
 }
 if (izquierda < j)
 {
 await QuickSortRecursivoVisualAsync(arreglo, izquierda, j, progreso, token, throttle);
 }
 if (i < derecha)
 {
 await QuickSortRecursivoVisualAsync(arreglo, i, derecha, progreso, token, throttle);
 }
 }
 #endregion

 #region Ejecución visual (medición con cronómetro)
 /// <summary>
 /// Ejecuta un algoritmo visual midiendo su tiempo real (incluye el coste del throttle/animación).
 /// </summary>
 private async Task<(string nombre, TimeSpan tiempo, long ticks)> EjecutarAlgoritmoVisual(
 string nombre, Func<int[], IProgress<(int[] estado, int[] resaltos)>, CancellationToken, Task> algoritmo,
 int[] datos, IProgress<(int[] estado, int[] resaltos)> progreso, CancellationToken token)
 {
 var cronometro = Stopwatch.StartNew();
 await algoritmo(datos, progreso, token);
 cronometro.Stop();
 return (nombre, cronometro.Elapsed, cronometro.ElapsedTicks);
 }
 #endregion

 private CancellationTokenSource? ctsVisualizacion; // Fuente de cancelación para la animación en curso

 /// <summary>
 /// Ajusta anchos de columnas del ListView proporcionalmente al tamaño del control.
 /// </summary>
 private void AjustarColumnasResultados(object? sender, EventArgs e)
 {
 if (listaResultados == null || listaResultados.Columns.Count ==0) return;
 int anchoTotal = listaResultados.ClientSize.Width;
 if (anchoTotal <=0) return;
 // Reparto proporcional
 int colAlg = (int)(anchoTotal *0.30);
 int colMs = (int)(anchoTotal *0.22);
 int colS = (int)(anchoTotal *0.18);
 int colTicks = anchoTotal - (colAlg + colMs + colS) -4; // margen
 listaResultados.BeginUpdate();
 try
 {
 listaResultados.Columns[0].Width = Math.Max(120, colAlg);
 listaResultados.Columns[1].Width = Math.Max(100, colMs);
 listaResultados.Columns[2].Width = Math.Max(90, colS);
 listaResultados.Columns[3].Width = Math.Max(140, colTicks);
 }
 finally
 {
 listaResultados.EndUpdate();
 }
 }
 }
}