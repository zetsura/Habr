namespace Habr.DataAccess.Entities
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public ICollection<Comment> Comments { get; set; }
        public ICollection<Post> Posts { get; set; }
    }
}
