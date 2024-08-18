namespace AI_Chess;

public class SmtpConfig
{
    public required string Host { get; set; }
    public int Port { get; set; }
    public required string From { get; set; }
    public required string Password { get; set; }
    public required string[] To { get; set; }

}
