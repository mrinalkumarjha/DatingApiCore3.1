using System;

namespace DatingApp.Models
{
    public class Photo
    {
        public int Id { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
        public DateTime DateAdded { get; set; }
        public bool IsMain { get; set; }
        public string PublicID { get; set; } // this is id returned by cloudinary

        // By adding this column of user photos will also be deleted once user will be deleted
        public User User { get; set; }
        public int UserId { get; set; }
    }
}