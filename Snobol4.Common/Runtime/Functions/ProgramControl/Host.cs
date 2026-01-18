//namespace Snobol4.Common;

//    //"erroneous argument for host" /* 254 */,
//    //"error during execution of host" /* 255 */,

//public partial class Executive
//{
//    internal void Host(List<Var> arguments)
//    {
//        if (arguments[0].Convert(VarType.STRING, out _, out var valueStr, this) && (string)valueStr == "")
//        {
//            SystemStack.Push(new StringVar("x64:Windows 11:Snobol4.net 1.0 #"));
//            return;
//        }

//        if (arguments[0].Convert(VarType.INTEGER, out _, out var valueInt, this) && (long)valueInt == 0)
//        {
//            SystemStack.Push(new StringVar(Parent.HostCommand));
//            return;
//        }

//        LogRuntimeException(254);
//    }
//}