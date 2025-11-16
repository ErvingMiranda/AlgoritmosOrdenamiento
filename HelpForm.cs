using System; // Tipos base, IDisposable, etc.
using System.Drawing; // Tipos gráficos para colores, fuentes y tamaños
using System.Windows.Forms; // Controles WinForms para construir la UI de ayuda

namespace BusquedaYOrdenamientoDemo
{
 /// <summary>
 /// Ventana de ayuda con dos secciones: Uso/Asignación y Glosario.
 /// Explica cuándo usar la visualización o el modo rápido y describe cada algoritmo.
 /// </summary>
 public class HelpForm : Form
 {
 private Panel panelAsignacion;
 private Panel panelGlosario;

 public HelpForm()
 {
 // Propiedades básicas de la ventana
 Text = "Ayuda - Algoritmos de Ordenamiento";
 StartPosition = FormStartPosition.CenterParent;
 Size = new Size(900,600); // Tamaño inicial amplio pero no a pantalla completa
 MinimumSize = new Size(700,480);
 MinimizeBox = true;
 MaximizeBox = true; // Permite maximizar si el usuario desea
 BackColor = Color.FromArgb(248,250,253);
 Font = new Font("Segoe UI",10);

 // Layout raíz para evitar superposición: Header (auto) | Content (100%) | Footer (auto)
 var root = new TableLayoutPanel
 {
 Dock = DockStyle.Fill,
 ColumnCount =1,
 RowCount =3,
 Padding = new Padding(0),
 BackColor = BackColor
 };
 root.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Header
 root.RowStyles.Add(new RowStyle(SizeType.Percent,100)); // Content
 root.RowStyles.Add(new RowStyle(SizeType.AutoSize)); // Footer
 Controls.Add(root);

 // Header con botones de pestañas
 var header = new Panel
 {
 Dock = DockStyle.Fill,
 Height =52,
 Padding = new Padding(10,8,10,6),
 BackColor = Color.FromArgb(235,238,245)
 };
 root.Controls.Add(header,0,0);

 var panelBotones = new FlowLayoutPanel
 {
 Dock = DockStyle.Fill,
 Height =40,
 FlowDirection = FlowDirection.LeftToRight,
 WrapContents = false,
 AutoSize = true,
 AutoSizeMode = AutoSizeMode.GrowAndShrink,
 BackColor = header.BackColor
 };
 header.Controls.Add(panelBotones);

 var btnAsignacion = new Button { Text = "Uso y Asignación", AutoSize = true, FlatStyle = FlatStyle.Flat };
 var btnGlosario = new Button { Text = "Glosario", AutoSize = true, FlatStyle = FlatStyle.Flat };
 btnAsignacion.FlatAppearance.BorderSize =0;
 btnGlosario.FlatAppearance.BorderSize =0;
 btnAsignacion.BackColor = Color.FromArgb(0,122,204);
 btnAsignacion.ForeColor = Color.White;
 btnGlosario.BackColor = Color.FromArgb(70,70,90);
 btnGlosario.ForeColor = Color.White;
 var toolTip = new ToolTip();
 toolTip.SetToolTip(btnAsignacion, "Explica cómo y cuándo usar cada modo de comparación.");
 toolTip.SetToolTip(btnGlosario, "Describe cada algoritmo y sus casos de uso recomendados.");
 panelBotones.Controls.AddRange(new Control[] { btnAsignacion, btnGlosario });

 // Contenido: panel contenedor que cambia entre vistas
 var content = new Panel { Dock = DockStyle.Fill, Padding = new Padding(14), BackColor = Color.White };
 root.Controls.Add(content,0,1);

 // Vista: Uso/Asignación y recomendaciones
 panelAsignacion = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8), BackColor = Color.White };
 var lblUso = new Label
 {
 Text = "Uso de los Modos de Comparación",
 Dock = DockStyle.Top,
 Height =28,
 Font = new Font("Segoe UI",12, FontStyle.Bold)
 };
 var txtUso = new RichTextBox
 {
 Dock = DockStyle.Fill,
 ReadOnly = true,
 BorderStyle = BorderStyle.None,
 BackColor = Color.White,
 DetectUrls = false,
 Font = new Font("Segoe UI",10),
 ScrollBars = RichTextBoxScrollBars.Vertical,
 Text =
 "Al pulsar 'Comparar Algoritmos', la aplicación te preguntará cómo proceder:" +
 "\r\n\r\n• Visualizar Carrera (Sí): Inicia una animación en tiempo real donde puedes ver cómo cada algoritmo ordena el mismo conjunto de datos en paralelo. " +
 "Es ideal para entender visualmente las diferencias en sus estrategias. Esta opción solo está disponible para conjuntos de hasta500 elementos para garantizar una animación fluida." +
 "\r\n\r\n• Resultados Rápidos (No): Ejecuta los tres algoritmos sin animación y muestra instantáneamente una tabla con los tiempos de ejecución. Es la única opción para conjuntos grandes (>500 elementos) y es útil cuando solo te interesa el rendimiento puro."
 };
 panelAsignacion.Controls.Add(txtUso);
 panelAsignacion.Controls.Add(lblUso);

