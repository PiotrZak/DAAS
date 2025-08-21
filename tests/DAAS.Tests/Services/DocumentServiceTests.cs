using DAAS.Application.DTOs;
using DAAS.Application.Handlers;
using DAAS.Application.Queries;
using DAAS.Domain.Entities;
using DAAS.Domain.Interfaces;
using FluentAssertions;
using Moq;

namespace DAAS.Tests.Services;

public class DocumentQueryHandlerTests
{
    private readonly Mock<IDocumentRepository> _documentRepositoryMock;

    public DocumentQueryHandlerTests()
    {
        _documentRepositoryMock = new Mock<IDocumentRepository>();
    }

    [Fact]
    public async Task GetAllDocumentsQueryHandler_ReturnsAllDocuments()
    {
        // Arrange
        var documents = new List<Document>
        {
            new()
            {
                Id = 1,
                Title = "Document 1",
                Description = "Description 1",
                CreatedAt = DateTime.UtcNow.AddDays(-10)
            },
            new()
            {
                Id = 2,
                Title = "Document 2",
                Description = "Description 2",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new()
            {
                Id = 3,
                Title = "Document 3",
                Description = "Description 3",
                CreatedAt = DateTime.UtcNow.AddDays(-1)
            }
        };

        _documentRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(documents);

        var handler = new GetAllDocumentsQueryHandler(_documentRepositoryMock.Object);
        var query = new GetAllDocumentsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Should().OnlyContain(d => !string.IsNullOrEmpty(d.Title) && !string.IsNullOrEmpty(d.Description));
        
        var documentsList = result.ToList();
        documentsList[0].Title.Should().Be("Document 1");
        documentsList[0].Description.Should().Be("Description 1");
        
        _documentRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetDocumentByIdQueryHandler_WithValidId_ReturnsDocument()
    {
        // Arrange
        var documentId = 1;
        var document = new Document
        {
            Id = documentId,
            Title = "Test Document",
            Description = "Test Description",
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        _documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId))
            .ReturnsAsync(document);

        var handler = new GetDocumentByIdQueryHandler(_documentRepositoryMock.Object);
        var query = new GetDocumentByIdQuery(documentId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(documentId);
        result.Title.Should().Be("Test Document");
        result.Description.Should().Be("Test Description");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(-1), TimeSpan.FromMinutes(1));

        _documentRepositoryMock.Verify(x => x.GetByIdAsync(documentId), Times.Once);
    }

    [Fact]
    public async Task GetDocumentByIdQueryHandler_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var documentId = 999;
        _documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId))
            .ReturnsAsync((Document?)null);

        var handler = new GetDocumentByIdQueryHandler(_documentRepositoryMock.Object);
        var query = new GetDocumentByIdQuery(documentId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
        _documentRepositoryMock.Verify(x => x.GetByIdAsync(documentId), Times.Once);
    }

    [Fact]
    public async Task GetAllDocumentsQueryHandler_WithEmptyRepository_ReturnsEmptyList()
    {
        // Arrange
        _documentRepositoryMock.Setup(x => x.GetAllAsync())
            .ReturnsAsync(new List<Document>());

        var handler = new GetAllDocumentsQueryHandler(_documentRepositoryMock.Object);
        var query = new GetAllDocumentsQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
        _documentRepositoryMock.Verify(x => x.GetAllAsync(), Times.Once);
    }

    [Fact]
    public async Task GetDocumentByIdQueryHandler_RepositoryMapping_MapsAllProperties()
    {
        // Arrange
        var documentId = 5;
        var createdAt = DateTime.UtcNow.AddDays(-3);
        var document = new Document
        {
            Id = documentId,
            Title = "Complex Document Title",
            Description = "Very detailed description of the document contents and purpose",
            CreatedAt = createdAt
        };

        _documentRepositoryMock.Setup(x => x.GetByIdAsync(documentId))
            .ReturnsAsync(document);

        var handler = new GetDocumentByIdQueryHandler(_documentRepositoryMock.Object);
        var query = new GetDocumentByIdQuery(documentId);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(documentId);
        result.Title.Should().Be("Complex Document Title");
        result.Description.Should().Be("Very detailed description of the document contents and purpose");
        result.CreatedAt.Should().Be(createdAt);

        _documentRepositoryMock.Verify(x => x.GetByIdAsync(documentId), Times.Once);
    }

    [Theory]
    [InlineData(1, "Doc1", "Desc1")]
    [InlineData(100, "Document with ID 100", "Long description")]
    [InlineData(999, "Final Document", "Last description")]
    public async Task GetDocumentByIdQueryHandler_WithVariousDocuments_ReturnsCorrectDocument(int id, string title, string description)
    {
        // Arrange
        var document = new Document
        {
            Id = id,
            Title = title,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        _documentRepositoryMock.Setup(x => x.GetByIdAsync(id))
            .ReturnsAsync(document);

        var handler = new GetDocumentByIdQueryHandler(_documentRepositoryMock.Object);
        var query = new GetDocumentByIdQuery(id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        result.Title.Should().Be(title);
        result.Description.Should().Be(description);
        _documentRepositoryMock.Verify(x => x.GetByIdAsync(id), Times.Once);
    }
}