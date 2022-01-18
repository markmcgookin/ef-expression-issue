using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace simple.dbapp
{
    public class Person
    {
        [Required]
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
        
        public Nullable<Guid> VetId { get; set; }
        
        [ExpressionBuilderNullCheckForProperty(nameof(VetId))]
        public Vet Vet { get; set; }
        
        public List<Pet> Pets { get; set; }
    }
}