

namespace SOCKCSProxy
{
    public enum Bytes : byte
    {
        Version = 0x05,
        Reserve = 0x00,
        WithoutAuthentication = 0x00,
        Connect = 0x01,
        Success = 0x00,
        AddressPort = 0x00,
        UnknownAddress = 0x08,
        UnavailableHost = 0x04,
        IPV4 = 0x01,
        Domen = 0x03,
        Accept = 0x00
    }
}
