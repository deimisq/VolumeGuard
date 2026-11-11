using System.Text.Json;
using System.Security.Principal;

namespace VolumeGuard.Admin;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
#if STORE
        using var singleInstance = new Mutex(true, "Local\\VolumeGuardStore", out bool firstInstance);
        if (!firstInstance)
        {
            MessageBox.Show(L.T("VolumeGuard ya está abierto. Usa el icono junto al reloj.", "VolumeGuard is already running. Use the notification area icon."), "VolumeGuard");
            return;
        }
        var folder = VolumeGuard.Service.VolumeEngine.DataFolder;
        Directory.CreateDirectory(folder);
        var settings = Path.Combine(folder, "settings.json");
        if (!File.Exists(settings)) File.WriteAllText(settings, "{\"MaxVolumePercent\":40}");
        using var engine = new VolumeGuard.Service.VolumeEngine();
        engine.Start();
#else
        if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
        {
            MessageBox.Show(L.T("Se requieren permisos de administrador.", "Administrator privileges are required."), "VolumeGuard");
            return;
        }
#endif
        Application.Run(new MainForm());
    }
}

internal sealed class MainForm : Form
{
    private static readonly Color Ink = Color.FromArgb(25, 38, 61);
    private static readonly Color Muted = Color.FromArgb(99, 113, 134);
    private static readonly Color Accent = Color.FromArgb(37, 99, 235);
    private readonly TrackBar slider = new() { Minimum = 0, Maximum = 100, TickStyle = TickStyle.None, Dock = DockStyle.Fill, Height = 42, AccessibleName = L.T("Límite máximo de volumen", "Maximum volume limit") };
    private readonly Label value = new()
    {
        Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Cursor = Cursors.Hand,
        Font = new Font("Segoe UI", 40, FontStyle.Bold), ForeColor = Ink,
        BackColor = Color.White, Margin = Padding.Empty,
        AccessibleName = L.T("Porcentaje máximo de volumen. Pulsa para editar.", "Maximum volume percentage. Click to edit.")
    };
    private readonly TextBox valueEditor = new()
    {
        Dock = DockStyle.Fill, BorderStyle = BorderStyle.None, Visible = false, MaxLength = 4,
        Font = new Font("Segoe UI", 40, FontStyle.Bold), ForeColor = Ink, BackColor = Color.White,
        AccessibleName = L.T("Editar porcentaje de volumen, de 0 a 100", "Edit volume percentage, from 0 to 100")
    };
#if STORE
    private static readonly string Folder = VolumeGuard.Service.VolumeEngine.DataFolder;
#else
    private static readonly string Folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "VolumeGuard");
