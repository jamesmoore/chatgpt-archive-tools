namespace ChatGPTExport.Decoders
{
    public interface IDecoder<T, R>
    {
        R Decode(T content, MessageContext context);
    }
}
