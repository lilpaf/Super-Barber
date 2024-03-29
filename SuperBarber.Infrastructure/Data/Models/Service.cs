﻿using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static SuperBarber.Infrastructure.Data.DataConstraints;

namespace SuperBarber.Infrastructure.Data.Models
{
    public class Service
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(ServiceNameMaxLength)]
        public string Name { get; set; }

        public int CategoryId { get; set; }

        [ForeignKey(nameof(CategoryId))]
        public Category Category { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? DeleteDate { get; set; }
    }
}