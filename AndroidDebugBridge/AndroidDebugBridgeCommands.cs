namespace AndroidDebugBridge
{
    public enum AndroidDebugBridgeCommands
    {
        AUTH = 0x48545541,
        CLSE = 0x45534C43,
        CNXN = 0x4E584E43,
        OKAY = 0x59414B4F,
        OPEN = 0x4E45504F,
        SYNC = 0x434E5953,
        WRTE = 0x45545257,

        // SYNC sub commands
        DATA = 0x44415441,
        QUIT = 0x51554954,
        RECV = 0x52454356,
        SEND = 0x53454E44,
        STAT = 0x53544154
    }
}