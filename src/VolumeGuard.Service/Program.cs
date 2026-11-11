#if !STORE
using System.ServiceProcess;
#endif
using System.Runtime.InteropServices;
using System.Text.Json;

namespace VolumeGuard.Service;

#if !STORE
internal static class Program
{
    static void Main(string[] args)
    {
        if (Environment.UserInteractive && args.Contains("--console"))
        {
            using var engine = new VolumeEngine();
            engine.Start();
            Console.WriteLine("VolumeGuard en modo consola. Enter para terminar.");
            Console.ReadLine();
            engine.Stop();
        }
        else ServiceBase.Run(new GuardService());
    }
}

public sealed class GuardService : ServiceBase
{
    private readonly VolumeEngine engine = new();
    public GuardService() { ServiceName = "VolumeGuard"; CanStop = true; AutoLog = true; }
    protected override void OnStart(string[] args) => engine.Start();
    protected override void OnStop() => engine.Stop();
    protected override void Dispose(bool disposing) { if (disposing) engine.Dispose(); base.Dispose(disposing); }
}

#endif
public sealed class VolumeEngine : IDisposable
{
    private CancellationTokenSource? cts;
    private Task? worker;
    private static readonly string ConfigFile = Path.Combine(DataFolder, "settings.json");
    internal static string DataFolder => Path.Combine(Environment.GetFolderPath(
#if STORE
        Environment.SpecialFolder.LocalApplicationData), "VolumeGuardStore");
#else
        Environment.SpecialFolder.CommonApplicationData), "VolumeGuard");
#endif
    public void Start()
    {
        cts = new CancellationTokenSource();
        worker = Task.Run(() => Loop(cts.Token));
    }
    private static int lastValidLimit; // Fail closed until a valid configuration has been read.
    private static int ReadLimit()
    {
        try
        {
            using var doc = JsonDocument.Parse(File.ReadAllText(ConfigFile));
            int limit = doc.RootElement.GetProperty("MaxVolumePercent").GetInt32();
            if (limit is < 0 or > 100) throw new InvalidDataException("Límite fuera de rango.");
            lastValidLimit = limit;
            Diagnostics.ConfigurationError = null;
            return limit;
        }
        catch (Exception ex)
        {
            Diagnostics.ConfigurationError = "No se pudo leer la configuración: " + ex.Message;
            Diagnostics.Log(Diagnostics.ConfigurationError);
            return lastValidLimit;
        }
    }
    private static async Task Loop(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                int limit = ReadLimit();
                int devices = AudioLimiter.Enforce(limit);
                Diagnostics.Publish(limit, devices, Diagnostics.ConfigurationError);
            }
            catch (Exception ex)
            {
                Diagnostics.Log(ex.ToString());
                Diagnostics.Publish(lastValidLimit, 0, ex.Message);
            }
            try { await Task.Delay(120, token); } catch (OperationCanceledException) { break; }
        }
    }
    public void Stop() { cts?.Cancel(); try { worker?.Wait(TimeSpan.FromSeconds(3)); } catch { } }
    public void Dispose() { Stop(); cts?.Dispose(); }
}

internal static class Diagnostics
{
    private static readonly string Folder = VolumeEngine.DataFolder;
    private static DateTime nextStatus;
    private static DateTime nextLog;
    public static string? ConfigurationError { get; set; }

    public static void Log(string message)
    {
        if (DateTime.UtcNow < nextLog) return;
        nextLog = DateTime.UtcNow.AddMinutes(1);
        try
        {
            Directory.CreateDirectory(Folder);
            string path = Path.Combine(Folder, "service.log");
            if (File.Exists(path) && new FileInfo(path).Length > 1_000_000)
                File.Move(path, path + ".old", true);
            File.AppendAllText(path, $"{DateTime.UtcNow:O} {message}{Environment.NewLine}");
        }
        catch (Exception ex) { Console.Error.WriteLine("No se pudo escribir el registro: " + ex.Message); }
    }

    public static void Publish(int limit, int devices, string? error)
    {
        if (DateTime.UtcNow < nextStatus) return;
        nextStatus = DateTime.UtcNow.AddSeconds(2);
        try
        {
            Directory.CreateDirectory(Folder);
            string path = Path.Combine(Folder, "status.json");
            File.WriteAllText(path + ".tmp", JsonSerializer.Serialize(new
            {
                UpdatedUtc = DateTime.UtcNow, MaxVolumePercent = limit, Devices = devices,
                Error = error ?? (devices == 0 ? "No hay dispositivos de salida activos." : null),
                ErrorCode = error != null ? (error == ConfigurationError ? "Configuration" : "Audio")
                    : devices == 0 ? "NoDevices" : null
            }));
            File.Move(path + ".tmp", path, true);
        }
        catch (Exception ex) { Log("No se pudo publicar el estado: " + ex.Message); }
    }
}

