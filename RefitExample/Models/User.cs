namespace RefitExample.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }

    public override string ToString() =>
        string.Join(Environment.NewLine, $"Id: {Id}, Name: {Name}, Email: {Email}");
}
