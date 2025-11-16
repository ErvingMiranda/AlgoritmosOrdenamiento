namespace BusquedaYOrdenamientoDemo
{
    partial class Form1
    {
        /// <summary>
        /// Contenedor de componentes generado por el diseñador.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        // Controles principales de la interfaz
        // NumericUpDown para elegir tamaño de la muestra a generar
        private System.Windows.Forms.NumericUpDown numericCantidad;
        // Botón que genera números aleatorios según la cantidad especificada
        private System.Windows.Forms.Button botonGenerar;
        // Botón que lanza la comparación (visual o rápida) entre algoritmos
        private System.Windows.Forms.Button botonCompararAlgoritmos;
        // Botón que abre la ventana de ayuda
        private System.Windows.Forms.Button botonAyuda;
        // Botón que cancela la visualización en curso (si la hay)
        private System.Windows.Forms.Button botonCancelar;
        // Tabla de resultados (ListView en modo Details) para mostrar tiempos de cada algoritmo
        private System.Windows.Forms.ListView listaResultados;
        // Muestra hasta ~50 datos generados, solo a modo ilustrativo
        private System.Windows.Forms.ListBox listaMuestraDatos;
        // Barra de estado simple para mensajes de progreso/estado
        private System.Windows.Forms.Label etiquetaEstado;
        // SplitContainer principal: Panel1 configuración | Panel2 visualización+resultados
        private System.Windows.Forms.SplitContainer splitPrincipal;
        // Contenedor superior con3 columnas para pintar los3 algoritmos a la vez
        private System.Windows.Forms.TableLayoutPanel panelVisualizaciones;
        // Contenedores y paneles de dibujo específicos por algoritmo
        private System.Windows.Forms.Panel panelBurbujaContenedor;
        private System.Windows.Forms.Panel panelInsercionContenedor;
        private System.Windows.Forms.Panel panelQuickContenedor;
        private System.Windows.Forms.Panel panelBurbuja;
        private System.Windows.Forms.Panel panelInsercion;
        private System.Windows.Forms.Panel panelQuick;
        // Etiquetas de título por algoritmo
        private System.Windows.Forms.Label etiquetaBurbuja;
        private System.Windows.Forms.Label etiquetaInsercion;
        private System.Windows.Forms.Label etiquetaQuick;
        // Control para ajustar la velocidad de animación (Throttle)
        private System.Windows.Forms.TrackBar trackBarVelocidad;

        /// <summary>
        /// Libera recursos administrados/no administrados.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Inicializa y configura todos los controles de la ventana.
        /// Este código está pensado para ser mantenido por el diseñador.
        /// Agregar comentarios es seguro, pero evite cambios drásticos manuales.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            // Ajustes de ventana
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Text = "Comparación de Algoritmos de Ordenamiento";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.BackColor = System.Drawing.Color.FromArgb(237,241,250);
            this.ClientSize = new System.Drawing.Size(1000,600);
            this.MinimumSize = new System.Drawing.Size(900,540);
            // Reajusta columnas de resultados al redimensionar la ventana
            this.Resize += (s, e) => AjustarColumnasResultados(listaResultados, e);

            // Split principal (izquierda: configuración, derecha: visualización+resultados)
            splitPrincipal = new System.Windows.Forms.SplitContainer
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                IsSplitterFixed = false,
                BackColor = System.Drawing.Color.FromArgb(222,229,245)
            };
            this.Controls.Add(splitPrincipal);
            // Reposiciona el splitter cuando cambia el tamaño total disponible
            splitPrincipal.SizeChanged += (s, e) => AjustarSplitterInicial();

            // Panel izquierdo: configuración con scroll
            var panelConfiguracion = new System.Windows.Forms.FlowLayoutPanel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                FlowDirection = System.Windows.Forms.FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                Padding = new System.Windows.Forms.Padding(16,14,12,12),
                BackColor = System.Drawing.Color.White
            };
            splitPrincipal.Panel1.Controls.Add(panelConfiguracion);

            var etiquetaTitulo = new System.Windows.Forms.Label
            {
                Text = "Configuración",
                Font = new System.Drawing.Font("Segoe UI",16, System.Drawing.FontStyle.Bold),
                AutoSize = true,
                Margin = new System.Windows.Forms.Padding(3,0,3,10)
            };
            panelConfiguracion.Controls.Add(etiquetaTitulo);

            var etiquetaCantidad = new System.Windows.Forms.Label
            {
                Text = "Cantidad de datos (para visualizar fluido usa <= ~10k):",
                AutoSize = true,
                Margin = new System.Windows.Forms.Padding(3,4,3,2)
            };
            panelConfiguracion.Controls.Add(etiquetaCantidad);

            // Control numérico para el tamaño del arreglo a generar
            numericCantidad = new System.Windows.Forms.NumericUpDown
            {
                Minimum =100,
                Maximum =1000000,
                Value =5000,
                Increment =100,
                Width =220
            };
            panelConfiguracion.Controls.Add(numericCantidad);

            // Botón: generar datos aleatorios
            botonGenerar = new System.Windows.Forms.Button
            {
                Text = "Generar datos",
                Width =220,
                Height =40,
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(0,122,204),
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Segoe UI",10, System.Drawing.FontStyle.Bold)
            };
            botonGenerar.FlatAppearance.BorderSize =0;
            botonGenerar.Click += BotonGenerar_Click;
            panelConfiguracion.Controls.Add(botonGenerar);

            // Botón: comparar algoritmos (se habilita tras generar datos)
            botonCompararAlgoritmos = new System.Windows.Forms.Button
            {
                Text = "Comparar Algoritmos",
                Width =220,
                Height =40,
                Enabled = false,
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(0,153,102),
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Segoe UI",10, System.Drawing.FontStyle.Bold)
            };
            botonCompararAlgoritmos.FlatAppearance.BorderSize =0;
            botonCompararAlgoritmos.Click += BotonCompararAlgoritmos_Click;
            panelConfiguracion.Controls.Add(botonCompararAlgoritmos);

            var etiquetaVelocidadTitulo = new System.Windows.Forms.Label
            {
                Text = "Velocidad de animación:",
                AutoSize = true,
                Margin = new System.Windows.Forms.Padding(3,14,3,2)
            };
            panelConfiguracion.Controls.Add(etiquetaVelocidadTitulo);

            // TrackBar: controla la cadencia de renders/operaciones en animación
            trackBarVelocidad = new System.Windows.Forms.TrackBar
            {
                Minimum =1,
                Maximum =100,
                Value =80,
                Width =220,
                TickFrequency =10
            };
            panelConfiguracion.Controls.Add(trackBarVelocidad);

            var etiquetaMuestra = new System.Windows.Forms.Label
            {
                Text = "Muestra de datos generados:",
                AutoSize = true,
                Margin = new System.Windows.Forms.Padding(3,14,3,2)
            };
            panelConfiguracion.Controls.Add(etiquetaMuestra);

            // ListBox: muestra una porción de los datos generados para ver su distribución
            listaMuestraDatos = new System.Windows.Forms.ListBox
            {
                Width =240,
                Height =210,
                Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right
            };
            panelConfiguracion.Controls.Add(listaMuestraDatos);

            // Panel derecho: visualización (arriba) y resultados (abajo)
            var panelDerecho = new System.Windows.Forms.SplitContainer
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                Orientation = System.Windows.Forms.Orientation.Horizontal,
                SplitterDistance = (int)(this.Height *0.55)
            };
            splitPrincipal.Panel2.Controls.Add(panelDerecho);

            // Panel inferior: resultados
            var panelResultados = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                Padding = new System.Windows.Forms.Padding(12),
                BackColor = System.Drawing.Color.FromArgb(247,249,254)
            };
            panelDerecho.Panel2.Controls.Add(panelResultados);

            // Panel cabecera para evitar superposición con la tabla de resultados
            var panelCabeceraResultados = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Top,
                Height =60,
                BackColor = System.Drawing.Color.FromArgb(247,249,254)
            };
            panelResultados.Controls.Add(panelCabeceraResultados);

            var etiquetaResultados = new System.Windows.Forms.Label
            {
                Text = "Resultados de la Comparación",
                Font = new System.Drawing.Font("Segoe UI",15, System.Drawing.FontStyle.Bold),
                Dock = System.Windows.Forms.DockStyle.Top,
                Height =32,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft
            };
            panelCabeceraResultados.Controls.Add(etiquetaResultados);

            var etiquetaColumnas = new System.Windows.Forms.Label
            {
                Text = "Algoritmo | Tiempo (ms) | Tiempo (s) | Ticks",
                Dock = System.Windows.Forms.DockStyle.Top,
                Height =20,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                ForeColor = System.Drawing.Color.FromArgb(90,100,120)
            };
            panelCabeceraResultados.Controls.Add(etiquetaColumnas);

            // ListView en modo detalles con4 columnas para los datos de rendimiento
            listaResultados = new System.Windows.Forms.ListView
            {
                View = System.Windows.Forms.View.Details,
                FullRowSelect = true,
                GridLines = true,
                HideSelection = false,
                Font = new System.Drawing.Font("Segoe UI",10),
                BackColor = System.Drawing.Color.White,
                ForeColor = System.Drawing.Color.Black,
                Dock = System.Windows.Forms.DockStyle.Fill,
                UseCompatibleStateImageBehavior = false,
                OwnerDraw = false,
                HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable,
                ShowGroups = false,
                Margin = new System.Windows.Forms.Padding(0,4,0,0)
            };
            listaResultados.Columns.Add("Algoritmo",220);
            listaResultados.Columns.Add("Tiempo (ms)",170);
            listaResultados.Columns.Add("Tiempo (s)",130);
            listaResultados.Columns.Add("Tiempo (ticks)",190);
            panelResultados.Controls.Add(listaResultados);
            // Asegura que la lista ocupe el espacio restante bajo la cabecera
            panelResultados.Controls.SetChildIndex(listaResultados,0);

            // Barra inferior de estado
            etiquetaEstado = new System.Windows.Forms.Label
            {
                Text = "Genera los datos para comenzar.",
                AutoSize = false,
                Height =30,
                Dock = System.Windows.Forms.DockStyle.Bottom,
                TextAlign = System.Drawing.ContentAlignment.MiddleLeft,
                ForeColor = System.Drawing.Color.FromArgb(80,90,110),
                Padding = new System.Windows.Forms.Padding(4)
            };
            panelResultados.Controls.Add(etiquetaEstado);

            // Panel superior: visualización simultánea de3 algoritmos
            panelVisualizaciones = new System.Windows.Forms.TableLayoutPanel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                ColumnCount =3,
                RowCount =1,
                BackColor = System.Drawing.Color.Black,
                Visible = true,
                Padding = new System.Windows.Forms.Padding(8)
            };
            panelVisualizaciones.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent,33.33f));
            panelVisualizaciones.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent,33.33f));
            panelVisualizaciones.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent,33.33f));

            // Crea contenedores por algoritmo con título + panel de dibujo que usa el manejador PanelAlgoritmo_Pintar
            panelBurbujaContenedor = CrearContenedorAlgoritmo("Burbuja", out panelBurbuja, out etiquetaBurbuja);
            panelInsercionContenedor = CrearContenedorAlgoritmo("Inserción", out panelInsercion, out etiquetaInsercion);
            panelQuickContenedor = CrearContenedorAlgoritmo("QuickSort", out panelQuick, out etiquetaQuick);

            panelVisualizaciones.Controls.Add(panelBurbujaContenedor,0,0);
            panelVisualizaciones.Controls.Add(panelInsercionContenedor,1,0);
            panelVisualizaciones.Controls.Add(panelQuickContenedor,2,0);
            panelDerecho.Panel1.Controls.Add(panelVisualizaciones);

            // Botón cancelar animación (solo activo en modo visual)
            botonCancelar = new System.Windows.Forms.Button
            {
                Text = "Cancelar Visualización",
                Width =220,
                Height =36,
                Enabled = false,
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(200,40,40),
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Segoe UI",9, System.Drawing.FontStyle.Bold)
            };
            botonCancelar.FlatAppearance.BorderSize =0;
            botonCancelar.Click += BotonCancelar_Click;
            panelConfiguracion.Controls.Add(botonCancelar);

            // Botón ayuda
            botonAyuda = new System.Windows.Forms.Button
            {
                Text = "Ayuda",
                Width =220,
                Height =36,
                Enabled = true,
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(70,70,90),
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Segoe UI",9, System.Drawing.FontStyle.Bold)
            };
            botonAyuda.FlatAppearance.BorderSize =0;
            botonAyuda.Click += BotonAyuda_Click;
            panelConfiguracion.Controls.Add(botonAyuda);
        }

        #endregion

        /// <summary>
        /// Crea un contenedor visual con etiqueta de título y un panel de dibujo
        /// que delega su pintado a <see cref="PanelAlgoritmo_Pintar"/>.
        /// </summary>
        private System.Windows.Forms.Panel CrearContenedorAlgoritmo(string titulo, out System.Windows.Forms.Panel panelDibujo, out System.Windows.Forms.Label etiqueta)
        {
            var panelContenedor = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                BackColor = System.Drawing.Color.FromArgb(30,30,34),
                Margin = new System.Windows.Forms.Padding(6),
                Padding = new System.Windows.Forms.Padding(4)
            };

            etiqueta = new System.Windows.Forms.Label
            {
                Text = titulo,
                Dock = System.Windows.Forms.DockStyle.Top,
                Height =24,
                ForeColor = System.Drawing.Color.White,
                Font = new System.Drawing.Font("Segoe UI",11, System.Drawing.FontStyle.Bold),
                TextAlign = System.Drawing.ContentAlignment.MiddleCenter,
                BackColor = System.Drawing.Color.FromArgb(50,50,55)
            };
            panelContenedor.Controls.Add(etiqueta);

            // Panel donde se dibujan las barras del arreglo y las comparaciones/intercambios
            panelDibujo = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                BackColor = System.Drawing.Color.Black
            };
            // El evento Paint manda a dibujar según el arreglo de ese algoritmo
            panelDibujo.Paint += PanelAlgoritmo_Pintar;
            panelContenedor.Controls.Add(panelDibujo);

            // Deja la etiqueta por encima del panel de dibujo
            panelContenedor.Controls.SetChildIndex(etiqueta,1);

            return panelContenedor;
        }
    }
}
