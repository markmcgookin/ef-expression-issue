using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace simple.dbapp
{
    public class Pet
    {
        [Required]
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Species Species { get; set; }
        public Guid OwnerId { get; set; }

        
        public Person Owner { get; set; }
    }

    public enum Species
    {
        Dog,
        Cat,
        Fish,
        Snake
    }
}