using SOAP.Web.Models;

namespace SOAP.Web.Services.Interfaces
{
    /// <summary>
    /// Interface for document validation strategies
    /// Demonstrates: ISP (Interface Segregation), Abstraction
    /// </summary>
    public interface IDocumentValidator
    {
        /// <summary>
        /// Validates a document file
        /// </summary>
        Task<DocumentValidationResult> ValidateAsync(IFormFile file, DocumentType documentType);

        /// <summary>
        /// Determines if this validator can handle the document type
        /// </summary>
        bool CanValidate(DocumentType documentType);

        /// <summary>
        /// Gets the document types this validator supports
        /// </summary>
        IEnumerable<DocumentType> SupportedTypes { get; }

        /// <summary>
        /// Gets the validation priority (higher number = higher priority)
        /// </summary>
        int Priority { get; }
    }
}