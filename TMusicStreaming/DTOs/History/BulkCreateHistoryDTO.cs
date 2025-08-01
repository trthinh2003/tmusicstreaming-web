namespace TMusicStreaming.DTOs.History
{
    public class BulkCreateHistoryDTO
    {
        public List<CreateHistoryDTO> Histories { get; set; } = new List<CreateHistoryDTO>();
    }
}
