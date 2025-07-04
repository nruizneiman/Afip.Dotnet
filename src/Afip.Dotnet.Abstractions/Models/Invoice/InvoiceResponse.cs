using System;
using System.Collections.Generic;

namespace Afip.Dotnet.Abstractions.Models.Invoice
{
    /// <summary>
    /// Represents the response from an invoice authorization request
    /// </summary>
    public class InvoiceResponse
    {
        /// <summary>
        /// Authorization result (A=Approved, R=Rejected, P=Partial)
        /// </summary>
        public string Result { get; set; } = string.Empty;
        
        /// <summary>
        /// CAE (Electronic Authorization Code)
        /// </summary>
        public string Cae { get; set; } = string.Empty;
        
        /// <summary>
        /// CAE expiration date
        /// </summary>
        public DateTime? CaeExpirationDate { get; set; }
        
        /// <summary>
        /// Processing date
        /// </summary>
        public DateTime ProcessingDate { get; set; }
        
        /// <summary>
        /// Whether this was a reprocess
        /// </summary>
        public bool IsReprocess { get; set; }
        
        /// <summary>
        /// Invoice details that were authorized
        /// </summary>
        public InvoiceDetail InvoiceDetail { get; set; } = new InvoiceDetail();
        
        /// <summary>
        /// Any observations from AFIP
        /// </summary>
        public List<AfipObservation> Observations { get; set; } = new List<AfipObservation>();
        
        /// <summary>
        /// Whether the authorization was successful
        /// </summary>
        public bool IsSuccessful => Result == "A";
    }
    
    /// <summary>
    /// Details of the authorized invoice
    /// </summary>
    public class InvoiceDetail
    {
        public int Concept { get; set; }
        public int DocumentType { get; set; }
        public long DocumentNumber { get; set; }
        public long InvoiceNumberFrom { get; set; }
        public long InvoiceNumberTo { get; set; }
        public DateTime InvoiceDate { get; set; }
    }
    
    /// <summary>
    /// AFIP observation message
    /// </summary>
    public class AfipObservation
    {
        /// <summary>
        /// Observation code
        /// </summary>
        public int Code { get; set; }
        
        /// <summary>
        /// Observation message
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }
}