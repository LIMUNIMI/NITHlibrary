namespace NITHlibrary.Tools.Ports
{
    public interface IPortListener
    {
        void ReceivePortData(string line);
    }
}