namespace ChatGPTExport.Decoders
{
    public interface IDecodeTo<T, R>
    {
        R DecodeTo(T content, MessageContext context);
    }
}
