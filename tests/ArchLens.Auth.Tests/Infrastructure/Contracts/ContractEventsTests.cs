using ArchLens.Contracts.Events;
using FluentAssertions;

namespace ArchLens.Auth.Tests.Infrastructure.Contracts;

public class ContractEventsTests
{
    // ─── UserAccountDeletedEvent ─────────────────────────────────────────

    [Fact]
    public void UserAccountDeletedEvent_ShouldSetAllProperties()
    {
        var userId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;

        var evt = new UserAccountDeletedEvent
        {
            UserId = userId,
            Timestamp = timestamp
        };

        evt.UserId.Should().Be(userId);
        evt.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void UserAccountDeletedEvent_RecordEquality_ShouldWork()
    {
        var id = Guid.NewGuid();
        var ts = DateTime.UtcNow;
        var a = new UserAccountDeletedEvent { UserId = id, Timestamp = ts };
        var b = new UserAccountDeletedEvent { UserId = id, Timestamp = ts };

        a.Should().Be(b);
    }

    [Fact]
    public void UserAccountDeletedEvent_DifferentIds_ShouldNotBeEqual()
    {
        var a = new UserAccountDeletedEvent { UserId = Guid.NewGuid() };
        var b = new UserAccountDeletedEvent { UserId = Guid.NewGuid() };

        a.Should().NotBe(b);
    }

    // ─── AnalysisCompletedEvent ──────────────────────────────────────────

    [Fact]
    public void AnalysisCompletedEvent_ShouldSetAllProperties()
    {
        var analysisId = Guid.NewGuid();
        var diagramId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;
        var providers = new List<string> { "openai", "anthropic" };

        var evt = new AnalysisCompletedEvent
        {
            AnalysisId = analysisId,
            DiagramId = diagramId,
            ResultJson = "{\"result\":\"ok\"}",
            ProvidersUsed = providers,
            ProcessingTimeMs = 1500,
            Timestamp = timestamp
        };

        evt.AnalysisId.Should().Be(analysisId);
        evt.DiagramId.Should().Be(diagramId);
        evt.ResultJson.Should().Be("{\"result\":\"ok\"}");
        evt.ProvidersUsed.Should().BeEquivalentTo(providers);
        evt.ProcessingTimeMs.Should().Be(1500);
        evt.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void AnalysisCompletedEvent_DefaultValues_ShouldBeSet()
    {
        var evt = new AnalysisCompletedEvent();

        evt.ResultJson.Should().BeEmpty();
        evt.ProvidersUsed.Should().BeEmpty();
        evt.ProcessingTimeMs.Should().Be(0);
    }

    [Fact]
    public void AnalysisCompletedEvent_RecordEquality_ShouldWork()
    {
        var id = Guid.NewGuid();
        var ts = DateTime.UtcNow;
        var a = new AnalysisCompletedEvent { AnalysisId = id, Timestamp = ts };
        var b = new AnalysisCompletedEvent { AnalysisId = id, Timestamp = ts };

        a.Should().Be(b);
    }

    // ─── AnalysisFailedEvent ─────────────────────────────────────────────

    [Fact]
    public void AnalysisFailedEvent_ShouldSetAllProperties()
    {
        var analysisId = Guid.NewGuid();
        var diagramId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;
        var failedProviders = new List<string> { "openai" };

        var evt = new AnalysisFailedEvent
        {
            AnalysisId = analysisId,
            DiagramId = diagramId,
            ErrorMessage = "Timeout after 30s",
            FailedProviders = failedProviders,
            Timestamp = timestamp
        };

        evt.AnalysisId.Should().Be(analysisId);
        evt.DiagramId.Should().Be(diagramId);
        evt.ErrorMessage.Should().Be("Timeout after 30s");
        evt.FailedProviders.Should().BeEquivalentTo(failedProviders);
        evt.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void AnalysisFailedEvent_DefaultValues_ShouldBeSet()
    {
        var evt = new AnalysisFailedEvent();

        evt.ErrorMessage.Should().BeEmpty();
        evt.FailedProviders.Should().BeEmpty();
    }

    // ─── DiagramUploadedEvent ────────────────────────────────────────────

    [Fact]
    public void DiagramUploadedEvent_ShouldSetAllProperties()
    {
        var diagramId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;

        var evt = new DiagramUploadedEvent
        {
            DiagramId = diagramId,
            FileName = "arch.png",
            FileHash = "abc123hash",
            StoragePath = "bucket/arch.png",
            UserId = "user-42",
            Timestamp = timestamp
        };

        evt.DiagramId.Should().Be(diagramId);
        evt.FileName.Should().Be("arch.png");
        evt.FileHash.Should().Be("abc123hash");
        evt.StoragePath.Should().Be("bucket/arch.png");
        evt.UserId.Should().Be("user-42");
        evt.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void DiagramUploadedEvent_WithNullUserId_ShouldBeNull()
    {
        var evt = new DiagramUploadedEvent { UserId = null };

        evt.UserId.Should().BeNull();
    }

    [Fact]
    public void DiagramUploadedEvent_DefaultValues_ShouldBeSet()
    {
        var evt = new DiagramUploadedEvent();

        evt.FileName.Should().BeEmpty();
        evt.FileHash.Should().BeEmpty();
        evt.StoragePath.Should().BeEmpty();
    }

    // ─── GenerateReportCommand ───────────────────────────────────────────

    [Fact]
    public void GenerateReportCommand_ShouldSetAllProperties()
    {
        var analysisId = Guid.NewGuid();
        var diagramId = Guid.NewGuid();
        var providers = new List<string> { "anthropic" };
        var timestamp = DateTime.UtcNow;

        var cmd = new GenerateReportCommand
        {
            AnalysisId = analysisId,
            DiagramId = diagramId,
            UserId = "user-1",
            ResultJson = "{\"data\":1}",
            ProvidersUsed = providers,
            ProcessingTimeMs = 2000,
            Timestamp = timestamp
        };

        cmd.AnalysisId.Should().Be(analysisId);
        cmd.DiagramId.Should().Be(diagramId);
        cmd.UserId.Should().Be("user-1");
        cmd.ResultJson.Should().Be("{\"data\":1}");
        cmd.ProvidersUsed.Should().BeEquivalentTo(providers);
        cmd.ProcessingTimeMs.Should().Be(2000);
        cmd.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void GenerateReportCommand_WithNullUserId_ShouldBeNull()
    {
        var cmd = new GenerateReportCommand { UserId = null };

        cmd.UserId.Should().BeNull();
    }

    // ─── ProcessingStartedEvent ──────────────────────────────────────────

    [Fact]
    public void ProcessingStartedEvent_ShouldSetAllProperties()
    {
        var analysisId = Guid.NewGuid();
        var diagramId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;

        var evt = new ProcessingStartedEvent
        {
            AnalysisId = analysisId,
            DiagramId = diagramId,
            StoragePath = "bucket/file.png",
            Timestamp = timestamp
        };

        evt.AnalysisId.Should().Be(analysisId);
        evt.DiagramId.Should().Be(diagramId);
        evt.StoragePath.Should().Be("bucket/file.png");
        evt.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void ProcessingStartedEvent_DefaultValues_ShouldBeSet()
    {
        var evt = new ProcessingStartedEvent();

        evt.StoragePath.Should().BeEmpty();
        evt.AnalysisId.Should().Be(Guid.Empty);
    }

    // ─── ReportFailedEvent ───────────────────────────────────────────────

    [Fact]
    public void ReportFailedEvent_ShouldSetAllProperties()
    {
        var analysisId = Guid.NewGuid();
        var diagramId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;

        var evt = new ReportFailedEvent
        {
            AnalysisId = analysisId,
            DiagramId = diagramId,
            ErrorMessage = "Report generation failed",
            Timestamp = timestamp
        };

        evt.AnalysisId.Should().Be(analysisId);
        evt.DiagramId.Should().Be(diagramId);
        evt.ErrorMessage.Should().Be("Report generation failed");
        evt.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void ReportFailedEvent_DefaultValues_ShouldBeSet()
    {
        var evt = new ReportFailedEvent();

        evt.ErrorMessage.Should().BeEmpty();
    }

    // ─── ReportGeneratedEvent ────────────────────────────────────────────

    [Fact]
    public void ReportGeneratedEvent_ShouldSetAllProperties()
    {
        var reportId = Guid.NewGuid();
        var analysisId = Guid.NewGuid();
        var diagramId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;

        var evt = new ReportGeneratedEvent
        {
            ReportId = reportId,
            AnalysisId = analysisId,
            DiagramId = diagramId,
            Timestamp = timestamp
        };

        evt.ReportId.Should().Be(reportId);
        evt.AnalysisId.Should().Be(analysisId);
        evt.DiagramId.Should().Be(diagramId);
        evt.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void ReportGeneratedEvent_RecordEquality_ShouldWork()
    {
        var id = Guid.NewGuid();
        var ts = DateTime.UtcNow;
        var a = new ReportGeneratedEvent { ReportId = id, Timestamp = ts };
        var b = new ReportGeneratedEvent { ReportId = id, Timestamp = ts };

        a.Should().Be(b);
    }

    // ─── StatusChangedEvent ──────────────────────────────────────────────

    [Fact]
    public void StatusChangedEvent_ShouldSetAllProperties()
    {
        var analysisId = Guid.NewGuid();
        var timestamp = DateTime.UtcNow;

        var evt = new StatusChangedEvent
        {
            AnalysisId = analysisId,
            OldStatus = "Pending",
            NewStatus = "Processing",
            Timestamp = timestamp
        };

        evt.AnalysisId.Should().Be(analysisId);
        evt.OldStatus.Should().Be("Pending");
        evt.NewStatus.Should().Be("Processing");
        evt.Timestamp.Should().Be(timestamp);
    }

    [Fact]
    public void StatusChangedEvent_DefaultValues_ShouldBeSet()
    {
        var evt = new StatusChangedEvent();

        evt.OldStatus.Should().BeEmpty();
        evt.NewStatus.Should().BeEmpty();
    }

    [Fact]
    public void StatusChangedEvent_RecordEquality_ShouldWork()
    {
        var id = Guid.NewGuid();
        var ts = DateTime.UtcNow;
        var a = new StatusChangedEvent { AnalysisId = id, OldStatus = "A", NewStatus = "B", Timestamp = ts };
        var b = new StatusChangedEvent { AnalysisId = id, OldStatus = "A", NewStatus = "B", Timestamp = ts };

        a.Should().Be(b);
    }
}
