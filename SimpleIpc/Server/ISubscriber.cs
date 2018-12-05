namespace SimpleIpc.Server
{
    public interface ISubscriber
    {
        void Subscribe(string publisherId);
        void Unsubscribe(string publisherId);
    }
}