#endif
    private static readonly string FilePath = Path.Combine(Folder, "settings.json");
    private readonly Label status = new() { AutoSize = true, Dock = DockStyle.Fill, ForeColor = Muted, Margin = new Padding(0, 7, 0, 0) };
    private readonly Label statusTitle = new() { AutoSize = true, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 11, FontStyle.Bold), Margin = Padding.Empty };
    private readonly Label feedback = new() { AutoSize = true, Dock = DockStyle.Fill, ForeColor = Muted, Margin = new Padding(0, 10, 0, 18) };
    private readonly Button save = new() { Dock = DockStyle.Fill, Height = 46, FlatStyle = FlatStyle.Flat, BackColor = Accent, ForeColor = Color.White, Font = new Font("Segoe UI", 11, FontStyle.Bold), Cursor = Cursors.Hand, Margin = Padding.Empty, UseVisualStyleBackColor = false };
    private readonly TableLayoutPanel statusCard = Stack();
    private readonly System.Windows.Forms.Timer statusTimer = new() { Interval = 2000 };
    private readonly NotifyIcon trayIcon = new();
    private readonly ContextMenuStrip trayMenu = new();
    private readonly Icon brandIcon = new(typeof(MainForm).Assembly.GetManifestResourceStream("VolumeGuard.Admin.Assets.VolumeGuard.ico")!);
    private bool exitRequested;
    private bool trayHintShown;
    private bool editingValue;
    private int savedLimit;
    public MainForm(int? previewLimit = null)
    {
        Text = "VolumeGuard";
        Icon = brandIcon;
        Font = new Font("Segoe UI", 10);
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96, 96);
        ClientSize = new Size(500, 660);
        MinimumSize = new Size(480, 680);
        BackColor = Color.FromArgb(243, 246, 251);
        ForeColor = Ink;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        var page = Stack();
        page.Dock = DockStyle.Top;
        page.Padding = new Padding(28);
        var scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
        scroll.Controls.Add(page);
        Controls.Add(scroll);

        Add(page, TextLabel("VolumeGuard", 23, FontStyle.Bold, Ink));
        var subtitle = TextLabel(L.T("Control del volumen de este equipo", "Volume control for this computer"), 10, FontStyle.Regular, Muted);
        subtitle.Margin = new Padding(0, 3, 0, 24);
        Add(page, subtitle);

        var limitCard = Stack();
        limitCard.BackColor = Color.White;
        limitCard.Padding = new Padding(22, 18, 22, 20);
        limitCard.Margin = new Padding(0, 0, 0, 20);
        Add(limitCard, TextLabel(L.T("LÍMITE MÁXIMO", "MAXIMUM VOLUME"), 9, FontStyle.Bold, Muted));
        var valueRow = new Panel { Height = 90, Dock = DockStyle.Top, Margin = new Padding(0, 6, 0, 0) };
        valueRow.Controls.Add(value);
        valueRow.Controls.Add(valueEditor);
        Add(limitCard, valueRow);
        var selected = TextLabel(L.T("Pulsa el porcentaje para editarlo o mueve la barra", "Click the percentage to edit it or move the slider"), 10, FontStyle.Regular, Muted);
        selected.Margin = new Padding(0, 0, 0, 16);
        Add(limitCard, selected);
        Add(limitCard, slider);
        var scale = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2, Margin = Padding.Empty };
        scale.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        scale.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        scale.Controls.Add(TextLabel(L.T("0 % · Silencio", "0 % · Silent"), 9, FontStyle.Regular, Muted), 0, 0);
        var full = TextLabel("100 %", 9, FontStyle.Regular, Muted);
        full.TextAlign = ContentAlignment.MiddleRight;
        scale.Controls.Add(full, 1, 0);
        Add(limitCard, scale);
        Add(page, limitCard);

        save.FlatAppearance.BorderSize = 0;
        save.FlatAppearance.MouseOverBackColor = Color.FromArgb(29, 78, 216);
        save.FlatAppearance.MouseDownBackColor = Color.FromArgb(30, 64, 175);
        save.Click += (_, _) => Save();
        Add(page, save);
        Add(page, feedback);

        statusCard.Padding = new Padding(18);
        statusCard.Margin = new Padding(0, 0, 0, 18);
        Add(statusCard, statusTitle);
        Add(statusCard, status);
        Add(page, statusCard);
#if STORE
        Add(page, TextLabel(L.T("Edición Store · Activa mientras la app esté abierta.\nCerrar el panel desde su icono detiene el control.", "Store edition · Active while the app is running.\nExiting from the tray menu stops volume control."), 9, FontStyle.Regular, Muted));
#else
        Add(page, TextLabel(L.T("Administración protegida con permisos de Windows.", "Administration protected by Windows permissions."), 9, FontStyle.Regular, Muted));
