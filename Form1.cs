using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BusquedaYOrdenamientoDemo
{
 /// <summary>
 /// Formulario principal que permite:
 /// - Generar datos aleatorios.
 /// - Comparar tiempos de ejecución de Burbuja, Inserción y QuickSort (modo rápido).
 /// - Visualizar en paralelo el proceso de ordenamiento con animación (modo visual).
 /// La UI se construye dinámicamente en <see cref="ConfigurarInterfaz"/>.
 /// </summary>
 public partial class Form1 : Form
 {
 #region Campos - UI principal
 private NumericUpDown numericCantidad;
 private Button botonGenerar;
 private ListView listaResultados;
 private ListBox listaMuestraDatos;
 private Label etiquetaEstado;
 private SplitContainer splitPrincipal;

 // Controles de visualización y ayuda
 private TrackBar trackBarVelocidad;
 private CancellationTokenSource? ctsVisualizacion;
 private Button botonCompararAlgoritmos;
 private Button botonAyuda;
 private Button botonCancelar;

 // Contenedores para visualización simultánea
 private TableLayoutPanel panelMulti;
 private Panel panelBurbujaContenedor;
 private Panel panelInsercionContenedor;
 private Panel panelQuickContenedor;
 private Panel panelBurbuja;
 private Panel panelInsercion;
 private Panel panelQuick;
 private Label etiquetaBurbuja;
 private Label etiquetaInsercion;
 private Label etiquetaQuick;
 #endregion

 #region Campos - Estado visualización
 private int[]? datosBurbuja;
 private int[]? datosInsercion;
 private int[]? datosQuick;
 private int[]? resaltosBurbuja;
 private int[]? resaltosInsercion;
 private int[]? resaltosQuick;
 #endregion

 #region Campos - Pantalla completa
 private bool estaPantallaCompleta;
 private Rectangle prevBounds;
 private FormWindowState prevWindowState;
 private FormBorderStyle prevBorderStyle;
 #endregion

 #region Campos - Datos y utilidades
 private int[] datosOriginales = Array.Empty<int>(); // Siempre inicializado para evitar null
 private readonly Random rng = new Random();
 private bool interfazConfigurada; // Evita construir UI dos veces

 // Constantes con nombres de algoritmos
 private const string ALG_BURBUJA = "Burbuja";
 private const string ALG_INSERCION = "Inserción";
 private const string ALG_QUICKSORT = "QuickSort";

 private int quickVisualCounter; // Acumulador para limitar frames de QuickSort visual
 #endregion

 #region Inicialización
 public Form1()
 {
 InitializeComponent();
 DoubleBuffered = true; // Reduce parpadeo al repintar
 KeyPreview = true; // Permite capturar teclas a nivel formulario
 KeyDown += Form1_KeyDown; // Manejo de teclas (F11 / Escape)
 }

 /// <summary>
 /// Construye la interfaz cuando el formulario ya tiene tamaño real.
 /// </summary>
 protected override void OnLoad(EventArgs e)
 {
 base.OnLoad(e);
 if (!interfazConfigurada)
 {
 ConfigurarInterfaz();
 AjustarSplitterInicial();
 }
 }
 #endregion

 #region Pantalla completa y teclado
 private void Form1_KeyDown(object? sender, KeyEventArgs e)
 {
 if (e.KeyCode == Keys.F11)
 {
 AlternarPantallaCompleta(!estaPantallaCompleta);
 e.Handled = true;
 }
 else if (e.KeyCode == Keys.Escape && estaPantallaCompleta)
 {
 AlternarPantallaCompleta(false);
 e.Handled = true;
 }
 }

 /// <summary>
 /// Activa o desactiva el modo pantalla completa guardando el estado anterior.
 /// </summary>
 private void AlternarPantallaCompleta(bool activar)
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

 #region Construcción de interfaz
 /// <summary>
 /// Crea y agrega todos los controles del formulario.
 /// </summary>
 private void ConfigurarInterfaz()
 {
 interfazConfigurada = true;
 SuspendLayout();

 Text = "Comparación de Algoritmos de Ordenamiento";
 Width =1000;
 Height =600;
 StartPosition = FormStartPosition.CenterScreen;
 BackColor = Color.FromArgb(237,241,250);

 // Split principal: Panel izquierdo (configuración) y derecho (visualización/resultados)
 // NOTA: No establecer Panel1MinSize/Panel2MinSize aquí para evitar InvalidOperationException.
 splitPrincipal = new SplitContainer
 {
 Dock = DockStyle.Fill,
 IsSplitterFixed = false,
 BackColor = Color.FromArgb(222,229,245)
 };
 Controls.Add(splitPrincipal);

 // Ajuste dinámico del splitter
 splitPrincipal.SizeChanged += (s, e) => AjustarSplitterInicial();

 // Panel izquierdo: configuración (layout vertical con scroll)
 var panelConfiguracion = new FlowLayoutPanel
 {
 Dock = DockStyle.Fill,
 FlowDirection = FlowDirection.TopDown,
 WrapContents = false,
 AutoScroll = true,
 Padding = new Padding(12),
 BackColor = Color.White
 };
 splitPrincipal.Panel1.Controls.Add(panelConfiguracion);

 var etiquetaTitulo = new Label
 {
 Text = "Configuración",
 Font = new Font(Font.FontFamily,14, FontStyle.Bold),
 AutoSize = true,
 Margin = new Padding(3,0,3,8)
 };
 panelConfiguracion.Controls.Add(etiquetaTitulo);

 var etiquetaCantidad = new Label
 {
 Text = "Cantidad de datos (máx500 para visualización):",
 AutoSize = true
 };
 panelConfiguracion.Controls.Add(etiquetaCantidad);

 numericCantidad = new NumericUpDown
 {
 Minimum =10,
 Maximum =100000, // Permitimos grandes cantidades para modo rápido
 Value =100,
 Increment =10,
 Width =220
 };
 panelConfiguracion.Controls.Add(numericCantidad);

 botonGenerar = new Button
 {
 Text = "Generar datos",
 Width =220,
 Height =38,
 FlatStyle = FlatStyle.Flat,
 BackColor = Color.FromArgb(0,122,204),
 ForeColor = Color.White
 };
 botonGenerar.FlatAppearance.BorderSize =0;
 botonGenerar.Click += BotonGenerar_Click;
 panelConfiguracion.Controls.Add(botonGenerar);

 // Botón de comparación unificado (elige visual o rápido)
 botonCompararAlgoritmos = new Button
 {
 Text = "Comparar Algoritmos",
 Width =220,
 Height =38,
 Enabled = false,
 FlatStyle = FlatStyle.Flat,
 BackColor = Color.FromArgb(0,153,102),
 ForeColor = Color.White
 };
 botonCompararAlgoritmos.FlatAppearance.BorderSize =0;
 botonCompararAlgoritmos.Click += BotonCompararAlgoritmos_Click;
 panelConfiguracion.Controls.Add(botonCompararAlgoritmos);

 var etiquetaAlgoritmo = new Label
 {
 Text = "Velocidad (anima carrera):",
 AutoSize = true,
 Margin = new Padding(3,12,3,3)
 };
 panelConfiguracion.Controls.Add(etiquetaAlgoritmo);

 var etiquetaVelocidad = new Label
 {
 Text = "Velocidad de animación:",
 AutoSize = true,
 Margin = new Padding(3,4,3,3)
 };
 panelConfiguracion.Controls.Add(etiquetaVelocidad);

 trackBarVelocidad = new TrackBar
 {
 Minimum =1,
 Maximum =100,
 Value =80,
 Width =220,
 TickFrequency =10
 };
 panelConfiguracion.Controls.Add(trackBarVelocidad);

 var etiquetaMuestra = new Label
 {
 Text = "Muestra de datos generados:",
 AutoSize = true,
 Margin = new Padding(3,12,3,3)
 };
 panelConfiguracion.Controls.Add(etiquetaMuestra);

 listaMuestraDatos = new ListBox
 {
 Width =240,
 Height =200
 };
 listaMuestraDatos.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
 panelConfiguracion.Controls.Add(listaMuestraDatos);

 // Panel derecho: visualización (arriba) y resultados (abajo)
 var panelDerecho = new SplitContainer
 {
 Dock = DockStyle.Fill,
 Orientation = Orientation.Horizontal,
 SplitterDistance = (int)(Height *0.55)
 };
 splitPrincipal.Panel2.Controls.Add(panelDerecho);

 // Panel inferior: resultados
 var panelResultados = new Panel
 {
 Dock = DockStyle.Fill,
 Padding = new Padding(12),
 BackColor = Color.FromArgb(247,249,254)
 };
 panelDerecho.Panel2.Controls.Add(panelResultados);

 var etiquetaResultados = new Label
 {
 Text = "Resultados de la Comparación",
 Font = new Font(Font.FontFamily,14, FontStyle.Bold),
 Dock = DockStyle.Top,
 Height =30,
 TextAlign = ContentAlignment.MiddleLeft
 };
 panelResultados.Controls.Add(etiquetaResultados);

 var etiquetaColumnas = new Label
 {
 Text = "Columnas: Algoritmo | Tiempo (ms) | Tiempo (s) | Tiempo (ticks)",
 Dock = DockStyle.Top,
 Height =20,
 TextAlign = ContentAlignment.MiddleLeft,
 ForeColor = Color.FromArgb(80,90,110)
 };
 panelResultados.Controls.Add(etiquetaColumnas);

 listaResultados = new ListView
 {
 View = View.Details,
 FullRowSelect = true,
 GridLines = true,
 HideSelection = false,
 Font = new Font("Segoe UI",10),
 BackColor = Color.White,
 ForeColor = Color.Black,
 Dock = DockStyle.Fill,
 UseCompatibleStateImageBehavior = false,
 OwnerDraw = false,
 HeaderStyle = ColumnHeaderStyle.Clickable
 };
 listaResultados.Columns.Add("Algoritmo",220);
 listaResultados.Columns.Add("Tiempo (ms)",170);
 listaResultados.Columns.Add("Tiempo (s)",130);
 listaResultados.Columns.Add("Tiempo (ticks)",190);
 panelResultados.Controls.Add(listaResultados);

 etiquetaEstado = new Label
 {
 Text = "Genera los datos para comenzar.",
 AutoSize = false,
 Height =30,
 Dock = DockStyle.Bottom,
 TextAlign = ContentAlignment.MiddleLeft,
 ForeColor = Color.FromArgb(80,90,110),
 Padding = new Padding(4)
 };
 panelResultados.Controls.Add(etiquetaEstado);

 // Panel superior: visualización simultánea de3 algoritmos
 panelMulti = new TableLayoutPanel
 {
 Dock = DockStyle.Fill,
 ColumnCount =3,
 RowCount =1,
 BackColor = Color.Black,
 Visible = true,
 Padding = new Padding(8)
 };
 panelMulti.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,33.33f));
 panelMulti.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,33.33f));
 panelMulti.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,33.33f));

 panelBurbujaContenedor = CrearContenedorAlgoritmo("Burbuja", out panelBurbuja, out etiquetaBurbuja);
 panelInsercionContenedor = CrearContenedorAlgoritmo("Inserción", out panelInsercion, out etiquetaInsercion);
 panelQuickContenedor = CrearContenedorAlgoritmo("QuickSort", out panelQuick, out etiquetaQuick);

 panelMulti.Controls.Add(panelBurbujaContenedor,0,0);
 panelMulti.Controls.Add(panelInsercionContenedor,1,0);
 panelMulti.Controls.Add(panelQuickContenedor,2,0);
 panelDerecho.Panel1.Controls.Add(panelMulti);

 // Botón cancelar animación
 botonCancelar = new Button
 {
 Text = "Cancelar Visualización",
 Width =220,
 Height =32,
 Enabled = false,
 FlatStyle = FlatStyle.Flat,
 BackColor = Color.FromArgb(180,30,30),
 ForeColor = Color.White
 };
 botonCancelar.FlatAppearance.BorderSize =0;
 botonCancelar.Click += BotonCancelar_Click;
 panelConfiguracion.Controls.Add(botonCancelar);

 // Botón ayuda
 botonAyuda = new Button
 {
 Text = "Ayuda",
 Width =220,
 Height =32,
 Enabled = true,
 FlatStyle = FlatStyle.Flat,
 BackColor = Color.FromArgb(70,70,90),
 ForeColor = Color.White
 };
 botonAyuda.FlatAppearance.BorderSize =0;
 botonAyuda.Click += BotonAyuda_Click;
 panelConfiguracion.Controls.Add(botonAyuda);

 ResumeLayout();
 }

 /// <summary>
 /// Ajusta de forma segura los mínimos de panel y la distancia del splitter.
 /// </summary>
 private void AjustarSplitterInicial()
 {
 if (splitPrincipal == null) return;

 int min1 =250;
 int min2 =400;
 splitPrincipal.Panel1MinSize = min1;
 splitPrincipal.Panel2MinSize = min2;

 int ancho = splitPrincipal.ClientSize.Width;
 if (min1 + min2 > ancho)
 {
 // Si los mínimos exceden el ancho disponible, reducimos el segundo
 splitPrincipal.Panel2MinSize = Math.Max(0, ancho - min1);
 }

 int maxPermitido = ancho - splitPrincipal.Panel2MinSize;
 int distanciaDeseada =280;
 int distancia = Math.Clamp(distanciaDeseada, splitPrincipal.Panel1MinSize, Math.Max(splitPrincipal.Panel1MinSize, maxPermitido));
 if (distancia >= splitPrincipal.Panel1MinSize && distancia <= ancho - splitPrincipal.Panel2MinSize)
 {
 splitPrincipal.SplitterDistance = distancia;
 }
 }
 #endregion

 #region Utilidades UI
 /// <summary>
 /// Crea un contenedor con etiqueta y un panel de dibujo para un algoritmo.
 /// </summary>
 private Panel CrearContenedorAlgoritmo(string titulo, out Panel panelDibujo, out Label etiqueta)
 {
 var cont = new Panel
 {
 Dock = DockStyle.Fill,
 BackColor = Color.FromArgb(25,25,25),
 Margin = new Padding(6),
 Padding = new Padding(4)
 };

 etiqueta = new Label
 {
 Text = titulo,
 Dock = DockStyle.Top,
 Height =22,
 ForeColor = Color.White,
 Font = new Font("Segoe UI",10, FontStyle.Bold),
 TextAlign = ContentAlignment.MiddleCenter,
 BackColor = Color.FromArgb(45,45,45)
 };
 cont.Controls.Add(etiqueta);

 panelDibujo = new Panel
 {
 Dock = DockStyle.Fill,
 BackColor = Color.Black
 };
 panelDibujo.Paint += PanelGenerico_Paint;
 cont.Controls.Add(panelDibujo);

 // Asegurar etiqueta arriba
 cont.Controls.SetChildIndex(etiqueta,1);
 return cont;
 }

 /// <summary>
 /// Dibuja el estado actual del arreglo (barras) para cada panel de algoritmo.
 /// </summary>
 private void PanelGenerico_Paint(object? sender, PaintEventArgs e)
 {
 var panel = sender as Panel;
 if (panel == null) return;

 int[]? datos;
 int[]? resaltos;

 if (panel == panelBurbuja)
 {
 datos = datosBurbuja;
 resaltos = resaltosBurbuja;
 }
 else if (panel == panelInsercion)
 {
 datos = datosInsercion;
 resaltos = resaltosInsercion;
 }
 else
 {
 datos = datosQuick;
 resaltos = resaltosQuick;
 }

 if (datos == null || datos.Length ==0) return;

 var g = e.Graphics;
 g.Clear(Color.Black);

 int w = panel.ClientSize.Width;
 int h = panel.ClientSize.Height;
 if (w <=0 || h <=0) return;

 float anchoBarra = (float)w / datos.Length;
 int max =0;
 for (int i =0; i < datos.Length; i++) if (datos[i] > max) max = datos[i];
 if (max ==0) return;

 var resaltosSet = resaltos ?? Array.Empty<int>();
 var normalBrush = Brushes.White;
 using var highlightBrush = new SolidBrush(Color.FromArgb(200,50,50));

 for (int i =0; i < datos.Length; i++)
 {
 float alt = ((float)datos[i] / max) * h;
 bool esResalto = Array.IndexOf(resaltosSet, i) >=0;
 var brush = esResalto ? highlightBrush : normalBrush;
 g.FillRectangle(brush, i * anchoBarra, h - alt, anchoBarra, alt);
 }
 }

 /// <summary>
 /// Habilita/deshabilita controles mientras se ejecuta la carrera.
 /// </summary>
 private void SetControles(bool habilitar)
 {
 botonGenerar.Enabled = habilitar;
 numericCantidad.Enabled = habilitar;
 trackBarVelocidad.Enabled = habilitar;
 botonCompararAlgoritmos.Enabled = habilitar && datosOriginales.Length >0;
 botonAyuda.Enabled = true;
 botonCancelar.Enabled = !habilitar; // Solo activo mientras corre
 }
 #endregion

 #region Eventos de botones
 /// <summary>
 /// Genera los datos aleatorios y refresca la muestra/estado inicial.
 /// </summary>
 private void BotonGenerar_Click(object sender, EventArgs e)
 {
 int cantidad = (int)numericCantidad.Value;
 datosOriginales = new int[cantidad];
 for (int i =0; i < cantidad; i++)
 datosOriginales[i] = rng.Next(1,1001); // Valores entre1 y1000 para mejor visualización

 listaMuestraDatos.BeginUpdate();
 listaMuestraDatos.Items.Clear();
 int maxMostrar = Math.Min(50, cantidad);
 for (int i =0; i < maxMostrar; i++)
 listaMuestraDatos.Items.Add(datosOriginales[i]);
 listaMuestraDatos.EndUpdate();

 botonCompararAlgoritmos.Enabled = true;
 listaResultados.Items.Clear();
 etiquetaEstado.Text = $"Datos generados: {cantidad} números aleatorios.";

 // Reset paneles de carrera
 datosBurbuja = (int[])datosOriginales.Clone();
 datosInsercion = (int[])datosOriginales.Clone();
 datosQuick = (int[])datosOriginales.Clone();
 resaltosBurbuja = resaltosInsercion = resaltosQuick = Array.Empty<int>();
 panelBurbuja.Invalidate();
 panelInsercion.Invalidate();
 panelQuick.Invalidate();
 }

 /// <summary>
 /// Selecciona modalidad de comparación (visual o rápida) según tamaño de datos y preferencia del usuario.
 /// </summary>
 private void BotonCompararAlgoritmos_Click(object? sender, EventArgs e)
 {
 if (datosOriginales == null || datosOriginales.Length ==0)
 {
 MessageBox.Show("Primero genera los datos.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
 return;
 }

 if (datosOriginales.Length >500)
 {
 // Si son demasiados datos, solo se permiten resultados rápidos
 MessageBox.Show("Debido a la gran cantidad de datos (>500), solo se mostrarán los resultados rápidos sin visualización.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
 EjecutarResultadosRapidos();
 }
 else
 {
 var resultado = MessageBox.Show(
 "¿Desea visualizar la carrera en tiempo real?\n\n" +
 "• Sí: Verá la animación de los tres algoritmos compitiendo.\n" +
 "• No: Obtendrá los resultados de tiempo instantáneamente.",
 "Modo de Comparación",
 MessageBoxButtons.YesNo,
 MessageBoxIcon.Question);

 if (resultado == DialogResult.Yes)
 {
 BotonVisualizarTres_Click(sender, e);
 }
 else
 {
 EjecutarResultadosRapidos();
 }
 }
 }

 /// <summary>
 /// Inicia la visualización en paralelo de los tres algoritmos y, al finalizar,
 /// mide los tiempos reales sin animación para mostrarlos en la tabla.
 /// </summary>
 private async void BotonVisualizarTres_Click(object? sender, EventArgs e)
 {
 ctsVisualizacion?.Cancel();
 ctsVisualizacion = new CancellationTokenSource();
 var token = ctsVisualizacion.Token;

 // Preparar copias independientes para cada algoritmo
 datosBurbuja = (int[])datosOriginales.Clone();
 datosInsercion = (int[])datosOriginales.Clone();
 datosQuick = (int[])datosOriginales.Clone();
 resaltosBurbuja = Array.Empty<int>();
 resaltosInsercion = Array.Empty<int>();
 resaltosQuick = Array.Empty<int>();

 // Asegurar que el panelMulti está agregado al panel superior
 var splitSecundario = splitPrincipal.Panel2.Controls[0] as SplitContainer;
 if (splitSecundario != null && !splitSecundario.Panel1.Controls.Contains(panelMulti))
 {
 splitSecundario.Panel1.Controls.Clear();
 splitSecundario.Panel1.Controls.Add(panelMulti);
 }
 panelMulti.Visible = true;

 etiquetaEstado.Text = "Visualizando simultáneamente...";
 listaResultados.Items.Clear();
 SetControles(false);
 botonCancelar.Enabled = true;

 try
 {
 // Progresos para actualizar cada panel
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

 // Lanzar las tres tareas visuales (sus tiempos incluyen delays, NO se usarán para la tabla)
 var tBurbuja = EjecutarVisual(ALG_BURBUJA, OrdenamientoBurbujaVisual, datosBurbuja!, progressBurbuja, token);
 var tInsercion = EjecutarVisual(ALG_INSERCION, OrdenamientoInsercionVisual, datosInsercion!, progressInsercion, token);
 var tQuick = EjecutarVisual(ALG_QUICKSORT, OrdenamientoQuickSortVisual, datosQuick!, progressQuick, token);

 await Task.WhenAll(tBurbuja, tInsercion, tQuick);

 if (!token.IsCancellationRequested)
 {
 // Medimos nuevamente SIN animación (tiempos reales del algoritmo) sobre el arreglo original
 var resultadosReales = new System.Collections.Generic.List<(string nombre, TimeSpan tiempo, long ticks)>
 {
 MedirAlgoritmo(ALG_BURBUJA, OrdenamientoBurbuja),
 MedirAlgoritmo(ALG_INSERCION, OrdenamientoInsercion),
 MedirAlgoritmo(ALG_QUICKSORT, OrdenamientoQuickSort)
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
 SetControles(true);
 botonCancelar.Enabled = false;
 ctsVisualizacion = null;
 }
 }

 private void BotonCancelar_Click(object? sender, EventArgs e)
 {
 ctsVisualizacion?.Cancel();
 etiquetaEstado.Text = "Cancelando...";
 }

 private void BotonAyuda_Click(object? sender, EventArgs e)
 {
 using var dlg = new HelpForm();
 dlg.ShowDialog(this);
 }
 #endregion

 #region Comparación y renderizado de resultados
 /// <summary>
 /// Ejecuta los tres algoritmos en modo rápido (sin animación) y muestra la tabla de tiempos.
 /// </summary>
 private void EjecutarResultadosRapidos()
 {
 if (datosOriginales == null || datosOriginales.Length ==0) return;

 var resultados = new System.Collections.Generic.List<(string nombre, TimeSpan tiempo, long ticks)>();
 Cursor = Cursors.WaitCursor;
 SetControles(false);
 try
 {
 resultados.Add(MedirAlgoritmo(ALG_BURBUJA, OrdenamientoBurbuja));
 resultados.Add(MedirAlgoritmo(ALG_INSERCION, OrdenamientoInsercion));
 resultados.Add(MedirAlgoritmo(ALG_QUICKSORT, OrdenamientoQuickSort));
 }
 finally
 {
 Cursor = Cursors.Default;
 SetControles(true);
 }

 RenderizarResultados(resultados, true);
        }

        /// <summary>
        /// Muestra resultados en el ListView en un orden fijo y resalta el ganador.
        /// Asegura que Burbuja, Inserción y QuickSort estén presentes; si falta alguno se mide al vuelo.
        /// </summary>
        private void RenderizarResultados(
    System.Collections.Generic.List<(string nombre, TimeSpan tiempo, long ticks)> resultados,
    bool mostrarMensaje)
        {
            // Función local: devuelve el resultado de un algoritmo.
            // Si no está en la lista, lo mide y lo agrega.
            (string nombre, TimeSpan tiempo, long ticks) ObtenerOMedir(
                string nombre,
                Action<int[]> algoritmo)
            {
                var encontrado = resultados.FirstOrDefault(r =>
                    string.Equals(r.nombre, nombre, StringComparison.Ordinal));

                if (!string.IsNullOrEmpty(encontrado.nombre))
                    return encontrado;

                var medido = MedirAlgoritmo(nombre, algoritmo);
                resultados.Add(medido);
                return medido;
            }

            // Asegurar los tres algoritmos
            var resQuick = ObtenerOMedir(ALG_QUICKSORT, OrdenamientoQuickSort);
            var resInsercion = ObtenerOMedir(ALG_INSERCION, OrdenamientoInsercion);
            var resBurbuja = ObtenerOMedir(ALG_BURBUJA, OrdenamientoBurbuja);

            // Limpiar y llenar la tabla siempre con los tres
            listaResultados.BeginUpdate();
            try
            {
                listaResultados.Sorting = SortOrder.None;
                if (listaResultados.Groups.Count > 0)
                {
                    listaResultados.ShowGroups = false;
                    listaResultados.Groups.Clear();
                }

                listaResultados.Items.Clear();

                // Orden fijo: QuickSort, Inserción, Burbuja
                AgregarFila(resQuick);
                AgregarFila(resInsercion);
                AgregarFila(resBurbuja);
            }
            finally
            {
                listaResultados.EndUpdate();
                listaResultados.Refresh();
            }

            // Elegir ganador entre los tres
            var todos = new[] { resQuick, resInsercion, resBurbuja };
            var ganador = todos.OrderBy(r => r.tiempo).First();

            foreach (ListViewItem itm in listaResultados.Items)
            {
                if (string.Equals(itm.Text, ganador.nombre, StringComparison.Ordinal))
                {
                    itm.BackColor = Color.FromArgb(230, 255, 230);
                    itm.Font = new Font(itm.Font, FontStyle.Bold);
                    itm.EnsureVisible();
                }
            }

            etiquetaEstado.Text = $"Ganador: {ganador.nombre} con {ganador.tiempo.TotalMilliseconds:N3} ms";

            if (mostrarMensaje)
            {
                MessageBox.Show(
                    $"Algoritmo ganador: {ganador.nombre}\n" +
                    $"Tiempo: {ganador.tiempo.TotalMilliseconds:N3} ms" +
                    (ganador.tiempo.TotalMilliseconds >= 1000
                        ? $" ({ganador.tiempo.TotalSeconds:N3} s)"
                        : "") +
                    "\n\nRevisa la tabla de resultados para ver el detalle de cada algoritmo.",
                    "Resultados de la Comparación",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }


        /// <summary>
        /// Agrega una fila al ListView con los tiempos medidos.
        /// </summary>
        private void AgregarFila((string nombre, TimeSpan tiempo, long ticks) r)
 {
 var item = new ListViewItem(r.nombre);
 item.SubItems.Add(r.tiempo.TotalMilliseconds.ToString("N3"));
 item.SubItems.Add(r.tiempo.TotalMilliseconds >=1000 ? r.tiempo.TotalSeconds.ToString("N3") : "-");
 item.SubItems.Add(r.ticks.ToString("N0"));
 listaResultados.Items.Add(item);
 }

 /// <summary>
 /// Mide el tiempo de un algoritmo sobre una copia de los datos originales.
 /// </summary>
 private (string nombre, TimeSpan tiempo, long ticks) MedirAlgoritmo(string nombre, Action<int[]> algoritmo)
 {
 int[] copia = (int[])datosOriginales.Clone();
 var sw = Stopwatch.StartNew();
 algoritmo(copia);
 sw.Stop();
 return (nombre, sw.Elapsed, sw.ElapsedTicks);
 }
 #endregion

 #region Algoritmos no visuales (modo rápido)
 private void OrdenamientoBurbuja(int[] arreglo)
 {
 int n = arreglo.Length;
 bool huboIntercambio;
 do
 {
 huboIntercambio = false;
 for (int i =0; i < n -1; i++)
 {
 if (arreglo[i] > arreglo[i +1])
 {
 (arreglo[i], arreglo[i +1]) = (arreglo[i +1], arreglo[i]);
 huboIntercambio = true;
 }
 }
 n--;
 } while (huboIntercambio);
 }

 private void OrdenamientoInsercion(int[] arreglo)
 {
 for (int i =1; i < arreglo.Length; i++)
 {
 int clave = arreglo[i];
 int j = i -1;
 while (j >=0 && arreglo[j] > clave)
 {
 arreglo[j +1] = arreglo[j];
 j--;
 }
 arreglo[j +1] = clave;
 }
 }

 private void OrdenamientoQuickSort(int[] arreglo)
 {
 QuickSortRecursivo(arreglo,0, arreglo.Length -1);
 }

 private void QuickSortRecursivo(int[] arreglo, int izquierda, int derecha)
 {
 if (izquierda >= derecha) return;

 int i = izquierda;
 int j = derecha;
 int pivote = arreglo[(izquierda + derecha) /2];

 while (i <= j)
 {
 while (arreglo[i] < pivote) i++;
 while (arreglo[j] > pivote) j--;

 if (i <= j)
 {
 (arreglo[i], arreglo[j]) = (arreglo[j], arreglo[i]);
 i++;
 j--;
 }
 }

 if (izquierda < j) QuickSortRecursivo(arreglo, izquierda, j);
 if (i < derecha) QuickSortRecursivo(arreglo, i, derecha);
 }
 #endregion

 #region Algoritmos visuales (async con animación)
 /// <summary>
 /// Versión visual del ordenamiento Burbuja con reportes periódicos de estado.
 /// </summary>
 private async Task OrdenamientoBurbujaVisual(int[] arreglo, IProgress<(int[] estado, int[] resaltos)> progreso, CancellationToken token)
 {
 int n = arreglo.Length;
 bool hubo;
 int delay = Math.Max(0,100 - trackBarVelocidad.Value);
 int frameSkip = Math.Max(1, trackBarVelocidad.Value /10);
 int counter =0;

 do
 {
 hubo = false;
 for (int i =0; i < n -1; i++)
 {
 if (token.IsCancellationRequested) return;
 if (arreglo[i] > arreglo[i +1])
 {
 (arreglo[i], arreglo[i +1]) = (arreglo[i +1], arreglo[i]);
 hubo = true;
 counter++;
 if (counter % frameSkip ==0)
 {
 progreso.Report((arreglo, new[] { i, i +1 }));
 if (delay >0) await Task.Delay(delay, token);
 }
 }
 }
 n--;
 } while (hubo);

 progreso.Report((arreglo, Array.Empty<int>()));
 }

 /// <summary>
 /// Versión visual del ordenamiento por Inserción.
 /// </summary>
 private async Task OrdenamientoInsercionVisual(int[] arreglo, IProgress<(int[] estado, int[] resaltos)> progreso, CancellationToken token)
 {
 int delay = Math.Max(0,100 - trackBarVelocidad.Value);
 int frameSkip = Math.Max(1, trackBarVelocidad.Value /10);
 int counter =0;

 for (int i =1; i < arreglo.Length; i++)
 {
 if (token.IsCancellationRequested) return;
 int clave = arreglo[i];
 int j = i -1;
 while (j >=0 && arreglo[j] > clave)
 {
 if (token.IsCancellationRequested) return;
 arreglo[j +1] = arreglo[j];
 j--;
 counter++;
 if (counter % frameSkip ==0)
 {
 progreso.Report((arreglo, new[] { j +1, i }));
 if (delay >0) await Task.Delay(delay, token);
 }
 }
 arreglo[j +1] = clave;
 counter++;
 if (counter % frameSkip ==0)
 {
 progreso.Report((arreglo, new[] { j +1 }));
 if (delay >0) await Task.Delay(delay, token);
 }
 }

 progreso.Report((arreglo, Array.Empty<int>()));
 }

 /// <summary>
 /// Versión visual de QuickSort (recursivo) con reportes periódicos.
 /// </summary>
 private async Task OrdenamientoQuickSortVisual(int[] arreglo, IProgress<(int[] estado, int[] resaltos)> progreso, CancellationToken token)
 {
 int delay = Math.Max(0,100 - trackBarVelocidad.Value);
 int frameSkip = Math.Max(1, trackBarVelocidad.Value /10);
 quickVisualCounter =0; // Reiniciar acumulador visual

 await QuickSortRecursivoVisual(arreglo,0, arreglo.Length -1, progreso, token, delay, frameSkip);
 progreso.Report((arreglo, Array.Empty<int>()));
 }

 private async Task QuickSortRecursivoVisual(int[] a, int izq, int der, IProgress<(int[] estado, int[] resaltos)> progreso, CancellationToken token, int delay, int frameSkip)
 {
 if (izq >= der) return;

 int i = izq;
 int j = der;
 int pivote = a[(izq + der) /2];

 while (i <= j)
 {
 if (token.IsCancellationRequested) return;
 while (a[i] < pivote) i++;
 while (a[j] > pivote) j--;

 if (i <= j)
 {
 (a[i], a[j]) = (a[j], a[i]);
 quickVisualCounter++;
 if (quickVisualCounter % frameSkip ==0)
 {
 progreso.Report((a, new[] { i, j }));
 if (delay >0) await Task.Delay(delay, token);
 }
 i++; j--;
 }
 }

 if (izq < j) await QuickSortRecursivoVisual(a, izq, j, progreso, token, delay, frameSkip);
 if (i < der) await QuickSortRecursivoVisual(a, i, der, progreso, token, delay, frameSkip);
 }
 #endregion

 #region Ejecución visual - medición con cronómetro
 /// <summary>
 /// Ejecuta un algoritmo visual, midiendo su duración (incluye delays de animación).
 /// El tiempo medido AQUÍ no se usa para la tabla; solo para sincronizar fin.
 /// </summary>
 private async Task<(string nombre, TimeSpan tiempo, long ticks)> EjecutarVisual(
 string nombre,
 Func<int[], IProgress<(int[] estado, int[] resaltos)>, CancellationToken, Task> algoritmo,
 int[] datos,
 IProgress<(int[] estado, int[] resaltos)> progreso,
 CancellationToken token)
 {
 var sw = Stopwatch.StartNew();
 await algoritmo(datos, progreso, token);
 sw.Stop();
 return (nombre, sw.Elapsed, sw.ElapsedTicks);
 }
 #endregion
 }
}
