namespace LostFound_API.DTOs.Institution
{
    public class InstitutionSearchDTO
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Address { get; set; }

        public string? City { get; set; }

        public string? State { get; set; }

        public string? ZipCode { get; set; }

        public string? Website { get; set; }

        public double? Latitude { get; set; }

        public double? Longitude { get; set; }
    }
}