internal static class AudioLimiter
{
    // Core Audio COM is accessed on an MTA worker thread. Each poll re-enumerates endpoints
    // so newly connected USB/HDMI/Bluetooth devices are also covered.
    public static int Enforce(int maxPercent)
    {
        IMMDeviceEnumerator? enumerator = null;
        IMMDeviceCollection? collection = null;
        try
        {
            enumerator = (IMMDeviceEnumerator)new MMDeviceEnumeratorComObject();
            Marshal.ThrowExceptionForHR(enumerator.EnumAudioEndpoints(EDataFlow.eRender, DeviceState.Active, out collection));
            Marshal.ThrowExceptionForHR(collection.GetCount(out uint count));
            Exception? failure = null;
            for (uint i = 0; i < count; i++)
            {
                IMMDevice? device = null;
                IAudioEndpointVolume? volume = null;
                try
                {
                    Marshal.ThrowExceptionForHR(collection.Item(i, out device));
                    Guid iid = typeof(IAudioEndpointVolume).GUID;
                    Marshal.ThrowExceptionForHR(device.Activate(ref iid, CLSCTX.ALL, IntPtr.Zero, out object obj));
                    volume = (IAudioEndpointVolume)obj;
                    Marshal.ThrowExceptionForHR(volume.GetMasterVolumeLevelScalar(out float current));
                    float maximum = maxPercent / 100f;
                    if (current > maximum + 0.0001f)
                    {
                        Guid context = Guid.Empty;
                        Marshal.ThrowExceptionForHR(volume.SetMasterVolumeLevelScalar(maximum, ref context));
                    }
                }
                catch (COMException ex) { failure ??= ex; }
                finally
                {
                    if (volume != null) Marshal.ReleaseComObject(volume);
                    if (device != null) Marshal.ReleaseComObject(device);
                }
            }
            if (failure != null) throw new InvalidOperationException("No se pudo controlar uno o más dispositivos de audio.", failure);
            return checked((int)count);
        }
        finally
        {
            if (collection != null) Marshal.ReleaseComObject(collection);
            if (enumerator != null) Marshal.ReleaseComObject(enumerator);
        }
    }
}

[ComImport, Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
internal class MMDeviceEnumeratorComObject { }
internal enum EDataFlow { eRender, eCapture, eAll }
[Flags] internal enum DeviceState : uint { Active = 1, Disabled = 2, NotPresent = 4, Unplugged = 8, All = 15 }
[Flags] internal enum CLSCTX : uint { INPROC_SERVER = 1, INPROC_HANDLER = 2, LOCAL_SERVER = 4, REMOTE_SERVER = 16, ALL = 23 }
[ComImport, Guid("A95664D2-9614-4F35-A746-DE8DB63617E6"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMMDeviceEnumerator
{
    [PreserveSig] int EnumAudioEndpoints(EDataFlow dataFlow, DeviceState stateMask, out IMMDeviceCollection devices);
    [PreserveSig] int GetDefaultAudioEndpoint(EDataFlow dataFlow, int role, out IMMDevice endpoint);
    [PreserveSig] int GetDevice([MarshalAs(UnmanagedType.LPWStr)] string id, out IMMDevice device);
    [PreserveSig] int RegisterEndpointNotificationCallback(IntPtr client);
    [PreserveSig] int UnregisterEndpointNotificationCallback(IntPtr client);
}
[ComImport, Guid("0BD7A1BE-7A1A-44DB-8397-CC5392387B5E"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMMDeviceCollection
{
    [PreserveSig] int GetCount(out uint count);
    [PreserveSig] int Item(uint index, out IMMDevice device);
}
[ComImport, Guid("D666063F-1587-4E43-81F1-B948E807363F"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IMMDevice
{
    [PreserveSig] int Activate(ref Guid iid, CLSCTX clsCtx, IntPtr activationParams, [MarshalAs(UnmanagedType.IUnknown)] out object instance);
    [PreserveSig] int OpenPropertyStore(int access, out IntPtr properties);
    [PreserveSig] int GetId(out IntPtr id);
    [PreserveSig] int GetState(out DeviceState state);
}
[ComImport, Guid("5CDF2C82-841E-4546-9722-0CF74078229A"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
internal interface IAudioEndpointVolume
{
    [PreserveSig] int RegisterControlChangeNotify(IntPtr notify);
    [PreserveSig] int UnregisterControlChangeNotify(IntPtr notify);
    [PreserveSig] int GetChannelCount(out uint channelCount);
    [PreserveSig] int SetMasterVolumeLevel(float levelDb, ref Guid eventContext);
    [PreserveSig] int SetMasterVolumeLevelScalar(float level, ref Guid eventContext);
    [PreserveSig] int GetMasterVolumeLevel(out float levelDb);
    [PreserveSig] int GetMasterVolumeLevelScalar(out float level);
    [PreserveSig] int SetChannelVolumeLevel(uint channel, float levelDb, ref Guid eventContext);
    [PreserveSig] int SetChannelVolumeLevelScalar(uint channel, float level, ref Guid eventContext);
    [PreserveSig] int GetChannelVolumeLevel(uint channel, out float levelDb);
    [PreserveSig] int GetChannelVolumeLevelScalar(uint channel, out float level);
    [PreserveSig] int SetMute([MarshalAs(UnmanagedType.Bool)] bool mute, ref Guid eventContext);
    [PreserveSig] int GetMute([MarshalAs(UnmanagedType.Bool)] out bool mute);
    [PreserveSig] int GetVolumeStepInfo(out uint step, out uint stepCount);
    [PreserveSig] int VolumeStepUp(ref Guid eventContext);
    [PreserveSig] int VolumeStepDown(ref Guid eventContext);
    [PreserveSig] int QueryHardwareSupport(out uint mask);
    [PreserveSig] int GetVolumeRange(out float minDb, out float maxDb, out float incrementDb);
}
