using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;

namespace Portal.API.Common.Entities.Integrations
{
    public class IntegrationLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Reference { get; set; }
        public string ReferenceType { get; set; }
        public string Request { get; set; }
        public string Response { get; set; }
        public HttpStatusCode? StatusCode { get; set; }
        public DateTime RequestedAt { get; set; }
        public DateTime? RepliedAt { get; set; }
        public double? TimeSpent { get; set; }
        public string Exception { get; set; }
    }
}
