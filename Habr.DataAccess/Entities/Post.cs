﻿namespace Habr.DataAccess.Entities
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public DateTime Created { get; set; }
    }
}
