using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
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
    /// La UI se construye dinámicamente en <see cref="ConfigurarInterfazGrafica"/>.
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
    private TableLayoutPanel panelVisualizaciones;
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
        DoubleBuffered = true;         // Reduce parpadeo al repintar
        KeyPreview = true;             // Permite capturar teclas a nivel formulario
        KeyDown += Formulario_KeyDown; // Manejo de teclas (F11 / Escape)
    }

    /// <summary>
    /// Construye la interfaz cuando el formulario ya tiene tamaño real.
    /// </summary>
    protected override void OnLoad(EventArgs e)
    {
        // ----- INICIO: OnLoad -----
        base.OnLoad(e);
        if (!interfazConfigurada)
        {
            ConfigurarInterfazGrafica();
            AjustarSplitterInicial();
        }

        // ----- FIN: OnLoad -----
    }
    #endregion

    #region Pantalla completa y teclado
    private void Formulario_KeyDown(object? remitente, KeyEventArgs eventoTeclado)
    {
        // ----- INICIO: Formulario_KeyDown -----
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

        // ----- FIN: Formulario_KeyDown -----
    }

    /// <summary>
    /// Activa o desactiva el modo pantalla completa guardando el estado anterior.
    /// </summary>
    private void CambiarModoPantallaCompleta(bool activar)
    {
        // ----- INICIO: CambiarModoPantallaCompleta -----
        if (activar == estaPantallaCompleta)
        {
            return;
        }

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

        // ----- FIN: CambiarModoPantallaCompleta -----
    }
    #endregion

    #region Construcción de interfaz
    /// <summary>
    /// Crea y agrega todos los controles del formulario.
    /// </summary>
    private void ConfigurarInterfazGrafica()
    {
        // ----- INICIO: ConfigurarInterfazGrafica -----
        interfazConfigurada = true;
        SuspendLayout();

        Text = "Comparación de Algoritmos de Ordenamiento";
        Width = 1000;
        Height = 600;
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(237, 241, 250);

        // Split principal: Panel izquierdo (configuración) y derecho (visualización/resultados)
        // NOTA: No establecer Panel1MinSize/Panel2MinSize aquí para evitar InvalidOperationException.
        splitPrincipal = new SplitContainer { Dock = DockStyle.Fill, IsSplitterFixed = false,
                                              BackColor = Color.FromArgb(222, 229, 245) };
        Controls.Add(splitPrincipal);

        // Ajuste dinámico del splitter
        splitPrincipal.SizeChanged += (s, e) => AjustarSplitterInicial();

        // Panel izquierdo: configuración (layout vertical con scroll)
        var panelConfiguracion = new FlowLayoutPanel { Dock = DockStyle.Fill,     FlowDirection = FlowDirection.TopDown,
                                                       WrapContents = false,      AutoScroll = true,
                                                       Padding = new Padding(12), BackColor = Color.White };
        splitPrincipal.Panel1.Controls.Add(panelConfiguracion);

        var etiquetaTitulo = new Label { Text = "Configuración", Font = new Font(Font.FontFamily, 14, FontStyle.Bold),
                                         AutoSize = true, Margin = new Padding(3, 0, 3, 8) };
        panelConfiguracion.Controls.Add(etiquetaTitulo);

        var etiquetaCantidad = new Label { Text = "Cantidad de datos (máx500 para visualización):", AutoSize = true };
        panelConfiguracion.Controls.Add(etiquetaCantidad);

        numericCantidad = new NumericUpDown { Minimum = 10,
                                              Maximum = 100000, // Permitimos grandes cantidades para modo rápido
                                              Value = 100, Increment = 10, Width = 220 };
        panelConfiguracion.Controls.Add(numericCantidad);

        botonGenerar = new Button { Text = "Generar datos",
                                    Width = 220,
                                    Height = 38,
                                    FlatStyle = FlatStyle.Flat,
                                    BackColor = Color.FromArgb(0, 122, 204),
                                    ForeColor = Color.White };
        botonGenerar.FlatAppearance.BorderSize = 0;
        botonGenerar.Click += BotonGenerar_Click;
        panelConfiguracion.Controls.Add(botonGenerar);

        // Botón de comparación unificado (elige visual o rápido)
        botonCompararAlgoritmos = new Button { Text = "Comparar Algoritmos",
                                               Width = 220,
                                               Height = 38,
                                               Enabled = false,
                                               FlatStyle = FlatStyle.Flat,
                                               BackColor = Color.FromArgb(0, 153, 102),
                                               ForeColor = Color.White };
        botonCompararAlgoritmos.FlatAppearance.BorderSize = 0;
        botonCompararAlgoritmos.Click += BotonCompararAlgoritmos_Click;
        panelConfiguracion.Controls.Add(botonCompararAlgoritmos);

        var etiquetaAlgoritmo =
            new Label { Text = "Velocidad (anima carrera):", AutoSize = true, Margin = new Padding(3, 12, 3, 3) };
        panelConfiguracion.Controls.Add(etiquetaAlgoritmo);

        var etiquetaVelocidad =
            new Label { Text = "Velocidad de animación:", AutoSize = true, Margin = new Padding(3, 4, 3, 3) };
        panelConfiguracion.Controls.Add(etiquetaVelocidad);

        trackBarVelocidad = new TrackBar { Minimum = 1, Maximum = 100, Value = 80, Width = 220, TickFrequency = 10 };
        panelConfiguracion.Controls.Add(trackBarVelocidad);

        var etiquetaMuestra =
            new Label { Text = "Muestra de datos generados:", AutoSize = true, Margin = new Padding(3, 12, 3, 3) };
        panelConfiguracion.Controls.Add(etiquetaMuestra);

        listaMuestraDatos = new ListBox { Width = 240, Height = 200 };
        listaMuestraDatos.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        panelConfiguracion.Controls.Add(listaMuestraDatos);

        // Panel derecho: visualización (arriba) y resultados (abajo)
        var panelDerecho = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal,
                                                SplitterDistance = (int)(Height * 0.55) };
        splitPrincipal.Panel2.Controls.Add(panelDerecho);

        // Panel inferior: resultados
        var panelResultados =
            new Panel { Dock = DockStyle.Fill, Padding = new Padding(12), BackColor = Color.FromArgb(247, 249, 254) };
        panelDerecho.Panel2.Controls.Add(panelResultados);

        var etiquetaResultados =
            new Label { Text = "Resultados de la Comparación", Font = new Font(Font.FontFamily, 14, FontStyle.Bold),
                        Dock = DockStyle.Top, Height = 30, TextAlign = ContentAlignment.MiddleLeft };
        panelResultados.Controls.Add(etiquetaResultados);

        var etiquetaColumnas =
            new Label { Text = "Columnas: Algoritmo | Tiempo (ms) | Tiempo (s) | Tiempo (ticks)", Dock = DockStyle.Top,
                        Height = 20, TextAlign = ContentAlignment.MiddleLeft, ForeColor = Color.FromArgb(80, 90, 110) };
        panelResultados.Controls.Add(etiquetaColumnas);

        listaResultados = new ListView { View = View.Details,
                                         FullRowSelect = true,
                                         GridLines = true,
                                         HideSelection = false,
                                         Font = new Font("Segoe UI", 10),
                                         BackColor = Color.White,
                                         ForeColor = Color.Black,
                                         Dock = DockStyle.Fill,
                                         UseCompatibleStateImageBehavior = false,
                                         OwnerDraw = false,
                                         HeaderStyle = ColumnHeaderStyle.Clickable };
        listaResultados.Columns.Add("Algoritmo", 220);
        listaResultados.Columns.Add("Tiempo (ms)", 170);
        listaResultados.Columns.Add("Tiempo (s)", 130);
        listaResultados.Columns.Add("Tiempo (ticks)", 190);
        panelResultados.Controls.Add(listaResultados);

        etiquetaEstado = new Label { Text = "Genera los datos para comenzar.",
                                     AutoSize = false,
                                     Height = 30,
                                     Dock = DockStyle.Bottom,
                                     TextAlign = ContentAlignment.MiddleLeft,
                                     ForeColor = Color.FromArgb(80, 90, 110),
                                     Padding = new Padding(4) };
        panelResultados.Controls.Add(etiquetaEstado);

        // Panel superior: visualización simultánea de3 algoritmos
        panelVisualizaciones =
            new TableLayoutPanel { Dock = DockStyle.Fill,   ColumnCount = 3, RowCount = 1,
                                   BackColor = Color.Black, Visible = true,  Padding = new Padding(8) };
        panelVisualizaciones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        panelVisualizaciones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        panelVisualizaciones.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));

        panelBurbujaContenedor = CrearContenedorAlgoritmo("Burbuja", out panelBurbuja, out etiquetaBurbuja);
        panelInsercionContenedor = CrearContenedorAlgoritmo("Inserción", out panelInsercion, out etiquetaInsercion);
        panelQuickContenedor = CrearContenedorAlgoritmo("QuickSort", out panelQuick, out etiquetaQuick);

        panelVisualizaciones.Controls.Add(panelBurbujaContenedor, 0, 0);
        panelVisualizaciones.Controls.Add(panelInsercionContenedor, 1, 0);
        panelVisualizaciones.Controls.Add(panelQuickContenedor, 2, 0);
        panelDerecho.Panel1.Controls.Add(panelVisualizaciones);

        // Botón cancelar animación
        botonCancelar = new Button { Text = "Cancelar Visualización",
                                     Width = 220,
                                     Height = 32,
                                     Enabled = false,
                                     FlatStyle = FlatStyle.Flat,
                                     BackColor = Color.FromArgb(180, 30, 30),
                                     ForeColor = Color.White };
        botonCancelar.FlatAppearance.BorderSize = 0;
        botonCancelar.Click += BotonCancelar_Click;
        panelConfiguracion.Controls.Add(botonCancelar);

        // Botón ayuda
        botonAyuda = new Button { Text = "Ayuda",
                                  Width = 220,
                                  Height = 32,
                                  Enabled = true,
                                  FlatStyle = FlatStyle.Flat,
                                  BackColor = Color.FromArgb(70, 70, 90),
                                  ForeColor = Color.White };
        botonAyuda.FlatAppearance.BorderSize = 0;
        botonAyuda.Click += BotonAyuda_Click;
        panelConfiguracion.Controls.Add(botonAyuda);

        ResumeLayout();

        // ----- FIN: ConfigurarInterfazGrafica -----
    }

    /// <summary>
    /// Ajusta de forma segura los mínimos de panel y la distancia del splitter.
    /// </summary>
    private void AjustarSplitterInicial()
    {
        // ----- INICIO: AjustarSplitterInicial -----
        if (splitPrincipal == null)
        {
            return;
        }

        int anchoMinimoPanelIzquierdo = 250;
        int anchoMinimoPanelDerecho = 400;
        splitPrincipal.Panel1MinSize = anchoMinimoPanelIzquierdo;
        splitPrincipal.Panel2MinSize = anchoMinimoPanelDerecho;

        int anchoDisponible = splitPrincipal.ClientSize.Width;
        if (anchoMinimoPanelIzquierdo + anchoMinimoPanelDerecho > anchoDisponible)
        {
            splitPrincipal.Panel2MinSize = Math.Max(0, anchoDisponible - anchoMinimoPanelIzquierdo);
        }

        int anchoMaximoPermitido = anchoDisponible - splitPrincipal.Panel2MinSize;
        int distanciaPreferida = 280;
        int distanciaAplicada = Math.Clamp(distanciaPreferida, splitPrincipal.Panel1MinSize,
                                           Math.Max(splitPrincipal.Panel1MinSize, anchoMaximoPermitido));

        if (distanciaAplicada >= splitPrincipal.Panel1MinSize &&
            distanciaAplicada <= anchoDisponible - splitPrincipal.Panel2MinSize)
        {
            splitPrincipal.SplitterDistance = distanciaAplicada;
        }

        // ----- FIN: AjustarSplitterInicial -----
    }
    #endregion

    #region Utilidades UI
    /// <summary>
    /// Crea un contenedor con etiqueta y un panel de dibujo para un algoritmo.
    /// </summary>
    private Panel CrearContenedorAlgoritmo(string titulo, out Panel panelDibujo, out Label etiqueta)
    {
        // ----- INICIO: CrearContenedorAlgoritmo -----
        var panelContenedor = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(25, 25, 25),
                                          Margin = new Padding(6), Padding = new Padding(4) };

        etiqueta = new Label { Text = titulo,
                               Dock = DockStyle.Top,
                               Height = 22,
                               ForeColor = Color.White,
                               Font = new Font("Segoe UI", 10, FontStyle.Bold),
                               TextAlign = ContentAlignment.MiddleCenter,
                               BackColor = Color.FromArgb(45, 45, 45) };
        panelContenedor.Controls.Add(etiqueta);

        panelDibujo = new Panel { Dock = DockStyle.Fill, BackColor = Color.Black };
        panelDibujo.Paint += PanelAlgoritmo_Pintar;
        panelContenedor.Controls.Add(panelDibujo);

        panelContenedor.Controls.SetChildIndex(etiqueta, 1);

        // ----- FIN: CrearContenedorAlgoritmo -----
        return panelContenedor;
    }

    /// <summary>
    /// Dibuja el estado actual del arreglo (barras) para cada panel de algoritmo.
    /// </summary>
    private void PanelAlgoritmo_Pintar(object? remitente, PaintEventArgs argumentosPintado)
    {
        // ----- INICIO: PanelAlgoritmo_Pintar -----
        if (remitente is not Panel panelDestino)
        {
            return;
        }

        int[] ? datosParaDibujar;
        int[] ? indicesResaltados;

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

        if (datosParaDibujar == null || datosParaDibujar.Length == 0)
        {
            return;
        }

        var lienzo = argumentosPintado.Graphics;
        lienzo.Clear(Color.Black);

        int anchoPanel = panelDestino.ClientSize.Width;
        int altoPanel = panelDestino.ClientSize.Height;
        if (anchoPanel <= 0 || altoPanel <= 0)
        {
            return;
        }

        float anchoBarra = (float)anchoPanel / datosParaDibujar.Length;
        int valorMaximo = 0;
        for (int indice = 0; indice < datosParaDibujar.Length; indice++)
        {
            if (datosParaDibujar[indice] > valorMaximo)
            {
                valorMaximo = datosParaDibujar[indice];
            }
        }

        if (valorMaximo == 0)
        {
            return;
        }

        var resaltos = indicesResaltados ?? Array.Empty<int>();
        var pincelNormal = Brushes.White;
        using var pincelResaltado = new SolidBrush(Color.FromArgb(200, 50, 50));

        for (int indice = 0; indice < datosParaDibujar.Length; indice++)
        {
            float alturaBarra = ((float)datosParaDibujar[indice] / valorMaximo) * altoPanel;
            bool esResaltado = Array.IndexOf(resaltos, indice) >= 0;
            var pincelActual = esResaltado ? pincelResaltado : pincelNormal;
            lienzo.FillRectangle(pincelActual, indice * anchoBarra, altoPanel - alturaBarra, anchoBarra, alturaBarra);
        }

        // ----- FIN: PanelAlgoritmo_Pintar -----
    }

    /// <summary>
    /// Habilita/deshabilita controles mientras se ejecuta la carrera.
    /// </summary>
    private void ActualizarEstadoControles(bool habilitarControles)
    {
        // ----- INICIO: ActualizarEstadoControles -----
        botonGenerar.Enabled = habilitarControles;
        numericCantidad.Enabled = habilitarControles;
        trackBarVelocidad.Enabled = habilitarControles;
        botonCompararAlgoritmos.Enabled = habilitarControles && datosOriginales.Length > 0;
        botonAyuda.Enabled = true;
        botonCancelar.Enabled = !habilitarControles;

        // ----- FIN: ActualizarEstadoControles -----
    }
    #endregion

    #region Eventos de botones
    /// <summary>
    /// Genera los datos aleatorios y refresca la muestra/estado inicial.
    /// </summary>
    private void BotonGenerar_Click(object remitente, EventArgs argumentos)
    {
        // ----- INICIO: BotonGenerar_Click -----
        int cantidadDatos = (int)numericCantidad.Value;
        datosOriginales = new int[cantidadDatos];
        for (int indice = 0; indice < cantidadDatos; indice++)
        {
            datosOriginales[indice] = rng.Next(1, 1001);
        }

        listaMuestraDatos.BeginUpdate();
        listaMuestraDatos.Items.Clear();
        int cantidadAMostrar = Math.Min(50, cantidadDatos);
        for (int indice = 0; indice < cantidadAMostrar; indice++)
        {
            listaMuestraDatos.Items.Add(datosOriginales[indice]);
        }
        listaMuestraDatos.EndUpdate();

        botonCompararAlgoritmos.Enabled = true;
        listaResultados.Items.Clear();
        etiquetaEstado.Text = $"Datos generados: {cantidadDatos} números aleatorios.";

        datosBurbuja = (int[])datosOriginales.Clone();
        datosInsercion = (int[])datosOriginales.Clone();
        datosQuick = (int[])datosOriginales.Clone();
        resaltosBurbuja = resaltosInsercion = resaltosQuick = Array.Empty<int>();
        panelBurbuja.Invalidate();
        panelInsercion.Invalidate();
        panelQuick.Invalidate();

        // ----- FIN: BotonGenerar_Click -----
    }

    /// <summary>
    /// Selecciona modalidad de comparación (visual o rápida) según tamaño de datos y preferencia del usuario.
    /// </summary>
    private void BotonCompararAlgoritmos_Click(object? remitente, EventArgs argumentos)
    {
        // ----- INICIO: BotonCompararAlgoritmos_Click -----
        if (datosOriginales == null || datosOriginales.Length == 0)
        {
            MessageBox.Show("Primero genera los datos.", "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        if (datosOriginales.Length > 500)
        {
            MessageBox.Show("Debido a la gran cantidad de datos (>500), solo se mostrarán los resultados rápidos sin " +
                            "visualización.",
                            "Aviso", MessageBoxButtons.OK, MessageBoxIcon.Information);
            EjecutarModoRapido();
        }
        else
        {
            var resultado = MessageBox.Show("¿Desea visualizar la carrera en tiempo real?\n\n" +
                                                "  Sí: Ver la animación de los tres algoritmos compitiendo.\n" +
                                                "  No: Obtendrás los resultados de tiempo instantáneamente.",
                                            "Modo de Comparación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (resultado == DialogResult.Yes)
            {
                BotonVisualizarCarrera_Click(remitente, argumentos);
            }
            else
            {
                EjecutarModoRapido();
            }
        }

        // ----- FIN: BotonCompararAlgoritmos_Click -----
    }

    /// <summary>
    /// Inicia la visualización en paralelo de los tres algoritmos y, al finalizar,
    /// mide los tiempos reales sin animación para mostrarlos en la tabla.
    /// </summary>
    private async void BotonVisualizarCarrera_Click(object? remitente, EventArgs argumentos)
    {
        // ----- INICIO: BotonVisualizarCarrera_Click -----
        ctsVisualizacion?.Cancel();
        ctsVisualizacion = new CancellationTokenSource();
        var tokenVisualizacion = ctsVisualizacion.Token;

        // Preparar copias independientes para cada algoritmo
        datosBurbuja = (int[])datosOriginales.Clone();
        datosInsercion = (int[])datosOriginales.Clone();
        datosQuick = (int[])datosOriginales.Clone();
        resaltosBurbuja = Array.Empty<int>();
        resaltosInsercion = Array.Empty<int>();
        resaltosQuick = Array.Empty<int>();

        // Asegurar que el panelVisualizaciones está agregado al panel superior
        var splitSecundario = splitPrincipal.Panel2.Controls[0] as SplitContainer;
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

        try
        {
            // Progresos para actualizar cada panel
            var progressBurbuja = new Progress<(int[] estado, int[] resaltos)>(p =>
                                                                               {
                                                                                   datosBurbuja =
                                                                                       (int[])p.estado.Clone();
                                                                                   resaltosBurbuja = p.resaltos;
                                                                                   panelBurbuja.Invalidate();
                                                                               });
            var progressInsercion = new Progress<(int[] estado, int[] resaltos)>(p =>
                                                                                 {
                                                                                     datosInsercion =
                                                                                         (int[])p.estado.Clone();
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
            var tareaBurbuja = EjecutarAlgoritmoVisual(ALG_BURBUJA, OrdenarPorBurbujaVisual, datosBurbuja!,
                                                       progressBurbuja, tokenVisualizacion);
            var tareaInsercion = EjecutarAlgoritmoVisual(ALG_INSERCION, OrdenarPorInsercionVisual, datosInsercion!,
                                                         progressInsercion, tokenVisualizacion);
            var tareaQuickSort = EjecutarAlgoritmoVisual(ALG_QUICKSORT, OrdenarPorQuickSortVisual, datosQuick!,
                                                         progressQuick, tokenVisualizacion);

            await Task.WhenAll(tareaBurbuja, tareaInsercion, tareaQuickSort);

            if (!tokenVisualizacion.IsCancellationRequested)
            {
                // Medimos nuevamente SIN animación (tiempos reales del algoritmo) sobre el arreglo original
                var resultadosReales =
                    new System.Collections.Generic.List<(string nombre, TimeSpan tiempo, long ticks)> {
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
            ActualizarEstadoControles(true);
            botonCancelar.Enabled = false;
            ctsVisualizacion = null;
        }
        // ----- FIN: BotonVisualizarCarrera_Click -----
    }

    private void BotonCancelar_Click(object? remitente, EventArgs argumentos)
    {
        // ----- INICIO: BotonCancelar_Click -----
        ctsVisualizacion?.Cancel();
        etiquetaEstado.Text = "Cancelando...";

        // ----- FIN: BotonCancelar_Click -----
    }

    private void BotonAyuda_Click(object? remitente, EventArgs argumentos)
    {
        // ----- INICIO: BotonAyuda_Click -----
        using var ventanaAyuda = new HelpForm();
        ventanaAyuda.ShowDialog(this);

        // ----- FIN: BotonAyuda_Click -----
    }
    #endregion

    #region Comparación y renderizado de resultados
    /// <summary>
    /// Ejecuta los tres algoritmos en modo rápido (sin animación) y muestra la tabla de tiempos.
    /// </summary>
    private void EjecutarModoRapido()
    {
        // ----- INICIO: EjecutarModoRapido -----
        if (datosOriginales == null || datosOriginales.Length == 0)
        {
            return;
        }

        var resultados = new System.Collections.Generic.List<(string nombre, TimeSpan tiempo, long ticks)>();
        Cursor = Cursors.WaitCursor;
        ActualizarEstadoControles(false);
        try
        {
            resultados.Add(MedirAlgoritmo(ALG_BURBUJA, OrdenarPorBurbuja));
            resultados.Add(MedirAlgoritmo(ALG_INSERCION, OrdenarPorInsercion));
            resultados.Add(MedirAlgoritmo(ALG_QUICKSORT, OrdenarPorQuickSort));
        }
        finally
        {
            Cursor = Cursors.Default;
            ActualizarEstadoControles(true);
        }

        RenderizarResultados(resultados, true);

        // ----- FIN: EjecutarModoRapido -----
    }

    /// <summary>
    /// Muestra resultados en el ListView en un orden fijo y resalta el ganador.
    /// Asegura que Burbuja, Inserción y QuickSort estén presentes; si falta alguno se mide al vuelo.
    /// </summary>
    private void RenderizarResultados(
        System.Collections.Generic.List<(string nombre, TimeSpan tiempo, long ticks)> resultados, bool mostrarMensaje)
    {
        // ----- INICIO: RenderizarResultados -----
        void ActualizarListaOriginal(string nombreAnterior,
                                     (string nombre, TimeSpan tiempo, long ticks) actualizado)
        {
            if (string.IsNullOrWhiteSpace(nombreAnterior))
            {
                return;
            }

            string nombreNormalizado = nombreAnterior.Trim();
            for (int indice = 0; indice < resultados.Count; indice++)
            {
                var actual = resultados[indice];
                string etiquetaActual = (actual.nombre ?? string.Empty).Trim();
                if (string.Equals(etiquetaActual, nombreNormalizado, StringComparison.OrdinalIgnoreCase))
                {
                    resultados[indice] = actualizado;
                    break;
                }
            }
        }

        var algoritmosEsperados = new (string nombre, Action<int[]> algoritmo)[]
        {
            (ALG_QUICKSORT, OrdenarPorQuickSort),
            (ALG_INSERCION, OrdenarPorInsercion),
            (ALG_BURBUJA, OrdenarPorBurbuja)
        };

        var resultadosNormalizados = new Dictionary<string, (string nombre, TimeSpan tiempo, long ticks)>(
            StringComparer.OrdinalIgnoreCase);
        foreach (var resultado in resultados.Where(r => !string.IsNullOrWhiteSpace(r.nombre)))
        {
            resultadosNormalizados[resultado.nombre.Trim()] = resultado;
        }

        var listaGarantizada = new List<(string nombre, TimeSpan tiempo, long ticks)>(algoritmosEsperados.Length);

        foreach (var (nombreAlgoritmo, algoritmo) in algoritmosEsperados)
        {
            string clave = nombreAlgoritmo.Trim();
            if (!resultadosNormalizados.TryGetValue(clave, out var existente))
            {
                existente = MedirAlgoritmo(nombreAlgoritmo, algoritmo);
                resultados.Add(existente);
                resultadosNormalizados[clave] = existente;
            }
            else if (!string.Equals(existente.nombre, nombreAlgoritmo, StringComparison.Ordinal))
            {
                var corregido = (nombreAlgoritmo, existente.tiempo, existente.ticks);
                resultadosNormalizados[clave] = corregido;
                ActualizarListaOriginal(existente.nombre, corregido);
                existente = corregido;
            }

            listaGarantizada.Add(existente);
        }

        var resultadosGarantizados = listaGarantizada.ToArray();

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
            foreach (var resultado in resultadosGarantizados)
            {
                AgregarFilaResultado(resultado);
            }
        }
        finally
        {
            listaResultados.EndUpdate();
            listaResultados.Refresh();
        }

        var resultadoGanador = resultadosGarantizados.OrderBy(r => r.tiempo).First();

        foreach (ListViewItem fila in listaResultados.Items)
        {
            if (string.Equals(fila.Text, resultadoGanador.nombre, StringComparison.Ordinal))
            {
                fila.BackColor = Color.FromArgb(230, 255, 230);
                fila.EnsureVisible();
            }
        }

        etiquetaEstado.Text =
            $"Ganador: {resultadoGanador.nombre} con {resultadoGanador.tiempo.TotalMilliseconds:N3} ms";

        if (mostrarMensaje)
        {
            MessageBox.Show($"Algoritmo ganador: {resultadoGanador.nombre}\n" +
                                $"Tiempo: {resultadoGanador.tiempo.TotalMilliseconds:N3} ms" +
                                (resultadoGanador.tiempo.TotalMilliseconds >= 1000
                                     ? $" ({resultadoGanador.tiempo.TotalSeconds:N3} s)"
                                     : string.Empty) +
                                "\n\nRevisa la tabla de resultados para ver el detalle de cada algoritmo.",
                            "Resultados de la Comparación", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ----- FIN: RenderizarResultados -----
    }

    private void AgregarFilaResultado((string nombre, TimeSpan tiempo, long ticks) resultado)
    {
        // ----- INICIO: AgregarFilaResultado -----
        if (string.IsNullOrWhiteSpace(resultado.nombre))
        {
            return;
        }

        var fila = new ListViewItem(resultado.nombre);
        var cultura = CultureInfo.CurrentCulture;

        string textoMilisegundos = resultado.tiempo.TotalMilliseconds >= 1
            ? resultado.tiempo.TotalMilliseconds.ToString("N3", cultura)
            : FormatearTiempoSubSegundo(resultado.tiempo.TotalMilliseconds, cultura);

        string textoSegundos = resultado.tiempo.TotalSeconds >= 1
            ? resultado.tiempo.TotalSeconds.ToString("N3", cultura)
            : FormatearTiempoSubSegundo(resultado.tiempo.TotalSeconds, cultura);

        fila.SubItems.Add(textoMilisegundos);
        fila.SubItems.Add(textoSegundos);
        fila.SubItems.Add(resultado.ticks.ToString("N0", cultura));
        listaResultados.Items.Add(fila);

        // ----- FIN: AgregarFilaResultado -----
    }

    private static string FormatearTiempoSubSegundo(double valor, CultureInfo cultura)
    {
        if (valor <= 0)
        {
            return "0";
        }

        const double umbral = 0.000001; // 1 micro unidad en la escala actual
        if (valor < umbral)
        {
            return "<" + umbral.ToString("0.######", cultura);
        }

        return valor.ToString("0.######", cultura);
    }

    /// <summary>
    /// Mide el tiempo de un algoritmo sobre una copia de los datos originales.
    /// </summary>
    private (string nombre, TimeSpan tiempo, long ticks) MedirAlgoritmo(string nombre, Action<int[]> algoritmo)
    {
        // ----- INICIO: MedirAlgoritmo -----
        int[] copiaDatos = (int[])datosOriginales.Clone();
        var cronometro = Stopwatch.StartNew();
        algoritmo(copiaDatos);
        cronometro.Stop();
        return (nombre, cronometro.Elapsed, cronometro.ElapsedTicks);
        // ----- FIN: MedirAlgoritmo -----
    }
    #endregion

    #region Algoritmos no visuales(modo rápido)
    private void OrdenarPorBurbuja(int[] arreglo)
    {
        // ----- INICIO: OrdenarPorBurbuja -----
        int elementosPendientes = arreglo.Length;
        bool huboIntercambio;
        do
        {
            huboIntercambio = false;
            for (int indice = 0; indice < elementosPendientes - 1; indice++)
            {
                if (arreglo[indice] > arreglo[indice + 1])
                {
                    (arreglo[indice], arreglo[indice + 1]) = (arreglo[indice + 1], arreglo[indice]);
                    huboIntercambio = true;
                }
            }
            elementosPendientes--;
        } while (huboIntercambio);

        // ----- FIN: OrdenarPorBurbuja -----
    }

    private void OrdenarPorInsercion(int[] arreglo)
    {
        // ----- INICIO: OrdenarPorInsercion -----
        for (int indice = 1; indice < arreglo.Length; indice++)
        {
            int valorActual = arreglo[indice];
            int posicion = indice - 1;
            while (posicion >= 0 && arreglo[posicion] > valorActual)
            {
                arreglo[posicion + 1] = arreglo[posicion];
                posicion--;
            }
            arreglo[posicion + 1] = valorActual;
        }

        // ----- FIN: OrdenarPorInsercion -----
    }

    private void OrdenarPorQuickSort(int[] arreglo)
    {
        // ----- INICIO: OrdenarPorQuickSort -----
        QuickSortRecursivoBasico(arreglo, 0, arreglo.Length - 1);

        // ----- FIN: OrdenarPorQuickSort -----
    }

    private void QuickSortRecursivoBasico(int[] arreglo, int izquierda, int derecha)
    {
        // ----- INICIO: QuickSortRecursivoBasico -----
        if (izquierda >= derecha)
        {
            return;
        }

        int indiceIzquierdo = izquierda;
        int indiceDerecho = derecha;
        int pivote = arreglo[(izquierda + derecha) / 2];

        while (indiceIzquierdo <= indiceDerecho)
        {
            while (arreglo[indiceIzquierdo] < pivote)
            {
                indiceIzquierdo++;
            }

            while (arreglo[indiceDerecho] > pivote)
            {
                indiceDerecho--;
            }

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

        // ----- FIN: QuickSortRecursivoBasico -----
    }
    #endregion

    #region Algoritmos visuales(async con animación)
    /// <summary>
    /// Versión visual del ordenamiento Burbuja con reportes periódicos de estado.
    /// </summary>
    private async Task OrdenarPorBurbujaVisual(int[] arreglo, IProgress<(int[] estado, int[] resaltos)> progreso,
                                               CancellationToken token)
    {
        // ----- INICIO: OrdenarPorBurbujaVisual -----
        int elementosPendientes = arreglo.Length;
        bool huboIntercambio;
        int retrasoAnimacion = Math.Max(0, 100 - trackBarVelocidad.Value);
        int pasosPorReporte = Math.Max(1, trackBarVelocidad.Value / 10);
        int contadorOperaciones = 0;

        do
        {
            huboIntercambio = false;
            for (int indice = 0; indice < elementosPendientes - 1; indice++)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                if (arreglo[indice] > arreglo[indice + 1])
                {
                    (arreglo[indice], arreglo[indice + 1]) = (arreglo[indice + 1], arreglo[indice]);
                    huboIntercambio = true;
                    contadorOperaciones++;
                    if (contadorOperaciones % pasosPorReporte == 0)
                    {
                        progreso.Report((arreglo, new[] { indice, indice + 1 }));
                        if (retrasoAnimacion > 0)
                        {
                            await Task.Delay(retrasoAnimacion, token);
                        }
                    }
                }
            }

            elementosPendientes--;
        } while (huboIntercambio);

        progreso.Report((arreglo, Array.Empty<int>()));

        // ----- FIN: OrdenarPorBurbujaVisual -----
    }

    /// <summary>
    /// Versión visual del ordenamiento por Inserción.
    /// </summary>
    private async Task OrdenarPorInsercionVisual(int[] arreglo, IProgress<(int[] estado, int[] resaltos)> progreso,
                                                 CancellationToken token)
    {
        // ----- INICIO: OrdenarPorInsercionVisual -----
        int retrasoAnimacion = Math.Max(0, 100 - trackBarVelocidad.Value);
        int pasosPorReporte = Math.Max(1, trackBarVelocidad.Value / 10);
        int contadorOperaciones = 0;

        for (int indice = 1; indice < arreglo.Length; indice++)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            int valorActual = arreglo[indice];
            int posicion = indice - 1;
            while (posicion >= 0 && arreglo[posicion] > valorActual)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }

                arreglo[posicion + 1] = arreglo[posicion];
                posicion--;
                contadorOperaciones++;
                if (contadorOperaciones % pasosPorReporte == 0)
                {
                    progreso.Report((arreglo, new[] { posicion + 1, indice }));
                    if (retrasoAnimacion > 0)
                    {
                        await Task.Delay(retrasoAnimacion, token);
                    }
                }
            }

            arreglo[posicion + 1] = valorActual;
            contadorOperaciones++;
            if (contadorOperaciones % pasosPorReporte == 0)
            {
                progreso.Report((arreglo, new[] { posicion + 1 }));
                if (retrasoAnimacion > 0)
                {
                    await Task.Delay(retrasoAnimacion, token);
                }
            }
        }

        progreso.Report((arreglo, Array.Empty<int>()));

        // ----- FIN: OrdenarPorInsercionVisual -----
    }

    /// <summary>
    /// Versión visual de QuickSort (recursivo) con reportes periódicos.
    /// </summary>
    private async Task OrdenarPorQuickSortVisual(int[] arreglo, IProgress<(int[] estado, int[] resaltos)> progreso,
                                                 CancellationToken token)
    {
        // ----- INICIO: OrdenarPorQuickSortVisual -----
        int retrasoAnimacion = Math.Max(0, 100 - trackBarVelocidad.Value);
        int pasosPorReporte = Math.Max(1, trackBarVelocidad.Value / 10);
        quickVisualCounter = 0;

        await QuickSortRecursivoVisualAsync(arreglo, 0, arreglo.Length - 1, progreso, token, retrasoAnimacion,
                                            pasosPorReporte);
        progreso.Report((arreglo, Array.Empty<int>()));

        // ----- FIN: OrdenarPorQuickSortVisual -----
    }

    private async Task QuickSortRecursivoVisualAsync(int[] arreglo, int indiceIzquierdo, int indiceDerecho,
                                                     IProgress<(int[] estado, int[] resaltos)> progreso,
                                                     CancellationToken token, int retrasoAnimacion, int pasosPorReporte)
    {
        // ----- INICIO: QuickSortRecursivoVisualAsync -----
        if (indiceIzquierdo >= indiceDerecho)
        {
            return;
        }

        int i = indiceIzquierdo;
        int j = indiceDerecho;
        int pivote = arreglo[(indiceIzquierdo + indiceDerecho) / 2];

        while (i <= j)
        {
            if (token.IsCancellationRequested)
            {
                return;
            }

            while (arreglo[i] < pivote)
            {
                i++;
            }

            while (arreglo[j] > pivote)
            {
                j--;
            }

            if (i <= j)
            {
                (arreglo[i], arreglo[j]) = (arreglo[j], arreglo[i]);
                quickVisualCounter++;
                if (quickVisualCounter % pasosPorReporte == 0)
                {
                    progreso.Report((arreglo, new[] { i, j }));
                    if (retrasoAnimacion > 0)
                    {
                        await Task.Delay(retrasoAnimacion, token);
                    }
                }

                i++;
                j--;
            }
        }

        if (indiceIzquierdo < j)
        {
            await QuickSortRecursivoVisualAsync(arreglo, indiceIzquierdo, j, progreso, token, retrasoAnimacion,
                                                pasosPorReporte);
        }

        if (i < indiceDerecho)
        {
            await QuickSortRecursivoVisualAsync(arreglo, i, indiceDerecho, progreso, token, retrasoAnimacion,
                                                pasosPorReporte);
        }

        // ----- FIN: QuickSortRecursivoVisualAsync -----
    }
    #endregion

    #region Ejecución visual - medición con cronómetro
    /// <summary>
    /// Ejecuta un algoritmo visual, midiendo su duración (incluye delays de animación).
    /// El tiempo medido AQUÍ no se usa para la tabla; solo para sincronizar fin.
    /// </summary>
    private async Task<(string nombre, TimeSpan tiempo, long ticks)> EjecutarAlgoritmoVisual(
        string nombre, Func<int[], IProgress<(int[] estado, int[] resaltos)>, CancellationToken, Task> algoritmo,
        int[] datos, IProgress<(int[] estado, int[] resaltos)> progreso, CancellationToken token)
    {
        // ----- INICIO: EjecutarAlgoritmoVisual -----
        var cronometro = Stopwatch.StartNew();
        await algoritmo(datos, progreso, token);
        cronometro.Stop();
        return (nombre, cronometro.Elapsed, cronometro.ElapsedTicks);
        // ----- FIN: EjecutarAlgoritmoVisual -----
    }
    #endregion
}
}
