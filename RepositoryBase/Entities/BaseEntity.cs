using System;

namespace RepositoryBase.Entities
{
    public class BaseEntity
    {
        public Guid Id { get; set; } = new Guid();
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime LastSavedDate { get; set; } = DateTime.UtcNow;
        public Guid CreatedBy { get; set; }
        public Guid LastSavedBy { get; set; }
        public bool IsDeleted { get; set; }        
    }
}
