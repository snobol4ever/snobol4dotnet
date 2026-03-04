namespace Snobol4.Common;

using System.Management;

//"erroneous argument for host" /* 254 */,
//"error during execution of host" /* 255 */,

// HOST() snd HOST(0) are supported. 
// Any second or higher argument is ignored
// ALl other arguments result in runtime error 254

public partial class Executive
{
    internal void Host(List<Var> arguments)
    {
        if (arguments[0] is IntegerVar { Data: 0 })
        {
            SystemStack.Push(new StringVar(Parent.BuildOptions.HostParameter));
            return;
        }

        if (arguments[0] is StringVar { Data: "" })
        {
            var cpuName = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");

            if (OperatingSystem.IsWindows())
            {
                var moc = new ManagementObjectSearcher("select * from Win32_Processor").Get();
                foreach (var obj in moc)
                {
                    try
                    {
                        cpuName = obj["Name"].ToString();
                    }
                    catch
                    {
                        // ignored
                    }
                }
            }

            SystemStack.Push(new StringVar(@$"{cpuName}:{Environment.OSVersion}:SNOBOL4.net:Version 0.1#"));
            return;
        }

        SystemStack.Push(StringVar.Null());
    }

}