using System;

namespace UnitTest
{
    public class StreamEntity
    {
        public Guid EventId { get; set; }
        public long Id { get; set; }
        public override string ToString()
        {
            return $" {this.EventId}, {this.Id}";
        }
    }
}