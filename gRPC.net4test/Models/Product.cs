using System;
namespace gRPC.net4test.Models
{
    public class Product
    {
        public DateTime CreatedTime { get; set; }
        public string Name { get; set; }
        public float Speed { get; set; }

        public Product() { }

        public override String ToString() {
            return CreatedTime.ToString("dd.MM.yyyy HH:mm:ss")+ " " + Name + " " + Speed.ToString() ;
        }
    }
}