#endif

        var support = new LinkLabel
        {
            Text = L.T("Apoyar al creador en GitHub Sponsors", "Support the creator on GitHub Sponsors"),
            AutoSize = true, Dock = DockStyle.Fill, Margin = new Padding(0, 14, 0, 0),
            LinkColor = Accent, ActiveLinkColor = Accent, VisitedLinkColor = Accent,
            LinkBehavior = LinkBehavior.HoverUnderline,
            AccessibleDescription = L.T("Aporte voluntario. Abre GitHub en el navegador.", "Voluntary contribution. Opens GitHub in your browser.")
        };
        support.LinkClicked += (_, _) =>
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo("https://github.com/sponsors/deimisq") { UseShellExecute = true });
            }
            catch (Exception)
            {
                MessageBox.Show(L.T("No se pudo abrir el navegador. Visita https://github.com/sponsors/deimisq", "Could not open your browser. Visit https://github.com/sponsors/deimisq"), "VolumeGuard");
            }
        };
        Add(page, support);

        savedLimit = previewLimit ?? ReadLimit();
        slider.Value = savedLimit;
        slider.ValueChanged += (_, _) => UpdateSelection();
        value.Click += (_, _) => BeginValueEdit();
        valueEditor.Leave += (_, _) => FinishValueEdit(true);
        valueEditor.KeyDown += (_, e) =>
        {
            if (e.KeyCode is Keys.Enter or Keys.Escape)
            {
                e.SuppressKeyPress = true;
                FinishValueEdit(e.KeyCode == Keys.Enter);
                slider.Focus();
            }
        };
        valueEditor.KeyPress += (_, e) => e.Handled = !char.IsControl(e.KeyChar) && !char.IsAsciiDigit(e.KeyChar);
        KeyPreview = true;
        KeyDown += (_, e) => { if (e.KeyCode == Keys.F2) { BeginValueEdit(); e.Handled = true; } };
        UpdateSelection();
        statusTimer.Tick += (_, _) => RefreshStatus();
        if (previewLimit.HasValue) SetStatus(true, L.T("Protección activa", "Protection active"), L.T("Aplicando 40 % en 3 dispositivos de salida.", "Enforcing 40 % on 3 output devices."));
        else
        {
            RefreshStatus();
            statusTimer.Start();
            InitializeTray();
        }
    }

    private void InitializeTray()
    {
        trayMenu.Items.Add(L.T("Abrir VolumeGuard", "Open VolumeGuard"), null, (_, _) => RestoreFromTray());
        trayMenu.Items.Add(new ToolStripSeparator());
        trayMenu.Items.Add(L.T("Cerrar panel", "Close panel"), null, (_, _) => { exitRequested = true; Close(); });
        trayIcon.Icon = Icon;
        trayIcon.Text = L.T("VolumeGuard · Control de volumen", "VolumeGuard · Volume control");
        trayIcon.ContextMenuStrip = trayMenu;
        trayIcon.DoubleClick += (_, _) => RestoreFromTray();
        trayIcon.Visible = true;
        Resize += (_, _) => { if (WindowState == FormWindowState.Minimized) HideToTray(); };
    }

    private static TableLayoutPanel Stack() => new()
    {
        AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink,
        ColumnCount = 1, Dock = DockStyle.Top, Margin = Padding.Empty,
        ColumnStyles = { new ColumnStyle(SizeType.Percent, 100) }
    };

    private static Label TextLabel(string text, float size, FontStyle style, Color color) => new()
    {
        Text = text, AutoSize = true, Dock = DockStyle.Fill, Margin = Padding.Empty,
        Font = new Font("Segoe UI", size, style), ForeColor = color
    };

    private static void Add(TableLayoutPanel parent, Control child)
    {
        int row = parent.RowCount++;
        parent.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        parent.Controls.Add(child, 0, row);
    }

    private void UpdateSelection()
    {
        value.Text = $"{slider.Value} %";
        bool changed = slider.Value != savedLimit;
        save.Text = changed ? L.T($"Guardar límite de {slider.Value} %", $"Save {slider.Value} % limit") : L.T("Guardar límite", "Save limit");
        feedback.ForeColor = Muted;
        feedback.Text = changed
            ? L.T("Cambios sin guardar. La protección mantiene el límite actual.", "Unsaved changes. Protection keeps the current limit.")
            : L.T("Selecciona un porcentaje y guarda para aplicarlo.", "Choose a percentage and save to apply it.");
    }

    private void BeginValueEdit()
    {
        if (editingValue) return;
        editingValue = true;
        valueEditor.Text = slider.Value.ToString();
        value.Visible = false;
        valueEditor.Visible = true;
        valueEditor.BringToFront();
        valueEditor.Focus();
        valueEditor.SelectionStart = valueEditor.Text.Length;
        valueEditor.SelectionLength = 0;
    }

    private void FinishValueEdit(bool accept)
    {
        if (!editingValue) return;
        editingValue = false;
        string text = valueEditor.Text;
        // Clear the editing flag before hiding: hiding can raise Leave synchronously.
        valueEditor.Visible = false;
        value.Visible = true;
        if (accept && int.TryParse(text, out int limit)) slider.Value = Math.Clamp(limit, 0, 100);
        value.Text = $"{slider.Value} %";
    }

    private void SetStatus(bool healthy, string title, string detail)
    {
        statusCard.BackColor = healthy ? Color.FromArgb(232, 246, 239) : Color.FromArgb(255, 242, 229);
        statusTitle.ForeColor = healthy ? Color.FromArgb(21, 112, 69) : Color.FromArgb(154, 77, 12);
        statusTitle.Text = (healthy ? "●  " : "!  ") + title;
        status.Text = detail;
    }

    private void HideToTray()
    {
        Hide();
        if (trayHintShown) return;
        trayHintShown = true;
        trayIcon.ShowBalloonTip(3000, "VolumeGuard", L.T("El panel está junto al reloj. Haz doble clic en el icono para abrirlo.", "The panel is in the notification area. Double-click its icon to open it."), ToolTipIcon.Info);
    }

    private void RestoreFromTray()
    {
        Show();
        WindowState = FormWindowState.Normal;
        Activate();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (e.CloseReason == CloseReason.UserClosing && !exitRequested && trayIcon.Visible)
        {
            e.Cancel = true;
            HideToTray();
        }
        base.OnFormClosing(e);
    }
    private static int ReadLimit()
    {
        try
        {
            using var doc = JsonDocument.Parse(System.IO.File.ReadAllText(FilePath));
            int limit = doc.RootElement.GetProperty("MaxVolumePercent").GetInt32();
            if (limit is < 0 or > 100) throw new InvalidDataException(L.T("Límite fuera de rango.", "The limit is out of range."));
            return limit;
        }
        catch (Exception ex)
        {
            MessageBox.Show(L.T("No se pudo cargar el límite configurado: ", "Could not load the configured limit: ") + ex.Message +
                L.T("\nSelecciona y guarda un valor válido. El servicio conservará su último límite válido, o 0 % si aún no leyó ninguno.",
                    "\nSelect and save a valid value. The service keeps its last valid limit, or 0 % until it reads one."),
                L.T("Configuración no disponible", "Configuration unavailable"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return 0;
        }
    }
    private void RefreshStatus()
    {
        try
        {
            using var doc = JsonDocument.Parse(System.IO.File.ReadAllText(Path.Combine(Folder, "status.json")));
            var root = doc.RootElement;
            var age = DateTime.UtcNow - root.GetProperty("UpdatedUtc").GetDateTime().ToUniversalTime();
            if (age > TimeSpan.FromSeconds(10) || age < TimeSpan.Zero)
                throw new InvalidDataException(L.T("El servicio no está actualizando su estado.", "The service is not updating its status."));
            string? error = root.GetProperty("Error").GetString();
            string? code = root.TryGetProperty("ErrorCode", out var errorCode) ? errorCode.GetString() : null;
            string detail = code switch
            {
                "Configuration" => L.T("No se puede leer la configuración. Se conserva el último límite válido.", "Cannot read the configuration. The last valid limit is kept."),
                "NoDevices" => L.T("No hay dispositivos de salida activos.", "No active output devices."),
                _ => L.T("No se pudo controlar el audio. Consulta service.log para ver los detalles.", "Audio could not be controlled. See service.log for details.")
            };
            SetStatus(error == null, error == null ? L.T("Protección activa", "Protection active") : L.T("Revisa la protección", "Check protection"), error == null
                ? L.T($"Aplicando {root.GetProperty("MaxVolumePercent").GetInt32()} % en {root.GetProperty("Devices").GetInt32()} dispositivo(s) de salida.",
                    $"Enforcing {root.GetProperty("MaxVolumePercent").GetInt32()} % on {root.GetProperty("Devices").GetInt32()} output device(s).")
                : detail);
        }
        catch (Exception)
        {
            SetStatus(false, L.T("Protección sin confirmar", "Protection unconfirmed"),
#if STORE
                L.T("No se recibe un estado válido del motor de audio. Cierra y vuelve a abrir la app.", "No valid audio engine status received. Exit and reopen the app."));
#else
                L.T("No se recibe un estado válido del servicio. Comprueba que esté iniciado.", "No valid status received from the service. Make sure it is running."));
#endif
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            statusTimer.Dispose();
            trayIcon.Visible = false;
            trayIcon.Dispose();
            trayMenu.Dispose();
            brandIcon.Dispose();
        }
        base.Dispose(disposing);
    }
    private void Save()
    {
        try
        {
            FinishValueEdit(true);
            Directory.CreateDirectory(Folder);
            var temp = Path.Combine(Folder, $"settings-{Guid.NewGuid():N}.tmp");
            System.IO.File.WriteAllText(temp, JsonSerializer.Serialize(new { MaxVolumePercent = slider.Value }));
            System.IO.File.Move(temp, FilePath, true);
            savedLimit = slider.Value;
            UpdateSelection();
            feedback.ForeColor = Color.FromArgb(21, 112, 69);
            feedback.Text = L.T($"Límite de {savedLimit} % guardado. Esperando confirmación.", $"Saved {savedLimit} % limit. Waiting for confirmation.");
            RefreshStatus();
        }
        catch (Exception ex) { MessageBox.Show(ex.Message, L.T("No se pudo guardar", "Could not save"), MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
}
