namespace Intalio.Tools.Common.Ldap
{
    public class LdapSettings
    {
        public string? Path { get; set; }
        public string? User { get; set; }
        public string Password { get; set; } = null!;
        public string Domain { get; set; }
        public bool Enabled { get; set; }
    }
}
