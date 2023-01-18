public class DebugCommandBase
{
    private string m_commandID;
    private string m_commandDescription;
    private string m_commandFormat;

    public string CommandID => m_commandID;
    public string CommandDescription => m_commandDescription;
    public string CommandFormat => m_commandFormat;

    public DebugCommandBase(string id, string description, string format)
    {
        m_commandID = id;
        m_commandDescription = description;
        m_commandFormat = format;
    }
}