 // Vista: Glosario con definiciones y complejidad
 panelGlosario = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8), Visible = false, BackColor = Color.White };
 var lblGlosario = new Label
 {
 Text = "Glosario y Recomendaciones",
 Dock = DockStyle.Top,
 Height =28,
 Font = new Font("Segoe UI",12, FontStyle.Bold)
 };
 var txtGlosario = new RichTextBox
 {
 Dock = DockStyle.Fill,
 ReadOnly = true,
 BorderStyle = BorderStyle.None,
 BackColor = Color.White,
 DetectUrls = false,
 Font = new Font("Segoe UI",10),
 ScrollBars = RichTextBoxScrollBars.Vertical,
 Text =
 "• Burbuja (O(n²)): El más simple. Compara pares adyacentes e intercambia si están en orden incorrecto.\r\n" +
 " Recomendado: Solo para fines educativos o con muy pocos datos.\r\n\r\n" +
 "• Inserción (O(n²)): Construye el arreglo ordenado insertando cada elemento en su posición correcta.\r\n" +
 " Recomendado: Cuando los datos ya están 'casi' ordenados o el conjunto es pequeño.\r\n\r\n" +
 "• QuickSort (O(n log n) promedio): Divide y vencerás con pivote. Muy rápido en promedio.\r\n" +
 " Recomendado: Para la mayoría de los casos con conjuntos de datos medianos a grandes."
 };
 panelGlosario.Controls.Add(txtGlosario);
 panelGlosario.Controls.Add(lblGlosario);

 // Agregar la primera vista por defecto
 content.Controls.Add(panelAsignacion);
 content.Controls.Add(panelGlosario);

 // Footer con botón cerrar sin superposición
 var footer = new Panel
 {
 Dock = DockStyle.Fill,
 Height =50,
 Padding = new Padding(0),
 BackColor = Color.FromArgb(200,40,40)
 };
 var btnCerrar = new Button
 {
 Text = "Cerrar",
 Dock = DockStyle.Fill,
 Height =46,
 DialogResult = DialogResult.OK,
 FlatStyle = FlatStyle.Flat,
 BackColor = Color.FromArgb(200,40,40),
 ForeColor = Color.White
 };
 btnCerrar.FlatAppearance.BorderSize =0;
 btnCerrar.Click += (s, e) => Close();
 footer.Controls.Add(btnCerrar);
 root.Controls.Add(footer,0,2);

 // Cambiar vistas
 btnAsignacion.Click += (s, e) => { panelAsignacion.Visible = true; panelGlosario.Visible = false; };
 btnGlosario.Click += (s, e) => { panelAsignacion.Visible = false; panelGlosario.Visible = true; };
 }
 }
}
