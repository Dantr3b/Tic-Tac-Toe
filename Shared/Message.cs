namespace Shared
{
    public class Message
    {
        public string Type { get; set; }  // MOVE, STATE, INFO, etc.
        public string Content { get; set; } // Exemple : "1,2" pour un coup
    }
}
