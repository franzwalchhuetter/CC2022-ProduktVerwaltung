namespace ProduktVerwaltung.Models
{
    public class ProductItem
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? Bezeichnung { get; set; }
        public float? Preis { get; set; }
        public string? Secret { get; set; }
    }
}
